namespace SDNet.Services
{
    public sealed class SqlConnectionContext
    {
        private static SqlConnectionContext? _instance;
        private static readonly object _instanceLock = new();

        private readonly object _sync = new();

        private SqlConnectionContext()
        {
        }

        public static SqlConnectionContext Instance => GetInstance();

        public static SqlConnectionContext GetInstance()
        {
            if (_instance is not null)
            {
                return _instance;
            }

            lock (_instanceLock)
            {
                _instance ??= new SqlConnectionContext();
                return _instance;
            }
        }

        public string ConnectionString { get; private set; } = string.Empty;

        public string Server { get; private set; } = string.Empty;

        public string Database { get; private set; } = string.Empty;

        public string SqlLogin { get; private set; } = string.Empty;

        public bool IsInitialized => !string.IsNullOrWhiteSpace(ConnectionString);

        public DateTime? CreatedAt { get; private set; }

        public static void Initialize(string server, string database, string sqlLogin, string sqlPassword)
        {
            SqlConnectionContext instance = GetInstance();
            instance.InitializeInternal(server, database, sqlLogin, sqlPassword);
        }

        private void InitializeInternal(string server, string database, string sqlLogin, string sqlPassword)
        {
            lock (_sync)
            {
                string safeServer = string.IsNullOrWhiteSpace(server) ? "localhost" : server.Trim();
                string safeDatabase = string.IsNullOrWhiteSpace(database) ? "SDNet" : database.Trim();
                string safeUser = string.IsNullOrWhiteSpace(sqlLogin) ? "sdnet_app" : sqlLogin.Trim();
                string safePassword = string.IsNullOrWhiteSpace(sqlPassword) ? "sdnet123" : sqlPassword;

                Server = safeServer;
                Database = safeDatabase;
                SqlLogin = safeUser;
                ConnectionString =
                    $"Server={safeServer};Database={safeDatabase};User ID={safeUser};Password={safePassword};TrustServerCertificate=True;Encrypt=False;Connect Timeout=30;Application Name=SDNet[{safeUser}];";
                CreatedAt = DateTime.Now;
            }
        }
    }
}
