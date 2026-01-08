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
        public void AddAppUsage(string appName, int timeUsedSeconds)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            string today = DateTime.Today.ToString("yyyy-MM-dd");

            string checkSql = @"SELECT Id, TimeUsedSeconds
                                FROM AppUsage
                                WHERE AppName = @app AND UsageDate = @date;";

            using var checkCmd = new SqliteCommand(checkSql, connection);
            checkCmd.Parameters.AddWithValue("@app", appName);
            checkCmd.Parameters.AddWithValue("@date", today);

            using var reader = checkCmd.ExecuteReader();

            if(reader.Read())
            {
                int id = reader.GetInt32(0);
                int existingSeconds = reader.GetInt32(1);
                reader.Close();

                string updateSql = @"UPDATE AppUsage SET 
                                    TimeUsedSeconds = @total
                                    WHERE Id = @id;";

                using var updateCmd = new SqliteCommand( updateSql, connection);
                updateCmd.Parameters.AddWithValue("@total",existingSeconds + timeUsedSeconds);
                updateCmd.Parameters.AddWithValue("@id", id);
                updateCmd.ExecuteNonQuery();
            }
            else
            {
                string insertSql = @"INSERT INTO AppUsage (AppName, UsageDate, TimeUsedSeconds)
                    Values(@app, @date, @time);";

                using var insertCmd = new SqliteCommand(insertSql, connection);
                insertCmd.Parameters.AddWithValue("@app", appName);
                insertCmd.Parameters.AddWithValue("@date", today);
                insertCmd.Parameters.AddWithValue("@time", timeUsedSeconds);
                insertCmd.ExecuteNonQuery();
            }
            _dailySummaryService.GenerateOrUpdateDailySummary();
        }

        //get all app usage records for today
        public List<AppUsage> GetTodayUsage()
        {
            var list = new List<AppUsage>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            string sql = @"SELECT AppName,
                            SUM(TimeUsedSeconds) AS TotalSeconds
                            FROM AppUsage WHERE
                            UsageDate = date('now','localtime')
                            GROUP BY AppName
                            ORDER BY TotalSeconds Desc;";

            using var cmd = new SqliteCommand(sql, connection);
           

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new AppUsage
                {
                    AppName = reader.GetString(0),
                    TimeUsedSeconds = reader.GetInt32(1),
                    UsageDate = DateTime.Today
                });
            }
            return list;
        }
    }
}
