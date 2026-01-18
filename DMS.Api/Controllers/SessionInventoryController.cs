using System.Data;
using DMS.Api.DL;
using DMS.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionInventoryController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Get all inventory items for a session
        /// </summary>
        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetSessionInventory(int sessionId)
        {
            try
            {
                var dt = await SessionInventoryDL.GetSessionInventoryAsync(sessionId);
                var inventory = ConvertDataTableToInventoryList(dt);

                return Ok(ApiResponse<List<SessionInventoryResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {inventory.Count} inventory item(s)",
                    inventory
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<SessionInventoryResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving session inventory: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get available items for session (dialysis-required + optional)
        /// </summary>
        [HttpGet("available-for-session")]
        public async Task<IActionResult> GetAvailableItemsForSession(
            [FromQuery] int centerId,
            [FromQuery] int sessionId)
        {
            try
            {
                // Get dialysis-required items
                var dtRequired = await InventoryItemsDL.GetDialysisRequiredItemsAsync();
                var availableItems = new List<AvailableSessionInventoryResponse>();

                foreach (DataRow itemRow in dtRequired.Rows)
                {
                    int inventoryItemId = Convert.ToInt32(itemRow["InventoryItemID"]);
                    bool isIndividualTracking = Convert.ToBoolean(itemRow["IsIndividualQtyTracking"]);

                    var availableItem = new AvailableSessionInventoryResponse
                    {
                        InventoryItemID = inventoryItemId,
                        ItemCode = itemRow["ItemCode"]?.ToString() ?? "",
                        ItemName = itemRow["ItemName"]?.ToString() ?? "",
                        IsRequired = Convert.ToBoolean(itemRow["IsRequiredForDialysis"]),
                        IsIndividualTracking = isIndividualTracking
                    };

                    if (isIndividualTracking)
                    {
                        // Get individual items
                        var dtIndividual = await IndividualItemsDL.GetAvailableItemsForSessionAsync(centerId, inventoryItemId);
                        availableItem.IndividualItems = new List<AvailableIndividualItem>();

                        foreach (DataRow indRow in dtIndividual.Rows)
                        {
                            int currentUsage = Convert.ToInt32(indRow["CurrentUsageCount"]);
                            int maxUsage = Convert.ToInt32(indRow["MaxUsageCount"]);
                            int remainingUses = maxUsage - currentUsage;
                            double usagePercentage = maxUsage > 0 ? (currentUsage * 100.0 / maxUsage) : 0;

                            string usageStatus = currentUsage > 0 && currentUsage < maxUsage
                                ? "Can Use"
                                : currentUsage == 0
                                    ? "New"
                                    : "Below Minimum";

                            int priority = currentUsage > 0 && currentUsage < maxUsage ? 1 : 2;

                            availableItem.IndividualItems.Add(new AvailableIndividualItem
                            {
                                IndividualItemID = Convert.ToInt32(indRow["IndividualItemID"]),
                                IndividualItemCode = indRow["IndividualItemCode"]?.ToString() ?? "",
                                CurrentUsageCount = currentUsage,
                                MaxUsageCount = maxUsage,
                                RemainingUses = remainingUses,
                                UsagePercentage = usagePercentage,
                                ItemStatus = indRow["ItemStatus"]?.ToString() ?? "",
                                UsageStatusDisplay = usageStatus,
                                Priority = priority,
                                BatchNumber = indRow["BatchNumber"]?.ToString(),
                                ExpiryDate = indRow["ExpiryDate"] != DBNull.Value
                                    ? Convert.ToDateTime(indRow["ExpiryDate"])
                                    : null,
                                StockID = Convert.ToInt32(indRow["StockID"])
                            });
                        }

                        // Sort by priority
                        availableItem.IndividualItems = availableItem.IndividualItems
                            .OrderBy(x => x.Priority)
                            .ThenByDescending(x => x.CurrentUsageCount)
                            .ToList();
                    }
                    else
                    {
                        // Get stock summary
                        var dtStock = await InventoryStockDL.GetStockSummaryAsync(centerId);
                        var stockRow = dtStock.AsEnumerable()
                            .FirstOrDefault(r => Convert.ToInt32(r["InventoryItemID"]) == inventoryItemId);

                        if (stockRow != null)
                        {
                            availableItem.AvailableQuantity = Convert.ToInt32(stockRow["TotalAvailable"]);
                            availableItem.StockID = Convert.ToInt32(stockRow["StockID"]);
                        }
                    }

                    availableItems.Add(availableItem);
                }

                return Ok(ApiResponse<List<AvailableSessionInventoryResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {availableItems.Count} available item(s) for session",
                    availableItems
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AvailableSessionInventoryResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving available items: {ex.Message}"
                ));
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Add single inventory item to session
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddInventoryToSession([FromBody] AddSessionInventoryRequest request)
        {
            try
            {
                // Validate session exists and status
                var dtSession = await DialysisSessionsDL.GetSessionByIdAsync(request.SessionID);
                if (dtSession.Rows.Count == 0)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Session not found"
                    ));
                }

                string sessionStatus = dtSession.Rows[0]["SessionStatus"]?.ToString() ?? "";
                if (sessionStatus != "Not Started")
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Cannot add inventory after session has started"
                    ));
                }

                // Check if item already added
                bool alreadyAdded = await SessionInventoryDL.IsItemAlreadySelectedAsync(
                    request.SessionID,
                    request.InventoryItemID
                );

                if (alreadyAdded)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "This item is already added to the session"
                    ));
                }

                // Validate individual item if required
                if (request.IndividualItemID.HasValue)
                {
                    var dtIndividual = await IndividualItemsDL.GetIndividualItemByIdAsync(request.IndividualItemID.Value);
                    if (dtIndividual.Rows.Count == 0)
                    {
                        return Ok(ApiResponse<int>.ErrorResponse(
                            ResponseStatus.NotFound,
                            "Individual item not found"
                        ));
                    }

                    bool isAvailable = Convert.ToBoolean(dtIndividual.Rows[0]["IsAvailable"]);
                    if (!isAvailable)
                    {
                        return Ok(ApiResponse<int>.ErrorResponse(
                            ResponseStatus.ValidationError,
                            "Selected item is not available"
                        ));
                    }
                }

                // Add inventory
                int sessionInventoryId = await SessionInventoryDL.AddInventoryToSessionAsync(
                    request.SessionID,
                    request.InventoryItemID,
                    request.IndividualItemID,
                    request.StockID,
                    request.QuantityUsed,
                    request.ItemCondition,
                    request.UsageNumber,
                    request.Notes,
                    request.SelectedBy
                );

                return Ok(ApiResponse<int>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    "Inventory item added to session successfully",
                    sessionInventoryId
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error adding inventory to session: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Add multiple inventory items to session (bulk)
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> AddBulkInventoryToSession([FromBody] AddBulkSessionInventoryRequest request)
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

                string sessionStatus = dtSession.Rows[0]["SessionStatus"]?.ToString() ?? "";
                if (sessionStatus != "Not Started")
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Cannot add inventory after session has started"
                    ));
                }

                if (request.Items == null || request.Items.Count == 0)
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "No items provided"
                    ));
                }

                int addedCount = 0;

                foreach (var item in request.Items)
                {
                    // Check if already added
                    bool alreadyAdded = await SessionInventoryDL.IsItemAlreadySelectedAsync(
                        request.SessionID,
                        item.InventoryItemID
                    );

                    if (!alreadyAdded)
                    {
                        await SessionInventoryDL.AddInventoryToSessionAsync(
                            request.SessionID,
                            item.InventoryItemID,
                            item.IndividualItemID,
                            item.StockID,
                            item.QuantityUsed,
                            item.ItemCondition,
                            item.UsageNumber,
                            item.Notes,
                            request.SelectedBy
                        );
                        addedCount++;
                    }
                }

                return Ok(ApiResponse<int>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    $"{addedCount} inventory item(s) added to session successfully",
                    addedCount
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error adding bulk inventory: {ex.Message}"
                ));
            }
        }

        #endregion

        #region DELETE Endpoints

        /// <summary>
        /// Remove inventory item from session (before session starts)
        /// </summary>
        [HttpDelete("{sessionInventoryId}")]
        public async Task<IActionResult> RemoveInventoryFromSession(int sessionInventoryId)
        {
            try
            {
                int result = await SessionInventoryDL.RemoveInventoryFromSessionAsync(sessionInventoryId);

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Inventory item removed from session successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.NotFound,
                    "Inventory item not found in session"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error removing inventory: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Helper Methods

        private SessionInventoryResponse ConvertRowToInventory(DataRow row)
        {
            return new SessionInventoryResponse
            {
                SessionInventoryID = Convert.ToInt32(row["SessionInventoryID"]),
                SessionID = Convert.ToInt32(row["SessionID"]),
                InventoryItemID = Convert.ToInt32(row["InventoryItemID"]),
                ItemCode = row["ItemCode"]?.ToString() ?? "",
                ItemName = row["ItemName"]?.ToString() ?? "",
                UnitOfMeasure = row["UnitOfMeasure"]?.ToString(),
                IndividualItemID = row["IndividualItemID"] != DBNull.Value
                    ? Convert.ToInt32(row["IndividualItemID"])
                    : null,
                IndividualItemCode = row["IndividualItemCode"]?.ToString(),
                CurrentUsageCount = row["CurrentUsageCount"] != DBNull.Value
                    ? Convert.ToInt32(row["CurrentUsageCount"])
                    : null,
                MaxUsageCount = row["MaxUsageCount"] != DBNull.Value
                    ? Convert.ToInt32(row["MaxUsageCount"])
                    : null,
                StockID = Convert.ToInt32(row["StockID"]),
                BatchNumber = row["BatchNumber"]?.ToString(),
                ExpiryDate = row["ExpiryDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["ExpiryDate"])
                    : null,
                QuantityUsed = Convert.ToDecimal(row["QuantityUsed"]),
                ItemCondition = row["ItemCondition"]?.ToString(),
                UsageNumber = row["UsageNumber"] != DBNull.Value
                    ? Convert.ToInt32(row["UsageNumber"])
                    : null,
                Notes = row["Notes"]?.ToString(),
                SelectedAt = Convert.ToDateTime(row["SelectedAt"]),
                SelectedBy = Convert.ToInt32(row["SelectedBy"])
            };
        }

        private List<SessionInventoryResponse> ConvertDataTableToInventoryList(DataTable dt)
        {
            var inventory = new List<SessionInventoryResponse>();
            foreach (DataRow row in dt.Rows)
            {
                inventory.Add(ConvertRowToInventory(row));
            }
            return inventory;
        }

        #endregion
    }
}
