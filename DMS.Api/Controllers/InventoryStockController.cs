using System.Data;
using DMS.Api.DL;
using DMS.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryStockController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Get all stock with filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllStock(
            [FromQuery] int? centerId = null,
            [FromQuery] int? inventoryItemId = null,
            [FromQuery] bool showExpired = false,
            [FromQuery] bool showNearExpiry = false,
            [FromQuery] bool showLowStock = false)
        {
            try
            {
                var dt = await InventoryStockDL.GetAllStockAsync(
                    centerId,
                    inventoryItemId,
                    showExpired,
                    showNearExpiry,
                    showLowStock
                );

                var stocks = ConvertDataTableToStockList(dt);

                return Ok(ApiResponse<List<InventoryStockResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {stocks.Count} stock record(s)",
                    stocks
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<InventoryStockResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving stock: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get stock by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStockById(int id)
        {
            try
            {
                var dt = await InventoryStockDL.GetStockByIdAsync(id);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<InventoryStockResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Stock not found"
                    ));
                }

                var stock = ConvertRowToStock(dt.Rows[0]);

                return Ok(ApiResponse<InventoryStockResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Stock retrieved successfully",
                    stock
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryStockResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving stock: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get stock summary by center
        /// </summary>
        [HttpGet("summary/{centerId}")]
        public async Task<IActionResult> GetStockSummary(int centerId)
        {
            try
            {
                var dt = await InventoryStockDL.GetStockSummaryAsync(centerId);

                var summary = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    summary.Add(new
                    {
                        itemCode = row["ItemCode"]?.ToString(),
                        itemName = row["ItemName"]?.ToString(),
                        unitOfMeasure = row["UnitOfMeasure"]?.ToString(),
                        totalQuantity = Convert.ToInt32(row["TotalQuantity"]),
                        totalAvailable = Convert.ToInt32(row["TotalAvailable"]),
                        stockCount = Convert.ToInt32(row["StockCount"]),
                        nearestExpiry = row["NearestExpiry"] != DBNull.Value
                            ? Convert.ToDateTime(row["NearestExpiry"])
                            : (DateTime?)null,
                        reorderLevel = row["ReorderLevel"] != DBNull.Value
                            ? Convert.ToInt32(row["ReorderLevel"])
                            : (int?)null,
                        isLowStock = Convert.ToBoolean(row["IsLowStock"])
                    });
                }

                return Ok(ApiResponse<List<object>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved stock summary for center",
                    summary
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving stock summary: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get expired stock items
        /// </summary>
        [HttpGet("expired")]
        public async Task<IActionResult> GetExpiredStock([FromQuery] int? centerId = null)
        {
            try
            {
                var dt = await InventoryStockDL.GetAllStockAsync(
                    centerId,
                    null,
                    showExpired: true,
                    showNearExpiry: false,
                    showLowStock: false
                );

                var stocks = ConvertDataTableToStockList(dt);

                return Ok(ApiResponse<List<InventoryStockResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {stocks.Count} expired stock item(s)",
                    stocks
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<InventoryStockResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving expired stock: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get near expiry stock items
        /// </summary>
        [HttpGet("near-expiry")]
        public async Task<IActionResult> GetNearExpiryStock([FromQuery] int? centerId = null)
        {
            try
            {
                var dt = await InventoryStockDL.GetAllStockAsync(
                    centerId,
                    null,
                    showExpired: false,
                    showNearExpiry: true,
                    showLowStock: false
                );

                var stocks = ConvertDataTableToStockList(dt);

                return Ok(ApiResponse<List<InventoryStockResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {stocks.Count} near-expiry stock item(s)",
                    stocks
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<InventoryStockResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving near-expiry stock: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get low stock items
        /// </summary>
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock([FromQuery] int? centerId = null)
        {
            try
            {
                var dt = await InventoryStockDL.GetAllStockAsync(
                    centerId,
                    null,
                    showExpired: false,
                    showNearExpiry: false,
                    showLowStock: true
                );

                var stocks = ConvertDataTableToStockList(dt);

                return Ok(ApiResponse<List<InventoryStockResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {stocks.Count} low-stock item(s)",
                    stocks
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<InventoryStockResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving low-stock items: {ex.Message}"
                ));
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Add new stock
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddStock([FromBody] InventoryStockRequest request)
        {
            try
            {
                // Validate request
                if (request.Quantity <= 0)
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Quantity must be greater than 0"
                    ));
                }

                // Validate item exists
                var dtItem = await InventoryItemsDL.GetItemByIdAsync(request.InventoryItemID);
                if (dtItem.Rows.Count == 0)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Inventory item not found"
                    ));
                }

                // Validate expiry date
                if (request.ExpiryDate.HasValue && request.ExpiryDate.Value < DateTime.Today)
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Expiry date cannot be in the past"
                    ));
                }

                // Add stock
                int stockId = await InventoryStockDL.AddStockAsync(
                    request.InventoryItemID,
                    request.CenterID,
                    request.CompanyID,
                    request.BatchNumber,
                    request.ManufactureDate,
                    request.ExpiryDate,
                    request.PurchaseDate,
                    request.PurchaseCost,
                    request.Quantity,
                    request.CreatedBy
                );

                // Get created stock
                var dt = await InventoryStockDL.GetStockByIdAsync(stockId);
                var stock = ConvertRowToStock(dt.Rows[0]);

                // Check if individual tracking was enabled
                bool isIndividualTracking = Convert.ToBoolean(dtItem.Rows[0]["IsIndividualQtyTracking"]);
                string message = isIndividualTracking
                    ? $"Stock added successfully. {request.Quantity} individual item(s) created for tracking."
                    : "Stock added successfully";

                return Ok(ApiResponse<InventoryStockResponse>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    message,
                    stock
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventoryStockResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error adding stock: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Helper Methods

        private InventoryStockResponse ConvertRowToStock(DataRow row)
        {
            return new InventoryStockResponse
            {
                StockID = Convert.ToInt32(row["StockID"]),
                InventoryItemID = Convert.ToInt32(row["InventoryItemID"]),
                ItemCode = row["ItemCode"]?.ToString() ?? "",
                ItemName = row["ItemName"]?.ToString() ?? "",
                CenterID = Convert.ToInt32(row["CenterID"]),
                CenterName = row["CenterName"]?.ToString() ?? "",
                CompanyID = Convert.ToInt32(row["CompanyID"]),
                BatchNumber = row["BatchNumber"]?.ToString(),
                ManufactureDate = row["ManufactureDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["ManufactureDate"])
                    : null,
                ExpiryDate = row["ExpiryDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["ExpiryDate"])
                    : null,
                PurchaseDate = row["PurchaseDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["PurchaseDate"])
                    : null,
                PurchaseCost = row["PurchaseCost"] != DBNull.Value
                    ? Convert.ToDecimal(row["PurchaseCost"])
                    : null,
                Quantity = Convert.ToInt32(row["Quantity"]),
                AvailableQuantity = Convert.ToInt32(row["AvailableQuantity"]),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private List<InventoryStockResponse> ConvertDataTableToStockList(DataTable dt)
        {
            var stocks = new List<InventoryStockResponse>();
            foreach (DataRow row in dt.Rows)
            {
                stocks.Add(ConvertRowToStock(row));
            }
            return stocks;
        }

        #endregion
    }
}
