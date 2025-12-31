using System;
using Microsoft.Data.Sqlite;
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

            var createDailySummaryTable = @"
                CREATE TABLE IF NOT EXISTS DailySummary (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UsageDate TEXT NOT NULL,
                    TotalTimeSeconds INTEGER NOT NULL,
                     AppUsageBreakdown TEXT
                );";

            using var cmd = connection.CreateCommand();

            cmd.CommandText = createAppUsageTable;
            cmd.ExecuteNonQuery();

            cmd.CommandText = createDailySummaryTable;
            cmd.ExecuteNonQuery();

        }
    }
}
