using System;

namespace DigitalWellbeing.Core.Models
{
    public class DailySummary
    {
        public int Id { get; set; }
        public DateTime UsageDate { get; set; }
        public string AppUsageBreakdown { get; set; } = string.Empty;
        public int TotalTimeSeconds { get; set; }
    }
}
