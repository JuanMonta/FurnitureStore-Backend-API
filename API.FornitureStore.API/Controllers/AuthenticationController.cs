using API.FornitureStore.API.Configuracion;
using API.FornitureStore.Data;
using API.FornitureStore.Shared;
using API.FornitureStore.Shared.Auth;
using API.FornitureStore.Shared.Common;
using API.FornitureStore.Shared.DTOs;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace API.FornitureStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;
        private readonly IEmailSender _emailSender;
        private readonly APIFornitureStoreContext _context;
        private readonly TokenValidationParameters _tokenValidationParameters;
        public AuthenticationController(UserManager<IdentityUser> userManager,
            IOptions<JwtConfig> jwtConfig,
            IEmailSender emailSender,
            APIFornitureStoreContext context,
            TokenValidationParameters tokenValidationParameters)
        {
            this._userManager = userManager;
            this._jwtConfig = jwtConfig.Value;
            this._emailSender = emailSender;
            this._context = context;
            this._tokenValidationParameters = tokenValidationParameters;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest();
            //verificar si el email ya existe
            var emailExists = await _userManager.FindByEmailAsync(request.EmailAddres);
            if (emailExists != null)
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>() { "El email ya esta en uso" }
                });
            }

            //Crear el usuario
            var user = new IdentityUser()
            {
                Email = request.EmailAddres,
                UserName = request.Name,
                EmailConfirmed = false
            };

            var isCreated = await _userManager.CreateAsync(user, request.Password);
            if (isCreated.Succeeded)
            {
                //var token = GenerateToken(user);
                await SendVerificationEmail(user);
                return Ok(new AuthResult()
                {
                    Result = true,
                    //Token = token
                });
            }
            else
            {
                var errors = new List<string>();
                foreach (var err in isCreated.Errors)
                {
                    errors.Add(err.Description);
                }
                return BadRequest(new AuthResult
                {
                    Result = false,
                    Errors = errors
                });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest();

            //Verificar si el usuario existe
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser == null) return BadRequest(
                new AuthResult()
                {
                    Errors = new List<string>() { "Invalid payload." },
                    Result = false
                }
                );

            //Verificar si el email esta confirmado
            if (!existingUser.EmailConfirmed) return BadRequest(
                new AuthResult()
                {
                    Errors = new List<string>() { "Email need to be confirmed." },
                    Result = false
                }
                );


            //Verificar la contrasena
            var checkPassword = await _userManager.CheckPasswordAsync(existingUser, request.Password);
            if (!checkPassword) return BadRequest(
                new AuthResult()
                {
                    Errors = new List<string>() { "Invalid credentials." },
                    Result = false
                }
                );

            var token = await GenerateTokenAsync(existingUser);

            return Ok(token);
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (!ModelState.IsValid) return BadRequest(new AuthResult
            {
                Errors = new List<string> { "Invalid parameters." },
                Result = false
            });

            var results = await verifyAndgenerateTokenAsync(tokenRequest);
            if (results == null)
            {
                return BadRequest(new AuthResult
                {
                    Errors = new List<string> { "Invalid token" },
                    Result = false
                });
            }
            return Ok(results);
        }


        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code)) return BadRequest(new AuthResult
            {
                Errors = new List<string> { "Invalid email confirmation." },
                Result = false
            });

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound($"Unable to load user with Id '{userId}'");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            var result = await _userManager.ConfirmEmailAsync(user, code);

            var status = result.Succeeded ? "Thank you for confirming your email." : "There has been an error confirming your email.";
            return Ok(status);
        }

        private async Task<AuthResult> GenerateTokenAsync(IdentityUser user)
        {
            //Implementar la generacion del token
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new ClaimsIdentity(
                    new[]
                    {
                        new Claim("Id", user.Id),
                        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
                    })),
                Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                //JwtId = Guid.NewGuid().ToString(),
                Token = RandomGenerator.GenerateRandomString(23),
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                IsRevoked = false,
                IsUsed = false,
                UserId = user.Id,
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResult
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
                Result = true
            };
        }


        private async Task SendVerificationEmail(IdentityUser user)
        {
            var verificationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            verificationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(verificationCode));

            //example: https://yourfrontend.com/api/verifyemail?userid=xxxx&code=yyyy
            var callbackUrl = Url.Action("ConfirmEmail", "Authentication",
                new { userId = user.Id, code = verificationCode },
                protocol: Request.Scheme);
            var emailBody = $"Porfavor confirme su cuenta en <a href='{callbackUrl}'> click here </a>";

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email", emailBody);
        }



        private async Task<AuthResult> verifyAndgenerateTokenAsync(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            try
            {
                _tokenValidationParameters.ValidateLifetime = false;

                var tokenBeingVerified = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                    if (!result || tokenBeingVerified == null)
                        throw new Exception("Invalid Token");
                }

                var utcExpiryDate = long.Parse(tokenBeingVerified.Claims
                    .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDate = DateTimeOffset.FromUnixTimeSeconds(utcExpiryDate).UtcDateTime;
                if (expiryDate < DateTime.UtcNow)
                    throw new Exception("Token Expired");

                var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == tokenRequest.RefreshToken);
                if (storedToken == null)
                    throw new Exception("Invalid Token");

                if (storedToken.IsUsed || storedToken.IsRevoked)
                    throw new Exception("Invalid Token");

                var jti = tokenBeingVerified.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

                if (jti != storedToken.JwtId)
                    throw new Exception("Invalid Token");

                if (storedToken.ExpiryDate < DateTime.UtcNow)
                    throw new Exception("Token Expired");

                storedToken.IsUsed = true;
                _context.RefreshTokens.Update(storedToken);
                await _context.SaveChangesAsync();

                var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);
                return await GenerateTokenAsync(dbUser);
            }
            catch (Exception ex)
            {
                var message = ex.Message == "Invalid Token" || ex.Message == "Token Expired" ? ex.Message : "Internal Server Error";

                return new AuthResult
                {
                    Errors = new List<string> { message },
                    Result = false
                };
            }



        }

    }

}
