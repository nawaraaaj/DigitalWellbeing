using DigitalWellbeing.Data;
using DigitalWellbeing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace DigitalWellbeing.Services
{
    public class DailySummaryService
    {
        private readonly string _dbPath;

        public DailySummaryService()
        {
            _dbPath = DatabaseInitializer.GetDatabasePath();
        }

        
        public DailySummary? GenerateOrUpdateDailySummary(DateTime date)
        {
            var appUsages = GetAppUsagesForDate(date);
            if (!appUsages.Any())
                return null;

            // Aggregate usage per app
            var appUsageDict = new Dictionary<string, int>();
            foreach (var usage in appUsages)
            {
                if (appUsageDict.ContainsKey(usage.AppName))
                    appUsageDict[usage.AppName] += usage.TimeUsedSeconds;
                else
                    appUsageDict[usage.AppName] = usage.TimeUsedSeconds;
            }

            int totalTime = appUsageDict.Values.Sum();
            string jsonBreakdown = JsonSerializer.Serialize(appUsageDict);

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            // Check if summary already exists
            string checkSql = "SELECT Id FROM DailySummary WHERE UsageDate = @date;";
            using var checkCmd = new SqliteCommand(checkSql, connection);
            checkCmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
            object? result = checkCmd.ExecuteScalar();

            if (result != null)
            {
                // Update existing record
                int id = Convert.ToInt32(result);
                string updateSql = @"UPDATE DailySummary 
                                     SET TotalTimeSeconds = @total, AppUsageBreakdown = @breakdown 
                                     WHERE Id = @id;";

                using var updateCmd = new SqliteCommand(updateSql, connection);
                updateCmd.Parameters.AddWithValue("@total", totalTime);
                updateCmd.Parameters.AddWithValue("@breakdown", jsonBreakdown);
                updateCmd.Parameters.AddWithValue("@id", id);
                updateCmd.ExecuteNonQuery();

                return new DailySummary
                {
                    Id = id,
                    UsageDate = date.Date,
                    TotalTimeSeconds = totalTime,
                    AppUsageBreakdown = jsonBreakdown
                };
            }
            else
            {
                // Insert new record
                string insertSql = @"INSERT INTO DailySummary (UsageDate, TotalTimeSeconds, AppUsageBreakdown)
                                     VALUES (@date, @total, @breakdown);";
                using var insertCmd = new SqliteCommand(insertSql, connection);
                insertCmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
                insertCmd.Parameters.AddWithValue("@total", totalTime);
                insertCmd.Parameters.AddWithValue("@breakdown", jsonBreakdown);
                insertCmd.ExecuteNonQuery();

                long newId;
                using (var cmd = new SqliteCommand("SELECT last_insert_rowid();", connection))
                {
                    newId = Convert.ToInt64(cmd.ExecuteScalar() ?? 0L);
                }

                return new DailySummary
                {
                    Id = (int)newId,
                    UsageDate = date.Date,
                    TotalTimeSeconds = totalTime,
                    AppUsageBreakdown = jsonBreakdown
                };
            }
        }

        /// Fetch all AppUsage records for a specific date
        private List<AppUsage> GetAppUsagesForDate(DateTime date)
        {
            var list = new List<AppUsage>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            string sql = @"SELECT Id, AppName, UsageDate, TimeUsedSeconds 
                           FROM AppUsage 
                           WHERE UsageDate = @date;";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));

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
