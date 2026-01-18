using System.Data;
using DMS.Api.DL;
using DMS.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryItemsController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Get all inventory items with filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllItems(
            [FromQuery] int? itemTypeId = null,
            [FromQuery] int? usageTypeId = null,
            [FromQuery] bool? isRequiredForDialysis = null,
            [FromQuery] bool? isIndividualQtyTracking = null,
            [FromQuery] bool activeOnly = true)
        {
            try
            {
                var dt = await InventoryItemsDL.GetAllItemsAsync(
                    itemTypeId,
                    usageTypeId,
                    isRequiredForDialysis,
                    isIndividualQtyTracking,
                    activeOnly
                );

                var items = ConvertDataTableToItemList(dt);

                return Ok(ApiResponse<List<InventoryItemResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {items.Count} inventory item(s)",
                    items
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<InventoryItemResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving inventory items: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get inventory item by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            try
            {
                var dt = await InventoryItemsDL.GetItemByIdAsync(id);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<InventoryItemResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Inventory item not found"
                    ));
                }

                var item = ConvertRowToItem(dt.Rows[0]);

                return Ok(ApiResponse<InventoryItemResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Inventory item retrieved successfully",
                    item
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryItemResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving inventory item: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get items required for dialysis session
        /// </summary>
        [HttpGet("dialysis-required")]
        public async Task<IActionResult> GetDialysisRequiredItems()
        {
            try
            {
                var dt = await InventoryItemsDL.GetDialysisRequiredItemsAsync();
                var items = ConvertDataTableToItemList(dt);

                return Ok(ApiResponse<List<InventoryItemResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {items.Count} dialysis-required item(s)",
                    items
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<InventoryItemResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving dialysis items: {ex.Message}"
                ));
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Create new inventory item
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] InventoryItemRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.ItemName))
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Item name is required"
                    ));
                }

                if (request.MinimumUsageCount < 1)
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Minimum usage count must be at least 1"
                    ));
                }

                if (request.MaximumUsageCount < request.MinimumUsageCount)
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Maximum usage count cannot be less than minimum usage count"
                    ));
                }

                // Validate item type exists
                var dtType = await InventoryItemTypesDL.GetItemTypeByIdAsync(request.ItemTypeID);
                if (dtType.Rows.Count == 0)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Item type not found"
                    ));
                }

                // Create item
                int itemId = await InventoryItemsDL.CreateItemAsync(
                    request.ItemName,
                    request.ItemTypeID,
                    request.UsageTypeID,
                    request.Description,
                    request.Manufacturer,
                    request.MinimumUsageCount,
                    request.MaximumUsageCount,
                    request.IsIndividualQtyTracking,
                    request.RequiresApprovalForEarlyDiscard,
                    request.RequiresApprovalForOveruse,
                    request.IsRequiredForDialysis,
                    request.UnitOfMeasure,
                    request.ReorderLevel,
                    request.CreatedBy
                );

                // Get created item
                var dt = await InventoryItemsDL.GetItemByIdAsync(itemId);
                var item = ConvertRowToItem(dt.Rows[0]);

                return Ok(ApiResponse<InventoryItemResponse>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    "Inventory item created successfully",
                    item
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryItemResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error creating inventory item: {ex.Message}"
                ));
            }
        }

        #endregion

        #region PUT Endpoints

        /// <summary>
        /// Update inventory item
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] InventoryItemRequest request)
        {
            try
            {
                // Check if item exists
                var dtExisting = await InventoryItemsDL.GetItemByIdAsync(id);
                if (dtExisting.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Inventory item not found"
                    ));
                }

                // Validate
                if (string.IsNullOrWhiteSpace(request.ItemName))
                {
                    return BadRequest(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Item name is required"
                    ));
                }

                if (request.MaximumUsageCount < request.MinimumUsageCount)
                {
                    return BadRequest(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Maximum usage count cannot be less than minimum usage count"
                    ));
                }

                // Update item
                int result = await InventoryItemsDL.UpdateItemAsync(
                    id,
                    request.ItemName,
                    request.Description,
                    request.Manufacturer,
                    request.MinimumUsageCount,
                    request.MaximumUsageCount,
                    request.RequiresApprovalForEarlyDiscard,
                    request.RequiresApprovalForOveruse,
                    request.IsRequiredForDialysis,
                    request.UnitOfMeasure,
                    request.ReorderLevel,
                    request.CreatedBy
                );

                if (result > 0)
                {
                    var dt = await InventoryItemsDL.GetItemByIdAsync(id);
                    var item = ConvertRowToItem(dt.Rows[0]);

                    return Ok(ApiResponse<InventoryItemResponse>.SuccessResponse(
                        ResponseStatus.DataUpdated,
                        "Inventory item updated successfully",
                        item
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to update inventory item"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error updating inventory item: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Toggle item status
        /// </summary>
        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleItemStatus(
            int id,
            [FromQuery] bool isActive,
            [FromQuery] int modifiedBy)
        {
            try
            {
                var dtExisting = await InventoryItemsDL.GetItemByIdAsync(id);
                if (dtExisting.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Inventory item not found"
                    ));
                }

                int result = await InventoryItemsDL.ToggleItemStatusAsync(id, isActive, modifiedBy);

                if (result > 0)
                {
                    string message = isActive
                        ? "Inventory item activated successfully"
                        : "Inventory item deactivated successfully";

                    return Ok(ApiResponse.SuccessResponse(message));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to update item status"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error updating item status: {ex.Message}"
                ));
            }
        }

        #endregion

        #region DELETE Endpoints

        /// <summary>
        /// Delete inventory item
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                int result = await InventoryItemsDL.DeleteItemAsync(id);

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Inventory item deleted successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.NotFound,
                    "Inventory item not found"
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
                    $"Error deleting inventory item: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Helper Methods

        private InventoryItemResponse ConvertRowToItem(DataRow row)
        {
            return new InventoryItemResponse
            {
                InventoryItemID = Convert.ToInt32(row["InventoryItemID"]),
                ItemCode = row["ItemCode"]?.ToString() ?? "",
                ItemName = row["ItemName"]?.ToString() ?? "",
                ItemTypeID = Convert.ToInt32(row["ItemTypeID"]),
                ItemTypeName = row["ItemTypeName"]?.ToString() ?? "",
                UsageTypeID = Convert.ToInt32(row["UsageTypeID"]),
                UsageTypeName = row["UsageTypeName"]?.ToString() ?? "",
                Description = row["Description"]?.ToString(),
                Manufacturer = row["Manufacturer"]?.ToString(),
                MinimumUsageCount = Convert.ToInt32(row["MinimumUsageCount"]),
                MaximumUsageCount = Convert.ToInt32(row["MaximumUsageCount"]),
                IsIndividualQtyTracking = Convert.ToBoolean(row["IsIndividualQtyTracking"]),
                RequiresApprovalForEarlyDiscard = Convert.ToBoolean(row["RequiresApprovalForEarlyDiscard"]),
                RequiresApprovalForOveruse = Convert.ToBoolean(row["RequiresApprovalForOveruse"]),
                IsRequiredForDialysis = Convert.ToBoolean(row["IsRequiredForDialysis"]),
                UnitOfMeasure = row["UnitOfMeasure"]?.ToString(),
                ReorderLevel = row["ReorderLevel"] != DBNull.Value
                    ? Convert.ToInt32(row["ReorderLevel"])
                    : null,
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private List<InventoryItemResponse> ConvertDataTableToItemList(DataTable dt)
        {
            var items = new List<InventoryItemResponse>();
            foreach (DataRow row in dt.Rows)
            {
                items.Add(ConvertRowToItem(row));
            }
            return items;
        }

        #endregion
    }
}
