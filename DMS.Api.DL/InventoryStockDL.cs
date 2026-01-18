using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class InventoryStockDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region GET Operations

        /// <summary>
        /// Get all stock with filters
        /// </summary>
        public static async Task<DataTable> GetAllStockAsync(
            int? centerId = null,
            int? inventoryItemId = null,
            bool showExpired = false,
            bool showNearExpiry = false,
            bool showLowStock = false)
        {
            string query = @"SELECT s.*, 
                                    i.ItemCode, i.ItemName, i.UnitOfMeasure, i.ReorderLevel,
                                    c.CenterName,
                                    comp.CompanyName
                             FROM T_Inventory_Stock s
                             INNER JOIN M_Inventory_Items i ON s.InventoryItemID = i.InventoryItemID
                             INNER JOIN M_Centers c ON s.CenterID = c.CenterID
                             INNER JOIN M_Companies comp ON s.CompanyID = comp.CompanyID
                             WHERE s.IsActive = 1";

            var parameters = new List<object>();

            if (centerId.HasValue)
            {
                query += " AND s.CenterID = @centerId";
                parameters.Add("@centerId");
                parameters.Add(centerId.Value);
            }

            if (inventoryItemId.HasValue)
            {
                query += " AND s.InventoryItemID = @inventoryItemId";
                parameters.Add("@inventoryItemId");
                parameters.Add(inventoryItemId.Value);
            }

            if (showExpired)
            {
                query += " AND s.ExpiryDate < CURDATE()";
            }

            if (showNearExpiry)
            {
                query += " AND s.ExpiryDate BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 30 DAY)";
            }

            if (showLowStock)
            {
                query += " AND s.AvailableQuantity <= i.ReorderLevel";
            }

            query += " ORDER BY s.ExpiryDate, i.ItemName";

            return await _sqlHelper.ExecDataTableAsync(query, parameters.ToArray());
        }

        /// <summary>
        /// Get stock by ID
        /// </summary>
        public static async Task<DataTable> GetStockByIdAsync(int stockId)
        {
            return await _sqlHelper.ExecDataTableAsync(
                @"SELECT s.*, 
                         i.ItemCode, i.ItemName, i.UnitOfMeasure, 
                         i.IsIndividualQtyTracking, i.MaximumUsageCount,
                         c.CenterName,
                         comp.CompanyName
                  FROM T_Inventory_Stock s
                  INNER JOIN M_Inventory_Items i ON s.InventoryItemID = i.InventoryItemID
                  INNER JOIN M_Centers c ON s.CenterID = c.CenterID
                  INNER JOIN M_Companies comp ON s.CompanyID = comp.CompanyID
                  WHERE s.StockID = @stockId",
                "@stockId", stockId
            );
        }

        /// <summary>
        /// Get stock summary by center
        /// </summary>
        public static async Task<DataTable> GetStockSummaryAsync(int centerId)
        {
            return await _sqlHelper.ExecDataTableAsync(
                @"SELECT i.ItemCode, i.ItemName, i.UnitOfMeasure,
                         SUM(s.Quantity) as TotalQuantity,
                         SUM(s.AvailableQuantity) as TotalAvailable,
                         COUNT(DISTINCT s.StockID) as StockCount,
                         MIN(s.ExpiryDate) as NearestExpiry,
                         i.ReorderLevel,
                         CASE WHEN SUM(s.AvailableQuantity) <= i.ReorderLevel THEN 1 ELSE 0 END as IsLowStock
                  FROM T_Inventory_Stock s
                  INNER JOIN M_Inventory_Items i ON s.InventoryItemID = i.InventoryItemID
                  WHERE s.CenterID = @centerId
                  AND s.IsActive = 1
                  GROUP BY i.InventoryItemID, i.ItemCode, i.ItemName, i.UnitOfMeasure, i.ReorderLevel
                  ORDER BY i.ItemName",
                "@centerId", centerId
            );
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Add new stock
        /// </summary>
        public static async Task<int> AddStockAsync(
            int inventoryItemId,
            int centerId,
            int companyId,
            string? batchNumber,
            DateTime? manufactureDate,
            DateTime? expiryDate,
            DateTime? purchaseDate,
            decimal? purchaseCost,
            int quantity,
            int createdBy)
        {
            try
            {
                await _sqlHelper.BeginTransactionAsync();

                // Insert stock
                var result = await _sqlHelper.ExecScalarAsync(
                    @"INSERT INTO T_Inventory_Stock 
                      (InventoryItemID, CenterID, CompanyID, BatchNumber, ManufactureDate,
                       ExpiryDate, PurchaseDate, PurchaseCost, Quantity, AvailableQuantity,
                       IsActive, CreatedDate, CreatedBy)
                      VALUES 
                      (@inventoryItemId, @centerId, @companyId, @batchNumber, @manufactureDate,
                       @expiryDate, @purchaseDate, @purchaseCost, @quantity, @quantity,
                       1, NOW(), @createdBy);
                      SELECT LAST_INSERT_ID();",
                    "@inventoryItemId", inventoryItemId,
                    "@centerId", centerId,
                    "@companyId", companyId,
                    "@batchNumber", batchNumber ?? (object)DBNull.Value,
                    "@manufactureDate", manufactureDate ?? (object)DBNull.Value,
                    "@expiryDate", expiryDate ?? (object)DBNull.Value,
                    "@purchaseDate", purchaseDate ?? (object)DBNull.Value,
                    "@purchaseCost", purchaseCost ?? (object)DBNull.Value,
                    "@quantity", quantity,
                    "@createdBy", createdBy
                );

                int stockId = Convert.ToInt32(result);

                // Check if item requires individual tracking
                var dtItem = await _sqlHelper.ExecDataTableAsync(
                    "SELECT IsIndividualQtyTracking, MaximumUsageCount FROM M_Inventory_Items WHERE InventoryItemID = @inventoryItemId",
                    "@inventoryItemId", inventoryItemId
                );

                if (dtItem.Rows.Count > 0 && Convert.ToBoolean(dtItem.Rows[0]["IsIndividualQtyTracking"]))
                {
                    int maxUsageCount = Convert.ToInt32(dtItem.Rows[0]["MaximumUsageCount"]);

                    // Create individual items for each quantity
                    for (int i = 1; i <= quantity; i++)
                    {
                        string individualCode = await GenerateIndividualItemCodeAsync(inventoryItemId, centerId);

                        await _sqlHelper.ExecNonQueryAsync(
                            @"INSERT INTO T_Inventory_Individual_Items 
                              (StockID, InventoryItemID, CenterID, IndividualItemCode, 
                               MaxUsageCount, CurrentUsageCount, ItemStatus, IsAvailable,
                               CreatedDate, CreatedBy)
                              VALUES 
                              (@stockId, @inventoryItemId, @centerId, @individualCode,
                               @maxUsageCount, 0, 'Available', 1,
                               NOW(), @createdBy)",
                            "@stockId", stockId,
                            "@inventoryItemId", inventoryItemId,
                            "@centerId", centerId,
                            "@individualCode", individualCode,
                            "@maxUsageCount", maxUsageCount,
                            "@createdBy", createdBy
                        );
                    }
                }

                await _sqlHelper.CommitAsync();
                return stockId;
            }
            catch
            {
                await _sqlHelper.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Generate individual item code
        /// </summary>
        private static async Task<string> GenerateIndividualItemCodeAsync(int inventoryItemId, int centerId)
        {
            // Get item code
            var dtItem = await _sqlHelper.ExecDataTableAsync(
                "SELECT ItemCode FROM M_Inventory_Items WHERE InventoryItemID = @inventoryItemId",
                "@inventoryItemId", inventoryItemId
            );

            string itemCode = dtItem.Rows.Count > 0
                ? dtItem.Rows[0]["ItemCode"]?.ToString() ?? "ITM"
                : "ITM";

            // Get last number
            var lastNumber = await _sqlHelper.ExecScalarAsync(
                @"SELECT MAX(CAST(SUBSTRING(IndividualItemCode, LENGTH(@prefix) + 2) AS UNSIGNED)) 
                  FROM T_Inventory_Individual_Items 
                  WHERE InventoryItemID = @inventoryItemId 
                  AND CenterID = @centerId
                  AND IndividualItemCode LIKE CONCAT(@prefix, '-%')",
                "@inventoryItemId", inventoryItemId,
                "@centerId", centerId,
                "@prefix", itemCode
            );

            int nextNumber = lastNumber != DBNull.Value && lastNumber != null
                ? Convert.ToInt32(lastNumber) + 1
                : 1;

            // Format: DLSR-0001-001, DLSR-0001-002
            return $"{itemCode}-{nextNumber:D3}";
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update stock quantity
        /// </summary>
        public static async Task<int> UpdateStockQuantityAsync(int stockId, int quantityChange)
        {
            return await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE T_Inventory_Stock 
                  SET Quantity = Quantity + @quantityChange,
                      AvailableQuantity = AvailableQuantity + @quantityChange
                  WHERE StockID = @stockId",
                "@stockId", stockId,
                "@quantityChange", quantityChange
            );
        }

        /// <summary>
        /// Deduct available quantity
        /// </summary>
        public static async Task<int> DeductAvailableQuantityAsync(int stockId, int quantity)
        {
            return await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE T_Inventory_Stock 
                  SET AvailableQuantity = AvailableQuantity - @quantity
                  WHERE StockID = @stockId
                  AND AvailableQuantity >= @quantity",
                "@stockId", stockId,
                "@quantity", quantity
            );
        }

        #endregion
    }
}
