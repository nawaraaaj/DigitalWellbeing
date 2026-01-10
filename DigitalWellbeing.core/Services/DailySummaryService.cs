using DigitalWellbeing.Core.Data;
using DigitalWellbeing.Core.Models;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace DigitalWellbeing.Core.Services
{
    public class DailySummaryService
    {
        private readonly string dbPath;

        public DailySummaryService()
        {
            dbPath = DatabaseInitializer.GetDatabasePath();
        }

        
       public DailySummary? GenerateOrUpdateDailySummary()
        {
            var appUsages = GetAppUsagesForToday();
            if (!appUsages.Any())
                return null;

            var appUsageDict = new Dictionary<string, int>();
            foreach( var usage in appUsages)
            {
                if (appUsageDict.ContainsKey(usage.AppName))
                    appUsageDict[usage.AppName] += usage.TimeUsedSeconds;
                else
                    appUsageDict[usage.AppName] = usage.TimeUsedSeconds;
            }

            int totalTime = appUsageDict.Values.Sum();
            string jsonBreakdown = JsonSerializer.Serialize(appUsageDict);

            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            string sql = @"
                    INSERT INTO DailySummary (UsageDate, TotalTimeSeconds, AppUsageBreakdown)
                    VALUES (date('now','localtime'), @total, @breakdown)
                    ON CONFLICT(UsageDate)
                     DO UPDATE SET
                    TotalTimeSeconds = excluded.TotalTimeSeconds,
                    AppUsageBreakdown = excluded.AppUsageBreakdown;";

            using var cmd = new SqliteCommand(sql, connection);

            using var pragmaCmd = connection.CreateCommand();
            pragmaCmd.CommandText = "PRAGMA journal_mode=WAL;";
            pragmaCmd.ExecuteNonQuery();

            cmd.Parameters.AddWithValue("@total", totalTime);
            cmd.Parameters.AddWithValue("@breakdown", jsonBreakdown);
            cmd.ExecuteNonQuery();


            return new DailySummary
            {
                UsageDate = DateTime.Today,
                TotalTimeSeconds = totalTime,
                AppUsageBreakdown = jsonBreakdown
            };
        }

        /// Fetch all AppUsage records for a specific date
        private List<AppUsage> GetAppUsagesForToday()
        {
            var list = new List<AppUsage>();
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            string sql = @"SELECT Id, AppName, UsageDate, TimeUsedSeconds 
              FROM AppUsage 
              WHERE UsageDate = date('now','localtime');";
            using var cmd = new SqliteCommand(sql, connection);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new AppUsage
                {
                    Id = reader.GetInt32(0),
                    AppName = reader.GetString(1),
                    UsageDate = DateTime.Parse(reader.GetString(2)),
                    TimeUsedSeconds = reader.GetInt32(3)
                });
            }

            return list;
        }
    }
}
