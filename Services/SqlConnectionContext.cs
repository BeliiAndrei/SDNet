namespace SDNet.Services
{
    public sealed class SqlConnectionContext
    {
        private static readonly Lazy<SqlConnectionContext> _lazyInstance =
            new(() => new SqlConnectionContext(), LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly object _sync = new();

        private SqlConnectionContext()
        {
        }

        public static SqlConnectionContext Instance => _lazyInstance.Value;

        public string ConnectionString { get; private set; } = string.Empty;

        public string Server { get; private set; } = string.Empty;

        public string Database { get; private set; } = string.Empty;

        public bool IsInitialized => !string.IsNullOrWhiteSpace(ConnectionString);

        public DateTime? CreatedAt { get; private set; }

        public void Initialize(string server, string database, string appUserName)
        {
            lock (_sync)
            {
                string safeServer = string.IsNullOrWhiteSpace(server) ? "localhost" : server.Trim();
                string safeDatabase = string.IsNullOrWhiteSpace(database) ? "SDNet" : database.Trim();
                string safeUser = string.IsNullOrWhiteSpace(appUserName) ? "anonymous" : appUserName.Trim();

                Server = safeServer;
                Database = safeDatabase;
                ConnectionString =
                    $"Server={safeServer};Database={safeDatabase};Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;Connect Timeout=30;Application Name=SDNet[{safeUser}];";
                CreatedAt = DateTime.Now;
            }
        }
    }
}
