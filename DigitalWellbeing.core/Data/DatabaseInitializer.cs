using Microsoft.Data.Sqlite;
using System.IO;

namespace DigitalWellbeing.Core.Data
{
    public static class DatabaseInitializer
    {
        private static readonly string DbFileName = "DigitalWellbeing.db";

        public static string GetDatabasePath()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DigitalWellbeing");

            Directory.CreateDirectory(folder);
            return Path.Combine(folder, DbFileName);
        }

        public static void Initialize()
        {
            SQLitePCL.Batteries.Init();

            var dbPath = GetDatabasePath();

            using var connection = new SqliteConnection($"Data Source={dbPath}");

            connection.Open();

            CreateTables(connection);
        }

        private static void CreateTables(SqliteConnection connection)
        {
            var createAppUsageTable = @"
                CREATE TABLE IF NOT EXISTS AppUsage (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AppName TEXT NOT NULL,
                    UsageDate TEXT NOT NULL,
                    TimeUsedSeconds INTEGER NOT NULL
                );";

            using var cmd = connection.CreateCommand();

            cmd.CommandText = createAppUsageTable;
            cmd.ExecuteNonQuery();
        }
    }
}
