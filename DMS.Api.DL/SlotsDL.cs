using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class SlotsDL
    {
        // Removed static shared MySQLHelper to fix concurrency issues

        // Each method creates its own instance for thread-safety

        /// <summary>
        /// Get center configuration for slot calculation
        /// </summary>
        public static async Task<DataTable> GetCenterConfigurationAsync(int centerId)
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                @"SELECT CenterOpenTime, CenterCloseTime, SlotDuration, 
                         MachineSessionHours, IsFixedHoursForSession
                  FROM L_Center_Configuration
                  WHERE CenterID = @centerId",
                "@centerId", centerId
            );
            return dt;
        }

        /// <summary>
        /// Get available time slots for a date
        /// </summary>
        public static async Task<DataTable> GetAvailableSlotsAsync(int centerId, DateTime date)
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                @"SELECT SlotStartTime, SlotEndTime, 
                         a.AppointmentID, p.PatientName, p.PatientCode
                  FROM T_Slots s
                  INNER JOIN T_Appointments a ON s.AppointmentID = a.AppointmentID
                  INNER JOIN M_Patients p ON s.PatientID = p.PatientID
                  WHERE s.CenterID = @centerId 
                  AND s.SlotDate = @date
                  AND s.IsActive = 1
                  AND a.AppointmentStatus NOT IN (5, 6)
                  ORDER BY s.SlotStartTime",
                "@centerId", centerId,
                "@date", date.Date
            );
            return dt;
        }
    }
}
