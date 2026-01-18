using System.Data;
using DMS.Api.DL;
using DMS.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MachineAvailabilityController : ControllerBase
    {
        /// <summary>
        /// Get available dialysis machines for session
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAvailableMachines(
            [FromQuery] int centerId,
            [FromQuery] DateTime? sessionDate = null)
        {
            try
            {
                DateTime checkDate = sessionDate ?? DateTime.Today;

                // Get all dialysis machines for center - FIXED
                var dtMachines = await AssetsDL.GetAssetsByTypeNameAsync("Dialysis Machine", centerId, true);

                // Get active sessions for today
                var dtActiveSessions = await DialysisSessionsDL.GetActiveSessionsAsync(centerId);

                var availableMachines = new List<MachineAvailabilityResponse>();

                foreach (DataRow machineRow in dtMachines.Rows)
                {
                    int assetId = Convert.ToInt32(machineRow["AssetID"]);

                    var machine = new MachineAvailabilityResponse
                    {
                        AssetID = assetId,
                        AssetCode = machineRow["AssetCode"]?.ToString() ?? "",
                        AssetName = machineRow["AssetName"]?.ToString() ?? "",
                        AssetType = machineRow["AssetTypeName"]?.ToString() ?? "",
                        IsAvailable = true,
                        Status = "Available"
                    };

                    // Check if machine is in use
                    foreach (DataRow sessionRow in dtActiveSessions.Rows)
                    {
                        if (sessionRow["AssetID"] != DBNull.Value &&
                            Convert.ToInt32(sessionRow["AssetID"]) == assetId)
                        {
                            machine.IsAvailable = false;
                            machine.Status = "In Use";
                            machine.CurrentSessionID = Convert.ToInt32(sessionRow["SessionID"]);
                            machine.CurrentPatientName = sessionRow["PatientName"]?.ToString();
                            machine.CurrentPatientCode = sessionRow["PatientCode"]?.ToString();
                            machine.SessionStartTime = sessionRow["ActualStartTime"] != DBNull.Value
                                ? Convert.ToDateTime(sessionRow["ActualStartTime"])
                                : null;
                            machine.SessionElapsedMinutes = sessionRow["ElapsedMinutes"] != DBNull.Value
                                ? Convert.ToInt32(sessionRow["ElapsedMinutes"])
                                : null;
                            break;
                        }
                    }

                    availableMachines.Add(machine);
                }

                // Sort: Available first, then by asset code
                availableMachines = availableMachines
                    .OrderByDescending(m => m.IsAvailable)
                    .ThenBy(m => m.AssetCode)
                    .ToList();

                int availableCount = availableMachines.Count(m => m.IsAvailable);

                return Ok(ApiResponse<List<MachineAvailabilityResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {availableMachines.Count} machine(s). {availableCount} available, {availableMachines.Count - availableCount} in use.",
                    availableMachines
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<MachineAvailabilityResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving machine availability: {ex.Message}"
                ));
            }
        }
    }
}
