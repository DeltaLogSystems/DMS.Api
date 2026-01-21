using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class InventoryUsageDL
    {
        // Removed static shared MySQLHelper to fix concurrency issues

        // Each method creates its own instance for thread-safety

        #region GET Operations

        /// <summary>
        /// Get all usage records
        /// </summary>
        public static async Task<DataTable> GetAllUsageAsync(
            int? centerId = null,
            int? inventoryItemId = null,
            int? appointmentId = null,
            int? patientId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            using var sqlHelper = new MySQLHelper();
            string query = @"SELECT u.*, 
                                    i.ItemCode, i.ItemName,
                                    ii.IndividualItemCode,
                                    p.PatientCode, p.PatientName,
                                    a.AppointmentDate
                             FROM T_Inventory_Usage u
                             INNER JOIN M_Inventory_Items i ON u.InventoryItemID = i.InventoryItemID
                             LEFT JOIN T_Inventory_Individual_Items ii ON u.IndividualItemID = ii.IndividualItemID
                             INNER JOIN M_Patients p ON u.PatientID = p.PatientID
                             INNER JOIN T_Appointments a ON u.AppointmentID = a.AppointmentID
                             WHERE 1=1";

            var parameters = new List<object>();

            if (centerId.HasValue)
            {
                query += " AND u.CenterID = @centerId";
                parameters.Add("@centerId");
                parameters.Add(centerId.Value);
            }

            if (inventoryItemId.HasValue)
            {
                query += " AND u.InventoryItemID = @inventoryItemId";
                parameters.Add("@inventoryItemId");
                parameters.Add(inventoryItemId.Value);
            }

            if (appointmentId.HasValue)
            {
                query += " AND u.AppointmentID = @appointmentId";
                parameters.Add("@appointmentId");
                parameters.Add(appointmentId.Value);
            }

            if (patientId.HasValue)
            {
                query += " AND u.PatientID = @patientId";
                parameters.Add("@patientId");
                parameters.Add(patientId.Value);
            }

            if (startDate.HasValue)
            {
                query += " AND u.UsageDate >= @startDate";
                parameters.Add("@startDate");
                parameters.Add(startDate.Value);
            }

            if (endDate.HasValue)
            {
                query += " AND u.UsageDate <= @endDate";
                parameters.Add("@endDate");
                parameters.Add(endDate.Value.Date.AddDays(1).AddSeconds(-1));
            }

            query += " ORDER BY u.UsageDate DESC";

            return await sqlHelper.ExecDataTableAsync(query, parameters.ToArray());
        }

        /// <summary>
        /// Get usage by appointment
        /// </summary>
        public static async Task<DataTable> GetUsageByAppointmentAsync(int appointmentId)
        {
            using var sqlHelper = new MySQLHelper();
            return await sqlHelper.ExecDataTableAsync(
                @"SELECT u.*, 
                         i.ItemCode, i.ItemName,
                         ii.IndividualItemCode
                  FROM T_Inventory_Usage u
                  INNER JOIN M_Inventory_Items i ON u.InventoryItemID = i.InventoryItemID
                  LEFT JOIN T_Inventory_Individual_Items ii ON u.IndividualItemID = ii.IndividualItemID
                  WHERE u.AppointmentID = @appointmentId
                  ORDER BY i.ItemName",
                "@appointmentId", appointmentId
            );
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Record inventory usage
        /// </summary>
        public static async Task<int> RecordUsageAsync(
            int inventoryItemId,
            int? individualItemId,
            int stockId,
            int centerId,
            int appointmentId,
            int patientId,
            decimal quantityUsed,
            string? itemCondition,
            string? notes,
            int usedBy)
        {
            using var sqlHelper = new MySQLHelper();
            try
            {
                await sqlHelper.BeginTransactionAsync();

                // Get usage number for individual items
                int usageNumber = 1;
                if (individualItemId.HasValue)
                {
                    var currentUsage = await sqlHelper.ExecScalarAsync(
                        "SELECT CurrentUsageCount FROM T_Inventory_Individual_Items WHERE IndividualItemID = @individualItemId",
                        "@individualItemId", individualItemId.Value
                    );
                    usageNumber = Convert.ToInt32(currentUsage) + 1;
                }

                // Insert usage record
                var result = await sqlHelper.ExecScalarAsync(
                    @"INSERT INTO T_Inventory_Usage 
                      (InventoryItemID, IndividualItemID, StockID, CenterID, AppointmentID, PatientID,
                       UsageDate, QuantityUsed, UsageNumber, ItemCondition, Notes, UsedBy)
                      VALUES 
                      (@inventoryItemId, @individualItemId, @stockId, @centerId, @appointmentId, @patientId,
                       NOW(), @quantityUsed, @usageNumber, @itemCondition, @notes, @usedBy);
                      SELECT LAST_INSERT_ID();",
                    "@inventoryItemId", inventoryItemId,
                    "@individualItemId", individualItemId ?? (object)DBNull.Value,
                    "@stockId", stockId,
                    "@centerId", centerId,
                    "@appointmentId", appointmentId,
                    "@patientId", patientId,
                    "@quantityUsed", quantityUsed,
                    "@usageNumber", usageNumber,
                    "@itemCondition", itemCondition ?? (object)DBNull.Value,
                    "@notes", notes ?? (object)DBNull.Value,
                    "@usedBy", usedBy
                );

                // Update individual item if applicable (use transaction-aware internal overloads)
                if (individualItemId.HasValue)
                {
                    await IndividualItemsDL.IncrementUsageCountAsync(sqlHelper, individualItemId.Value);
                }
                else
                {
                    // Deduct from stock for non-individual items
                    await InventoryStockDL.DeductAvailableQuantityAsync(sqlHelper, stockId, (int)quantityUsed);
                }

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
    }
}
