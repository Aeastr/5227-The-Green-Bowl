using System;
using System.ComponentModel.DataAnnotations;

namespace TheGreenBowl.Models
{
    public class tblOpeningTimes
    {
        [Key]
        public int OpeningTimeId { get; set; }
        
        public DayOfWeek DayOfWeek { get; set; }
        
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        
        public bool IsEnabled { get; set; }
    
        public DateTime? EnabledUntil { get; set; }
    }
}