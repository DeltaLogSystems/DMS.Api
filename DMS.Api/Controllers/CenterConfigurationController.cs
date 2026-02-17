using System.Data;
using DMS.Api.DL;
using DMS.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CenterConfigurationController : ControllerBase
    {
        /// <summary>
        /// Get center configuration by center ID
        /// </summary>
        [HttpGet("{centerId}")]
        public async Task<IActionResult> GetCenterConfiguration(int centerId)
        {
            try
            {
                var dt = await CenterConfigurationDL.GetConfigurationByCenterIdAsync(centerId);

                if (dt.Rows.Count == 0)
                {
                    return NotFound(ApiResponse<CenterConfigurationResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Center configuration not found"
                    ));
                }

                var config = ConvertRowToConfiguration(dt.Rows[0]);

                return Ok(ApiResponse<CenterConfigurationResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Configuration retrieved successfully",
                    config
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CenterConfigurationResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving configuration: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Update center configuration
        /// </summary>
        [HttpPut("{centerId}")]
        public async Task<IActionResult> UpdateCenterConfiguration(
            int centerId,
            [FromBody] UpdateCenterConfigurationRequest request)
        {
            try
            {
                int result = await CenterConfigurationDL.UpdateFullConfigurationByCenterIdAsync(
                    centerId,
                    request.MachineSessionHours,
                    request.IsFixedHoursForSession,
                    request.CenterOpenTime,
                    request.CenterCloseTime,
                    request.SlotDuration,
                    request.AutoCancelOverdueAppointments,
                    request.AutoCancelOverdueSessions,
                    request.OverdueThresholdHours
                );

                if (result > 0)
                {
                    var dt = await CenterConfigurationDL.GetConfigurationByCenterIdAsync(centerId);
                    var config = ConvertRowToConfiguration(dt.Rows[0]);

                    return Ok(ApiResponse<CenterConfigurationResponse>.SuccessResponse(
                        ResponseStatus.DataUpdated,
                        "Configuration updated successfully",
                        config
                    ));
                }

                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to update configuration"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error updating configuration: {ex.Message}"
                ));
            }
        }

        private CenterConfigurationResponse ConvertRowToConfiguration(DataRow row)
        {
            return new CenterConfigurationResponse
            {
                ConfigurationID = Convert.ToInt32(row["ConfigurationID"]),
                CenterID = Convert.ToInt32(row["CenterID"]),
                MachineSessionHours = row["MachineSessionHours"] != DBNull.Value
                    ? Convert.ToDecimal(row["MachineSessionHours"])
                    : null,
                IsFixedHoursForSession = Convert.ToBoolean(row["IsFixedHoursForSession"]),
                CenterOpenTime = row["CenterOpenTime"] != DBNull.Value
                    ? TimeSpan.Parse(row["CenterOpenTime"].ToString() ?? "08:00:00")
                    : TimeSpan.FromHours(8),
                CenterCloseTime = row["CenterCloseTime"] != DBNull.Value
                    ? TimeSpan.Parse(row["CenterCloseTime"].ToString() ?? "20:00:00")
                    : TimeSpan.FromHours(20),
                SlotDuration = row["SlotDuration"] != DBNull.Value
                    ? Convert.ToInt32(row["SlotDuration"])
                    : 240,
                AutoCancelOverdueAppointments = row.Table.Columns.Contains("AutoCancelOverdueAppointments") && row["AutoCancelOverdueAppointments"] != DBNull.Value
                    ? Convert.ToBoolean(row["AutoCancelOverdueAppointments"])
                    : false,
                AutoCancelOverdueSessions = row.Table.Columns.Contains("AutoCancelOverdueSessions") && row["AutoCancelOverdueSessions"] != DBNull.Value
                    ? Convert.ToBoolean(row["AutoCancelOverdueSessions"])
                    : false,
                OverdueThresholdHours = row.Table.Columns.Contains("OverdueThresholdHours") && row["OverdueThresholdHours"] != DBNull.Value
                    ? Convert.ToInt32(row["OverdueThresholdHours"])
                    : 24
            };
        }
    }
}
