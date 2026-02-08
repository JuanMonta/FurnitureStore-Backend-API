namespace API.FornitureStore.API.Configuracion
{
    public class JwtConfig
    {
        public string Secret { get; set; }
        public TimeSpan ExpiryTime { get; set; }
    }
}
