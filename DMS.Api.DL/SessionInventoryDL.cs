using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class SessionInventoryDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region GET Operations

        /// <summary>
        /// Get all inventory items for a session
        /// </summary>
        public static async Task<DataTable> GetSessionInventoryAsync(int sessionId)
        {
            return await _sqlHelper.ExecDataTableAsync(
                @"SELECT si.*, 
                         ii.ItemCode, ii.ItemName, ii.UnitOfMeasure,
                         iti.IndividualItemCode, iti.CurrentUsageCount, iti.MaxUsageCount,
                         s.BatchNumber, s.ExpiryDate
                  FROM T_Session_Inventory si
                  INNER JOIN M_Inventory_Items ii ON si.InventoryItemID = ii.InventoryItemID
                  LEFT JOIN T_Inventory_Individual_Items iti ON si.IndividualItemID = iti.IndividualItemID
                  LEFT JOIN T_Inventory_Stock s ON si.StockID = s.StockID
                  WHERE si.SessionID = @sessionId
                  ORDER BY si.SelectedAt",
                "@sessionId", sessionId
            );
        }

        /// <summary>
        /// Check if item already selected in session
        /// </summary>
        public static async Task<bool> IsItemAlreadySelectedAsync(int sessionId, int inventoryItemId)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                @"SELECT COUNT(*) FROM T_Session_Inventory 
                  WHERE SessionID = @sessionId AND InventoryItemID = @inventoryItemId",
                "@sessionId", sessionId,
                "@inventoryItemId", inventoryItemId
            );

            return Convert.ToInt32(result) > 0;
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Add inventory item to session
        /// </summary>
        /// <summary>
        /// Add inventory item to session
        /// </summary>
        public static async Task<int> AddInventoryToSessionAsync(
            int sessionId,
            int inventoryItemId,
            int? individualItemId,
            int stockId,
            decimal quantityUsed,
            string? itemCondition,
            int? usageNumber,
            string? notes,
            int selectedBy)
        {
            try
            {
                await _sqlHelper.BeginTransactionAsync();

                // Insert into session inventory
                var result = await _sqlHelper.ExecScalarAsync(
                    @"INSERT INTO T_Session_Inventory 
              (SessionID, InventoryItemID, IndividualItemID, StockID, 
               QuantityUsed, ItemCondition, UsageNumber, Notes, SelectedAt, SelectedBy)
              VALUES 
              (@sessionId, @inventoryItemId, @individualItemId, @stockId,
               @quantityUsed, @itemCondition, @usageNumber, @notes, NOW(), @selectedBy);
              SELECT LAST_INSERT_ID();",
                    "@sessionId", sessionId,
                    "@inventoryItemId", inventoryItemId,
                    "@individualItemId", individualItemId ?? (object)DBNull.Value,
                    "@stockId", stockId,
                    "@quantityUsed", quantityUsed,
                    "@itemCondition", itemCondition ?? (object)DBNull.Value,
                    "@usageNumber", usageNumber ?? (object)DBNull.Value,
                    "@notes", notes ?? (object)DBNull.Value,
                    "@selectedBy", selectedBy
                );

                int sessionInventoryId = Convert.ToInt32(result);

                // Get item details for timeline
                var dtItem = await InventoryItemsDL.GetItemByIdAsync(inventoryItemId);
                string itemName = dtItem.Rows.Count > 0 ? dtItem.Rows[0]["ItemName"]?.ToString() ?? "" : "";

                // Log timeline event - NOW PUBLIC
                await DialysisSessionsDL.InsertTimelineEventAsync(
                    sessionId,
                    "InventoryAdded",
                    $"Item added: {itemName} (Qty: {quantityUsed})",
                    selectedBy
                );

                await _sqlHelper.CommitAsync();
                return sessionInventoryId;
            }
            catch
            {
                await _sqlHelper.RollbackAsync();
                throw;
            }
        }


        #endregion

        #region DELETE Operations

        /// <summary>
        /// Remove inventory item from session (before session starts)
        /// </summary>
        public static async Task<int> RemoveInventoryFromSessionAsync(int sessionInventoryId)
        {
            return await _sqlHelper.ExecNonQueryAsync(
                "DELETE FROM T_Session_Inventory WHERE SessionInventoryID = @sessionInventoryId",
                "@sessionInventoryId", sessionInventoryId
            );
        }

        #endregion
    }
}
