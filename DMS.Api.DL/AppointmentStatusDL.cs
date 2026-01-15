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
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        /// <summary>
        /// Get all appointment statuses
        /// </summary>
        public static async Task<DataTable> GetAllAppointmentStatusesAsync(bool activeOnly = true)
        {
            string query = activeOnly
                ? "SELECT * FROM M_AppointmentStatus WHERE IsActive = 1 ORDER BY StatusID"
                : "SELECT * FROM M_AppointmentStatus ORDER BY StatusID";

            var dt = await _sqlHelper.ExecDataTableAsync(query);
            return dt;
        }

        /// <summary>
        /// Get appointment status by ID
        /// </summary>
        public static async Task<DataTable> GetAppointmentStatusByIdAsync(int statusId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_AppointmentStatus WHERE StatusID = @statusId",
                "@statusId", statusId
            );
            return dt;
        }
    }
}
