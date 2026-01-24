using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class AppointmentStatusDL
    {
        // Removed static shared MySQLHelper to fix concurrency issues

        // Each method creates its own instance for thread-safety

        /// <summary>
        /// Get all appointment statuses
        /// </summary>
        public static async Task<DataTable> GetAllAppointmentStatusesAsync(bool activeOnly = true)
        {
            using var sqlHelper = new MySQLHelper();
            string query = activeOnly
                ? "SELECT * FROM M_AppointmentStatus WHERE IsActive = 1 ORDER BY StatusID"
                : "SELECT * FROM M_AppointmentStatus ORDER BY StatusID";

            var dt = await sqlHelper.ExecDataTableAsync(query);
            return dt;
        }

        /// <summary>
        /// Get appointment status by ID
        /// </summary>
        public static async Task<DataTable> GetAppointmentStatusByIdAsync(int statusId)
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_AppointmentStatus WHERE StatusID = @statusId",
                "@statusId", statusId
            );
            return dt;
        }
    }
}
