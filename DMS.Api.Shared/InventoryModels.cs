using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.Shared
{
    #region Item Type Models

    public class InventoryItemTypeRequest
    {
        public string ItemTypeName { get; set; } = string.Empty;
        public string ItemTypeCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
    }

    public class InventoryItemTypeResponse
    {
        public int ItemTypeID { get; set; }
        public string ItemTypeName { get; set; } = string.Empty;
        public string ItemTypeCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    #endregion

    #region Inventory Item Models

    public class InventoryItemRequest
    {
        public string ItemName { get; set; } = string.Empty;
        public int ItemTypeID { get; set; }
        public int UsageTypeID { get; set; }
        public string? Description { get; set; }
        public string? Manufacturer { get; set; }

        // Usage Configuration
        public int MinimumUsageCount { get; set; } = 1;
        public int MaximumUsageCount { get; set; } = 1;
        public bool IsIndividualQtyTracking { get; set; } = false;

        // Approval Settings
        public bool RequiresApprovalForEarlyDiscard { get; set; } = false;
        public bool RequiresApprovalForOveruse { get; set; } = false;

        // Session Requirements
        public bool IsRequiredForDialysis { get; set; } = false;

        // Stock Settings
        public string? UnitOfMeasure { get; set; }
        public int? ReorderLevel { get; set; }

        public int CreatedBy { get; set; }
    }

    public class InventoryItemResponse
    {
        public int InventoryItemID { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int ItemTypeID { get; set; }
        public string ItemTypeName { get; set; } = string.Empty;
        public int UsageTypeID { get; set; }
        public string UsageTypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Manufacturer { get; set; }

        public int MinimumUsageCount { get; set; }
        public int MaximumUsageCount { get; set; }
        public bool IsIndividualQtyTracking { get; set; }

        public bool RequiresApprovalForEarlyDiscard { get; set; }
        public bool RequiresApprovalForOveruse { get; set; }

        public bool IsRequiredForDialysis { get; set; }

        public string? UnitOfMeasure { get; set; }
        public int? ReorderLevel { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    #endregion

    #region Stock Models

    public class InventoryStockRequest
    {
        public int InventoryItemID { get; set; }
        public int CenterID { get; set; }
        public int CompanyID { get; set; }

        public string? BatchNumber { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchaseCost { get; set; }
        public int Quantity { get; set; }

        public int CreatedBy { get; set; }
    }

    public class InventoryStockResponse
    {
        public int StockID { get; set; }
        public int InventoryItemID { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int CenterID { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public int CompanyID { get; set; }

        public string? BatchNumber { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchaseCost { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        // Calculated
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Today;
        public bool IsNearExpiry => ExpiryDate.HasValue &&
            ExpiryDate.Value >= DateTime.Today &&
            ExpiryDate.Value <= DateTime.Today.AddDays(30);
        public int DaysToExpiry => ExpiryDate.HasValue
            ? (ExpiryDate.Value - DateTime.Today).Days
            : int.MaxValue;
    }

    #endregion

    #region Individual Item Models

    public class IndividualItemResponse
    {
        public int IndividualItemID { get; set; }
        public int StockID { get; set; }
        public int InventoryItemID { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int CenterID { get; set; }

        public string IndividualItemCode { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }

        public int CurrentUsageCount { get; set; }
        public int MaxUsageCount { get; set; }
        public string ItemStatus { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }

        public DateTime? FirstUsedDate { get; set; }
        public DateTime? LastUsedDate { get; set; }
        public DateTime? DiscardedDate { get; set; }
        public string? DiscardReason { get; set; }

        public DateTime CreatedDate { get; set; }

        // Calculated
        public int RemainingUses => MaxUsageCount - CurrentUsageCount;
        public double UsagePercentage => MaxUsageCount > 0
            ? (CurrentUsageCount * 100.0 / MaxUsageCount)
            : 0;
        public bool CanBeUsed => IsAvailable && CurrentUsageCount < MaxUsageCount;
        public bool RequiresApprovalToDiscard => CurrentUsageCount < MaxUsageCount;
    }

    #endregion

    #region Usage Models

    public class InventoryUsageRequest
    {
        public int InventoryItemID { get; set; }
        public int? IndividualItemID { get; set; }
        public int StockID { get; set; }
        public int CenterID { get; set; }
        public int AppointmentID { get; set; }
        public int PatientID { get; set; }

        public decimal QuantityUsed { get; set; } = 1;
        public string? ItemCondition { get; set; }
        public string? Notes { get; set; }

        public int UsedBy { get; set; }
    }

    public class InventoryUsageResponse
    {
        public int UsageID { get; set; }
        public int InventoryItemID { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int? IndividualItemID { get; set; }
        public string? IndividualItemCode { get; set; }
        public int StockID { get; set; }

        public int AppointmentID { get; set; }
        public int PatientID { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientCode { get; set; } = string.Empty;

        public DateTime UsageDate { get; set; }
        public decimal QuantityUsed { get; set; }
        public int UsageNumber { get; set; }
        public string? ItemCondition { get; set; }
        public string? Notes { get; set; }

        public int UsedBy { get; set; }
        public string UsedByName { get; set; } = string.Empty;
    }

    #endregion

    #region Discard Request Models

    public class DiscardRequestRequest
    {
        public int IndividualItemID { get; set; }
        public string RequestType { get; set; } = string.Empty; // EarlyDiscard, Overuse, Damaged, Expired
        public string Reason { get; set; } = string.Empty;
        public int RequestedBy { get; set; }
    }

    public class DiscardRequestResponse
    {
        public int RequestID { get; set; }
        public int IndividualItemID { get; set; }
        public string IndividualItemCode { get; set; } = string.Empty;
        public int InventoryItemID { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int CenterID { get; set; }
        public string CenterName { get; set; } = string.Empty;

        public string RequestType { get; set; } = string.Empty;
        public int CurrentUsageCount { get; set; }
        public int MinimumUsageCount { get; set; }
        public string Reason { get; set; } = string.Empty;

        public string RequestStatus { get; set; } = string.Empty;
        public int RequestedBy { get; set; }
        public string RequestedByName { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; }

        public int? ReviewedBy { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string? ReviewComments { get; set; }
    }

    public class ApproveDiscardRequest
    {
        public int RequestID { get; set; }
        public bool IsApproved { get; set; }
        public string? ReviewComments { get; set; }
        public int ReviewedBy { get; set; }
    }

    #endregion

    #region Filter Models

    public class InventoryFilterRequest
    {
        public int? CenterID { get; set; }
        public int? ItemTypeID { get; set; }
        public int? UsageTypeID { get; set; }
        public bool? IsRequiredForDialysis { get; set; }
        public bool? IsIndividualQtyTracking { get; set; }
        public bool? ActiveOnly { get; set; } = true;
    }

    public class StockFilterRequest
    {
        public int? CenterID { get; set; }
        public int? InventoryItemID { get; set; }
        public bool? ShowExpired { get; set; } = false;
        public bool? ShowNearExpiry { get; set; } = false;
        public bool? ShowLowStock { get; set; } = false;
    }

    public class IndividualItemFilterRequest
    {
        public int? CenterID { get; set; }
        public int? InventoryItemID { get; set; }
        public int? StockID { get; set; }
        public string? ItemStatus { get; set; }
        public bool? AvailableOnly { get; set; } = true;
    }

    #endregion
}
