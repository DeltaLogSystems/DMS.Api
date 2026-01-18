using System.Data;
using DMS.Api.DL;
using DMS.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiscardRequestsController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Get all discard requests
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRequests(
            [FromQuery] int? centerId = null,
            [FromQuery] string? requestStatus = null)
        {
            try
            {
                var dt = await DiscardRequestsDL.GetAllRequestsAsync(centerId, requestStatus);
                var requests = ConvertDataTableToRequestList(dt);

                return Ok(ApiResponse<List<DiscardRequestResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {requests.Count} discard request(s)",
                    requests
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<DiscardRequestResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving discard requests: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get pending discard requests
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRequests([FromQuery] int? centerId = null)
        {
            try
            {
                var dt = await DiscardRequestsDL.GetPendingRequestsAsync(centerId);
                var requests = ConvertDataTableToRequestList(dt);

                return Ok(ApiResponse<List<DiscardRequestResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {requests.Count} pending discard request(s)",
                    requests
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<DiscardRequestResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving pending requests: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get discard requests by status
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetRequestsByStatus(
            string status,
            [FromQuery] int? centerId = null)
        {
            try
            {
                var validStatuses = new[] { "Pending", "Approved", "Rejected" };
                if (!validStatuses.Contains(status))
                {
                    return BadRequest(ApiResponse<List<DiscardRequestResponse>>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Invalid status. Must be: Pending, Approved, or Rejected"
                    ));
                }

                var dt = await DiscardRequestsDL.GetAllRequestsAsync(centerId, status);
                var requests = ConvertDataTableToRequestList(dt);

                return Ok(ApiResponse<List<DiscardRequestResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {requests.Count} {status.ToLower()} request(s)",
                    requests
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<DiscardRequestResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving requests: {ex.Message}"
                ));
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Create discard request (for items below minimum usage)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateDiscardRequest([FromBody] DiscardRequestRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Reason is required for discard request"
                    ));
                }

                var validTypes = new[] { "EarlyDiscard", "Overuse", "Damaged", "Expired" };
                if (!validTypes.Contains(request.RequestType))
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Invalid request type. Must be: EarlyDiscard, Overuse, Damaged, or Expired"
                    ));
                }

                // Validate individual item exists
                var dtItem = await IndividualItemsDL.GetIndividualItemByIdAsync(request.IndividualItemID);
                if (dtItem.Rows.Count == 0)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Individual item not found"
                    ));
                }

                // Check if item is already in discard request state
                string itemStatus = dtItem.Rows[0]["ItemStatus"]?.ToString() ?? "";
                if (itemStatus == "DiscardRequested")
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Discard request already pending for this item"
                    ));
                }

                if (itemStatus == "Discarded")
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Item is already discarded"
                    ));
                }

                // Create request
                int requestId = await DiscardRequestsDL.CreateDiscardRequestAsync(
                    request.IndividualItemID,
                    request.RequestType,
                    request.Reason,
                    request.RequestedBy
                );

                return Ok(ApiResponse<int>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    "Discard request submitted successfully. Waiting for admin approval.",
                    requestId
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error creating discard request: {ex.Message}"
                ));
            }
        }

        #endregion

        #region PUT Endpoints

        /// <summary>
        /// Approve or reject discard request (Admin only)
        /// </summary>
        [HttpPut("{id}/process")]
        public async Task<IActionResult> ProcessDiscardRequest(
            int id,
            [FromBody] ApproveDiscardRequest request)
        {
            try
            {
                // Validate request
                if (request.RequestID != id)
                {
                    return BadRequest(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Request ID mismatch"
                    ));
                }

                // Process request
                int result = await DiscardRequestsDL.ProcessDiscardRequestAsync(
                    id,
                    request.IsApproved,
                    request.ReviewComments,
                    request.ReviewedBy
                );

                if (result > 0)
                {
                    string message = request.IsApproved
                        ? "Discard request approved successfully. Item has been discarded."
                        : "Discard request rejected. Item is now available for use.";

                    return Ok(ApiResponse.SuccessResponse(message));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to process discard request"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error processing discard request: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Approve discard request
        /// </summary>
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveDiscardRequest(
            int id,
            [FromQuery] int reviewedBy,
            [FromQuery] string? comments = null)
        {
            try
            {
                var request = new ApproveDiscardRequest
                {
                    RequestID = id,
                    IsApproved = true,
                    ReviewComments = comments,
                    ReviewedBy = reviewedBy
                };

                return await ProcessDiscardRequest(id, request);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error approving discard request: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Reject discard request
        /// </summary>
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectDiscardRequest(
            int id,
            [FromQuery] int reviewedBy,
            [FromQuery] string? comments = null)
        {
            try
            {
                var request = new ApproveDiscardRequest
                {
                    RequestID = id,
                    IsApproved = false,
                    ReviewComments = comments,
                    ReviewedBy = reviewedBy
                };

                return await ProcessDiscardRequest(id, request);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error rejecting discard request: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Helper Methods

        private DiscardRequestResponse ConvertRowToRequest(DataRow row)
        {
            return new DiscardRequestResponse
            {
                RequestID = Convert.ToInt32(row["RequestID"]),
                IndividualItemID = Convert.ToInt32(row["IndividualItemID"]),
                IndividualItemCode = row["IndividualItemCode"]?.ToString() ?? "",
                InventoryItemID = Convert.ToInt32(row["InventoryItemID"]),
                ItemName = row["ItemName"]?.ToString() ?? "",
                CenterID = Convert.ToInt32(row["CenterID"]),
                CenterName = row["CenterName"]?.ToString() ?? "",
                RequestType = row["RequestType"]?.ToString() ?? "",
                CurrentUsageCount = Convert.ToInt32(row["CurrentUsageCount"]),
                MinimumUsageCount = Convert.ToInt32(row["MinimumUsageCount"]),
                Reason = row["Reason"]?.ToString() ?? "",
                RequestStatus = row["RequestStatus"]?.ToString() ?? "",
                RequestedBy = Convert.ToInt32(row["RequestedBy"]),
                RequestedByName = "", // Can be populated from Users table
                RequestedDate = Convert.ToDateTime(row["RequestedDate"]),
                ReviewedBy = row["ReviewedBy"] != DBNull.Value
                    ? Convert.ToInt32(row["ReviewedBy"])
                    : null,
                ReviewedByName = null, // Can be populated from Users table
                ReviewedDate = row["ReviewedDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["ReviewedDate"])
                    : null,
                ReviewComments = row["ReviewComments"]?.ToString()
            };
        }

        private List<DiscardRequestResponse> ConvertDataTableToRequestList(DataTable dt)
        {
            var requests = new List<DiscardRequestResponse>();
            foreach (DataRow row in dt.Rows)
            {
                requests.Add(ConvertRowToRequest(row));
            }
            return requests;
        }

        #endregion
    }
}
