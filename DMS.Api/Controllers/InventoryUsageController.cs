using System.Data;
using DMS.Api.DL;
using DMS.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryUsageController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Get all usage records with filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllUsage(
            [FromQuery] int? centerId = null,
            [FromQuery] int? inventoryItemId = null,
            [FromQuery] int? appointmentId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var dt = await InventoryUsageDL.GetAllUsageAsync(
                    centerId,
                    inventoryItemId,
                    appointmentId,
                    patientId,
                    startDate,
                    endDate
                );

                var usages = ConvertDataTableToUsageList(dt);

                return Ok(ApiResponse<List<InventoryUsageResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {usages.Count} usage record(s)",
                    usages
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<InventoryUsageResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving usage records: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get usage by appointment
        /// </summary>
        [HttpGet("appointment/{appointmentId}")]
        public async Task<IActionResult> GetUsageByAppointment(int appointmentId)
        {
            try
            {
                var dt = await InventoryUsageDL.GetUsageByAppointmentAsync(appointmentId);
                var usages = ConvertDataTableToUsageList(dt);

                return Ok(ApiResponse<List<InventoryUsageResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {usages.Count} usage record(s) for appointment",
                    usages
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<InventoryUsageResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving usage records: {ex.Message}"
                ));
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Record inventory usage for dialysis session
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RecordUsage([FromBody] InventoryUsageRequest request)
        {
            try
            {
                // Validate request
                if (request.QuantityUsed <= 0)
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Quantity used must be greater than 0"
                    ));
                }

                // Validate stock availability
                var dtStock = await InventoryStockDL.GetStockByIdAsync(request.StockID);
                if (dtStock.Rows.Count == 0)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Stock not found"
                    ));
                }

                bool isIndividualTracking = Convert.ToBoolean(dtStock.Rows[0]["IsIndividualQtyTracking"]);

                // For individual tracking, validate individual item
                if (isIndividualTracking && !request.IndividualItemID.HasValue)
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Individual item must be selected for this inventory item"
                    ));
                }

                if (request.IndividualItemID.HasValue)
                {
                    // Validate individual item
                    var dtIndividual = await IndividualItemsDL.GetIndividualItemByIdAsync(request.IndividualItemID.Value);
                    if (dtIndividual.Rows.Count == 0)
                    {
                        return Ok(ApiResponse<int>.ErrorResponse(
                            ResponseStatus.NotFound,
                            "Individual item not found"
                        ));
                    }

                    if (!Convert.ToBoolean(dtIndividual.Rows[0]["IsAvailable"]))
                    {
                        return Ok(ApiResponse<int>.ErrorResponse(
                            ResponseStatus.ValidationError,
                            "Selected individual item is not available for use"
                        ));
                    }

                    int currentUsage = Convert.ToInt32(dtIndividual.Rows[0]["CurrentUsageCount"]);
                    int maxUsage = Convert.ToInt32(dtIndividual.Rows[0]["MaxUsageCount"]);

                    if (currentUsage >= maxUsage)
                    {
                        return Ok(ApiResponse<int>.ErrorResponse(
                            ResponseStatus.ValidationError,
                            "Selected individual item has reached maximum usage count"
                        ));
                    }
                }
                else
                {
                    // Check stock availability for non-individual items
                    int availableQty = Convert.ToInt32(dtStock.Rows[0]["AvailableQuantity"]);
                    if (availableQty < request.QuantityUsed)
                    {
                        return Ok(ApiResponse<int>.ErrorResponse(
                            ResponseStatus.ValidationError,
                            $"Insufficient stock. Available: {availableQty}, Requested: {request.QuantityUsed}"
                        ));
                    }
                }

                // Record usage
                int usageId = await InventoryUsageDL.RecordUsageAsync(
                    request.InventoryItemID,
                    request.IndividualItemID,
                    request.StockID,
                    request.CenterID,
                    request.AppointmentID,
                    request.PatientID,
                    request.QuantityUsed,
                    request.ItemCondition,
                    request.Notes,
                    request.UsedBy
                );

                return Ok(ApiResponse<int>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    "Inventory usage recorded successfully",
                    usageId
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error recording usage: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Helper Methods

        private InventoryUsageResponse ConvertRowToUsage(DataRow row)
        {
            return new InventoryUsageResponse
            {
                UsageID = Convert.ToInt32(row["UsageID"]),
                InventoryItemID = Convert.ToInt32(row["InventoryItemID"]),
                ItemName = row["ItemName"]?.ToString() ?? "",
                IndividualItemID = row["IndividualItemID"] != DBNull.Value
                    ? Convert.ToInt32(row["IndividualItemID"])
                    : null,
                IndividualItemCode = row["IndividualItemCode"]?.ToString(),
                StockID = Convert.ToInt32(row["StockID"]),
                AppointmentID = Convert.ToInt32(row["AppointmentID"]),
                PatientID = Convert.ToInt32(row["PatientID"]),
                PatientName = row["PatientName"]?.ToString() ?? "",
                PatientCode = row["PatientCode"]?.ToString() ?? "",
                UsageDate = Convert.ToDateTime(row["UsageDate"]),
                QuantityUsed = Convert.ToDecimal(row["QuantityUsed"]),
                UsageNumber = Convert.ToInt32(row["UsageNumber"]),
                ItemCondition = row["ItemCondition"]?.ToString(),
                Notes = row["Notes"]?.ToString(),
                UsedBy = Convert.ToInt32(row["UsedBy"]),
                UsedByName = "" // Can be populated from Users table if needed
            };
        }

        private List<InventoryUsageResponse> ConvertDataTableToUsageList(DataTable dt)
        {
            var usages = new List<InventoryUsageResponse>();
            foreach (DataRow row in dt.Rows)
            {
                usages.Add(ConvertRowToUsage(row));
            }
            return usages;
        }

        #endregion
    }
}
