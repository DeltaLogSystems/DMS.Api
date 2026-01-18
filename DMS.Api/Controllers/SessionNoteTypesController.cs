using System.Data;
using DMS.Api.DL;
using DMS.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/masters/session-note-types")]
    public class SessionNoteTypesController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Get all session note types
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllNoteTypes(
            [FromQuery] string? category = null,
            [FromQuery] bool? mandatoryOnly = null,
            [FromQuery] bool activeOnly = true)
        {
            try
            {
                var dt = await SessionNoteTypesDL.GetAllNoteTypesAsync(category, mandatoryOnly, activeOnly);
                var noteTypes = ConvertDataTableToNoteTypeList(dt);

                return Ok(ApiResponse<List<SessionNoteTypeResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {noteTypes.Count} note type(s)",
                    noteTypes
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<SessionNoteTypeResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving note types: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get note type by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNoteTypeById(int id)
        {
            try
            {
                var dt = await SessionNoteTypesDL.GetNoteTypeByIdAsync(id);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<SessionNoteTypeResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Note type not found"
                    ));
                }

                var noteType = ConvertRowToNoteType(dt.Rows[0]);

                return Ok(ApiResponse<SessionNoteTypeResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Note type retrieved successfully",
                    noteType
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SessionNoteTypeResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving note type: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get mandatory note types
        /// </summary>
        [HttpGet("mandatory")]
        public async Task<IActionResult> GetMandatoryNoteTypes()
        {
            try
            {
                var dt = await SessionNoteTypesDL.GetMandatoryNoteTypesAsync();
                var noteTypes = ConvertDataTableToNoteTypeList(dt);

                return Ok(ApiResponse<List<SessionNoteTypeResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {noteTypes.Count} mandatory note type(s)",
                    noteTypes
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<SessionNoteTypeResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving mandatory note types: {ex.Message}"
                ));
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Create session note type
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateNoteType([FromBody] SessionNoteTypeRequest request)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(request.NoteTypeName))
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Note type name is required"
                    ));
                }

                if (string.IsNullOrWhiteSpace(request.NoteTypeCode))
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Note type code is required"
                    ));
                }

                // Create note type
                int noteTypeId = await SessionNoteTypesDL.CreateNoteTypeAsync(
                    request.NoteTypeName,
                    request.NoteTypeCode,
                    request.Description,
                    request.UnitOfMeasure,
                    request.IsMandatory,
                    request.IsNumeric,
                    request.MinimumValue,
                    request.MaximumValue,
                    request.DefaultValue,
                    request.DisplayOrder,
                    request.Category,
                    request.CreatedBy
                );

                var dt = await SessionNoteTypesDL.GetNoteTypeByIdAsync(noteTypeId);
                var noteType = ConvertRowToNoteType(dt.Rows[0]);

                return Ok(ApiResponse<SessionNoteTypeResponse>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    "Note type created successfully",
                    noteType
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SessionNoteTypeResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error creating note type: {ex.Message}"
                ));
            }
        }

        #endregion

        #region PUT Endpoints

        /// <summary>
        /// Update session note type
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNoteType(int id, [FromBody] SessionNoteTypeRequest request)
        {
            try
            {
                var dtExisting = await SessionNoteTypesDL.GetNoteTypeByIdAsync(id);
                if (dtExisting.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Note type not found"
                    ));
                }

                int result = await SessionNoteTypesDL.UpdateNoteTypeAsync(
                    id,
                    request.NoteTypeName,
                    request.Description,
                    request.UnitOfMeasure,
                    request.IsMandatory,
                    request.IsNumeric,
                    request.MinimumValue,
                    request.MaximumValue,
                    request.DefaultValue,
                    request.DisplayOrder,
                    request.Category,
                    request.CreatedBy
                );

                if (result > 0)
                {
                    var dt = await SessionNoteTypesDL.GetNoteTypeByIdAsync(id);
                    var noteType = ConvertRowToNoteType(dt.Rows[0]);

                    return Ok(ApiResponse<SessionNoteTypeResponse>.SuccessResponse(
                        ResponseStatus.DataUpdated,
                        "Note type updated successfully",
                        noteType
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to update note type"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error updating note type: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Toggle note type status
        /// </summary>
        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleNoteTypeStatus(
            int id,
            [FromQuery] bool isActive,
            [FromQuery] int modifiedBy)
        {
            try
            {
                var dtExisting = await SessionNoteTypesDL.GetNoteTypeByIdAsync(id);
                if (dtExisting.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Note type not found"
                    ));
                }

                int result = await SessionNoteTypesDL.ToggleNoteTypeStatusAsync(id, isActive, modifiedBy);

                if (result > 0)
                {
                    string message = isActive
                        ? "Note type activated successfully"
                        : "Note type deactivated successfully";

                    return Ok(ApiResponse.SuccessResponse(message));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to update note type status"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error updating note type status: {ex.Message}"
                ));
            }
        }

        #endregion

        #region DELETE Endpoints

        /// <summary>
        /// Delete session note type
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNoteType(int id)
        {
            try
            {
                int result = await SessionNoteTypesDL.DeleteNoteTypeAsync(id);

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Note type deleted successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.NotFound,
                    "Note type not found"
                ));
            }
            catch (InvalidOperationException ex)
            {
                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.ValidationError,
                    ex.Message
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error deleting note type: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Helper Methods

        private SessionNoteTypeResponse ConvertRowToNoteType(DataRow row)
        {
            return new SessionNoteTypeResponse
            {
                NoteTypeID = Convert.ToInt32(row["NoteTypeID"]),
                NoteTypeName = row["NoteTypeName"]?.ToString() ?? "",
                NoteTypeCode = row["NoteTypeCode"]?.ToString() ?? "",
                Description = row["Description"]?.ToString(),
                UnitOfMeasure = row["UnitOfMeasure"]?.ToString(),
                IsMandatory = Convert.ToBoolean(row["IsMandatory"]),
                IsNumeric = Convert.ToBoolean(row["IsNumeric"]),
                MinimumValue = row["MinimumValue"] != DBNull.Value
                    ? Convert.ToDecimal(row["MinimumValue"])
                    : null,
                MaximumValue = row["MaximumValue"] != DBNull.Value
                    ? Convert.ToDecimal(row["MaximumValue"])
                    : null,
                DefaultValue = row["DefaultValue"]?.ToString(),
                DisplayOrder = Convert.ToInt32(row["DisplayOrder"]),
                Category = row["Category"]?.ToString(),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private List<SessionNoteTypeResponse> ConvertDataTableToNoteTypeList(DataTable dt)
        {
            var noteTypes = new List<SessionNoteTypeResponse>();
            foreach (DataRow row in dt.Rows)
            {
                noteTypes.Add(ConvertRowToNoteType(row));
            }
            return noteTypes;
        }

        #endregion
    }
}
