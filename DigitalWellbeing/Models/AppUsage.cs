using System;

namespace DigitalWellbeing.Models
{
    public class AppUsage
    {
        public int Id { get; set; }
        public string AppName { get; set; } = string.Empty;
        public DateTime UsageDate { get; set; }
        public int TimeUsedSeconds { get; set; }
    }
}
