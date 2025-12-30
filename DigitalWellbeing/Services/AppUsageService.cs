using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using DigitalWellbeing.Models;
using DigitalWellbeing.Data;
using System.Xml;

namespace DigitalWellbeing.Services
{
    public class AppUsageService
    {
        private readonly string _dbPath;
        private readonly DailySummaryService _dailySummaryService;

        public AppUsageService()
        {
            _dbPath = DatabaseInitializer.GetDatabasePath();
            _dailySummaryService = new DailySummaryService();
        }

        //new app usage record
        public void AddAppUsage ( string appName, int timeUsedSeconds)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string insertSql = @"INSERT INTO AppUsage (AppName,UsageDate, TimeUsedSeconds) Values (@appName, @date, @timeUsed);";
            using var cmd = new SqliteCommand(insertSql, connection);
            cmd.Parameters.AddWithValue("@appName", appName);
            cmd.Parameters.AddWithValue("@date", DateTime.Today.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@timeUsed", timeUsedSeconds);

            cmd.ExecuteNonQuery();

            _dailySummaryService.GenerateOrUpdateDailySummary(DateTime.Today);
        }

        //get all app usage records for today
        public List<AppUsage> GetTodayUsage()
        {
            var list = new List<AppUsage>();
            using var connection = new SqliteConnection($"Data Source={_dbPath};Version=3;");
            connection.Open();

            string sql = @"SELECT id, AppName,UsageDate, TimeUsedSeconds FROM AppUsage WHERE UsageDate = @today;";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@today", DateTime.Today.ToString("yyyy-MM-dd"));

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
