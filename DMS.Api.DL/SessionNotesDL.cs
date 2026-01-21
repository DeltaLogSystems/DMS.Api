using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class SessionNotesDL
    {
        // Removed static shared MySQLHelper to fix concurrency issues
        // Each method creates its own instance for thread-safety

        #region GET Operations

        /// <summary>
        /// Get all notes for a session
        /// </summary>
        public static async Task<DataTable> GetSessionNotesAsync(int sessionId)
        {
            using var sqlHelper = new MySQLHelper();
            return await sqlHelper.ExecDataTableAsync(
                @"SELECT sn.*,
                         nt.NoteTypeName, nt.NoteTypeCode, nt.UnitOfMeasure,
                         nt.IsMandatory, nt.IsNumeric, nt.MinimumValue, nt.MaximumValue,
                         nt.Category
                  FROM T_Session_Notes sn
                  INNER JOIN M_Session_Note_Types nt ON sn.NoteTypeID = nt.NoteTypeID
                  WHERE sn.SessionID = @sessionId
                  ORDER BY sn.NoteTime",
                "@sessionId", sessionId
            );
        }

        /// <summary>
        /// Get latest notes for a session (one per note type)
        /// </summary>
        public static async Task<DataTable> GetLatestSessionNotesAsync(int sessionId)
        {
            using var sqlHelper = new MySQLHelper();
            return await sqlHelper.ExecDataTableAsync(
                @"SELECT sn.*,
                         nt.NoteTypeName, nt.NoteTypeCode, nt.UnitOfMeasure,
                         nt.Category
                  FROM T_Session_Notes sn
                  INNER JOIN M_Session_Note_Types nt ON sn.NoteTypeID = nt.NoteTypeID
                  WHERE sn.SessionID = @sessionId
                    AND sn.SessionNoteID IN (
                      SELECT MAX(SessionNoteID)
                      FROM T_Session_Notes
                      WHERE SessionID = @sessionId
                      GROUP BY NoteTypeID
                    )
                  ORDER BY nt.DisplayOrder",
                "@sessionId", sessionId
            );
        }

        /// <summary>
        /// Get notes by type (for trending)
        /// </summary>
        public static async Task<DataTable> GetNotesByTypeAsync(int sessionId, int noteTypeId)
        {
            using var sqlHelper = new MySQLHelper();
            return await sqlHelper.ExecDataTableAsync(
                @"SELECT sn.*, nt.NoteTypeName, nt.UnitOfMeasure
                  FROM T_Session_Notes sn
                  INNER JOIN M_Session_Note_Types nt ON sn.NoteTypeID = nt.NoteTypeID
                  WHERE sn.SessionID = @sessionId AND sn.NoteTypeID = @noteTypeId
                  ORDER BY sn.NoteTime",
                "@sessionId", sessionId,
                "@noteTypeId", noteTypeId
            );
        }

        /// <summary>
        /// Check if all mandatory notes are recorded
        /// </summary>
        public static async Task<bool> AreAllMandatoryNotesRecordedAsync(int sessionId)
        {
            using var sqlHelper = new MySQLHelper();
            var result = await sqlHelper.ExecScalarAsync(
                @"SELECT COUNT(*)
                  FROM M_Session_Note_Types nt
                  WHERE nt.IsMandatory = 1 AND nt.IsActive = 1
                    AND NOT EXISTS (
                      SELECT 1 FROM T_Session_Notes sn
                      WHERE sn.SessionID = @sessionId AND sn.NoteTypeID = nt.NoteTypeID
                    )",
                "@sessionId", sessionId
            );

            return Convert.ToInt32(result) == 0;
        }

        /// <summary>
        /// Get missing mandatory notes
        /// </summary>
        public static async Task<DataTable> GetMissingMandatoryNotesAsync(int sessionId)
        {
            using var sqlHelper = new MySQLHelper();
            return await sqlHelper.ExecDataTableAsync(
                @"SELECT nt.*
                  FROM M_Session_Note_Types nt
                  WHERE nt.IsMandatory = 1 AND nt.IsActive = 1
                    AND NOT EXISTS (
                      SELECT 1 FROM T_Session_Notes sn
                      WHERE sn.SessionID = @sessionId AND sn.NoteTypeID = nt.NoteTypeID
                    )
                  ORDER BY nt.DisplayOrder",
                "@sessionId", sessionId
            );
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Add session note
        /// </summary>
        public static async Task<int> AddSessionNoteAsync(
            int sessionId,
            int noteTypeId,
            string noteValue,
            int recordedBy,
            string? additionalNotes = null)
        {
            using var sqlHelper = new MySQLHelper();
            try
            {
                await sqlHelper.BeginTransactionAsync();

                // Get note type details
                var dtNoteType = await SessionNoteTypesDL.GetNoteTypeByIdAsync(noteTypeId);
                if (dtNoteType.Rows.Count == 0)
                {
                    throw new Exception("Note type not found");
                }

                bool isNumeric = Convert.ToBoolean(dtNoteType.Rows[0]["IsNumeric"]);
                bool isAbnormal = false;
                bool alertGenerated = false;

                // Validate numeric values
                if (isNumeric && !string.IsNullOrEmpty(noteValue))
                {
                    if (decimal.TryParse(noteValue, out decimal numericValue))
                    {
                        var minValue = dtNoteType.Rows[0]["MinimumValue"];
                        var maxValue = dtNoteType.Rows[0]["MaximumValue"];

                        if (minValue != DBNull.Value && numericValue < Convert.ToDecimal(minValue))
                        {
                            isAbnormal = true;
                            alertGenerated = true;
                        }

                        if (maxValue != DBNull.Value && numericValue > Convert.ToDecimal(maxValue))
                        {
                            isAbnormal = true;
                            alertGenerated = true;
                        }
                    }
                }

                // Insert note
                var result = await sqlHelper.ExecScalarAsync(
                    @"INSERT INTO T_Session_Notes
              (SessionID, NoteTypeID, NoteValue, NoteTime, IsAbnormal, AlertGenerated, RecordedBy, Notes)
              VALUES
              (@sessionId, @noteTypeId, @noteValue, NOW(), @isAbnormal, @alertGenerated, @recordedBy, @notes);
              SELECT LAST_INSERT_ID();",
                    "@sessionId", sessionId,
                    "@noteTypeId", noteTypeId,
                    "@noteValue", noteValue,
                    "@isAbnormal", isAbnormal,
                    "@alertGenerated", alertGenerated,
                    "@recordedBy", recordedBy,
                    "@notes", additionalNotes ?? (object)DBNull.Value
                );

                int sessionNoteId = Convert.ToInt32(result);

                // Get note type name for timeline
                string noteTypeName = dtNoteType.Rows[0]["NoteTypeName"]?.ToString() ?? "";
                string unitOfMeasure = dtNoteType.Rows[0]["UnitOfMeasure"]?.ToString() ?? "";
                string displayValue = !string.IsNullOrEmpty(unitOfMeasure)
                    ? $"{noteValue} {unitOfMeasure}"
                    : noteValue;

                // Log timeline event
                string eventDescription = isAbnormal
                    ? $"⚠️ {noteTypeName}: {displayValue} (Abnormal)"
                    : $"{noteTypeName}: {displayValue}";

                await DialysisSessionsDL.InsertTimelineEventAsync(
                    sqlHelper,
                    sessionId,
                    "NoteAdded",
                    eventDescription,
                    recordedBy
                );

                await sqlHelper.CommitAsync();
                return sessionNoteId;
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
