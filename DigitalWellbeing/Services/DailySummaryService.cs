using DigitalWellbeing.Data;
using DigitalWellbeing.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text.Json;

namespace DigitalWellbeing.Services
{
    public class DailySummaryService
    {
        private readonly string _dbPath;
        public DailySummaryService()
        {
            _dbPath = DatabaseInitializer.GetDatabasePath();
        }

        public DailySummary GenerateOrUpdateDailySummary(DateTime date)
        {
            var appUsages = GetAppUsagesForDate(date);
            if (!appUsages.Any())
                return null;


            //per app usage
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
            using var connection = new SqliteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string checkSql = "SELECT Id FROM DailySummary WHERE UsageDate = @date;";
            using var checkCmd = new SqliteCommand(checkSql, connection);
            checkCmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
            object result = checkCmd.ExecuteScalar();

            DailySummary dailysummary = new DailySummary
            {
                UsageDate = date.Date,
                TotalTimeSeconds = totalTime,
                AppUsageBreakdown = jsonBreakdown,
            };

            if (result != null)
            {
                int id = Convert.ToInt32(result);
                string updateSql = @"UPDATE DailySummary SET TotalTimeSeconds = @total, AppUsageBreakDown = @breakdown WHERE ID = @id;";

                using var updateCmd = new SqliteCommand(updateSql, connection);
                updateCmd.Parameters.AddWithValue("@total", totalTime);
                updateCmd.Parameters.AddWithValue("@breakdown", jsonBreakdown);
                updateCmd.Parameters.AddWithValue("id", id);
                updateCmd.ExecuteNonQuery();
            }
            else
            {
                string insertSql = @"INSERT INTO DailySummary (UsageDate, TotalTimeSeconds, AppUsageBreakdown)
                                     VALUES (@date, @total, @breakdown);";
                using var insertCmd = new SqliteCommand(insertSql, connection);
                insertCmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
                insertCmd.Parameters.AddWithValue("@total", totalTime);
                insertCmd.Parameters.AddWithValue("@breakdown", jsonBreakdown);
                insertCmd.ExecuteNonQuery();
            }

            return dailysummary;
        }

        //fetch all appusage records for a specific date
        private List<AppUsage> GetAppUsagesForDate(DateTime date)
        {
            var list = new List<AppUsage>();
            using var connection = new SqliteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string sql = @"SELECT Id, AppName, UsageDate, TimeUsedSeconds FROM AppUsage WHERE UsageDate = @date;";
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