using System.Data;
using DMS.Api.DL;
using DMS.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionNotesController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Get all notes for a session
        /// </summary>
        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetSessionNotes(int sessionId)
        {
            try
            {
                var dt = await SessionNotesDL.GetSessionNotesAsync(sessionId);
                var notes = ConvertDataTableToNoteList(dt);

                return Ok(ApiResponse<List<SessionNoteResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {notes.Count} note(s)",
                    notes
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<SessionNoteResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving session notes: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get latest notes for a session (one per note type)
        /// </summary>
        [HttpGet("session/{sessionId}/latest")]
        public async Task<IActionResult> GetLatestSessionNotes(int sessionId)
        {
            try
            {
                var dt = await SessionNotesDL.GetLatestSessionNotesAsync(sessionId);
                var notes = ConvertDataTableToNoteList(dt);

                return Ok(ApiResponse<List<SessionNoteResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {notes.Count} latest note(s)",
                    notes
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<SessionNoteResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving latest notes: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get notes by type (for trending/history)
        /// </summary>
        [HttpGet("session/{sessionId}/type/{noteTypeId}")]
        public async Task<IActionResult> GetNotesByType(int sessionId, int noteTypeId)
        {
            try
            {
                var dt = await SessionNotesDL.GetNotesByTypeAsync(sessionId, noteTypeId);
                var notes = ConvertDataTableToNoteList(dt);

                return Ok(ApiResponse<List<SessionNoteResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {notes.Count} note(s) for this type",
                    notes
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<SessionNoteResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving notes by type: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get grouped notes by category
        /// </summary>
        [HttpGet("session/{sessionId}/grouped")]
        public async Task<IActionResult> GetGroupedSessionNotes(int sessionId)
        {
            try
            {
                var dt = await SessionNotesDL.GetLatestSessionNotesAsync(sessionId);
                var notes = ConvertDataTableToNoteList(dt);

                var grouped = notes
                    .GroupBy(n => n.Category ?? "Other")
                    .Select(g => new GroupedSessionNotesResponse
                    {
                        Category = g.Key,
                        Notes = g.ToList()
                    })
                    .ToList();

                return Ok(ApiResponse<List<GroupedSessionNotesResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved notes grouped by {grouped.Count} category(ies)",
                    grouped
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<GroupedSessionNotesResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving grouped notes: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Validate if all mandatory notes are recorded
        /// </summary>
        [HttpGet("session/{sessionId}/validate-mandatory")]
        public async Task<IActionResult> ValidateMandatoryNotes(int sessionId)
        {
            try
            {
                bool allRecorded = await SessionNotesDL.AreAllMandatoryNotesRecordedAsync(sessionId);

                var response = new MandatoryNotesValidationResponse
                {
                    AllMandatoryNotesRecorded = allRecorded
                };

                if (!allRecorded)
                {
                    var dtMissing = await SessionNotesDL.GetMissingMandatoryNotesAsync(sessionId);
                    response.MissingMandatoryNotes = new List<SessionNoteTypeResponse>();

                    foreach (DataRow row in dtMissing.Rows)
                    {
                        response.MissingMandatoryNotes.Add(new SessionNoteTypeResponse
                        {
                            NoteTypeID = Convert.ToInt32(row["NoteTypeID"]),
                            NoteTypeName = row["NoteTypeName"]?.ToString() ?? "",
                            NoteTypeCode = row["NoteTypeCode"]?.ToString() ?? "",
                            Category = row["Category"]?.ToString(),
                            IsMandatory = true
                        });
                    }

                    response.ValidationMessage = $"Missing {response.MissingMandatoryNotes.Count} mandatory note(s): " +
                        string.Join(", ", response.MissingMandatoryNotes.Select(n => n.NoteTypeName));
                }
                else
                {
                    response.ValidationMessage = "All mandatory notes recorded";
                }

                return Ok(ApiResponse<MandatoryNotesValidationResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    response.ValidationMessage,
                    response
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<MandatoryNotesValidationResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error validating mandatory notes: {ex.Message}"
                ));
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Add single session note
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddSessionNote([FromBody] AddSessionNoteRequest request)
        {
            try
            {
                // Validate session exists
                var dtSession = await DialysisSessionsDL.GetSessionByIdAsync(request.SessionID);
                if (dtSession.Rows.Count == 0)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Session not found"
                    ));
                }

                string sessionStatus = dtSession.Rows[0]["SessionStatus"]?.ToString() ?? "";
                if (sessionStatus == "Completed" || sessionStatus == "Terminated")
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Cannot add notes to completed or terminated session"
                    ));
                }

                // Validate note value
                if (string.IsNullOrWhiteSpace(request.NoteValue))
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Note value is required"
                    ));
                }

                // Add note
                int sessionNoteId = await SessionNotesDL.AddSessionNoteAsync(
                    request.SessionID,
                    request.NoteTypeID,
                    request.NoteValue,
                    request.RecordedBy,
                    request.AdditionalNotes
                );

                return Ok(ApiResponse<int>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    "Session note added successfully",
                    sessionNoteId
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error adding session note: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Add multiple session notes (batch)
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> AddBulkSessionNotes([FromBody] AddBulkSessionNotesRequest request)
        {
            try
            {
                // Validate session
                var dtSession = await DialysisSessionsDL.GetSessionByIdAsync(request.SessionID);
                if (dtSession.Rows.Count == 0)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Session not found"
                    ));
                }

                if (request.Notes == null || request.Notes.Count == 0)
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "No notes provided"
                    ));
                }

                int addedCount = 0;

                foreach (var note in request.Notes)
                {
                    if (!string.IsNullOrWhiteSpace(note.NoteValue))
                    {
                        await SessionNotesDL.AddSessionNoteAsync(
                            request.SessionID,
                            note.NoteTypeID,
                            note.NoteValue,
                            request.RecordedBy,
                            note.AdditionalNotes
                        );
                        addedCount++;
                    }
                }

                return Ok(ApiResponse<int>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    $"{addedCount} session note(s) added successfully",
                    addedCount
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error adding bulk notes: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Helper Methods

        private SessionNoteResponse ConvertRowToNote(DataRow row)
        {
            return new SessionNoteResponse
            {
                SessionNoteID = Convert.ToInt32(row["SessionNoteID"]),
                SessionID = Convert.ToInt32(row["SessionID"]),
                NoteTypeID = Convert.ToInt32(row["NoteTypeID"]),
                NoteTypeName = row["NoteTypeName"]?.ToString() ?? "",
                NoteTypeCode = row["NoteTypeCode"]?.ToString() ?? "",
                UnitOfMeasure = row["UnitOfMeasure"]?.ToString(),
                Category = row["Category"]?.ToString(),
                IsMandatory = row["IsMandatory"] != DBNull.Value && Convert.ToBoolean(row["IsMandatory"]),
                IsNumeric = row["IsNumeric"] != DBNull.Value && Convert.ToBoolean(row["IsNumeric"]),
                MinimumValue = row["MinimumValue"] != DBNull.Value
                    ? Convert.ToDecimal(row["MinimumValue"])
                    : null,
                MaximumValue = row["MaximumValue"] != DBNull.Value
                    ? Convert.ToDecimal(row["MaximumValue"])
                    : null,
                NoteValue = row["NoteValue"]?.ToString() ?? "",
                NoteTime = Convert.ToDateTime(row["NoteTime"]),
                IsAbnormal = Convert.ToBoolean(row["IsAbnormal"]),
                AlertGenerated = Convert.ToBoolean(row["AlertGenerated"]),
                RecordedBy = Convert.ToInt32(row["RecordedBy"]),
                Notes = row["Notes"]?.ToString()
            };
        }

        private List<SessionNoteResponse> ConvertDataTableToNoteList(DataTable dt)
        {
            var notes = new List<SessionNoteResponse>();
            foreach (DataRow row in dt.Rows)
            {
                notes.Add(ConvertRowToNote(row));
            }
            return notes;
        }

        #endregion
    }
}
