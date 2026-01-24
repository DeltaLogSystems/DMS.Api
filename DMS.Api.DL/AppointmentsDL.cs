using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class AppointmentsDL
    {
        // Removed static shared MySQLHelper to fix concurrency issues
        // Each method creates its own instance for thread-safety

        #region Validation Methods

        /// <summary>
        /// Check if patient already has appointment on specific date
        /// </summary>
        public static async Task<bool> PatientHasAppointmentOnDateAsync(int patientId, DateTime appointmentDate, int? excludeAppointmentId = null)
        {
            using var sqlHelper = new MySQLHelper();
            string query = excludeAppointmentId.HasValue
                ? @"SELECT COUNT(*) FROM T_Appointments
                    WHERE PatientID = @patientId
                    AND AppointmentDate = @appointmentDate
                    AND AppointmentStatus NOT IN (5, 6)
                    AND AppointmentID != @appointmentId"
                : @"SELECT COUNT(*) FROM T_Appointments
                    WHERE PatientID = @patientId
                    AND AppointmentDate = @appointmentDate
                    AND AppointmentStatus NOT IN (5, 6)";

            object[] parameters = excludeAppointmentId.HasValue
                ? new object[] { "@patientId", patientId, "@appointmentDate", appointmentDate.Date, "@appointmentId", excludeAppointmentId.Value }
                : new object[] { "@patientId", patientId, "@appointmentDate", appointmentDate.Date };

            var result = await sqlHelper.ExecScalarAsync(query, parameters);
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Check if slot time range is available (based on active machine count)
        /// </summary>
        public static async Task<bool> IsSlotAvailableAsync(int centerId, DateTime slotDate, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null)
        {
            using var sqlHelper = new MySQLHelper();

            // Get count of active dialysis machines
            int activeMachineCount = await AssetsDL.GetActiveMachineCountAsync(centerId);

            if (activeMachineCount == 0)
            {
                return false; // No machines available
            }

            // Count overlapping booked slots
            string query = excludeAppointmentId.HasValue
                ? @"SELECT COUNT(*) FROM T_Slots s
                    INNER JOIN T_Appointments a ON s.AppointmentID = a.AppointmentID
                    WHERE s.CenterID = @centerId
                    AND s.SlotDate = @slotDate
                    AND s.IsActive = 1
                    AND a.AppointmentStatus NOT IN (5, 6)
                    AND s.AppointmentID != @appointmentId
                    AND (
                        (@startTime >= s.SlotStartTime AND @startTime < s.SlotEndTime)
                        OR (@endTime > s.SlotStartTime AND @endTime <= s.SlotEndTime)
                        OR (@startTime <= s.SlotStartTime AND @endTime >= s.SlotEndTime)
                    )"
                : @"SELECT COUNT(*) FROM T_Slots s
                    INNER JOIN T_Appointments a ON s.AppointmentID = a.AppointmentID
                    WHERE s.CenterID = @centerId
                    AND s.SlotDate = @slotDate
                    AND s.IsActive = 1
                    AND a.AppointmentStatus NOT IN (5, 6)
                    AND (
                        (@startTime >= s.SlotStartTime AND @startTime < s.SlotEndTime)
                        OR (@endTime > s.SlotStartTime AND @endTime <= s.SlotEndTime)
                        OR (@startTime <= s.SlotStartTime AND @endTime >= s.SlotEndTime)
                    )";

            object[] parameters = excludeAppointmentId.HasValue
                ? new object[] { "@centerId", centerId, "@slotDate", slotDate.Date, "@startTime", startTime, "@endTime", endTime, "@appointmentId", excludeAppointmentId.Value }
                : new object[] { "@centerId", centerId, "@slotDate", slotDate.Date, "@startTime", startTime, "@endTime", endTime };

            var result = await sqlHelper.ExecScalarAsync(query, parameters);
            int bookedCount = Convert.ToInt32(result);

            // Slot is available if booked count is less than machine count
            return bookedCount < activeMachineCount;
        }

        /// <summary>
        /// Get booked slots count for a specific time range
        /// </summary>
        public static async Task<int> GetBookedSlotsCountAsync(int centerId, DateTime slotDate, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId = null)
        {
            using var sqlHelper = new MySQLHelper();

            string query = excludeAppointmentId.HasValue
                ? @"SELECT COUNT(*) FROM T_Slots s
                    INNER JOIN T_Appointments a ON s.AppointmentID = a.AppointmentID
                    WHERE s.CenterID = @centerId
                    AND s.SlotDate = @slotDate
                    AND s.IsActive = 1
                    AND a.AppointmentStatus NOT IN (5, 6)
                    AND s.AppointmentID != @appointmentId
                    AND (
                        (@startTime >= s.SlotStartTime AND @startTime < s.SlotEndTime)
                        OR (@endTime > s.SlotStartTime AND @endTime <= s.SlotEndTime)
                        OR (@startTime <= s.SlotStartTime AND @endTime >= s.SlotEndTime)
                    )"
                : @"SELECT COUNT(*) FROM T_Slots s
                    INNER JOIN T_Appointments a ON s.AppointmentID = a.AppointmentID
                    WHERE s.CenterID = @centerId
                    AND s.SlotDate = @slotDate
                    AND s.IsActive = 1
                    AND a.AppointmentStatus NOT IN (5, 6)
                    AND (
                        (@startTime >= s.SlotStartTime AND @startTime < s.SlotEndTime)
                        OR (@endTime > s.SlotStartTime AND @endTime <= s.SlotEndTime)
                        OR (@startTime <= s.SlotStartTime AND @endTime >= s.SlotEndTime)
                    )";

            object[] parameters = excludeAppointmentId.HasValue
                ? new object[] { "@centerId", centerId, "@slotDate", slotDate.Date, "@startTime", startTime, "@endTime", endTime, "@appointmentId", excludeAppointmentId.Value }
                : new object[] { "@centerId", centerId, "@slotDate", slotDate.Date, "@startTime", startTime, "@endTime", endTime };

            var result = await sqlHelper.ExecScalarAsync(query, parameters);
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Get booked slots for a date and center
        /// </summary>
        public static async Task<DataTable> GetBookedSlotsAsync(int centerId, DateTime date)
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                @"SELECT s.*,
                         a.AppointmentID, a.AppointmentStatus,
                         p.PatientName, p.PatientCode,
                         st.StatusName, st.StatusColor
                  FROM T_Slots s
                  INNER JOIN T_Appointments a ON s.AppointmentID = a.AppointmentID
                  INNER JOIN M_Patients p ON s.PatientID = p.PatientID
                  INNER JOIN M_AppointmentStatus st ON a.AppointmentStatus = st.StatusID
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

        #endregion

        #region GET Operations

        /// <summary>
        /// Get all appointments
        /// </summary>
        public static async Task<DataTable> GetAllAppointmentsAsync()
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                @"SELECT a.*,
                         p.PatientCode, p.PatientName, p.MobileNo as PatientMobile,
                         c.CenterName,
                         comp.CompanyName,
                         st.StatusName, st.StatusColor
                  FROM T_Appointments a
                  INNER JOIN M_Patients p ON a.PatientID = p.PatientID
                  INNER JOIN M_Centers c ON a.CenterID = c.CenterID
                  INNER JOIN M_Companies comp ON a.CompanyID = comp.CompanyID
                  INNER JOIN M_AppointmentStatus st ON a.AppointmentStatus = st.StatusID
                  ORDER BY a.AppointmentDate DESC, a.AppointmentID DESC"
            );
            return dt;
        }

        /// <summary>
        /// Get appointment by ID with slots
        /// </summary>
        public static async Task<DataTable> GetAppointmentByIdAsync(int appointmentId)
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                @"SELECT a.*,
                         p.PatientCode, p.PatientName, p.MobileNo as PatientMobile,
                         c.CenterName,
                         comp.CompanyName,
                         st.StatusName, st.StatusColor
                  FROM T_Appointments a
                  INNER JOIN M_Patients p ON a.PatientID = p.PatientID
                  INNER JOIN M_Centers c ON a.CenterID = c.CenterID
                  INNER JOIN M_Companies comp ON a.CompanyID = comp.CompanyID
                  INNER JOIN M_AppointmentStatus st ON a.AppointmentStatus = st.StatusID
                  WHERE a.AppointmentID = @appointmentId",
                "@appointmentId", appointmentId
            );
            return dt;
        }

        /// <summary>
        /// Get slots by appointment ID
        /// </summary>
        public static async Task<DataTable> GetSlotsByAppointmentIdAsync(int appointmentId)
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                @"SELECT * FROM T_Slots
                  WHERE AppointmentID = @appointmentId
                  AND IsActive = 1
                  ORDER BY SlotStartTime",
                "@appointmentId", appointmentId
            );
            return dt;
        }

        /// <summary>
        /// Get appointments by patient ID
        /// </summary>
        public static async Task<DataTable> GetAppointmentsByPatientIdAsync(int patientId)
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                @"SELECT a.*,
                         p.PatientCode, p.PatientName, p.MobileNo as PatientMobile,
                         c.CenterName,
                         comp.CompanyName,
                         st.StatusName, st.StatusColor
                  FROM T_Appointments a
                  INNER JOIN M_Patients p ON a.PatientID = p.PatientID
                  INNER JOIN M_Centers c ON a.CenterID = c.CenterID
                  INNER JOIN M_Companies comp ON a.CompanyID = comp.CompanyID
                  INNER JOIN M_AppointmentStatus st ON a.AppointmentStatus = st.StatusID
                  WHERE a.PatientID = @patientId
                  ORDER BY a.AppointmentDate DESC",
                "@patientId", patientId
            );
            return dt;
        }

        /// <summary>
        /// Get appointments by center and date
        /// </summary>
        public static async Task<DataTable> GetAppointmentsByCenterAndDateAsync(int centerId, DateTime date)
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                @"SELECT a.*,
                         p.PatientCode, p.PatientName, p.MobileNo as PatientMobile,
                         c.CenterName,
                         comp.CompanyName,
                         st.StatusName, st.StatusColor
                  FROM T_Appointments a
                  INNER JOIN M_Patients p ON a.PatientID = p.PatientID
                  INNER JOIN M_Centers c ON a.CenterID = c.CenterID
                  INNER JOIN M_Companies comp ON a.CompanyID = comp.CompanyID
                  INNER JOIN M_AppointmentStatus st ON a.AppointmentStatus = st.StatusID
                  WHERE a.CenterID = @centerId
                  AND a.AppointmentDate = @date
                  ORDER BY a.AppointmentID DESC",
                "@centerId", centerId,
                "@date", date.Date
            );
            return dt;
        }

        /// <summary>
        /// Get appointments by date range
        /// </summary>
        public static async Task<DataTable> GetAppointmentsByDateRangeAsync(int centerId, DateTime startDate, DateTime endDate)
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                @"SELECT a.*,
                         p.PatientCode, p.PatientName, p.MobileNo as PatientMobile,
                         c.CenterName,
                         comp.CompanyName,
                         st.StatusName, st.StatusColor
                  FROM T_Appointments a
                  INNER JOIN M_Patients p ON a.PatientID = p.PatientID
                  INNER JOIN M_Centers c ON a.CenterID = c.CenterID
                  INNER JOIN M_Companies comp ON a.CompanyID = comp.CompanyID
                  INNER JOIN M_AppointmentStatus st ON a.AppointmentStatus = st.StatusID
                  WHERE a.CenterID = @centerId
                  AND a.AppointmentDate BETWEEN @startDate AND @endDate
                  ORDER BY a.AppointmentDate, a.AppointmentID",
                "@centerId", centerId,
                "@startDate", startDate.Date,
                "@endDate", endDate.Date
            );
            return dt;
        }

        /// <summary>
        /// Get appointment count by patient (for tracking dialysis cycles)
        /// </summary>
        public static async Task<int> GetAppointmentCountByPatientAsync(int patientId, int? statusFilter = null)
        {
            using var sqlHelper = new MySQLHelper();
            string query = statusFilter.HasValue
                ? "SELECT COUNT(*) FROM T_Appointments WHERE PatientID = @patientId AND AppointmentStatus = @status"
                : "SELECT COUNT(*) FROM T_Appointments WHERE PatientID = @patientId AND AppointmentStatus = 4"; // Completed only

            object[] parameters = statusFilter.HasValue
                ? new object[] { "@patientId", patientId, "@status", statusFilter.Value }
                : new object[] { "@patientId", patientId };

            var result = await sqlHelper.ExecScalarAsync(query, parameters);
            return Convert.ToInt32(result);
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Create new appointment with slot
        /// </summary>
        public static async Task<int> CreateAppointmentAsync(
            int patientId,
            int centerId,
            int companyId,
            DateTime appointmentDate,
            TimeSpan slotStartTime,
            TimeSpan slotEndTime,
            int createdBy)
        {
            using var sqlHelper = new MySQLHelper();
            try
            {
                await sqlHelper.BeginTransactionAsync();

                // Insert appointment
                var appointmentId = await sqlHelper.ExecScalarAsync(
                    @"INSERT INTO T_Appointments
                      (PatientID, CenterID, CompanyID, AppointmentStatus, AppointmentDate,
                       CreatedBy, CreatedDate, IsRescheduled, RescheduleRevision)
                      VALUES
                      (@patientId, @centerId, @companyId, 1, @appointmentDate,
                       @createdBy, NOW(), 0, 0);
                      SELECT LAST_INSERT_ID();",
                    "@patientId", patientId,
                    "@centerId", centerId,
                    "@companyId", companyId,
                    "@appointmentDate", appointmentDate.Date,
                    "@createdBy", createdBy
                );

                int newAppointmentId = Convert.ToInt32(appointmentId);

                // Insert slot
                await sqlHelper.ExecNonQueryAsync(
                    @"INSERT INTO T_Slots
                      (AppointmentID, PatientID, CenterID, CompanyID,
                       SlotStartTime, SlotEndTime, SlotDate, IsActive)
                      VALUES
                      (@appointmentId, @patientId, @centerId, @companyId,
                       @slotStartTime, @slotEndTime, @slotDate, 1)",
                    "@appointmentId", newAppointmentId,
                    "@patientId", patientId,
                    "@centerId", centerId,
                    "@companyId", companyId,
                    "@slotStartTime", slotStartTime,
                    "@slotEndTime", slotEndTime,
                    "@slotDate", appointmentDate.Date
                );

                await sqlHelper.CommitAsync();
                return newAppointmentId;
            }
            catch
            {
                await sqlHelper.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update appointment status (public method for standalone calls)
        /// </summary>
        public static async Task<int> UpdateAppointmentStatusAsync(int appointmentId, int newStatus, int modifiedBy)
        {
            using var sqlHelper = new MySQLHelper();
            return await UpdateAppointmentStatusAsync(sqlHelper, appointmentId, newStatus, modifiedBy);
        }

        /// <summary>
        /// Update appointment status (internal method for use within transactions)
        /// </summary>
        internal static async Task<int> UpdateAppointmentStatusAsync(MySQLHelper sqlHelper, int appointmentId, int newStatus, int modifiedBy)
        {
            var result = await sqlHelper.ExecNonQueryAsync(
                @"UPDATE T_Appointments
                  SET AppointmentStatus = @newStatus,
                      ModifiedBy = @modifiedBy,
                      ModifiedDate = NOW()
                  WHERE AppointmentID = @appointmentId",
                "@appointmentId", appointmentId,
                "@newStatus", newStatus,
                "@modifiedBy", modifiedBy
            );
            return result;
        }

        /// <summary>
        /// Reschedule appointment
        /// </summary>
        public static async Task<int> RescheduleAppointmentAsync(
            int appointmentId,
            DateTime newAppointmentDate,
            TimeSpan newSlotStartTime,
            TimeSpan newSlotEndTime,
            string rescheduleReason,
            int modifiedBy)
        {
            using var sqlHelper = new MySQLHelper();
            try
            {
                await sqlHelper.BeginTransactionAsync();

                // Get current appointment details
                var dtAppointment = await sqlHelper.ExecDataTableAsync(
                    "SELECT RescheduleRevision FROM T_Appointments WHERE AppointmentID = @appointmentId",
                    "@appointmentId", appointmentId
                );

                if (dtAppointment.Rows.Count == 0)
                {
                    throw new Exception("Appointment not found");
                }

                int currentRevision = Convert.ToInt32(dtAppointment.Rows[0]["RescheduleRevision"]);
                int newRevision = currentRevision + 1;

                // Update appointment
                await sqlHelper.ExecNonQueryAsync(
                    @"UPDATE T_Appointments
                      SET AppointmentDate = @newAppointmentDate,
                          AppointmentStatus = 7,
                          IsRescheduled = 1,
                          RescheduleRevision = @newRevision,
                          RescheduleReason = @rescheduleReason,
                          ModifiedBy = @modifiedBy,
                          ModifiedDate = NOW()
                      WHERE AppointmentID = @appointmentId",
                    "@appointmentId", appointmentId,
                    "@newAppointmentDate", newAppointmentDate.Date,
                    "@newRevision", newRevision,
                    "@rescheduleReason", rescheduleReason,
                    "@modifiedBy", modifiedBy
                );

                // Deactivate old slots
                await sqlHelper.ExecNonQueryAsync(
                    "UPDATE T_Slots SET IsActive = 0 WHERE AppointmentID = @appointmentId",
                    "@appointmentId", appointmentId
                );

                // Get appointment details for new slot
                var dtDetails = await sqlHelper.ExecDataTableAsync(
                    "SELECT PatientID, CenterID, CompanyID FROM T_Appointments WHERE AppointmentID = @appointmentId",
                    "@appointmentId", appointmentId
                );

                var row = dtDetails.Rows[0];
                int patientId = Convert.ToInt32(row["PatientID"]);
                int centerId = Convert.ToInt32(row["CenterID"]);
                int companyId = Convert.ToInt32(row["CompanyID"]);

                // Insert new slot
                await sqlHelper.ExecNonQueryAsync(
                    @"INSERT INTO T_Slots
                      (AppointmentID, PatientID, CenterID, CompanyID,
                       SlotStartTime, SlotEndTime, SlotDate, IsActive)
                      VALUES
                      (@appointmentId, @patientId, @centerId, @companyId,
                       @slotStartTime, @slotEndTime, @slotDate, 1)",
                    "@appointmentId", appointmentId,
                    "@patientId", patientId,
                    "@centerId", centerId,
                    "@companyId", companyId,
                    "@slotStartTime", newSlotStartTime,
                    "@slotEndTime", newSlotEndTime,
                    "@slotDate", newAppointmentDate.Date
                );

                await sqlHelper.CommitAsync();
                return appointmentId;
            }
            catch
            {
                await sqlHelper.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Update patient dialysis cycles and manage cycle progression
        /// </summary>
        public static async Task<int> UpdatePatientDialysisCyclesAsync(int patientId, DateTime appointmentDate)
        {
            using var sqlHelper = new MySQLHelper();
            try
            {
                await sqlHelper.BeginTransactionAsync();

                // Get patient's current cycle info
                var dtPatient = await sqlHelper.ExecDataTableAsync(
                    @"SELECT CurrentCycleStartDate, CurrentCycleEndDate
              FROM M_Patients
              WHERE PatientID = @patientId",
                    "@patientId", patientId
                );

                if (dtPatient.Rows.Count == 0)
                {
                    throw new Exception("Patient not found");
                }

                var row = dtPatient.Rows[0];
                DateTime? cycleStartDate = row["CurrentCycleStartDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["CurrentCycleStartDate"])
                    : (DateTime?)null;
                DateTime? cycleEndDate = row["CurrentCycleEndDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["CurrentCycleEndDate"])
                    : (DateTime?)null;

                // If no active cycle, start new one
                if (!cycleStartDate.HasValue || !cycleEndDate.HasValue)
                {
                    await PatientCyclesDL.StartNewCycleAsync(patientId, appointmentDate);
                }
                // If cycle has expired, complete it and start new one
                else if (DateTime.Today > cycleEndDate.Value)
                {
                    await PatientCyclesDL.CompleteCycleAndStartNewAsync(patientId);
                    // Start new cycle with current appointment date
                    await PatientCyclesDL.StartNewCycleAsync(patientId, appointmentDate);
                }

                // Update session count for current cycle
                await PatientCyclesDL.UpdateCycleSessionCountAsync(patientId);

                await sqlHelper.CommitAsync();
                return 1;
            }
            catch
            {
                await sqlHelper.RollbackAsync();
                throw;
            }
        }


        #endregion

        #region DELETE Operations

        /// <summary>
        /// Cancel appointment and deactivate associated slots
        /// </summary>
        public static async Task<int> CancelAppointmentAsync(int appointmentId, int modifiedBy)
        {
            using var sqlHelper = new MySQLHelper();
            try
            {
                await sqlHelper.BeginTransactionAsync();

                // Update appointment status to Cancelled (5)
                var result = await sqlHelper.ExecNonQueryAsync(
                    @"UPDATE T_Appointments
              SET AppointmentStatus = 5,
                  ModifiedBy = @modifiedBy,
                  ModifiedDate = NOW()
              WHERE AppointmentID = @appointmentId",
                    "@appointmentId", appointmentId,
                    "@modifiedBy", modifiedBy
                );

                if (result > 0)
                {
                    // Deactivate all slots for this appointment
                    // This makes the time slots available for other patients
                    await sqlHelper.ExecNonQueryAsync(
                        @"UPDATE T_Slots
                  SET IsActive = 0
                  WHERE AppointmentID = @appointmentId",
                        "@appointmentId", appointmentId
                    );
                }

                await sqlHelper.CommitAsync();
                return result;
            }
            catch
            {
                await sqlHelper.RollbackAsync();
                throw;
            }
        }


        /// <summary>
        /// Delete appointment permanently
        /// </summary>
        public static async Task<int> DeleteAppointmentAsync(int appointmentId)
        {
            using var sqlHelper = new MySQLHelper();
            try
            {
                await sqlHelper.BeginTransactionAsync();

                // Delete slots first (cascade)
                await sqlHelper.ExecNonQueryAsync(
                    "DELETE FROM T_Slots WHERE AppointmentID = @appointmentId",
                    "@appointmentId", appointmentId
                );

                // Delete appointment
                var result = await sqlHelper.ExecNonQueryAsync(
                    "DELETE FROM T_Appointments WHERE AppointmentID = @appointmentId",
                    "@appointmentId", appointmentId
                );

                await sqlHelper.CommitAsync();
                return result;
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
