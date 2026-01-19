using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.Shared
{
    // Request Models

    /// <summary>
    /// Create session request (Initial session creation)
    /// </summary>
    public class CreateSessionRequest
    {
        public int AppointmentID { get; set; }                   // Required
        public int PatientID { get; set; }                       // Required
        public int CenterID { get; set; }                        // Required
        public DateTime SessionDate { get; set; }                // Required
        public TimeSpan? ScheduledStartTime { get; set; }
        public string? DialysisType { get; set; }                // Hemodialysis, Hemodiafiltration, etc.
        public string? PreSessionNotes { get; set; }
        public int CreatedBy { get; set; }                       // Required
    }

    /// <summary>
    /// Assign machine request
    /// </summary>
    public class AssignMachineRequest
    {
        public int SessionID { get; set; }                       // Required
        public int AssetID { get; set; }                         // Required (Machine ID)
        public int ModifiedBy { get; set; }                      // Required
    }

    /// <summary>
    /// Start dialysis request
    /// </summary>
    public class StartDialysisRequest
    {
        public int SessionID { get; set; }                       // Required
        public int StartedBy { get; set; }                       // Required
    }

    /// <summary>
    /// Complete session request
    /// </summary>
    public class CompleteSessionRequest
    {
        public int SessionID { get; set; }                       // Required
        public string? PostSessionNotes { get; set; }
        public int CompletedBy { get; set; }                     // Required
    }

    /// <summary>
    /// Terminate session request
    /// </summary>
    public class TerminateSessionRequest
    {
        public int SessionID { get; set; }                       // Required
        public string TerminationReason { get; set; } = "";      // Required
        public int CompletedBy { get; set; }                     // Required
    }

    // Response Models

    /// <summary>
    /// Dialysis session response
    /// </summary>
    public class DialysisSessionResponse
    {
        public int SessionID { get; set; }
        public string SessionCode { get; set; } = "";

        // Appointment Details
        public int AppointmentID { get; set; }
        public DateTime AppointmentDate { get; set; }

        // Patient Details
        public int PatientID { get; set; }
        public string PatientCode { get; set; } = "";
        public string PatientName { get; set; } = "";
        public string? ContactNumber { get; set; }

        // Center Details
        public int CenterID { get; set; }
        public string CenterName { get; set; } = "";

        // Machine Details
        public int? AssetID { get; set; }
        public string? AssetCode { get; set; }
        public string? AssetName { get; set; }
        public int? AssetAssignmentID { get; set; }

        // Session Status
        public string SessionStatus { get; set; } = "Not Started";

        // Session Timing
        public DateTime SessionDate { get; set; }
        public TimeSpan? ScheduledStartTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public int? SessionDuration { get; set; }                // Minutes

        // Treatment Details
        public string? DialysisType { get; set; }

        // User Tracking
        public int? StartedBy { get; set; }
        public int? CompletedBy { get; set; }

        // Session Notes
        public string? PreSessionNotes { get; set; }
        public string? PostSessionNotes { get; set; }
        public string? TerminationReason { get; set; }

        public DateTime CreatedDate { get; set; }

        // Calculated Properties
        public int ElapsedMinutes => ActualStartTime.HasValue
            ? (int)(DateTime.Now - ActualStartTime.Value).TotalMinutes
            : 0;

        public string StatusDisplay => SessionStatus switch
        {
            "Not Started" => "⏸️ Not Started",
            "In Progress" => "▶️ In Progress",
            "On Hold" => "⏸️ On Hold",
            "Completed" => "✅ Completed",
            "Terminated" => "⛔ Terminated",
            _ => SessionStatus
        };
    }

    /// <summary>
    /// Session summary (for listing)
    /// </summary>
    public class SessionSummaryResponse
    {
        public int SessionID { get; set; }
        public string SessionCode { get; set; } = "";
        public string PatientName { get; set; } = "";
        public string PatientCode { get; set; } = "";
        public DateTime SessionDate { get; set; }
        public string SessionStatus { get; set; } = "";
        public string? MachineName { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public int? SessionDuration { get; set; }
    }
}
