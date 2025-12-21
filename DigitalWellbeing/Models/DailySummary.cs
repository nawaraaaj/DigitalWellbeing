using System;

namespace DigitalWellbeing.Models
{
    public class DailySummary
    {
        public int Id { get; set; }
        public DateTime UsageDate { get; set; }
        public string AppUsageBreakdown { get; set; }
        public int TotalTimeSeconds { get; set; }
    }
}
