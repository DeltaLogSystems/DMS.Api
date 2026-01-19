using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class DialysisSessionsDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region Session Code Generation

        /// <summary>
        /// Generate unique session code
        /// </summary>
        public static async Task<string> GenerateSessionCodeAsync(int centerId, DateTime sessionDate)
        {
            // Get center name and create code from initials
            var dtCenter = await _sqlHelper.ExecDataTableAsync(
                "SELECT CenterName FROM M_Centers WHERE CenterID = @centerId",
                "@centerId", centerId
            );

            string centerCode = "CTR";
            if (dtCenter.Rows.Count > 0)
            {
                string centerName = dtCenter.Rows[0]["CenterName"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(centerName))
                {
                    // Extract initials from center name (e.g., "Nephro Dialysis Centre" -> "NDC")
                    var words = centerName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    centerCode = string.Join("", words.Select(w => w[0])).ToUpper();
                    centerCode = centerCode.Substring(0, Math.Min(3, centerCode.Length));
                }
            }

            // Format: SES-CTR-20260118-001
            string dateStr = sessionDate.ToString("yyyyMMdd");

            var lastNumber = await _sqlHelper.ExecScalarAsync(
                @"SELECT MAX(CAST(SUBSTRING(SessionCode, -3) AS UNSIGNED)) 
                  FROM T_Dialysis_Sessions 
                  WHERE SessionCode LIKE @pattern",
                "@pattern", $"SES-{centerCode}-{dateStr}-%"
            );

            int nextNumber = lastNumber != DBNull.Value && lastNumber != null
                ? Convert.ToInt32(lastNumber) + 1
                : 1;

            return $"SES-{centerCode}-{dateStr}-{nextNumber:D3}";
        }

        #endregion

        #region GET Operations

        /// <summary>
        /// Get all sessions with filters
        /// </summary>
        public static async Task<DataTable> GetAllSessionsAsync(
            int? centerId = null,
            int? patientId = null,
            DateTime? sessionDate = null,
            string? sessionStatus = null)
        {
            string query = @"SELECT s.*,
                                    p.PatientCode, p.PatientName,
                                    a.AppointmentDate,
                                    ast.AssetCode, ast.AssetName,
                                    c.CenterName
                             FROM T_Dialysis_Sessions s
                             INNER JOIN M_Patients p ON s.PatientID = p.PatientID
                             INNER JOIN T_Appointments a ON s.AppointmentID = a.AppointmentID
                             LEFT JOIN M_Assets ast ON s.AssetID = ast.AssetID
                             INNER JOIN M_Centers c ON s.CenterID = c.CenterID
                             WHERE 1=1";

            var parameters = new List<object>();

            if (centerId.HasValue)
            {
                query += " AND s.CenterID = @centerId";
                parameters.Add("@centerId");
                parameters.Add(centerId.Value);
            }

            if (patientId.HasValue)
            {
                query += " AND s.PatientID = @patientId";
                parameters.Add("@patientId");
                parameters.Add(patientId.Value);
            }

            if (sessionDate.HasValue)
            {
                query += " AND s.SessionDate = @sessionDate";
                parameters.Add("@sessionDate");
                parameters.Add(sessionDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(sessionStatus))
            {
                query += " AND s.SessionStatus = @sessionStatus";
                parameters.Add("@sessionStatus");
                parameters.Add(sessionStatus);
            }

            query += " ORDER BY s.SessionDate DESC, s.ActualStartTime DESC";

            return await _sqlHelper.ExecDataTableAsync(query, parameters.ToArray());
        }

        /// <summary>
        /// Get session by ID
        /// </summary>
        public static async Task<DataTable> GetSessionByIdAsync(int sessionId)
        {
            return await _sqlHelper.ExecDataTableAsync(
                @"SELECT s.*,
                         p.PatientCode, p.PatientName, p.MobileNo,
                         a.AppointmentDate,
                         ast.AssetCode, ast.AssetName,
                         c.CenterName
                  FROM T_Dialysis_Sessions s
                  INNER JOIN M_Patients p ON s.PatientID = p.PatientID
                  INNER JOIN T_Appointments a ON s.AppointmentID = a.AppointmentID
                  LEFT JOIN M_Assets ast ON s.AssetID = ast.AssetID
                  INNER JOIN M_Centers c ON s.CenterID = c.CenterID
                  WHERE s.SessionID = @sessionId",
                "@sessionId", sessionId
            );
        }

        /// <summary>
        /// Get session by appointment ID
        /// </summary>
        public static async Task<DataTable> GetSessionByAppointmentAsync(int appointmentId)
        {
            return await _sqlHelper.ExecDataTableAsync(
                @"SELECT s.*, 
                         p.PatientCode, p.PatientName,
                         ast.AssetCode, ast.AssetName
                  FROM T_Dialysis_Sessions s
                  INNER JOIN M_Patients p ON s.PatientID = p.PatientID
                  LEFT JOIN M_Assets ast ON s.AssetID = ast.AssetID
                  WHERE s.AppointmentID = @appointmentId",
                "@appointmentId", appointmentId
            );
        }

        /// <summary>
        /// Get active sessions (In Progress)
        /// </summary>
        public static async Task<DataTable> GetActiveSessionsAsync(int? centerId = null)
        {
            string query = @"SELECT s.*, 
                                    p.PatientCode, p.PatientName,
                                    ast.AssetCode, ast.AssetName,
                                    TIMESTAMPDIFF(MINUTE, s.ActualStartTime, NOW()) as ElapsedMinutes
                             FROM T_Dialysis_Sessions s
                             INNER JOIN M_Patients p ON s.PatientID = p.PatientID
                             LEFT JOIN M_Assets ast ON s.AssetID = ast.AssetID
                             WHERE s.SessionStatus = 'In Progress'";

            var parameters = new List<object>();

            if (centerId.HasValue)
            {
                query += " AND s.CenterID = @centerId";
                parameters.Add("@centerId");
                parameters.Add(centerId.Value);
            }

            query += " ORDER BY s.ActualStartTime";

            return await _sqlHelper.ExecDataTableAsync(query, parameters.ToArray());
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Create new session
        /// </summary>
        public static async Task<int> CreateSessionAsync(
            int appointmentId,
            int patientId,
            int centerId,
            DateTime sessionDate,
            TimeSpan? scheduledStartTime,
            string? dialysisType,
            string? preSessionNotes,
            int createdBy)
        {
            try
            {
                await _sqlHelper.BeginTransactionAsync();

                // Generate session code
                string sessionCode = await GenerateSessionCodeAsync(centerId, sessionDate);

                // Create session
                var result = await _sqlHelper.ExecScalarAsync(
                    @"INSERT INTO T_Dialysis_Sessions 
                      (SessionCode, AppointmentID, PatientID, CenterID, SessionStatus,
                       SessionDate, ScheduledStartTime, DialysisType, PreSessionNotes,
                       CreatedDate, CreatedBy)
                      VALUES 
                      (@sessionCode, @appointmentId, @patientId, @centerId, 'Not Started',
                       @sessionDate, @scheduledStartTime, @dialysisType, @preSessionNotes,
                       NOW(), @createdBy);
                      SELECT LAST_INSERT_ID();",
                    "@sessionCode", sessionCode,
                    "@appointmentId", appointmentId,
                    "@patientId", patientId,
                    "@centerId", centerId,
                    "@sessionDate", sessionDate.Date,
                    "@scheduledStartTime", scheduledStartTime ?? (object)DBNull.Value,
                    "@dialysisType", dialysisType ?? (object)DBNull.Value,
                    "@preSessionNotes", preSessionNotes ?? (object)DBNull.Value,
                    "@createdBy", createdBy
                );

                int sessionId = Convert.ToInt32(result);

                // Update appointment status
                await AppointmentsDL.UpdateAppointmentStatusAsync(appointmentId, 2, createdBy); // Status 2 = In Progress

                // Log timeline event
                await InsertTimelineEventAsync(sessionId, "SessionCreated", "Dialysis session created", createdBy);

                await _sqlHelper.CommitAsync();
                return sessionId;
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
        /// Assign machine to session
        /// </summary>
        public static async Task<int> AssignMachineToSessionAsync(
            int sessionId,
            int assetId,
            int assetAssignmentId,
            int modifiedBy)
        {
            try
            {
                await _sqlHelper.BeginTransactionAsync();

                // Update session
                var result = await _sqlHelper.ExecNonQueryAsync(
                    @"UPDATE T_Dialysis_Sessions 
                      SET AssetID = @assetId,
                          AssetAssignmentID = @assetAssignmentId,
                          ModifiedDate = NOW(),
                          ModifiedBy = @modifiedBy
                      WHERE SessionID = @sessionId",
                    "@sessionId", sessionId,
                    "@assetId", assetId,
                    "@assetAssignmentId", assetAssignmentId,
                    "@modifiedBy", modifiedBy
                );

                // Get asset details for timeline
                var dtAsset = await AssetsDL.GetAssetByIdAsync(assetId);
                string assetCode = dtAsset.Rows.Count > 0 ? dtAsset.Rows[0]["AssetCode"]?.ToString() ?? "" : "";

                // Log timeline event
                await InsertTimelineEventAsync(sessionId, "MachineAssigned", $"Dialysis machine {assetCode} assigned", modifiedBy);

                await _sqlHelper.CommitAsync();
                return result;
            }
            catch
            {
                await _sqlHelper.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Start dialysis session
        /// </summary>
        public static async Task<int> StartDialysisSessionAsync(
            int sessionId,
            int startedBy)
        {
            try
            {
                await _sqlHelper.BeginTransactionAsync();

                // Update session
                var result = await _sqlHelper.ExecNonQueryAsync(
                    @"UPDATE T_Dialysis_Sessions 
                      SET SessionStatus = 'In Progress',
                          ActualStartTime = NOW(),
                          StartedBy = @startedBy,
                          ModifiedDate = NOW(),
                          ModifiedBy = @startedBy
                      WHERE SessionID = @sessionId",
                    "@sessionId", sessionId,
                    "@startedBy", startedBy
                );

                // Log timeline event
                await InsertTimelineEventAsync(sessionId, "SessionStarted", "Dialysis session started", startedBy);

                await _sqlHelper.CommitAsync();
                return result;
            }
            catch
            {
                await _sqlHelper.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Complete dialysis session
        /// </summary>
        public static async Task<int> CompleteDialysisSessionAsync(
            int sessionId,
            string? postSessionNotes,
            int completedBy)
        {
            try
            {
                await _sqlHelper.BeginTransactionAsync();

                // Calculate duration
                var dtSession = await GetSessionByIdAsync(sessionId);
                if (dtSession.Rows.Count == 0)
                {
                    throw new Exception("Session not found");
                }

                int appointmentId = Convert.ToInt32(dtSession.Rows[0]["AppointmentID"]);

                // Update session
                var result = await _sqlHelper.ExecNonQueryAsync(
                    @"UPDATE T_Dialysis_Sessions 
                      SET SessionStatus = 'Completed',
                          ActualEndTime = NOW(),
                          SessionDuration = TIMESTAMPDIFF(MINUTE, ActualStartTime, NOW()),
                          PostSessionNotes = @postSessionNotes,
                          CompletedBy = @completedBy,
                          ModifiedDate = NOW(),
                          ModifiedBy = @completedBy
                      WHERE SessionID = @sessionId",
                    "@sessionId", sessionId,
                    "@postSessionNotes", postSessionNotes ?? (object)DBNull.Value,
                    "@completedBy", completedBy
                );

                // Update appointment status to Completed
                await AppointmentsDL.UpdateAppointmentStatusAsync(appointmentId, 3, completedBy); // Status 3 = Completed

                // Complete asset assignment
                var assetAssignmentId = dtSession.Rows[0]["AssetAssignmentID"];
                if (assetAssignmentId != DBNull.Value)
                {
                    await AssetAssignmentsDL.UpdateAssignmentStatusAsync(Convert.ToInt32(assetAssignmentId), "Completed");
                }

                // Log timeline event
                await InsertTimelineEventAsync(sessionId, "SessionCompleted", "Dialysis session completed successfully", completedBy);

                await _sqlHelper.CommitAsync();
                return result;
            }
            catch
            {
                await _sqlHelper.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Terminate session (due to complications)
        /// </summary>
        public static async Task<int> TerminateDialysisSessionAsync(
            int sessionId,
            string terminationReason,
            int completedBy)
        {
            try
            {
                await _sqlHelper.BeginTransactionAsync();

                var dtSession = await GetSessionByIdAsync(sessionId);
                if (dtSession.Rows.Count == 0)
                {
                    throw new Exception("Session not found");
                }

                int appointmentId = Convert.ToInt32(dtSession.Rows[0]["AppointmentID"]);

                // Update session
                var result = await _sqlHelper.ExecNonQueryAsync(
                    @"UPDATE T_Dialysis_Sessions 
                      SET SessionStatus = 'Terminated',
                          ActualEndTime = NOW(),
                          SessionDuration = TIMESTAMPDIFF(MINUTE, ActualStartTime, NOW()),
                          TerminationReason = @terminationReason,
                          CompletedBy = @completedBy,
                          ModifiedDate = NOW(),
                          ModifiedBy = @completedBy
                      WHERE SessionID = @sessionId",
                    "@sessionId", sessionId,
                    "@terminationReason", terminationReason,
                    "@completedBy", completedBy
                );

                // Update appointment status
                await AppointmentsDL.UpdateAppointmentStatusAsync(appointmentId, 6, completedBy); // Status 6 = Terminated

                // Complete asset assignment
                var assetAssignmentId = dtSession.Rows[0]["AssetAssignmentID"];
                if (assetAssignmentId != DBNull.Value)
                {
                    await AssetAssignmentsDL.UpdateAssignmentStatusAsync(Convert.ToInt32(assetAssignmentId), "Completed");
                }

                // Log timeline event
                await InsertTimelineEventAsync(sessionId, "SessionTerminated", $"Session terminated: {terminationReason}", completedBy);

                await _sqlHelper.CommitAsync();
                return result;
            }
            catch
            {
                await _sqlHelper.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region Timeline Operations

        /// <summary>
        /// Insert timeline event
        /// </summary>
        public static async Task<int> InsertTimelineEventAsync(
            int sessionId,
            string eventType,
            string eventDescription,
            int performedBy)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                @"INSERT INTO T_Session_Timeline 
                  (SessionID, EventType, EventDescription, EventTime, PerformedBy)
                  VALUES 
                  (@sessionId, @eventType, @eventDescription, NOW(), @performedBy);
                  SELECT LAST_INSERT_ID();",
                "@sessionId", sessionId,
                "@eventType", eventType,
                "@eventDescription", eventDescription,
                "@performedBy", performedBy
            );

            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Get session timeline
        /// </summary>
        public static async Task<DataTable> GetSessionTimelineAsync(int sessionId)
        {
            return await _sqlHelper.ExecDataTableAsync(
                @"SELECT * FROM T_Session_Timeline 
                  WHERE SessionID = @sessionId 
                  ORDER BY EventTime",
                "@sessionId", sessionId
            );
        }

        #endregion


    }
}
