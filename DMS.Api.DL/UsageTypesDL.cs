using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class UsageTypesDL
    {
        // Removed static shared MySQLHelper to fix concurrency issues

        // Each method creates its own instance for thread-safety

        #region GET Operations

        /// <summary>
        /// Get all usage types
        /// </summary>
        public static async Task<DataTable> GetAllUsageTypesAsync(bool activeOnly = true)
        {
            using var sqlHelper = new MySQLHelper();
            string query = activeOnly
                ? "SELECT * FROM M_Inventory_Usage_Types WHERE IsActive = 1 ORDER BY UsageTypeName"
                : "SELECT * FROM M_Inventory_Usage_Types ORDER BY UsageTypeName";

            return await sqlHelper.ExecDataTableAsync(query);
        }

        /// <summary>
        /// Get usage type by ID
        /// </summary>
        public static async Task<DataTable> GetUsageTypeByIdAsync(int usageTypeId)
        {
            using var sqlHelper = new MySQLHelper();
            return await sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_Inventory_Usage_Types WHERE UsageTypeID = @usageTypeId",
                "@usageTypeId", usageTypeId
            );
        }

        #endregion
    }
}
