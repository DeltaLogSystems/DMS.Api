using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class IndividualItemsDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region GET Operations

        /// <summary>
        /// Get all individual items with filters
        /// </summary>
        public static async Task<DataTable> GetAllIndividualItemsAsync(
            int? centerId = null,
            int? inventoryItemId = null,
            int? stockId = null,
            string? itemStatus = null,
            bool availableOnly = true)
        {
            string query = @"SELECT ii.*, 
                                    i.ItemCode, i.ItemName, i.MinimumUsageCount,
                                    s.BatchNumber, s.ExpiryDate
                             FROM T_Inventory_Individual_Items ii
                             INNER JOIN M_Inventory_Items i ON ii.InventoryItemID = i.InventoryItemID
                             INNER JOIN T_Inventory_Stock s ON ii.StockID = s.StockID
                             WHERE 1=1";

            var parameters = new List<object>();

            if (centerId.HasValue)
            {
                query += " AND ii.CenterID = @centerId";
                parameters.Add("@centerId");
                parameters.Add(centerId.Value);
            }

            if (inventoryItemId.HasValue)
            {
                query += " AND ii.InventoryItemID = @inventoryItemId";
                parameters.Add("@inventoryItemId");
                parameters.Add(inventoryItemId.Value);
            }

            if (stockId.HasValue)
            {
                query += " AND ii.StockID = @stockId";
                parameters.Add("@stockId");
                parameters.Add(stockId.Value);
            }

            if (!string.IsNullOrEmpty(itemStatus))
            {
                query += " AND ii.ItemStatus = @itemStatus";
                parameters.Add("@itemStatus");
                parameters.Add(itemStatus);
            }

            if (availableOnly)
            {
                query += " AND ii.IsAvailable = 1";
            }

            query += " ORDER BY ii.CurrentUsageCount ASC, ii.IndividualItemCode";

            return await _sqlHelper.ExecDataTableAsync(query, parameters.ToArray());
        }

        /// <summary>
        /// Get available items for selection in dialysis session
        /// </summary>
        public static async Task<DataTable> GetAvailableItemsForSessionAsync(int centerId, int inventoryItemId)
        {
            return await _sqlHelper.ExecDataTableAsync(
                @"SELECT ii.*, 
                         i.ItemCode, i.ItemName, i.MinimumUsageCount, i.MaximumUsageCount,
                         s.BatchNumber, s.ExpiryDate,
                         (ii.MaxUsageCount - ii.CurrentUsageCount) as RemainingUses,
                         CASE 
                             WHEN ii.CurrentUsageCount = 0 THEN 'New'
                             WHEN ii.CurrentUsageCount < i.MinimumUsageCount THEN 'Below Minimum'
                             WHEN ii.CurrentUsageCount >= i.MinimumUsageCount 
                                  AND ii.CurrentUsageCount < ii.MaxUsageCount THEN 'Can Use'
                             ELSE 'Exhausted'
                         END as UsageStatus
                  FROM T_Inventory_Individual_Items ii
                  INNER JOIN M_Inventory_Items i ON ii.InventoryItemID = i.InventoryItemID
                  INNER JOIN T_Inventory_Stock s ON ii.StockID = s.StockID
                  WHERE ii.CenterID = @centerId
                  AND ii.InventoryItemID = @inventoryItemId
                  AND ii.IsAvailable = 1
                  AND ii.ItemStatus IN ('Available', 'InUse')
                  AND s.IsActive = 1
                  AND (s.ExpiryDate IS NULL OR s.ExpiryDate >= CURDATE())
                  ORDER BY 
                      CASE 
                          WHEN ii.CurrentUsageCount > 0 AND ii.CurrentUsageCount < ii.MaxUsageCount THEN 1
                          WHEN ii.CurrentUsageCount = 0 THEN 2
                          ELSE 3
                      END,
                      ii.CurrentUsageCount DESC,
                      s.ExpiryDate ASC",
                "@centerId", centerId,
                "@inventoryItemId", inventoryItemId
            );
        }

        /// <summary>
        /// Get individual item by ID
        /// </summary>
        public static async Task<DataTable> GetIndividualItemByIdAsync(int individualItemId)
        {
            return await _sqlHelper.ExecDataTableAsync(
                @"SELECT ii.*, 
                         i.ItemCode, i.ItemName, i.MinimumUsageCount, i.MaximumUsageCount,
                         i.RequiresApprovalForEarlyDiscard,
                         s.BatchNumber, s.ExpiryDate
                  FROM T_Inventory_Individual_Items ii
                  INNER JOIN M_Inventory_Items i ON ii.InventoryItemID = i.InventoryItemID
                  INNER JOIN T_Inventory_Stock s ON ii.StockID = s.StockID
                  WHERE ii.IndividualItemID = @individualItemId",
                "@individualItemId", individualItemId
            );
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Increment usage count
        /// </summary>
        public static async Task<int> IncrementUsageCountAsync(int individualItemId)
        {
            return await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE T_Inventory_Individual_Items 
                  SET CurrentUsageCount = CurrentUsageCount + 1,
                      LastUsedDate = NOW(),
                      FirstUsedDate = COALESCE(FirstUsedDate, NOW()),
                      ItemStatus = CASE 
                          WHEN CurrentUsageCount + 1 >= MaxUsageCount THEN 'Exhausted'
                          ELSE 'InUse'
                      END,
                      IsAvailable = CASE 
                          WHEN CurrentUsageCount + 1 >= MaxUsageCount THEN 0
                          ELSE 1
                      END
                  WHERE IndividualItemID = @individualItemId",
                "@individualItemId", individualItemId
            );
        }

        /// <summary>
        /// Mark item as discarded
        /// </summary>
        public static async Task<int> DiscardItemAsync(int individualItemId, string discardReason)
        {
            return await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE T_Inventory_Individual_Items 
                  SET ItemStatus = 'Discarded',
                      IsAvailable = 0,
                      DiscardedDate = NOW(),
                      DiscardReason = @discardReason
                  WHERE IndividualItemID = @individualItemId",
                "@individualItemId", individualItemId,
                "@discardReason", discardReason
            );
        }

        /// <summary>
        /// Update item status
        /// </summary>
        public static async Task<int> UpdateItemStatusAsync(int individualItemId, string itemStatus)
        {
            return await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE T_Inventory_Individual_Items 
                  SET ItemStatus = @itemStatus,
                      IsAvailable = CASE 
                          WHEN @itemStatus IN ('Discarded', 'Exhausted', 'DiscardRequested') THEN 0
                          ELSE 1
                      END
                  WHERE IndividualItemID = @individualItemId",
                "@individualItemId", individualItemId,
                "@itemStatus", itemStatus
            );
        }

        #endregion
    }
}
