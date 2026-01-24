using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class InventoryItemsDL
    {
        // Removed static shared MySQLHelper to fix concurrency issues

        // Each method creates its own instance for thread-safety

        #region Item Code Generation

        /// <summary>
        /// Generate unique inventory item code
        /// </summary>
        public static async Task<string> GenerateItemCodeAsync(int itemTypeId)
        {
            using var sqlHelper = new MySQLHelper();
            return await GenerateItemCodeAsync(sqlHelper, itemTypeId);
        }

        /// <summary>
        /// Generate unique inventory item code (internal, transaction-aware)
        /// </summary>
        private static async Task<string> GenerateItemCodeAsync(MySQLHelper sqlHelper, int itemTypeId)
        {
            // Get item type code
            var dtType = await sqlHelper.ExecDataTableAsync(
                "SELECT ItemTypeCode FROM M_Inventory_Item_Types WHERE ItemTypeID = @itemTypeId",
                "@itemTypeId", itemTypeId
            );

            if (dtType.Rows.Count == 0)
            {
                throw new Exception("Item type not found");
            }

            string typeCode = dtType.Rows[0]["ItemTypeCode"]?.ToString() ?? "ITM";

            // Get last number for this type
            var lastNumber = await sqlHelper.ExecScalarAsync(
                @"SELECT MAX(CAST(SUBSTRING(ItemCode, LENGTH(@prefix) + 2) AS UNSIGNED))
                  FROM M_Inventory_Items
                  WHERE ItemCode LIKE CONCAT(@prefix, '-%')",
                "@prefix", typeCode
            );

            int nextNumber = lastNumber != DBNull.Value && lastNumber != null
                ? Convert.ToInt32(lastNumber) + 1
                : 1;

            // Format: DLSR-0001, BTS-0001
            return $"{typeCode}-{nextNumber:D4}";
        }

        #endregion

        #region GET Operations

        /// <summary>
        /// Get all inventory items with filters
        /// </summary>
        public static async Task<DataTable> GetAllItemsAsync(
            int? itemTypeId = null,
            int? usageTypeId = null,
            bool? isRequiredForDialysis = null,
            bool? isIndividualQtyTracking = null,
            bool activeOnly = true)
        {
            using var sqlHelper = new MySQLHelper();
            string query = @"SELECT i.*, 
                                    it.ItemTypeName, it.ItemTypeCode,
                                    ut.UsageTypeName, ut.UsageTypeCode
                             FROM M_Inventory_Items i
                             INNER JOIN M_Inventory_Item_Types it ON i.ItemTypeID = it.ItemTypeID
                             INNER JOIN M_Inventory_Usage_Types ut ON i.UsageTypeID = ut.UsageTypeID
                             WHERE 1=1";

            var parameters = new List<object>();

            if (itemTypeId.HasValue)
            {
                query += " AND i.ItemTypeID = @itemTypeId";
                parameters.Add("@itemTypeId");
                parameters.Add(itemTypeId.Value);
            }

            if (usageTypeId.HasValue)
            {
                query += " AND i.UsageTypeID = @usageTypeId";
                parameters.Add("@usageTypeId");
                parameters.Add(usageTypeId.Value);
            }

            if (isRequiredForDialysis.HasValue)
            {
                query += " AND i.IsRequiredForDialysis = @isRequiredForDialysis";
                parameters.Add("@isRequiredForDialysis");
                parameters.Add(isRequiredForDialysis.Value);
            }

            if (isIndividualQtyTracking.HasValue)
            {
                query += " AND i.IsIndividualQtyTracking = @isIndividualQtyTracking";
                parameters.Add("@isIndividualQtyTracking");
                parameters.Add(isIndividualQtyTracking.Value);
            }

            if (activeOnly)
            {
                query += " AND i.IsActive = 1";
            }

            query += " ORDER BY i.ItemName";

            return await sqlHelper.ExecDataTableAsync(query, parameters.ToArray());
        }

        /// <summary>
        /// Get inventory item by ID
        /// </summary>
        public static async Task<DataTable> GetItemByIdAsync(int inventoryItemId)
        {
            using var sqlHelper = new MySQLHelper();
            return await GetItemByIdAsync(sqlHelper, inventoryItemId);
        }

        /// <summary>
        /// Get inventory item by ID (internal, transaction-aware)
        /// </summary>
        internal static async Task<DataTable> GetItemByIdAsync(MySQLHelper sqlHelper, int inventoryItemId)
        {
            return await sqlHelper.ExecDataTableAsync(
                @"SELECT i.*,
                         it.ItemTypeName, it.ItemTypeCode,
                         ut.UsageTypeName, ut.UsageTypeCode
                  FROM M_Inventory_Items i
                  INNER JOIN M_Inventory_Item_Types it ON i.ItemTypeID = it.ItemTypeID
                  INNER JOIN M_Inventory_Usage_Types ut ON i.UsageTypeID = ut.UsageTypeID
                  WHERE i.InventoryItemID = @inventoryItemId",
                "@inventoryItemId", inventoryItemId
            );
        }

        /// <summary>
        /// Get items required for dialysis session
        /// </summary>
        public static async Task<DataTable> GetDialysisRequiredItemsAsync()
        {
            using var sqlHelper = new MySQLHelper();
            return await sqlHelper.ExecDataTableAsync(
                @"SELECT i.*, 
                         it.ItemTypeName,
                         ut.UsageTypeName
                  FROM M_Inventory_Items i
                  INNER JOIN M_Inventory_Item_Types it ON i.ItemTypeID = it.ItemTypeID
                  INNER JOIN M_Inventory_Usage_Types ut ON i.UsageTypeID = ut.UsageTypeID
                  WHERE i.IsRequiredForDialysis = 1
                  AND i.IsActive = 1
                  ORDER BY i.ItemName"
            );
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Create new inventory item
        /// </summary>
        public static async Task<int> CreateItemAsync(
            string itemName,
            int itemTypeId,
            int usageTypeId,
            string? description,
            string? manufacturer,
            int minimumUsageCount,
            int maximumUsageCount,
            bool isIndividualQtyTracking,
            bool requiresApprovalForEarlyDiscard,
            bool requiresApprovalForOveruse,
            bool isRequiredForDialysis,
            string? unitOfMeasure,
            int? reorderLevel,
            int createdBy)
        {
            using var sqlHelper = new MySQLHelper();
            try
            {
                await sqlHelper.BeginTransactionAsync();

                // Generate item code (use transaction-aware internal overload)
                string itemCode = await GenerateItemCodeAsync(sqlHelper, itemTypeId);

                // Insert item
                var result = await sqlHelper.ExecScalarAsync(
                    @"INSERT INTO M_Inventory_Items 
                      (ItemCode, ItemName, ItemTypeID, UsageTypeID, Description, Manufacturer,
                       MinimumUsageCount, MaximumUsageCount, IsIndividualQtyTracking,
                       RequiresApprovalForEarlyDiscard, RequiresApprovalForOveruse,
                       IsRequiredForDialysis, UnitOfMeasure, ReorderLevel,
                       IsActive, CreatedDate, CreatedBy)
                      VALUES 
                      (@itemCode, @itemName, @itemTypeId, @usageTypeId, @description, @manufacturer,
                       @minimumUsageCount, @maximumUsageCount, @isIndividualQtyTracking,
                       @requiresApprovalForEarlyDiscard, @requiresApprovalForOveruse,
                       @isRequiredForDialysis, @unitOfMeasure, @reorderLevel,
                       1, NOW(), @createdBy);
                      SELECT LAST_INSERT_ID();",
                    "@itemCode", itemCode,
                    "@itemName", itemName,
                    "@itemTypeId", itemTypeId,
                    "@usageTypeId", usageTypeId,
                    "@description", description ?? (object)DBNull.Value,
                    "@manufacturer", manufacturer ?? (object)DBNull.Value,
                    "@minimumUsageCount", minimumUsageCount,
                    "@maximumUsageCount", maximumUsageCount,
                    "@isIndividualQtyTracking", isIndividualQtyTracking,
                    "@requiresApprovalForEarlyDiscard", requiresApprovalForEarlyDiscard,
                    "@requiresApprovalForOveruse", requiresApprovalForOveruse,
                    "@isRequiredForDialysis", isRequiredForDialysis,
                    "@unitOfMeasure", unitOfMeasure ?? (object)DBNull.Value,
                    "@reorderLevel", reorderLevel ?? (object)DBNull.Value,
                    "@createdBy", createdBy
                );

                await sqlHelper.CommitAsync();
                return Convert.ToInt32(result);
            }
            catch
            {
                await sqlHelper.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update inventory item
        /// </summary>
        public static async Task<int> UpdateItemAsync(
            int inventoryItemId,
            string itemName,
            string? description,
            string? manufacturer,
            int minimumUsageCount,
            int maximumUsageCount,
            bool requiresApprovalForEarlyDiscard,
            bool requiresApprovalForOveruse,
            bool isRequiredForDialysis,
            string? unitOfMeasure,
            int? reorderLevel,
            int modifiedBy)
        {
            using var sqlHelper = new MySQLHelper();
            return await sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Inventory_Items 
                  SET ItemName = @itemName,
                      Description = @description,
                      Manufacturer = @manufacturer,
                      MinimumUsageCount = @minimumUsageCount,
                      MaximumUsageCount = @maximumUsageCount,
                      RequiresApprovalForEarlyDiscard = @requiresApprovalForEarlyDiscard,
                      RequiresApprovalForOveruse = @requiresApprovalForOveruse,
                      IsRequiredForDialysis = @isRequiredForDialysis,
                      UnitOfMeasure = @unitOfMeasure,
                      ReorderLevel = @reorderLevel,
                      ModifiedDate = NOW(),
                      ModifiedBy = @modifiedBy
                  WHERE InventoryItemID = @inventoryItemId",
                "@inventoryItemId", inventoryItemId,
                "@itemName", itemName,
                "@description", description ?? (object)DBNull.Value,
                "@manufacturer", manufacturer ?? (object)DBNull.Value,
                "@minimumUsageCount", minimumUsageCount,
                "@maximumUsageCount", maximumUsageCount,
                "@requiresApprovalForEarlyDiscard", requiresApprovalForEarlyDiscard,
                "@requiresApprovalForOveruse", requiresApprovalForOveruse,
                "@isRequiredForDialysis", isRequiredForDialysis,
                "@unitOfMeasure", unitOfMeasure ?? (object)DBNull.Value,
                "@reorderLevel", reorderLevel ?? (object)DBNull.Value,
                "@modifiedBy", modifiedBy
            );
        }

        /// <summary>
        /// Toggle item status
        /// </summary>
        public static async Task<int> ToggleItemStatusAsync(int inventoryItemId, bool isActive, int modifiedBy)
        {
            using var sqlHelper = new MySQLHelper();
            return await sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Inventory_Items 
                  SET IsActive = @isActive,
                      ModifiedDate = NOW(),
                      ModifiedBy = @modifiedBy
                  WHERE InventoryItemID = @inventoryItemId",
                "@inventoryItemId", inventoryItemId,
                "@isActive", isActive,
                "@modifiedBy", modifiedBy
            );
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Delete inventory item
        /// </summary>
        public static async Task<int> DeleteItemAsync(int inventoryItemId)
        {
            using var sqlHelper = new MySQLHelper();
            // Check if any stock exists
            var count = await sqlHelper.ExecScalarAsync(
                "SELECT COUNT(*) FROM T_Inventory_Stock WHERE InventoryItemID = @inventoryItemId",
                "@inventoryItemId", inventoryItemId
            );

            if (Convert.ToInt32(count) > 0)
            {
                throw new InvalidOperationException("Cannot delete item with existing stock");
            }

            return await sqlHelper.ExecNonQueryAsync(
                "DELETE FROM M_Inventory_Items WHERE InventoryItemID = @inventoryItemId",
                "@inventoryItemId", inventoryItemId
            );
        }

        #endregion
    }
}
