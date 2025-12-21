using System;
using System.Data.SQLite;
using System.IO;

namespace DigitalWellbeing.Data
{
    public static class DatabaseInitializer
    {
        private static readonly string DbFileName = "DigitalWellbeing.db";

        public static string GetDatabasePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DbFileName);
        }

        public static void Initialize()
        {
            var dbPath = GetDatabasePath();

            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();

            CreateTables(connection);
        }

        private static void CreateTables(SQLiteConnection connection)
        {
            var createAppUsageTable = @"
                CREATE TABLE IF NOT EXISTS AppUsage (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AppName TEXT NOT NULL,
                    UsageDate TEXT NOT NULL,
                    TimeUsedSeconds INTEGER NOT NULL
                );";

            var createDailySummaryTable = @"
                CREATE TABLE IF NOT EXISTS DailySummary (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UsageDate TEXT NOT NULL,
                    TotalTimeSeconds INTEGER NOT NULL
                );";

            using var cmd = new SQLiteCommand(connection);
            cmd.CommandText = createAppUsageTable;
            cmd.ExecuteNonQuery();

            cmd.CommandText = createDailySummaryTable;
            cmd.ExecuteNonQuery();

        }
    }
}
