using API.FornitureStore.API.Configuracion;
using API.FornitureStore.API.Services;
using API.FornitureStore.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API.FornitureStore.API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Porfavor introdusca un token válido utilizando Bearer schema: Ejm: Introducir Bearer 464644654646sfasdasasdada",
        Name = "Authorización",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("APIFornitureStoreContext");
/// Debes revisar ->
/// <see cref="API.FornitureStore.Data.APIFornitureStoreContext"/>
/// Porque cambia pequeñas cosas al usar entre SQLite y Mysql
builder.Services.AddDbContext<APIFornitureStoreContext>(options =>
    // Descomenta aquí la DB que necesites.

    //Para usar SQLite
    //options.UseSqlite(connectionString)

    //Para usar Mysql, en este caso uso MariaDB
    //ServerVersion se conecta a MariaDb, detecta la versión exacta
    //y ajusta las consultas automáticamente.
    options.UseMySql(
        connectionString, ServerVersion.AutoDetect(connectionString)
    )
);



builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddSingleton<IEmailSender, EmailService>();

var key = Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JwtConfig:Secret").Value);
var tokenValidationParameters  = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    //En porduccion todos estos deben ser true
    ValidateIssuer = false,
    ValidateAudience = false,
    RequireExpirationTime = false,
    //--------------------------
    ValidateLifetime = true
};

builder.Services.AddSingleton(tokenValidationParameters);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(jwt =>
    {
        
        jwt.SaveToken = true;
        jwt.TokenValidationParameters = tokenValidationParameters;
    });

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    //En producccion debe ser true
    options.SignIn.RequireConfirmedAccount = false
).AddEntityFrameworkStores<APIFornitureStoreContext>();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
