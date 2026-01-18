using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class DiscardRequestsDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region GET Operations

        /// <summary>
        /// Get all discard requests
        /// </summary>
        public static async Task<DataTable> GetAllRequestsAsync(
            int? centerId = null,
            string? requestStatus = null)
        {
            string query = @"SELECT dr.*, 
                                    ii.IndividualItemCode, ii.CurrentUsageCount,
                                    i.ItemCode, i.ItemName, i.MinimumUsageCount,
                                    c.CenterName
                             FROM T_Inventory_Discard_Requests dr
                             INNER JOIN T_Inventory_Individual_Items ii ON dr.IndividualItemID = ii.IndividualItemID
                             INNER JOIN M_Inventory_Items i ON dr.InventoryItemID = i.InventoryItemID
                             INNER JOIN M_Centers c ON dr.CenterID = c.CenterID
                             WHERE 1=1";

            var parameters = new List<object>();

            if (centerId.HasValue)
            {
                query += " AND dr.CenterID = @centerId";
                parameters.Add("@centerId");
                parameters.Add(centerId.Value);
            }

            if (!string.IsNullOrEmpty(requestStatus))
            {
                query += " AND dr.RequestStatus = @requestStatus";
                parameters.Add("@requestStatus");
                parameters.Add(requestStatus);
            }

            query += " ORDER BY dr.RequestedDate DESC";

            return await _sqlHelper.ExecDataTableAsync(query, parameters.ToArray());
        }

        /// <summary>
        /// Get pending requests
        /// </summary>
        public static async Task<DataTable> GetPendingRequestsAsync(int? centerId = null)
        {
            return await GetAllRequestsAsync(centerId, "Pending");
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Create discard request
        /// </summary>
        public static async Task<int> CreateDiscardRequestAsync(
            int individualItemId,
            string requestType,
            string reason,
            int requestedBy)
        {
            try
            {
                await _sqlHelper.BeginTransactionAsync();

                // Get item details
                var dtItem = await IndividualItemsDL.GetIndividualItemByIdAsync(individualItemId);
                if (dtItem.Rows.Count == 0)
                {
                    throw new Exception("Individual item not found");
                }

                var row = dtItem.Rows[0];
                int inventoryItemId = Convert.ToInt32(row["InventoryItemID"]);
                int centerId = Convert.ToInt32(row["CenterID"]);
                int currentUsageCount = Convert.ToInt32(row["CurrentUsageCount"]);
                int minimumUsageCount = Convert.ToInt32(row["MinimumUsageCount"]);

                // Insert request
                var result = await _sqlHelper.ExecScalarAsync(
                    @"INSERT INTO T_Inventory_Discard_Requests 
                      (IndividualItemID, InventoryItemID, CenterID, RequestType,
                       CurrentUsageCount, MinimumUsageCount, Reason, RequestStatus,
                       RequestedBy, RequestedDate)
                      VALUES 
                      (@individualItemId, @inventoryItemId, @centerId, @requestType,
                       @currentUsageCount, @minimumUsageCount, @reason, 'Pending',
                       @requestedBy, NOW());
                      SELECT LAST_INSERT_ID();",
                    "@individualItemId", individualItemId,
                    "@inventoryItemId", inventoryItemId,
                    "@centerId", centerId,
                    "@requestType", requestType,
                    "@currentUsageCount", currentUsageCount,
                    "@minimumUsageCount", minimumUsageCount,
                    "@reason", reason,
                    "@requestedBy", requestedBy
                );

                // Update item status to DiscardRequested
                await IndividualItemsDL.UpdateItemStatusAsync(individualItemId, "DiscardRequested");

                await _sqlHelper.CommitAsync();
                return Convert.ToInt32(result);
            }
            catch
            {
                await _sqlHelper.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Approve or reject discard request
        /// </summary>
        public static async Task<int> ProcessDiscardRequestAsync(
            int requestId,
            bool isApproved,
            string? reviewComments,
            int reviewedBy)
        {
            try
            {
                await _sqlHelper.BeginTransactionAsync();

                // Get request details
                var dtRequest = await _sqlHelper.ExecDataTableAsync(
                    "SELECT IndividualItemID FROM T_Inventory_Discard_Requests WHERE RequestID = @requestId",
                    "@requestId", requestId
                );

                if (dtRequest.Rows.Count == 0)
                {
                    throw new Exception("Request not found");
                }

                int individualItemId = Convert.ToInt32(dtRequest.Rows[0]["IndividualItemID"]);

                // Update request status
                await _sqlHelper.ExecNonQueryAsync(
                    @"UPDATE T_Inventory_Discard_Requests 
                      SET RequestStatus = @status,
                          ReviewedBy = @reviewedBy,
                          ReviewedDate = NOW(),
                          ReviewComments = @reviewComments
                      WHERE RequestID = @requestId",
                    "@requestId", requestId,
                    "@status", isApproved ? "Approved" : "Rejected",
                    "@reviewedBy", reviewedBy,
                    "@reviewComments", reviewComments ?? (object)DBNull.Value
                );

                // Update individual item status
                if (isApproved)
                {
                    await IndividualItemsDL.DiscardItemAsync(individualItemId, "Approved discard request");
                }
                else
                {
                    // Revert to previous status
                    await IndividualItemsDL.UpdateItemStatusAsync(individualItemId, "Available");
                }

                await _sqlHelper.CommitAsync();
                return 1;
            }
            catch
            {
                await _sqlHelper.RollbackAsync();
                throw;
            }
        }

        #endregion
    }
}
