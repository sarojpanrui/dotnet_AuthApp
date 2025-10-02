namespace AuthApp.Config
{

    public class AuthDataBaseSettings
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public string AuthCollection { get; set; }

        public string RefreshTokenCollection { get; set; } = null!; 


    }

}