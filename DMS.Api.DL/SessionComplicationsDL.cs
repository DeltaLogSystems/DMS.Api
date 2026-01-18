using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class SessionComplicationsDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region GET Operations

        /// <summary>
        /// Get all complications for a session
        /// </summary>
        public static async Task<DataTable> GetSessionComplicationsAsync(int sessionId)
        {
            return await _sqlHelper.ExecDataTableAsync(
                @"SELECT * FROM T_Session_Complications 
                  WHERE SessionID = @sessionId 
                  ORDER BY OccurredAt DESC",
                "@sessionId", sessionId
            );
        }

        /// <summary>
        /// Get unresolved complications
        /// </summary>
        public static async Task<DataTable> GetUnresolvedComplicationsAsync(int sessionId)
        {
            return await _sqlHelper.ExecDataTableAsync(
                @"SELECT * FROM T_Session_Complications 
                  WHERE SessionID = @sessionId AND ResolvedAt IS NULL
                  ORDER BY OccurredAt DESC",
                "@sessionId", sessionId
            );
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Report complication
        /// </summary>
        public static async Task<int> ReportComplicationAsync(
            int sessionId,
            string complicationType,
            string? severity,
            string? description,
            string? actionTaken,
            int reportedBy)
        {
            try
            {
                await _sqlHelper.BeginTransactionAsync();

                var result = await _sqlHelper.ExecScalarAsync(
                    @"INSERT INTO T_Session_Complications 
              (SessionID, ComplicationType, Severity, OccurredAt, Description, ActionTaken, ReportedBy)
              VALUES 
              (@sessionId, @complicationType, @severity, NOW(), @description, @actionTaken, @reportedBy);
              SELECT LAST_INSERT_ID();",
                    "@sessionId", sessionId,
                    "@complicationType", complicationType,
                    "@severity", severity ?? (object)DBNull.Value,
                    "@description", description ?? (object)DBNull.Value,
                    "@actionTaken", actionTaken ?? (object)DBNull.Value,
                    "@reportedBy", reportedBy
                );

                int complicationId = Convert.ToInt32(result);

                // Log timeline event - NOW PUBLIC
                string eventDescription = !string.IsNullOrEmpty(severity)
                    ? $"⚠️ Complication: {complicationType} ({severity})"
                    : $"⚠️ Complication: {complicationType}";

                await DialysisSessionsDL.InsertTimelineEventAsync(
                    sessionId,
                    "ComplicationReported",
                    eventDescription,
                    reportedBy
                );

                await _sqlHelper.CommitAsync();
                return complicationId;
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
        /// Resolve complication
        /// </summary>
        public static async Task<int> ResolveComplicationAsync(
            int complicationId,
            string? resolutionNotes)
        {
            return await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE T_Session_Complications 
                  SET ResolvedAt = NOW(),
                      ActionTaken = CONCAT(IFNULL(ActionTaken, ''), '\nResolved: ', @resolutionNotes)
                  WHERE ComplicationID = @complicationId",
                "@complicationId", complicationId,
                "@resolutionNotes", resolutionNotes ?? (object)DBNull.Value
            );
        }

        #endregion
    }
}
