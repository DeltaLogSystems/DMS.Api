using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class PatientCyclesDL
    {
        // Removed static shared MySQLHelper to fix concurrency issues

        // Each method creates its own instance for thread-safety

        #region Cycle Management

        /// <summary>
        /// Start new cycle for patient
        /// </summary>
        public static async Task<int> StartNewCycleAsync(int patientId, DateTime firstAppointmentDate)
        {
            using var sqlHelper = new MySQLHelper();
            try
            {
                await sqlHelper.BeginTransactionAsync();

                // Get patient's current cycle info
                var dtPatient = await sqlHelper.ExecDataTableAsync(
                    "SELECT CurrentCycleNumber, TotalCompletedCycles FROM M_Patients WHERE PatientID = @patientId",
                    "@patientId", patientId
                );

                if (dtPatient.Rows.Count == 0)
                {
                    throw new Exception("Patient not found");
                }

                int currentCycleNumber = Convert.ToInt32(dtPatient.Rows[0]["CurrentCycleNumber"]);
                int totalCompletedCycles = Convert.ToInt32(dtPatient.Rows[0]["TotalCompletedCycles"]);

                DateTime cycleStartDate = firstAppointmentDate.Date;
                DateTime cycleEndDate = cycleStartDate.AddDays(42); // MJPJAY: 42 days cycle

                // Update patient's current cycle information
                await sqlHelper.ExecNonQueryAsync(
                    @"UPDATE M_Patients 
                      SET CurrentCycleNumber = @cycleNumber,
                          CurrentCycleStartDate = @startDate,
                          CurrentCycleEndDate = @endDate,
                          CurrentCycleSessionCount = 0
                      WHERE PatientID = @patientId",
                    "@patientId", patientId,
                    "@cycleNumber", currentCycleNumber,
                    "@startDate", cycleStartDate,
                    "@endDate", cycleEndDate
                );

                // Insert cycle history record
                var cycleHistoryId = await sqlHelper.ExecScalarAsync(
                    @"INSERT INTO T_PatientCycles 
                      (PatientID, CycleNumber, CycleStartDate, CycleEndDate, 
                       PlannedSessions, CompletedSessions, CycleStatus, FirstAppointmentDate)
                      VALUES 
                      (@patientId, @cycleNumber, @startDate, @endDate,
                       18, 0, 'Active', @firstAppointmentDate);
                      SELECT LAST_INSERT_ID();",
                    "@patientId", patientId,
                    "@cycleNumber", currentCycleNumber,
                    "@startDate", cycleStartDate,
                    "@endDate", cycleEndDate,
                    "@firstAppointmentDate", firstAppointmentDate
                );

                await sqlHelper.CommitAsync();
                return Convert.ToInt32(cycleHistoryId);
            }
            catch
            {
                await sqlHelper.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Update patient cycle session count
        /// </summary>
        public static async Task<int> UpdateCycleSessionCountAsync(int patientId)
        {
            using var sqlHelper = new MySQLHelper();
            try
            {
                await sqlHelper.BeginTransactionAsync();

                // Get patient's current cycle info
                var dtPatient = await sqlHelper.ExecDataTableAsync(
                    @"SELECT CurrentCycleNumber, CurrentCycleStartDate, 
                             CurrentCycleEndDate, CurrentCycleSessionCount
                      FROM M_Patients 
                      WHERE PatientID = @patientId",
                    "@patientId", patientId
                );

                if (dtPatient.Rows.Count == 0)
                {
                    throw new Exception("Patient not found");
                }

                var row = dtPatient.Rows[0];
                int currentCycleNumber = Convert.ToInt32(row["CurrentCycleNumber"]);
                DateTime? cycleStartDate = row["CurrentCycleStartDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["CurrentCycleStartDate"])
                    : (DateTime?)null;
                DateTime? cycleEndDate = row["CurrentCycleEndDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["CurrentCycleEndDate"])
                    : (DateTime?)null;

                // Count completed appointments in current cycle
                int completedSessionsInCycle = 0;

                if (cycleStartDate.HasValue && cycleEndDate.HasValue)
                {
                    var result = await sqlHelper.ExecScalarAsync(
                        @"SELECT COUNT(*) 
                          FROM T_Appointments 
                          WHERE PatientID = @patientId 
                          AND AppointmentStatus = 4
                          AND AppointmentDate >= @startDate 
                          AND AppointmentDate <= @endDate",
                        "@patientId", patientId,
                        "@startDate", cycleStartDate.Value,
                        "@endDate", cycleEndDate.Value
                    );
                    completedSessionsInCycle = Convert.ToInt32(result);
                }

                // Update patient's current cycle session count
                await sqlHelper.ExecNonQueryAsync(
                    @"UPDATE M_Patients 
                      SET CurrentCycleSessionCount = @sessionCount,
                          DialysisCycles = (SELECT COUNT(*) FROM T_Appointments 
                                           WHERE PatientID = @patientId AND AppointmentStatus = 4)
                      WHERE PatientID = @patientId",
                    "@patientId", patientId,
                    "@sessionCount", completedSessionsInCycle
                );

                // Update cycle history
                if (cycleStartDate.HasValue)
                {
                    await sqlHelper.ExecNonQueryAsync(
                        @"UPDATE T_PatientCycles 
                          SET CompletedSessions = @completedSessions,
                              LastAppointmentDate = (SELECT MAX(AppointmentDate) 
                                                    FROM T_Appointments 
                                                    WHERE PatientID = @patientId 
                                                    AND AppointmentStatus = 4
                                                    AND AppointmentDate >= @startDate)
                          WHERE PatientID = @patientId 
                          AND CycleNumber = @cycleNumber
                          AND CycleStatus = 'Active'",
                        "@patientId", patientId,
                        "@cycleNumber", currentCycleNumber,
                        "@completedSessions", completedSessionsInCycle,
                        "@startDate", cycleStartDate.Value
                    );
                }

                await sqlHelper.CommitAsync();
                return completedSessionsInCycle;
            }
            catch
            {
                await sqlHelper.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Complete current cycle and start new one
        /// </summary>
        public static async Task<bool> CompleteCycleAndStartNewAsync(int patientId)
        {
            using var sqlHelper = new MySQLHelper();
            try
            {
                await sqlHelper.BeginTransactionAsync();

                // Get patient's current cycle info
                var dtPatient = await sqlHelper.ExecDataTableAsync(
                    @"SELECT CurrentCycleNumber, CurrentCycleStartDate, 
                             CurrentCycleEndDate, TotalCompletedCycles, CurrentCycleSessionCount
                      FROM M_Patients 
                      WHERE PatientID = @patientId",
                    "@patientId", patientId
                );

                if (dtPatient.Rows.Count == 0)
                {
                    throw new Exception("Patient not found");
                }

                var row = dtPatient.Rows[0];
                int currentCycleNumber = Convert.ToInt32(row["CurrentCycleNumber"]);
                int totalCompletedCycles = Convert.ToInt32(row["TotalCompletedCycles"]);
                int completedSessions = Convert.ToInt32(row["CurrentCycleSessionCount"]);

                // Mark current cycle as completed or incomplete
                string cycleStatus = completedSessions >= 18 ? "Completed" : "Incomplete";

                await sqlHelper.ExecNonQueryAsync(
                    @"UPDATE T_PatientCycles 
                      SET CycleStatus = @status,
                          CompletedDate = NOW()
                      WHERE PatientID = @patientId 
                      AND CycleNumber = @cycleNumber
                      AND CycleStatus = 'Active'",
                    "@patientId", patientId,
                    "@cycleNumber", currentCycleNumber,
                    "@status", cycleStatus
                );

                // Increment cycle number and completed cycles count
                int newCycleNumber = currentCycleNumber + 1;
                int newTotalCompletedCycles = cycleStatus == "Completed"
                    ? totalCompletedCycles + 1
                    : totalCompletedCycles;

                // Reset current cycle info (will be set when next appointment is created)
                await sqlHelper.ExecNonQueryAsync(
                    @"UPDATE M_Patients 
                      SET CurrentCycleNumber = @newCycleNumber,
                          TotalCompletedCycles = @totalCompleted,
                          CurrentCycleStartDate = NULL,
                          CurrentCycleEndDate = NULL,
                          CurrentCycleSessionCount = 0
                      WHERE PatientID = @patientId",
                    "@patientId", patientId,
                    "@newCycleNumber", newCycleNumber,
                    "@totalCompleted", newTotalCompletedCycles
                );

                await sqlHelper.CommitAsync();
                return true;
            }
            catch
            {
                await sqlHelper.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Check if patient's cycle has expired (42 days completed)
        /// </summary>
        public static async Task<bool> IsCycleExpiredAsync(int patientId)
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                @"SELECT CurrentCycleEndDate 
                  FROM M_Patients 
                  WHERE PatientID = @patientId",
                "@patientId", patientId
            );

            if (dt.Rows.Count == 0 || dt.Rows[0]["CurrentCycleEndDate"] == DBNull.Value)
            {
                return false;
            }

            DateTime cycleEndDate = Convert.ToDateTime(dt.Rows[0]["CurrentCycleEndDate"]);
            return DateTime.Today > cycleEndDate;
        }

        /// <summary>
        /// Get patient's current cycle information
        /// </summary>
        public static async Task<DataTable> GetPatientCurrentCycleAsync(int patientId)
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                @"SELECT p.PatientID, p.PatientCode, p.PatientName,
                         p.CurrentCycleNumber, p.CurrentCycleStartDate, p.CurrentCycleEndDate,
                         p.CurrentCycleSessionCount, p.TotalCompletedCycles, p.DialysisCycles,
                         DATEDIFF(p.CurrentCycleEndDate, CURDATE()) as DaysRemaining,
                         (18 - p.CurrentCycleSessionCount) as SessionsRemaining
                  FROM M_Patients p
                  WHERE p.PatientID = @patientId",
                "@patientId", patientId
            );
            return dt;
        }

        /// <summary>
        /// Get patient's cycle history
        /// </summary>
        public static async Task<DataTable> GetPatientCycleHistoryAsync(int patientId)
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                @"SELECT * 
                  FROM T_PatientCycles 
                  WHERE PatientID = @patientId 
                  ORDER BY CycleNumber DESC",
                "@patientId", patientId
            );
            return dt;
        }

        /// <summary>
        /// Get all active cycles that need to be checked for expiry
        /// </summary>
        public static async Task<DataTable> GetActiveCyclesForExpiryCheckAsync()
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                @"SELECT p.PatientID, p.CurrentCycleNumber, p.CurrentCycleEndDate
                  FROM M_Patients p
                  WHERE p.CurrentCycleEndDate IS NOT NULL
                  AND p.CurrentCycleEndDate < CURDATE()
                  AND EXISTS (
                      SELECT 1 FROM T_PatientCycles pc
                      WHERE pc.PatientID = p.PatientID
                      AND pc.CycleNumber = p.CurrentCycleNumber
                      AND pc.CycleStatus = 'Active'
                  )"
            );
            return dt;
        }

        /// <summary>
        /// Process expired cycles (to be run as scheduled job)
        /// </summary>
        public static async Task<int> ProcessExpiredCyclesAsync()
        {
            using var sqlHelper = new MySQLHelper();
            try
            {
                var dtExpired = await GetActiveCyclesForExpiryCheckAsync();
                int processedCount = 0;

                foreach (DataRow row in dtExpired.Rows)
                {
                    int patientId = Convert.ToInt32(row["PatientID"]);

                    // Complete the expired cycle and start new one
                    await CompleteCycleAndStartNewAsync(patientId);
                    processedCount++;
                }

                return processedCount;
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
