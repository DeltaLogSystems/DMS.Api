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
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region GET Operations

        /// <summary>
        /// Get all usage types
        /// </summary>
        public static async Task<DataTable> GetAllUsageTypesAsync(bool activeOnly = true)
        {
            string query = activeOnly
                ? "SELECT * FROM M_Inventory_Usage_Types WHERE IsActive = 1 ORDER BY UsageTypeName"
                : "SELECT * FROM M_Inventory_Usage_Types ORDER BY UsageTypeName";

            return await _sqlHelper.ExecDataTableAsync(query);
        }

        /// <summary>
        /// Get usage type by ID
        /// </summary>
        public static async Task<DataTable> GetUsageTypeByIdAsync(int usageTypeId)
        {
            return await _sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_Inventory_Usage_Types WHERE UsageTypeID = @usageTypeId",
                "@usageTypeId", usageTypeId
            );
        }

        #endregion
    }
}
