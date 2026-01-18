using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.Shared
{
    // Request Models

    /// <summary>
    /// Add inventory item to session
    /// </summary>
    public class AddSessionInventoryRequest
    {
        public int SessionID { get; set; }                       // Required
        public int InventoryItemID { get; set; }                 // Required
        public int? IndividualItemID { get; set; }               // Required for individually tracked items
        public int StockID { get; set; }                         // Required
        public decimal QuantityUsed { get; set; } = 1;
        public string? ItemCondition { get; set; }               // New, Good, Fair
        public int? UsageNumber { get; set; }
        public string? Notes { get; set; }
        public int SelectedBy { get; set; }                      // Required
    }

    /// <summary>
    /// Add multiple inventory items (bulk)
    /// </summary>
    public class AddBulkSessionInventoryRequest
    {
        public int SessionID { get; set; }                       // Required
        public List<SessionInventoryItemRequest> Items { get; set; } = new();
        public int SelectedBy { get; set; }                      // Required
    }

    public class SessionInventoryItemRequest
    {
        public int InventoryItemID { get; set; }
        public int? IndividualItemID { get; set; }
        public int StockID { get; set; }
        public decimal QuantityUsed { get; set; } = 1;
        public string? ItemCondition { get; set; }
        public int? UsageNumber { get; set; }
        public string? Notes { get; set; }
    }

    // Response Models

    /// <summary>
    /// Session inventory response
    /// </summary>
    public class SessionInventoryResponse
    {
        public int SessionInventoryID { get; set; }
        public int SessionID { get; set; }

        // Item Details
        public int InventoryItemID { get; set; }
        public string ItemCode { get; set; } = "";
        public string ItemName { get; set; } = "";
        public string? UnitOfMeasure { get; set; }

        // Individual Item Details (if applicable)
        public int? IndividualItemID { get; set; }
        public string? IndividualItemCode { get; set; }
        public int? CurrentUsageCount { get; set; }
        public int? MaxUsageCount { get; set; }

        // Stock Details
        public int StockID { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }

        // Usage Details
        public decimal QuantityUsed { get; set; }
        public string? ItemCondition { get; set; }
        public int? UsageNumber { get; set; }
        public string? Notes { get; set; }

        public DateTime SelectedAt { get; set; }
        public int SelectedBy { get; set; }

        // Calculated Properties
        public int? RemainingUses => MaxUsageCount.HasValue && CurrentUsageCount.HasValue
            ? MaxUsageCount.Value - CurrentUsageCount.Value
            : null;

        public double? UsagePercentage => MaxUsageCount.HasValue && CurrentUsageCount.HasValue && MaxUsageCount.Value > 0
            ? (CurrentUsageCount.Value * 100.0 / MaxUsageCount.Value)
            : null;
    }

    /// <summary>
    /// Available items for session selection
    /// </summary>
    public class AvailableSessionInventoryResponse
    {
        public int InventoryItemID { get; set; }
        public string ItemCode { get; set; } = "";
        public string ItemName { get; set; } = "";
        public bool IsRequired { get; set; }                     // IsRequiredForDialysis
        public bool IsIndividualTracking { get; set; }

        // For individual items
        public List<AvailableIndividualItem>? IndividualItems { get; set; }

        // For non-individual items
        public int? AvailableQuantity { get; set; }
        public int? StockID { get; set; }
    }

    public class AvailableIndividualItem
    {
        public int IndividualItemID { get; set; }
        public string IndividualItemCode { get; set; } = "";
        public int CurrentUsageCount { get; set; }
        public int MaxUsageCount { get; set; }
        public int RemainingUses { get; set; }
        public double UsagePercentage { get; set; }
        public string ItemStatus { get; set; } = "";
        public string UsageStatusDisplay { get; set; } = "";     // "Can Use", "New", etc.
        public int Priority { get; set; }                        // 1 = Use first, 2 = New
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int StockID { get; set; }
    }
}
