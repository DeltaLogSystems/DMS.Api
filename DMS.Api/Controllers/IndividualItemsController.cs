using System.Data;
using DMS.Api.DL;
using DMS.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IndividualItemsController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Get all individual items with filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllIndividualItems(
            [FromQuery] int? centerId = null,
            [FromQuery] int? inventoryItemId = null,
            [FromQuery] int? stockId = null,
            [FromQuery] string? itemStatus = null,
            [FromQuery] bool availableOnly = true)
        {
            try
            {
                var dt = await IndividualItemsDL.GetAllIndividualItemsAsync(
                    centerId,
                    inventoryItemId,
                    stockId,
                    itemStatus,
                    availableOnly
                );

                var items = ConvertDataTableToIndividualItemList(dt);

                return Ok(ApiResponse<List<IndividualItemResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {items.Count} individual item(s)",
                    items
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<IndividualItemResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving individual items: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get individual item by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIndividualItemById(int id)
        {
            try
            {
                var dt = await IndividualItemsDL.GetIndividualItemByIdAsync(id);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<IndividualItemResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Individual item not found"
                    ));
                }

                var item = ConvertRowToIndividualItem(dt.Rows[0]);

                return Ok(ApiResponse<IndividualItemResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Individual item retrieved successfully",
                    item
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IndividualItemResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving individual item: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get available items for dialysis session selection
        /// </summary>
        [HttpGet("available-for-session")]
        public async Task<IActionResult> GetAvailableItemsForSession(
            [FromQuery] int centerId,
            [FromQuery] int inventoryItemId)
        {
            try
            {
                var dt = await IndividualItemsDL.GetAvailableItemsForSessionAsync(centerId, inventoryItemId);

                var items = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    var item = ConvertRowToIndividualItem(row);
                    items.Add(new
                    {
                        individualItem = item,
                        usageStatus = row["UsageStatus"]?.ToString(),
                        batchNumber = row["BatchNumber"]?.ToString(),
                        expiryDate = row["ExpiryDate"] != DBNull.Value
                            ? Convert.ToDateTime(row["ExpiryDate"])
                            : (DateTime?)null,
                        priority = row["UsageStatus"]?.ToString() switch
                        {
                            "Can Use" => 1,      // Previously used, can continue
                            "New" => 2,          // New items
                            "Below Minimum" => 3, // Below minimum usage
                            _ => 4
                        }
                    });
                }

                return Ok(ApiResponse<List<object>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {items.Count} available item(s) for session",
                    items
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving available items: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get items by stock ID
        /// </summary>
        [HttpGet("by-stock/{stockId}")]
        public async Task<IActionResult> GetItemsByStock(int stockId)
        {
            try
            {
                var dt = await IndividualItemsDL.GetAllIndividualItemsAsync(
                    null,
                    null,
                    stockId,
                    null,
                    false
                );

                var items = ConvertDataTableToIndividualItemList(dt);

                return Ok(ApiResponse<List<IndividualItemResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {items.Count} item(s) from stock",
                    items
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<IndividualItemResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving items: {ex.Message}"
                ));
            }
        }

        #endregion

        #region PUT Endpoints

        /// <summary>
        /// Mark item for discard (direct discard without approval)
        /// </summary>
        [HttpPut("{id}/mark-discard")]
        public async Task<IActionResult> MarkItemForDiscard(
            int id,
            [FromQuery] string reason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    return BadRequest(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Discard reason is required"
                    ));
                }

                // Get item details
                var dt = await IndividualItemsDL.GetIndividualItemByIdAsync(id);
                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Individual item not found"
                    ));
                }

                int currentUsage = Convert.ToInt32(dt.Rows[0]["CurrentUsageCount"]);
                int minUsage = Convert.ToInt32(dt.Rows[0]["MinimumUsageCount"]);
                bool requiresApproval = Convert.ToBoolean(dt.Rows[0]["RequiresApprovalForEarlyDiscard"]);

                // Check if approval is required
                if (currentUsage < minUsage && requiresApproval)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        $"This item requires approval for early discard. Current usage: {currentUsage}, Minimum: {minUsage}. Please submit a discard request."
                    ));
                }

                // Direct discard
                int result = await IndividualItemsDL.DiscardItemAsync(id, reason);

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Item marked for discard successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to mark item for discard"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error marking item for discard: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Helper Methods

        private IndividualItemResponse ConvertRowToIndividualItem(DataRow row)
        {
            return new IndividualItemResponse
            {
                IndividualItemID = Convert.ToInt32(row["IndividualItemID"]),
                StockID = Convert.ToInt32(row["StockID"]),
                InventoryItemID = Convert.ToInt32(row["InventoryItemID"]),
                ItemCode = row["ItemCode"]?.ToString() ?? "",
                ItemName = row["ItemName"]?.ToString() ?? "",
                CenterID = Convert.ToInt32(row["CenterID"]),
                IndividualItemCode = row["IndividualItemCode"]?.ToString() ?? "",
                SerialNumber = row["SerialNumber"]?.ToString(),
                CurrentUsageCount = Convert.ToInt32(row["CurrentUsageCount"]),
                MaxUsageCount = Convert.ToInt32(row["MaxUsageCount"]),
                ItemStatus = row["ItemStatus"]?.ToString() ?? "",
                IsAvailable = Convert.ToBoolean(row["IsAvailable"]),
                FirstUsedDate = row["FirstUsedDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["FirstUsedDate"])
                    : null,
                LastUsedDate = row["LastUsedDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["LastUsedDate"])
                    : null,
                DiscardedDate = row["DiscardedDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["DiscardedDate"])
                    : null,
                DiscardReason = row["DiscardReason"]?.ToString(),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private List<IndividualItemResponse> ConvertDataTableToIndividualItemList(DataTable dt)
        {
            var items = new List<IndividualItemResponse>();
            foreach (DataRow row in dt.Rows)
            {
                items.Add(ConvertRowToIndividualItem(row));
            }
            return items;
        }

        #endregion
    }
}
