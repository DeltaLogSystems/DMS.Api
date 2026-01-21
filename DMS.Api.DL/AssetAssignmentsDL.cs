using System.Data;

namespace DMS.Api.DL
{
    public static class AssetAssignmentsDL
    {
        // Removed static shared MySQLHelper to fix concurrency issues
        // Each method creates its own instance for thread-safety

        #region GET Operations

        /// <summary>
        /// Get all assignments
        /// </summary>
        public static async Task<DataTable> GetAllAssignmentsAsync(int? assetId = null, int? centerId = null)
        {
            using var sqlHelper = new MySQLHelper();
            string query = @"SELECT aa.*,
                                    a.AssetCode, a.AssetName, a.CenterID,
                                    apt.AppointmentID, apt.AppointmentDate,
                                    p.PatientCode, p.PatientName
                             FROM T_Asset_Assignments aa
                             INNER JOIN M_Assets a ON aa.AssetID = a.AssetID
                             INNER JOIN T_Appointments apt ON aa.AppointmentID = apt.AppointmentID
                             INNER JOIN M_Patients p ON apt.PatientID = p.PatientID
                             WHERE 1=1";

            var parameters = new List<object>();

            if (assetId.HasValue)
            {
                query += " AND aa.AssetID = @assetId";
                parameters.Add("@assetId");
                parameters.Add(assetId.Value);
            }

            if (centerId.HasValue)
            {
                query += " AND a.CenterID = @centerId";
                parameters.Add("@centerId");
                parameters.Add(centerId.Value);
            }

            query += " ORDER BY aa.AssignedDate DESC, aa.AssignedTime DESC";

            return await sqlHelper.ExecDataTableAsync(query, parameters.ToArray());
        }

        /// <summary>
        /// Get assignments by date
        /// </summary>
        public static async Task<DataTable> GetAssignmentsByDateAsync(int centerId, DateTime date)
        {
            using var sqlHelper = new MySQLHelper();
            return await sqlHelper.ExecDataTableAsync(
                @"SELECT aa.*,
                         a.AssetCode, a.AssetName,
                         apt.AppointmentDate,
                         p.PatientCode, p.PatientName
                  FROM T_Asset_Assignments aa
                  INNER JOIN M_Assets a ON aa.AssetID = a.AssetID
                  INNER JOIN T_Appointments apt ON aa.AppointmentID = apt.AppointmentID
                  INNER JOIN M_Patients p ON apt.PatientID = p.PatientID
                  WHERE a.CenterID = @centerId
                  AND aa.AssignedDate = @date
                  AND aa.Status = 'Active'
                  ORDER BY aa.AssignedTime",
                "@centerId", centerId,
                "@date", date.Date
            );
        }

        /// <summary>
        /// Check if asset is available for specific time
        /// </summary>
        public static async Task<bool> IsAssetAvailableAsync(
            int assetId,
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            using var sqlHelper = new MySQLHelper();
            var result = await sqlHelper.ExecScalarAsync(
                @"SELECT COUNT(*) FROM T_Asset_Assignments
                  WHERE AssetID = @assetId
                  AND AssignedDate = @date
                  AND Status = 'Active'
                  AND (
                      (@startTime >= AssignedTime AND @startTime < ADDTIME(AssignedTime, SEC_TO_TIME(SessionDuration * 60)))
                      OR (@endTime > AssignedTime AND @endTime <= ADDTIME(AssignedTime, SEC_TO_TIME(SessionDuration * 60)))
                      OR (@startTime <= AssignedTime AND @endTime >= ADDTIME(AssignedTime, SEC_TO_TIME(SessionDuration * 60)))
                  )",
                "@assetId", assetId,
                "@date", date.Date,
                "@startTime", startTime,
                "@endTime", endTime
            );

            return Convert.ToInt32(result) == 0;
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Assign asset to appointment
        /// </summary>
        public static async Task<int> CreateAssignmentAsync(
            int assetId,
            int appointmentId,
            DateTime assignedDate,
            TimeSpan assignedTime,
            int sessionDuration,
            string? notes,
            int createdBy)
        {
            using var sqlHelper = new MySQLHelper();
            var result = await sqlHelper.ExecScalarAsync(
                @"INSERT INTO T_Asset_Assignments
                  (AssetID, AppointmentID, AssignedDate, AssignedTime,
                   SessionDuration, Status, Notes, CreatedDate, CreatedBy)
                  VALUES
                  (@assetId, @appointmentId, @assignedDate, @assignedTime,
                   @sessionDuration, 'Active', @notes, NOW(), @createdBy);
                  SELECT LAST_INSERT_ID();",
                "@assetId", assetId,
                "@appointmentId", appointmentId,
                "@assignedDate", assignedDate.Date,
                "@assignedTime", assignedTime,
                "@sessionDuration", sessionDuration,
                "@notes", notes ?? (object)DBNull.Value,
                "@createdBy", createdBy
            );

            return Convert.ToInt32(result);
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update assignment status
        /// </summary>
        public static async Task<int> UpdateAssignmentStatusAsync(int assignmentId, string status)
        {
            using var sqlHelper = new MySQLHelper();
            return await sqlHelper.ExecNonQueryAsync(
                @"UPDATE T_Asset_Assignments
                  SET Status = @status
                  WHERE AssignmentID = @assignmentId",
                "@assignmentId", assignmentId,
                "@status", status
            );
        }

        /// <summary>
        /// Cancel assignment (free up asset)
        /// </summary>
        public static async Task<int> CancelAssignmentAsync(int assignmentId)
        {
            return await UpdateAssignmentStatusAsync(assignmentId, "Cancelled");
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Delete assignment
        /// </summary>
        public static async Task<int> DeleteAssignmentAsync(int assignmentId)
        {
            using var sqlHelper = new MySQLHelper();
            return await sqlHelper.ExecNonQueryAsync(
                "DELETE FROM T_Asset_Assignments WHERE AssignmentID = @assignmentId",
                "@assignmentId", assignmentId
            );
        }

        #endregion
    }
}
