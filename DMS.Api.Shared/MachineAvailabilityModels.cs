using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.Shared
{
    /// <summary>
    /// Machine availability for session
    /// </summary>
    public class MachineAvailabilityResponse
    {
        public int AssetID { get; set; }
        public string AssetCode { get; set; } = "";
        public string AssetName { get; set; } = "";
        public string AssetType { get; set; } = "";
        public bool IsAvailable { get; set; }
        public string Status { get; set; } = "";                 // Available, In Use

        // If in use
        public int? CurrentSessionID { get; set; }
        public string? CurrentPatientName { get; set; }
        public string? CurrentPatientCode { get; set; }
        public DateTime? SessionStartTime { get; set; }
        public int? SessionElapsedMinutes { get; set; }

        // Display Properties
        public string StatusDisplay => IsAvailable
            ? "✅ Available"
            : $"🔴 In Use - {CurrentPatientName}";

        public string StatusBadgeColor => IsAvailable ? "success" : "danger";
    }
}
