
using DigitalWellbeing.Core.Data;
using DigitalWellbeing.Core.Models;
using Microsoft.Data.Sqlite;

namespace DigitalWellbeing.Core.Services
{
    public class AppUsageService
    {
        private readonly string dbPath;

        public AppUsageService()
        {
            dbPath = DatabaseInitializer.GetDatabasePath();
        }

        //new app usage record
        public void AddAppUsage(string appName, int timeUsedSeconds)
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
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
        }
        

        //get all app usage records for today
        public List<AppUsage> GetTodayUsage()
        {
            var list = new List<AppUsage>();
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            string today = DateTime.Today.ToString("yyyy-MM-dd");

            string sql = @"SELECT AppName,
                            SUM(TimeUsedSeconds) AS TotalSeconds
                            FROM AppUsage WHERE
                            UsageDate = @date
                            GROUP BY AppName
                            ORDER BY TotalSeconds DESC;";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@date", today);


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

        //
        public List<AppUsage> GetUsageByDate(DateTime date)
{
    var list = new List<AppUsage>();
    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    string dateStr = date.ToString("yyyy-MM-dd");

    string sql = @"SELECT AppName,
                    SUM(TimeUsedSeconds) AS TotalSeconds
                    FROM AppUsage WHERE
                    UsageDate = @date
                    GROUP BY AppName
                    ORDER BY TotalSeconds DESC;";

    using var cmd = new SqliteCommand(sql, connection);
    cmd.Parameters.AddWithValue("@date", dateStr);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        list.Add(new AppUsage
        {
            AppName = reader.GetString(0),
            TimeUsedSeconds = reader.GetInt32(1),
            UsageDate = date
        });
    }
    return list;
}
    }
}
