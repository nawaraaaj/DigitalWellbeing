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
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            using (var pragmaCmd = new SqliteCommand("PRAGMA journal_mode=WAL;", connection))
            {
                pragmaCmd.ExecuteNonQuery();
            }

            string insertSql = @"INSERT INTO AppUsage (AppName,UsageDate, TimeUsedSeconds) Values (@appName, date('now','localtime'), @timeUsed);";
            using var cmd = new SqliteCommand(insertSql, connection);
            cmd.Parameters.AddWithValue("@appName", appName);
            cmd.Parameters.AddWithValue("@timeUsed", timeUsedSeconds);
            
            cmd.ExecuteNonQuery();

            _dailySummaryService.GenerateOrUpdateDailySummary();
        }

        //get all app usage records for today
        public List<AppUsage> GetTodayUsage()
        {
            var list = new List<AppUsage>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            string sql = @"SELECT id, AppName, UsageDate, TimeUsedSeconds
               FROM AppUsage
               WHERE date(UsageDate, 'localtime') = date('now','localtime');";

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
