using Microsoft.AspNetCore.Mvc;
using DMS.Api.DL;
using DMS.Api.Shared;
using System.Data;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Get all maintenance records
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllMaintenance(
            [FromQuery] int? assetId = null,
            [FromQuery] int? centerId = null)
        {
            try
            {
                var dt = await MaintenanceDL.GetAllMaintenanceAsync(assetId, centerId);
                var maintenance = ConvertDataTableToMaintenanceList(dt);

                return Ok(ApiResponse<List<MaintenanceResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {maintenance.Count} maintenance record(s)",
                    maintenance
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<MaintenanceResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving maintenance records: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get maintenance by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMaintenanceById(int id)
        {
            try
            {
                var dt = await MaintenanceDL.GetMaintenanceByIdAsync(id);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<MaintenanceResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Maintenance record not found"
                    ));
                }

                var maintenance = ConvertRowToMaintenance(dt.Rows[0]);

                return Ok(ApiResponse<MaintenanceResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Maintenance record retrieved successfully",
                    maintenance
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<MaintenanceResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving maintenance record: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get maintenance history for specific asset
        /// </summary>
        [HttpGet("asset/{assetId}")]
        public async Task<IActionResult> GetMaintenanceByAsset(int assetId)
        {
            try
            {
                var dt = await MaintenanceDL.GetAllMaintenanceAsync(assetId, null);
                var maintenance = ConvertDataTableToMaintenanceList(dt);

                return Ok(ApiResponse<List<MaintenanceResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {maintenance.Count} maintenance record(s)",
                    maintenance
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<MaintenanceResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving maintenance records: {ex.Message}"
                ));
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Create maintenance record
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMaintenance([FromBody] MaintenanceRequest request)
        {
            try
            {
                // Validate maintenance type
                var validTypes = new[] { "Preventive", "Corrective", "Calibration" };
                if (!validTypes.Contains(request.MaintenanceType))
                {
                    return BadRequest(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Invalid maintenance type. Must be: Preventive, Corrective, or Calibration"
                    ));
                }

                // Validate asset exists
                var dtAsset = await AssetsDL.GetAssetByIdAsync(request.AssetID);
                if (dtAsset.Rows.Count == 0)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Asset not found"
                    ));
                }

                // Create maintenance record
                int maintenanceId = await MaintenanceDL.CreateMaintenanceAsync(
                    request.AssetID,
                    request.MaintenanceDate,
                    request.MaintenanceType,
                    request.Description,
                    request.TechnicianName,
                    request.Cost,
                    request.NextMaintenanceDate,
                    request.CreatedBy
                );

                // Get created maintenance details
                var dtMaintenance = await MaintenanceDL.GetMaintenanceByIdAsync(maintenanceId);
                var maintenance = ConvertRowToMaintenance(dtMaintenance.Rows[0]);

                return Ok(ApiResponse<MaintenanceResponse>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    "Maintenance record created successfully",
                    maintenance
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<MaintenanceResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error creating maintenance record: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Helper Methods

        private MaintenanceResponse ConvertRowToMaintenance(DataRow row)
        {
            return new MaintenanceResponse
            {
                MaintenanceID = Convert.ToInt32(row["MaintenanceID"]),
                AssetID = Convert.ToInt32(row["AssetID"]),
                AssetCode = row["AssetCode"]?.ToString() ?? "",
                AssetName = row["AssetName"]?.ToString() ?? "",
                MaintenanceDate = Convert.ToDateTime(row["MaintenanceDate"]),
                MaintenanceType = row["MaintenanceType"]?.ToString() ?? "",
                Description = row["Description"]?.ToString(),
                TechnicianName = row["TechnicianName"]?.ToString(),
                Cost = row["Cost"] != DBNull.Value ? Convert.ToDecimal(row["Cost"]) : null,
                NextMaintenanceDate = row["NextMaintenanceDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["NextMaintenanceDate"])
                    : null,
                Status = row["Status"]?.ToString() ?? "",
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private List<MaintenanceResponse> ConvertDataTableToMaintenanceList(DataTable dt)
        {
            var maintenance = new List<MaintenanceResponse>();
            foreach (DataRow row in dt.Rows)
            {
                maintenance.Add(ConvertRowToMaintenance(row));
            }
            return maintenance;
        }

        #endregion
    }
}
