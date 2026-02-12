using MySql.Data.MySqlClient;
using Next.Clients.Providers;
using System.Diagnostics;

namespace Base.Api.Data
{
    public class Connection
    {
        public MySqlDb Db { get; }

        public Connection(IConfiguration config)
        {
            // Decide which settings to use
            var sectionName = config.GetValue<bool>("DevMode") ? "DevSettings" : "ProdSettings";
            var settings = config.GetSection(sectionName);

            string host = settings["Host"] ?? throw new InvalidOperationException($"{sectionName}:Host missing");
            string dbName = settings["Name"] ?? throw new InvalidOperationException($"{sectionName}:Name missing");
            uint port = uint.TryParse(settings["Port"], out var p) ? p : 3306;
            string user = settings["User"] ?? throw new InvalidOperationException($"{sectionName}:User missing");
            string pass = settings["Pass"] ?? throw new InvalidOperationException($"{sectionName}:Pass missing");
            uint timeout = uint.TryParse(settings["ConnectTimeout"], out var t) ? t : 30;
            var ssl = Enum.TryParse<MySqlSslMode>(settings["SslMode"], true, out var s) ? s : MySqlSslMode.Disabled;

            // Build the connection
            Db = MySqlDbBuilder
                .For(host, dbName)
                .Port(port)
                .WithCredentials(user, pass)
                .ConnectTimeout(timeout)
                .SslMode(ssl)
                .Logger(DebugLog)
                .Build();
        }

        private void DebugLog(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
