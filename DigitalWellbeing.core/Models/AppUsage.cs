namespace DigitalWellbeing.Core.Models
{
    public class AppUsage
    {
        public int Id { get; set; }
        public string AppName { get; set; } = string.Empty;
        public DateTime UsageDate { get; set; }
        public int TimeUsedSeconds { get; set; }
    
        public string FormattedTime
        {
            get
            {
                var ts = TimeSpan.FromSeconds(TimeUsedSeconds);

                if (ts.TotalHours >= 1)
                    return $"{(int)ts.TotalHours} hr, {ts.Minutes} min";

                return $"{ts.Minutes} min";
            }
        }
    }
}
