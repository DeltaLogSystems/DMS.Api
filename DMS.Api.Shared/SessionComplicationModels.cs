using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.Shared
{
    // Request Models

    /// <summary>
    /// Report complication
    /// </summary>
    public class ReportComplicationRequest
    {
        public int SessionID { get; set; }                       // Required
        public string ComplicationType { get; set; } = "";       // Required (Hypotension, Cramps, Nausea, Bleeding, etc.)
        public string? Severity { get; set; }                    // Mild, Moderate, Severe
        public string? Description { get; set; }
        public string? ActionTaken { get; set; }
        public int ReportedBy { get; set; }                      // Required
    }

    /// <summary>
    /// Resolve complication
    /// </summary>
    public class ResolveComplicationRequest
    {
        public int ComplicationID { get; set; }                  // Required
        public string? ResolutionNotes { get; set; }
    }

    // Response Models

    /// <summary>
    /// Session complication response
    /// </summary>
    public class SessionComplicationResponse
    {
        public int ComplicationID { get; set; }
        public int SessionID { get; set; }

        public string ComplicationType { get; set; } = "";
        public string? Severity { get; set; }
        public DateTime OccurredAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public string? Description { get; set; }
        public string? ActionTaken { get; set; }

        public int ReportedBy { get; set; }

        // Calculated Properties
        public bool IsResolved => ResolvedAt.HasValue;

        public int DurationMinutes => ResolvedAt.HasValue
            ? (int)(ResolvedAt.Value - OccurredAt).TotalMinutes
            : (int)(DateTime.Now - OccurredAt).TotalMinutes;

        public string SeverityIndicator => Severity switch
        {
            "Mild" => "🟡",
            "Moderate" => "🟠",
            "Severe" => "🔴",
            _ => "⚪"
        };
    }
}
