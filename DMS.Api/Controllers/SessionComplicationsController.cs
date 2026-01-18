using System.Data;
using DMS.Api.DL;
using DMS.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionComplicationsController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Get all complications for a session
        /// </summary>
        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetSessionComplications(int sessionId)
        {
            try
            {
                var dt = await SessionComplicationsDL.GetSessionComplicationsAsync(sessionId);
                var complications = ConvertDataTableToComplicationList(dt);

                return Ok(ApiResponse<List<SessionComplicationResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {complications.Count} complication(s)",
                    complications
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<SessionComplicationResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving complications: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get unresolved complications for a session
        /// </summary>
        [HttpGet("session/{sessionId}/unresolved")]
        public async Task<IActionResult> GetUnresolvedComplications(int sessionId)
        {
            try
            {
                var dt = await SessionComplicationsDL.GetUnresolvedComplicationsAsync(sessionId);
                var complications = ConvertDataTableToComplicationList(dt);

                return Ok(ApiResponse<List<SessionComplicationResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {complications.Count} unresolved complication(s)",
                    complications
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<SessionComplicationResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving unresolved complications: {ex.Message}"
                ));
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Report complication
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ReportComplication([FromBody] ReportComplicationRequest request)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(request.ComplicationType))
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Complication type is required"
                    ));
                }

                // Validate session exists and is in progress
                var dtSession = await DialysisSessionsDL.GetSessionByIdAsync(request.SessionID);
                if (dtSession.Rows.Count == 0)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Session not found"
                    ));
                }

                string sessionStatus = dtSession.Rows[0]["SessionStatus"]?.ToString() ?? "";
                if (sessionStatus != "In Progress")
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        $"Cannot report complication. Session status: {sessionStatus}"
                    ));
                }

                // Report complication
                int complicationId = await SessionComplicationsDL.ReportComplicationAsync(
                    request.SessionID,
                    request.ComplicationType,
                    request.Severity,
                    request.Description,
                    request.ActionTaken,
                    request.ReportedBy
                );

                return Ok(ApiResponse<int>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    "Complication reported successfully",
                    complicationId
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error reporting complication: {ex.Message}"
                ));
            }
        }

        #endregion

        #region PUT Endpoints

        /// <summary>
        /// Resolve complication
        /// </summary>
        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> ResolveComplication(int id, [FromBody] ResolveComplicationRequest request)
        {
            try
            {
                int result = await SessionComplicationsDL.ResolveComplicationAsync(
                    id,
                    request.ResolutionNotes
                );

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Complication resolved successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.NotFound,
                    "Complication not found"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error resolving complication: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Helper Methods

        private SessionComplicationResponse ConvertRowToComplication(DataRow row)
        {
            return new SessionComplicationResponse
            {
                ComplicationID = Convert.ToInt32(row["ComplicationID"]),
                SessionID = Convert.ToInt32(row["SessionID"]),
                ComplicationType = row["ComplicationType"]?.ToString() ?? "",
                Severity = row["Severity"]?.ToString(),
                OccurredAt = Convert.ToDateTime(row["OccurredAt"]),
                ResolvedAt = row["ResolvedAt"] != DBNull.Value
                    ? Convert.ToDateTime(row["ResolvedAt"])
                    : null,
                Description = row["Description"]?.ToString(),
                ActionTaken = row["ActionTaken"]?.ToString(),
                ReportedBy = Convert.ToInt32(row["ReportedBy"])
            };
        }

        private List<SessionComplicationResponse> ConvertDataTableToComplicationList(DataTable dt)
        {
            var complications = new List<SessionComplicationResponse>();
            foreach (DataRow row in dt.Rows)
            {
                complications.Add(ConvertRowToComplication(row));
            }
            return complications;
        }

        #endregion
    }
}
