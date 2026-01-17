using Microsoft.AspNetCore.Mvc;
using DMS.Api.DL;
using DMS.Api.Shared;
using System.Data;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Get all assets
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAssets(
            [FromQuery] int? centerId = null,
            [FromQuery] bool? activeOnly = null)
        {
            try
            {
                var dt = await AssetsDL.GetAllAssetsAsync(centerId, activeOnly);
                var assets = ConvertDataTableToAssetList(dt);

                return Ok(ApiResponse<List<AssetResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {assets.Count} asset(s)",
                    assets
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AssetResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving assets: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get asset by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssetById(int id)
        {
            try
            {
                var dt = await AssetsDL.GetAssetByIdAsync(id);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<AssetResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Asset not found"
                    ));
                }

                var asset = ConvertRowToAsset(dt.Rows[0]);

                return Ok(ApiResponse<AssetResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Asset retrieved successfully",
                    asset
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AssetResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving asset: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get assets by type (e.g., all dialysis machines)
        /// </summary>
        [HttpGet("type/{assetTypeId}")]
        public async Task<IActionResult> GetAssetsByType(
            int assetTypeId,
            [FromQuery] int? centerId = null,
            [FromQuery] bool activeOnly = true)
        {
            try
            {
                var dt = await AssetsDL.GetAssetsByTypeAsync(assetTypeId, centerId, activeOnly);
                var assets = ConvertDataTableToAssetList(dt);

                return Ok(ApiResponse<List<AssetResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {assets.Count} asset(s)",
                    assets
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AssetResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving assets: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get assets requiring maintenance
        /// </summary>
        [HttpGet("maintenance-due")]
        public async Task<IActionResult> GetAssetsRequiringMaintenance(
            [FromQuery] int? centerId = null,
            [FromQuery] int daysThreshold = 7)
        {
            try
            {
                var dt = await AssetsDL.GetAssetsRequiringMaintenanceAsync(centerId, daysThreshold);
                var assets = new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    var asset = ConvertRowToAsset(row);
                    assets.Add(new
                    {
                        asset = asset,
                        daysUntilMaintenance = row["DaysUntilMaintenance"] != DBNull.Value
                            ? Convert.ToInt32(row["DaysUntilMaintenance"])
                            : 0,
                        isOverdue = row["DaysUntilMaintenance"] != DBNull.Value &&
                                   Convert.ToInt32(row["DaysUntilMaintenance"]) < 0
                    });
                }

                return Ok(ApiResponse<List<object>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {assets.Count} asset(s) requiring maintenance",
                    assets
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving assets: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get available assets for specific time slot
        /// </summary>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableAssets(
            [FromQuery] int centerId,
            [FromQuery] int assetTypeId,
            [FromQuery] DateTime date,
            [FromQuery] string startTime,
            [FromQuery] string endTime)
        {
            try
            {
                TimeSpan slotStartTime = TimeSpan.Parse(startTime);
                TimeSpan slotEndTime = TimeSpan.Parse(endTime);

                var dt = await AssetsDL.GetAvailableAssetsAsync(
                    centerId,
                    assetTypeId,
                    date,
                    slotStartTime,
                    slotEndTime
                );

                var assets = ConvertDataTableToAssetList(dt);

                return Ok(ApiResponse<List<AssetResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"{assets.Count} asset(s) available",
                    assets
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AssetResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error checking asset availability: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get asset statistics for dashboard
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetAssetStatistics([FromQuery] int? centerId = null)
        {
            try
            {
                var dtAll = await AssetsDL.GetAllAssetsAsync(centerId, null);
                var dtMaintenance = await AssetsDL.GetAssetsRequiringMaintenanceAsync(centerId, 7);

                var statistics = new
                {
                    totalAssets = dtAll.Rows.Count,
                    activeAssets = dtAll.AsEnumerable().Count(row => Convert.ToBoolean(row["IsActive"])),
                    inactiveAssets = dtAll.AsEnumerable().Count(row => !Convert.ToBoolean(row["IsActive"])),
                    maintenanceDue = dtMaintenance.Rows.Count,
                    maintenanceOverdue = dtMaintenance.AsEnumerable()
                        .Count(row => Convert.ToInt32(row["DaysUntilMaintenance"]) < 0),
                    assetsByType = dtAll.AsEnumerable()
                        .GroupBy(row => row["AssetTypeName"]?.ToString())
                        .Select(g => new
                        {
                            assetType = g.Key,
                            count = g.Count(),
                            active = g.Count(row => Convert.ToBoolean(row["IsActive"]))
                        })
                        .ToList()
                };

                return Ok(ApiResponse<object>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Asset statistics retrieved successfully",
                    statistics
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving statistics: {ex.Message}"
                ));
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Create new asset
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAsset([FromBody] AssetRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.AssetName))
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Asset name is required"
                    ));
                }

                // Validate asset type exists
                var dtType = await AssetTypesDL.GetAssetTypeByIdAsync(request.AssetType);
                if (dtType.Rows.Count == 0)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Asset type not found"
                    ));
                }

                // Validate center exists
                var dtCenter = await CentersDL.GetCenterByIdAsync(request.CenterID);
                if (dtCenter.Rows.Count == 0)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Center not found"
                    ));
                }

                // Create asset
                int assetId = await AssetsDL.CreateAssetAsync(
                    request.AssetName,
                    request.AssetType,
                    request.SerialNo,
                    request.ModelNo,
                    request.Manufacturer,
                    request.PurchaseDate,
                    request.PurchaseCost,
                    request.WarrantyExpiryDate,
                    request.CenterID,
                    request.CompanyID,
                    request.CreatedBy
                );

                // Get created asset details
                var dtAsset = await AssetsDL.GetAssetByIdAsync(assetId);
                var asset = ConvertRowToAsset(dtAsset.Rows[0]);

                return Ok(ApiResponse<AssetResponse>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    "Asset created successfully",
                    asset
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AssetResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error creating asset: {ex.Message}"
                ));
            }
        }

        #endregion

        #region PUT Endpoints

        /// <summary>
        /// Update asset details
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsset(int id, [FromBody] AssetRequest request)
        {
            try
            {
                // Check if asset exists
                var dtExisting = await AssetsDL.GetAssetByIdAsync(id);
                if (dtExisting.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Asset not found"
                    ));
                }

                // Update asset
                int result = await AssetsDL.UpdateAssetAsync(
                    id,
                    request.AssetName,
                    request.SerialNo,
                    request.ModelNo,
                    request.Manufacturer,
                    request.PurchaseDate,
                    request.PurchaseCost,
                    request.WarrantyExpiryDate,
                    request.CreatedBy // Should be ModifiedBy
                );

                if (result > 0)
                {
                    // Get updated asset
                    var dtAsset = await AssetsDL.GetAssetByIdAsync(id);
                    var asset = ConvertRowToAsset(dtAsset.Rows[0]);

                    return Ok(ApiResponse<AssetResponse>.SuccessResponse(
                        ResponseStatus.DataUpdated,
                        "Asset updated successfully",
                        asset
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to update asset"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error updating asset: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Update asset status (Activate/Deactivate)
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateAssetStatus(int id, [FromBody] UpdateAssetStatusRequest request)
        {
            try
            {
                // Check if asset exists
                var dtExisting = await AssetsDL.GetAssetByIdAsync(id);
                if (dtExisting.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Asset not found"
                    ));
                }

                var currentStatus = Convert.ToBoolean(dtExisting.Rows[0]["IsActive"]);

                // Validate deactivation requires reason
                if (!request.IsActive && string.IsNullOrWhiteSpace(request.Reason))
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Reason is required when deactivating an asset"
                    ));
                }

                // Update status
                int result = await AssetsDL.UpdateAssetStatusAsync(
                    id,
                    request.IsActive,
                    request.Reason,
                    request.ExpectedActiveDate,
                    request.ModifiedBy
                );

                if (result > 0)
                {
                    string message = request.IsActive
                        ? "Asset activated successfully"
                        : $"Asset deactivated successfully. {(request.ExpectedActiveDate.HasValue ? $"Expected back on {request.ExpectedActiveDate.Value:yyyy-MM-dd}" : "")}";

                    return Ok(ApiResponse.SuccessResponse(message));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to update asset status"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error updating asset status: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Activate asset (make it available)
        /// </summary>
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateAsset(int id, [FromQuery] int modifiedBy)
        {
            try
            {
                var request = new UpdateAssetStatusRequest
                {
                    AssetID = id,
                    IsActive = true,
                    Reason = "Asset reactivated and ready for use",
                    ModifiedBy = modifiedBy
                };

                return await UpdateAssetStatus(id, request);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error activating asset: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Deactivate asset (maintenance, repair, etc.)
        /// </summary>
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateAsset(
            int id,
            [FromQuery] string reason,
            [FromQuery] DateTime? expectedActiveDate,
            [FromQuery] int modifiedBy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    return BadRequest(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Reason is required for deactivation"
                    ));
                }

                var request = new UpdateAssetStatusRequest
                {
                    AssetID = id,
                    IsActive = false,
                    Reason = reason,
                    ExpectedActiveDate = expectedActiveDate,
                    ModifiedBy = modifiedBy
                };

                return await UpdateAssetStatus(id, request);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error deactivating asset: {ex.Message}"
                ));
            }
        }

        #endregion

        #region DELETE Endpoints

        /// <summary>
        /// Delete asset
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            try
            {
                int result = await AssetsDL.DeleteAssetAsync(id);

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Asset deleted successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.NotFound,
                    "Asset not found"
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
                    $"Error deleting asset: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Helper Methods

        private AssetResponse ConvertRowToAsset(DataRow row)
        {
            return new AssetResponse
            {
                AssetID = Convert.ToInt32(row["AssetID"]),
                AssetCode = row["AssetCode"]?.ToString() ?? "",
                AssetName = row["AssetName"]?.ToString() ?? "",
                AssetType = Convert.ToInt32(row["AssetType"]),
                AssetTypeName = row["AssetTypeName"]?.ToString() ?? "",
                SerialNo = row["SerialNo"]?.ToString(),
                ModelNo = row["ModelNo"]?.ToString(),
                Manufacturer = row["Manufacturer"]?.ToString(),
                PurchaseDate = row["PurchaseDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["PurchaseDate"])
                    : null,
                PurchaseCost = row["PurchaseCost"] != DBNull.Value
                    ? Convert.ToDecimal(row["PurchaseCost"])
                    : null,
                WarrantyExpiryDate = row["WarrantyExpiryDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["WarrantyExpiryDate"])
                    : null,
                CenterID = Convert.ToInt32(row["CenterID"]),
                CenterName = row["CenterName"]?.ToString() ?? "",
                CompanyID = Convert.ToInt32(row["CompanyID"]),
                CompanyName = row["CompanyName"]?.ToString() ?? "",
                IsActive = Convert.ToBoolean(row["IsActive"]),
                InactiveReason = row["InactiveReason"]?.ToString(),
                InactiveDate = row["InactiveDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["InactiveDate"])
                    : null,
                ExpectedActiveDate = row["ExpectedActiveDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["ExpectedActiveDate"])
                    : null,
                LastMaintenanceDate = row["LastMaintenanceDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["LastMaintenanceDate"])
                    : null,
                NextMaintenanceDate = row["NextMaintenanceDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["NextMaintenanceDate"])
                    : null,
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private List<AssetResponse> ConvertDataTableToAssetList(DataTable dt)
        {
            var assets = new List<AssetResponse>();
            foreach (DataRow row in dt.Rows)
            {
                assets.Add(ConvertRowToAsset(row));
            }
            return assets;
        }

        #endregion
    }
}
