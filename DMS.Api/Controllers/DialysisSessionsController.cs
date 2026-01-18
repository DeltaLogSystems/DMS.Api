using System.Data;
using DMS.Api.DL;
using DMS.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DialysisSessionsController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Get all sessions with filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllSessions(
            [FromQuery] int? centerId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] DateTime? sessionDate = null,
            [FromQuery] string? sessionStatus = null)
        {
            try
            {
                var dt = await DialysisSessionsDL.GetAllSessionsAsync(centerId, patientId, sessionDate, sessionStatus);
                var sessions = ConvertDataTableToSessionList(dt);

                return Ok(ApiResponse<List<DialysisSessionResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {sessions.Count} session(s)",
                    sessions
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<DialysisSessionResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving sessions: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get session by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSessionById(int id)
        {
            try
            {
                var dt = await DialysisSessionsDL.GetSessionByIdAsync(id);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<DialysisSessionResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Session not found"
                    ));
                }

                var session = ConvertRowToSession(dt.Rows[0]);

                return Ok(ApiResponse<DialysisSessionResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Session retrieved successfully",
                    session
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<DialysisSessionResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving session: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get session by appointment ID
        /// </summary>
        [HttpGet("by-appointment/{appointmentId}")]
        public async Task<IActionResult> GetSessionByAppointment(int appointmentId)
        {
            try
            {
                var dt = await DialysisSessionsDL.GetSessionByAppointmentAsync(appointmentId);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<DialysisSessionResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Session not found for this appointment"
                    ));
                }

                var session = ConvertRowToSession(dt.Rows[0]);

                return Ok(ApiResponse<DialysisSessionResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Session retrieved successfully",
                    session
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<DialysisSessionResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving session: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get active sessions (In Progress)
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSessions([FromQuery] int? centerId = null)
        {
            try
            {
                var dt = await DialysisSessionsDL.GetActiveSessionsAsync(centerId);
                var sessions = ConvertDataTableToSessionList(dt);

                return Ok(ApiResponse<List<DialysisSessionResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {sessions.Count} active session(s)",
                    sessions
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<DialysisSessionResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving active sessions: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get session timeline
        /// </summary>
        [HttpGet("{id}/timeline")]
        public async Task<IActionResult> GetSessionTimeline(int id)
        {
            try
            {
                var dt = await DialysisSessionsDL.GetSessionTimelineAsync(id);
                var timeline = ConvertDataTableToTimelineList(dt);

                return Ok(ApiResponse<List<SessionTimelineResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {timeline.Count} timeline event(s)",
                    timeline
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<SessionTimelineResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving timeline: {ex.Message}"
                ));
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Create new dialysis session
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            try
            {
                // Validation
                if (request.AppointmentID <= 0)
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Appointment ID is required"
                    ));
                }

                // Check if session already exists for this appointment
                var dtExisting = await DialysisSessionsDL.GetSessionByAppointmentAsync(request.AppointmentID);
                if (dtExisting.Rows.Count > 0)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Session already exists for this appointment"
                    ));
                }

                // Create session
                int sessionId = await DialysisSessionsDL.CreateSessionAsync(
                    request.AppointmentID,
                    request.PatientID,
                    request.CenterID,
                    request.SessionDate,
                    request.ScheduledStartTime,
                    request.DialysisType,
                    request.PreSessionNotes,
                    request.CreatedBy
                );

                var dt = await DialysisSessionsDL.GetSessionByIdAsync(sessionId);
                var session = ConvertRowToSession(dt.Rows[0]);

                return Ok(ApiResponse<DialysisSessionResponse>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    "Dialysis session created successfully. Appointment status updated to 'In Progress'.",
                    session
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<DialysisSessionResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error creating session: {ex.Message}"
                ));
            }
        }

        #endregion

        #region PUT Endpoints

        /// <summary>
        /// Assign machine to session
        /// </summary>
        [HttpPut("{id}/assign-machine")]
        public async Task<IActionResult> AssignMachine(int id, [FromBody] AssignMachineRequest request)
        {
            try
            {
                // Validate session exists
                var dtSession = await DialysisSessionsDL.GetSessionByIdAsync(id);
                if (dtSession.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Session not found"
                    ));
                }

                // Check if machine is available
                var dtActiveSessions = await DialysisSessionsDL.GetActiveSessionsAsync(null);
                bool machineInUse = false;
                string currentPatient = "";

                foreach (DataRow row in dtActiveSessions.Rows)
                {
                    if (row["AssetID"] != DBNull.Value && Convert.ToInt32(row["AssetID"]) == request.AssetID)
                    {
                        machineInUse = true;
                        currentPatient = row["PatientName"]?.ToString() ?? "";
                        break;
                    }
                }

                if (machineInUse)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        $"Machine is already in use by patient: {currentPatient}"
                    ));
                }

                // Create asset assignment
                int assetAssignmentId = await AssetAssignmentsDL.CreateAssignmentAsync(
                    request.AssetID,
                    Convert.ToInt32(dtSession.Rows[0]["AppointmentID"]),
                    DateTime.Now.Date,
                    TimeSpan.FromHours(DateTime.Now.Hour).Add(TimeSpan.FromMinutes(DateTime.Now.Minute)),
                    request.ModifiedBy,     // createdBy parameter
                    "Active",               // status parameter
                    request.ModifiedBy      // modifiedBy parameter (can be same as createdBy)
                );


                // Assign to session
                int result = await DialysisSessionsDL.AssignMachineToSessionAsync(
                    id,
                    request.AssetID,
                    assetAssignmentId,
                    request.ModifiedBy
                );

                if (result > 0)
                {
                    var dt = await DialysisSessionsDL.GetSessionByIdAsync(id);
                    var session = ConvertRowToSession(dt.Rows[0]);

                    return Ok(ApiResponse<DialysisSessionResponse>.SuccessResponse(
                        ResponseStatus.DataUpdated,
                        "Machine assigned to session successfully",
                        session
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to assign machine"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error assigning machine: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Start dialysis session
        /// </summary>
        [HttpPut("{id}/start")]
        public async Task<IActionResult> StartSession(int id, [FromBody] StartDialysisRequest request)
        {
            try
            {
                // Validate session exists
                var dtSession = await DialysisSessionsDL.GetSessionByIdAsync(id);
                if (dtSession.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Session not found"
                    ));
                }

                string currentStatus = dtSession.Rows[0]["SessionStatus"]?.ToString() ?? "";
                if (currentStatus != "Not Started")
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        $"Cannot start session. Current status: {currentStatus}"
                    ));
                }

                // Check if machine is assigned
                if (dtSession.Rows[0]["AssetID"] == DBNull.Value)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Please assign a dialysis machine before starting the session"
                    ));
                }

                // Check if inventory items are selected
                var dtInventory = await SessionInventoryDL.GetSessionInventoryAsync(id);
                if (dtInventory.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Please select inventory items before starting the session"
                    ));
                }

                // Start session
                int result = await DialysisSessionsDL.StartDialysisSessionAsync(id, request.StartedBy);

                if (result > 0)
                {
                    var dt = await DialysisSessionsDL.GetSessionByIdAsync(id);
                    var session = ConvertRowToSession(dt.Rows[0]);

                    return Ok(ApiResponse<DialysisSessionResponse>.SuccessResponse(
                        ResponseStatus.DataUpdated,
                        "Dialysis session started successfully",
                        session
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to start session"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error starting session: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Complete dialysis session
        /// </summary>
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteSession(int id, [FromBody] CompleteSessionRequest request)
        {
            try
            {
                // Validate session exists
                var dtSession = await DialysisSessionsDL.GetSessionByIdAsync(id);
                if (dtSession.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Session not found"
                    ));
                }

                string currentStatus = dtSession.Rows[0]["SessionStatus"]?.ToString() ?? "";
                if (currentStatus != "In Progress")
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        $"Cannot complete session. Current status: {currentStatus}"
                    ));
                }

                // Check if all mandatory notes are recorded
                bool allMandatoryRecorded = await SessionNotesDL.AreAllMandatoryNotesRecordedAsync(id);
                if (!allMandatoryRecorded)
                {
                    var dtMissing = await SessionNotesDL.GetMissingMandatoryNotesAsync(id);
                    var missingNotes = new List<string>();
                    foreach (DataRow row in dtMissing.Rows)
                    {
                        missingNotes.Add(row["NoteTypeName"]?.ToString() ?? "");
                    }

                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        $"Missing mandatory notes: {string.Join(", ", missingNotes)}"
                    ));
                }

                // Complete session
                int result = await DialysisSessionsDL.CompleteDialysisSessionAsync(
                    id,
                    request.PostSessionNotes,
                    request.CompletedBy
                );

                if (result > 0)
                {
                    var dt = await DialysisSessionsDL.GetSessionByIdAsync(id);
                    var session = ConvertRowToSession(dt.Rows[0]);

                    return Ok(ApiResponse<DialysisSessionResponse>.SuccessResponse(
                        ResponseStatus.DataUpdated,
                        "Dialysis session completed successfully. Appointment status updated to 'Completed'.",
                        session
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to complete session"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error completing session: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Terminate dialysis session (due to complications)
        /// </summary>
        [HttpPut("{id}/terminate")]
        public async Task<IActionResult> TerminateSession(int id, [FromBody] TerminateSessionRequest request)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(request.TerminationReason))
                {
                    return BadRequest(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Termination reason is required"
                    ));
                }

                var dtSession = await DialysisSessionsDL.GetSessionByIdAsync(id);
                if (dtSession.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Session not found"
                    ));
                }

                string currentStatus = dtSession.Rows[0]["SessionStatus"]?.ToString() ?? "";
                if (currentStatus != "In Progress")
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        $"Cannot terminate session. Current status: {currentStatus}"
                    ));
                }

                // Terminate session
                int result = await DialysisSessionsDL.TerminateDialysisSessionAsync(
                    id,
                    request.TerminationReason,
                    request.CompletedBy
                );

                if (result > 0)
                {
                    var dt = await DialysisSessionsDL.GetSessionByIdAsync(id);
                    var session = ConvertRowToSession(dt.Rows[0]);

                    return Ok(ApiResponse<DialysisSessionResponse>.SuccessResponse(
                        ResponseStatus.DataUpdated,
                        "Session terminated. Appointment status updated to 'Terminated'.",
                        session
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to terminate session"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error terminating session: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Helper Methods

        private DialysisSessionResponse ConvertRowToSession(DataRow row)
        {
            return new DialysisSessionResponse
            {
                SessionID = Convert.ToInt32(row["SessionID"]),
                SessionCode = row["SessionCode"]?.ToString() ?? "",
                AppointmentID = Convert.ToInt32(row["AppointmentID"]),
                AppointmentCode = row["AppointmentCode"]?.ToString() ?? "",
                AppointmentDate = row["AppointmentDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["AppointmentDate"])
                    : DateTime.MinValue,
                PatientID = Convert.ToInt32(row["PatientID"]),
                PatientCode = row["PatientCode"]?.ToString() ?? "",
                PatientName = row["PatientName"]?.ToString() ?? "",
                ContactNumber = row["ContactNumber"]?.ToString(),
                CenterID = Convert.ToInt32(row["CenterID"]),
                CenterName = row["CenterName"]?.ToString() ?? "",
                AssetID = row["AssetID"] != DBNull.Value
                    ? Convert.ToInt32(row["AssetID"])
                    : null,
                AssetCode = row["AssetCode"]?.ToString(),
                AssetName = row["AssetName"]?.ToString(),
                AssetAssignmentID = row["AssetAssignmentID"] != DBNull.Value
                    ? Convert.ToInt32(row["AssetAssignmentID"])
                    : null,
                SessionStatus = row["SessionStatus"]?.ToString() ?? "Not Started",
                SessionDate = Convert.ToDateTime(row["SessionDate"]),
                ScheduledStartTime = row["ScheduledStartTime"] != DBNull.Value
                    ? TimeSpan.Parse(row["ScheduledStartTime"].ToString() ?? "00:00")
                    : null,
                ActualStartTime = row["ActualStartTime"] != DBNull.Value
                    ? Convert.ToDateTime(row["ActualStartTime"])
                    : null,
                ActualEndTime = row["ActualEndTime"] != DBNull.Value
                    ? Convert.ToDateTime(row["ActualEndTime"])
                    : null,
                SessionDuration = row["SessionDuration"] != DBNull.Value
                    ? Convert.ToInt32(row["SessionDuration"])
                    : null,
                DialysisType = row["DialysisType"]?.ToString(),
                StartedBy = row["StartedBy"] != DBNull.Value
                    ? Convert.ToInt32(row["StartedBy"])
                    : null,
                CompletedBy = row["CompletedBy"] != DBNull.Value
                    ? Convert.ToInt32(row["CompletedBy"])
                    : null,
                PreSessionNotes = row["PreSessionNotes"]?.ToString(),
                PostSessionNotes = row["PostSessionNotes"]?.ToString(),
                TerminationReason = row["TerminationReason"]?.ToString(),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private List<DialysisSessionResponse> ConvertDataTableToSessionList(DataTable dt)
        {
            var sessions = new List<DialysisSessionResponse>();
            foreach (DataRow row in dt.Rows)
            {
                sessions.Add(ConvertRowToSession(row));
            }
            return sessions;
        }

        private SessionTimelineResponse ConvertRowToTimeline(DataRow row)
        {
            return new SessionTimelineResponse
            {
                TimelineID = Convert.ToInt32(row["TimelineID"]),
                SessionID = Convert.ToInt32(row["SessionID"]),
                EventType = row["EventType"]?.ToString() ?? "",
                EventDescription = row["EventDescription"]?.ToString() ?? "",
                EventTime = Convert.ToDateTime(row["EventTime"]),
                PerformedBy = Convert.ToInt32(row["PerformedBy"])
            };
        }

        private List<SessionTimelineResponse> ConvertDataTableToTimelineList(DataTable dt)
        {
            var timeline = new List<SessionTimelineResponse>();
            foreach (DataRow row in dt.Rows)
            {
                timeline.Add(ConvertRowToTimeline(row));
            }
            return timeline;
        }

        #endregion
    }
}
