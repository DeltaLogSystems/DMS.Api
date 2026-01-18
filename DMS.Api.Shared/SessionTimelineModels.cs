using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.Shared
{
    /// <summary>
    /// Session timeline event response
    /// </summary>
    public class SessionTimelineResponse
    {
        public int TimelineID { get; set; }
        public int SessionID { get; set; }

        public string EventType { get; set; } = "";
        public string EventDescription { get; set; } = "";
        public DateTime EventTime { get; set; }

        public int PerformedBy { get; set; }

        // Calculated Properties
        public string EventIcon => EventType switch
        {
            "SessionCreated" => "📝",
            "MachineAssigned" => "🏥",
            "InventoryAdded" => "📦",
            "SessionStarted" => "▶️",
            "NoteAdded" => "📊",
            "ComplicationReported" => "⚠️",
            "SessionCompleted" => "✅",
            "SessionTerminated" => "⛔",
            "StatusChanged" => "🔄",
            _ => "•"
        };

        public string TimeDisplay => EventTime.ToString("hh:mm tt");
    }
}
