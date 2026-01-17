using Microsoft.AspNetCore.Mvc;
using DMS.Api.DL;
using DMS.Api.Shared;
using System.Data;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetAssignmentsController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Get all asset assignments
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAssignments(
            [FromQuery] int? assetId = null,
            [FromQuery] int? centerId = null)
        {
            try
            {
                var dt = await AssetAssignmentsDL.GetAllAssignmentsAsync(assetId, centerId);
                var assignments = ConvertDataTableToAssignmentList(dt);

                return Ok(ApiResponse<List<AssetAssignmentResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {assignments.Count} assignment(s)",
                    assignments
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AssetAssignmentResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving assignments: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get assignments by date
        /// </summary>
        [HttpGet("date/{centerId}/{date}")]
        public async Task<IActionResult> GetAssignmentsByDate(int centerId, DateTime date)
        {
            try
            {
                var dt = await AssetAssignmentsDL.GetAssignmentsByDateAsync(centerId, date);
                var assignments = ConvertDataTableToAssignmentList(dt);

                return Ok(ApiResponse<List<AssetAssignmentResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {assignments.Count} assignment(s) for {date:yyyy-MM-dd}",
                    assignments
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AssetAssignmentResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving assignments: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Check if asset is available for specific time
        /// </summary>
        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckAssetAvailability(
            [FromQuery] int assetId,
            [FromQuery] DateTime date,
            [FromQuery] string startTime,
            [FromQuery] string endTime)
        {
            try
            {
                TimeSpan slotStartTime = TimeSpan.Parse(startTime);
                TimeSpan slotEndTime = TimeSpan.Parse(endTime);

                bool isAvailable = await AssetAssignmentsDL.IsAssetAvailableAsync(
                    assetId,
                    date,
                    slotStartTime,
                    slotEndTime
                );

                return Ok(new ApiResponse<object>(
                    ResponseStatus.Success,
                    isAvailable ? "Asset is available" : "Asset is not available",
                    new
                    {
                        assetId = assetId,
                        date = date.ToString("yyyy-MM-dd"),
                        startTime = startTime,
                        endTime = endTime,
                        isAvailable = isAvailable
                    }
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error checking availability: {ex.Message}"
                ));
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Assign asset to appointment
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAssignment([FromBody] AssetAssignmentRequest request)
        {
            try
            {
                // Validate asset exists and is active
                var dtAsset = await AssetsDL.GetAssetByIdAsync(request.AssetID);
                if (dtAsset.Rows.Count == 0)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Asset not found"
                    ));
                }

                if (!Convert.ToBoolean(dtAsset.Rows[0]["IsActive"]))
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Asset is inactive and cannot be assigned"
                    ));
                }

                // Validate appointment exists
                var dtAppointment = await AppointmentsDL.GetAppointmentByIdAsync(request.AppointmentID);
                if (dtAppointment.Rows.Count == 0)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Appointment not found"
                    ));
                }

                // Check if asset is available for the time slot
                TimeSpan endTime = request.AssignedTime.Add(TimeSpan.FromMinutes(request.SessionDuration));
                bool isAvailable = await AssetAssignmentsDL.IsAssetAvailableAsync(
                    request.AssetID,
                    request.AssignedDate,
                    request.AssignedTime,
                    endTime
                );

                if (!isAvailable)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Asset is not available for the selected time slot"
                    ));
                }

                // Create assignment
                int assignmentId = await AssetAssignmentsDL.CreateAssignmentAsync(
                    request.AssetID,
                    request.AppointmentID,
                    request.AssignedDate,
                    request.AssignedTime,
                    request.SessionDuration,
                    request.Notes,
                    request.CreatedBy
                );

                return Ok(ApiResponse<int>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    "Asset assigned successfully",
                    assignmentId
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error creating assignment: {ex.Message}"
                ));
            }
        }

        #endregion

        #region PUT Endpoints

        /// <summary>
        /// Complete assignment
        /// </summary>
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteAssignment(int id)
        {
            try
            {
                int result = await AssetAssignmentsDL.UpdateAssignmentStatusAsync(id, "Completed");

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Assignment completed successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.NotFound,
                    "Assignment not found"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error completing assignment: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Cancel assignment
        /// </summary>
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelAssignment(int id)
        {
            try
            {
                int result = await AssetAssignmentsDL.CancelAssignmentAsync(id);

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Assignment cancelled successfully. Asset is now available."
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.NotFound,
                    "Assignment not found"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error cancelling assignment: {ex.Message}"
                ));
            }
        }

        #endregion

        #region DELETE Endpoints

        /// <summary>
        /// Delete assignment
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            try
            {
                int result = await AssetAssignmentsDL.DeleteAssignmentAsync(id);

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Assignment deleted successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.NotFound,
                    "Assignment not found"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error deleting assignment: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Helper Methods

        private AssetAssignmentResponse ConvertRowToAssignment(DataRow row)
        {
            return new AssetAssignmentResponse
            {
                AssignmentID = Convert.ToInt32(row["AssignmentID"]),
                AssetID = Convert.ToInt32(row["AssetID"]),
                AssetCode = row["AssetCode"]?.ToString() ?? "",
                AssetName = row["AssetName"]?.ToString() ?? "",
                AppointmentID = Convert.ToInt32(row["AppointmentID"]),
                PatientName = row["PatientName"]?.ToString() ?? "",
                PatientCode = row["PatientCode"]?.ToString() ?? "",
                AssignedDate = Convert.ToDateTime(row["AssignedDate"]),
                AssignedTime = (TimeSpan)row["AssignedTime"],
                SessionDuration = Convert.ToInt32(row["SessionDuration"]),
                Status = row["Status"]?.ToString() ?? "",
                Notes = row["Notes"]?.ToString(),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private List<AssetAssignmentResponse> ConvertDataTableToAssignmentList(DataTable dt)
        {
            var assignments = new List<AssetAssignmentResponse>();
            foreach (DataRow row in dt.Rows)
            {
                assignments.Add(ConvertRowToAssignment(row));
            }
            return assignments;
        }

        #endregion
    }
}
