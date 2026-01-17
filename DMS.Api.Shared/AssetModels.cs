using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.Shared
{
    #region Asset Type Models

    public class AssetTypeRequest
    {
        public string AssetTypeName { get; set; } = string.Empty;
        public string AssetTypeCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool RequiresMaintenance { get; set; }
        public int MaintenanceIntervalDays { get; set; }
        public int CreatedBy { get; set; }
    }

    public class AssetTypeResponse
    {
        public int AssetTypeID { get; set; }
        public string AssetTypeName { get; set; } = string.Empty;
        public string AssetTypeCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool RequiresMaintenance { get; set; }
        public int MaintenanceIntervalDays { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    #endregion

    #region Asset Models

    public class AssetRequest
    {
        public string AssetName { get; set; } = string.Empty;
        public int AssetType { get; set; }
        public string? SerialNo { get; set; }
        public string? ModelNo { get; set; }
        public string? Manufacturer { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchaseCost { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public int CenterID { get; set; }
        public int CompanyID { get; set; }
        public int CreatedBy { get; set; }
    }

    public class AssetResponse
    {
        public int AssetID { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public int AssetType { get; set; }
        public string AssetTypeName { get; set; } = string.Empty;
        public string? SerialNo { get; set; }
        public string? ModelNo { get; set; }
        public string? Manufacturer { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchaseCost { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public int CenterID { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public int CompanyID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? InactiveReason { get; set; }
        public DateTime? InactiveDate { get; set; }
        public DateTime? ExpectedActiveDate { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public DateTime CreatedDate { get; set; }

        // Calculated fields
        public bool IsWarrantyValid => WarrantyExpiryDate.HasValue && WarrantyExpiryDate.Value >= DateTime.Today;
        public int? DaysUntilMaintenance => NextMaintenanceDate.HasValue
            ? (NextMaintenanceDate.Value - DateTime.Today).Days
            : null;
        public bool MaintenanceDue => DaysUntilMaintenance.HasValue && DaysUntilMaintenance.Value <= 7;
    }

    public class UpdateAssetStatusRequest
    {
        public int AssetID { get; set; }
        public bool IsActive { get; set; }
        public string? Reason { get; set; }
        public DateTime? ExpectedActiveDate { get; set; }
        public int ModifiedBy { get; set; }
    }

    #endregion

    #region Maintenance Models

    public class MaintenanceRequest
    {
        public int AssetID { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public string MaintenanceType { get; set; } = string.Empty; // Preventive, Corrective, Calibration
        public string? Description { get; set; }
        public string? TechnicianName { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public int CreatedBy { get; set; }
    }

    public class MaintenanceResponse
    {
        public int MaintenanceID { get; set; }
        public int AssetID { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public DateTime MaintenanceDate { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? TechnicianName { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    #endregion

    #region Assignment Models

    public class AssetAssignmentRequest
    {
        public int AssetID { get; set; }
        public int AppointmentID { get; set; }
        public DateTime AssignedDate { get; set; }
        public TimeSpan AssignedTime { get; set; }
        public int SessionDuration { get; set; }
        public string? Notes { get; set; }
        public int CreatedBy { get; set; }
    }

    public class AssetAssignmentResponse
    {
        public int AssignmentID { get; set; }
        public int AssetID { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public int AppointmentID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public TimeSpan AssignedTime { get; set; }
        public int SessionDuration { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class AssetAvailabilityRequest
    {
        public int CenterID { get; set; }
        public int AssetType { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    #endregion
}
