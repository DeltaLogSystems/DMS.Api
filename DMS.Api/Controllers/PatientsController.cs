using Microsoft.AspNetCore.Mvc;
using DMS.Api.DL;
using DMS.Api.Shared;
using System.Data;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        #region GET Endpoints

        /// <summary>
        /// Get all patients
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllPatients([FromQuery] bool activeOnly = false)
        {
            try
            {
                DataTable dt = activeOnly
                    ? await PatientsDL.GetActivePatientsAsync()
                    : await PatientsDL.GetAllPatientsAsync();

                var patients = ConvertDataTableToPatientList(dt);

                return Ok(ApiResponse<List<PatientResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {patients.Count} patient(s)",
                    patients
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<PatientResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving patients: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get patient by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            try
            {
                var dt = await PatientsDL.GetPatientByIdAsync(id);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<PatientResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Patient not found"
                    ));
                }

                var patient = ConvertRowToPatient(dt.Rows[0]);

                return Ok(ApiResponse<PatientResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Patient retrieved successfully",
                    patient
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PatientResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving patient: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get patient by code
        /// </summary>
        [HttpGet("code/{patientCode}")]
        public async Task<IActionResult> GetPatientByCode(string patientCode)
        {
            try
            {
                var dt = await PatientsDL.GetPatientByCodeAsync(patientCode);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<PatientResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Patient not found"
                    ));
                }

                var patient = ConvertRowToPatient(dt.Rows[0]);

                return Ok(ApiResponse<PatientResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Patient retrieved successfully",
                    patient
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PatientResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving patient: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get patient by mobile number
        /// </summary>
        [HttpGet("mobile/{mobileNo}")]
        public async Task<IActionResult> GetPatientByMobile(string mobileNo)
        {
            try
            {
                var dt = await PatientsDL.GetPatientByMobileAsync(mobileNo);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<PatientResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Patient not found"
                    ));
                }

                var patient = ConvertRowToPatient(dt.Rows[0]);

                return Ok(ApiResponse<PatientResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Patient retrieved successfully",
                    patient
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PatientResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving patient: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get patients by company
        /// </summary>
        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetPatientsByCompany(int companyId)
        {
            try
            {
                var dt = await PatientsDL.GetPatientsByCompanyIdAsync(companyId);
                var patients = ConvertDataTableToPatientList(dt);

                return Ok(ApiResponse<List<PatientResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {patients.Count} patient(s)",
                    patients
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<PatientResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving patients: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get patients by center
        /// </summary>
        [HttpGet("center/{centerId}")]
        public async Task<IActionResult> GetPatientsByCenter(int centerId)
        {
            try
            {
                var dt = await PatientsDL.GetPatientsByCenterIdAsync(centerId);
                var patients = ConvertDataTableToPatientList(dt);

                return Ok(ApiResponse<List<PatientResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {patients.Count} patient(s)",
                    patients
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<PatientResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving patients: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Search patients
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchPatients(
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? companyId = null,
            [FromQuery] int? centerId = null,
            [FromQuery] int? schemeType = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var dt = await PatientsDL.SearchPatientsAsync(searchTerm, companyId, centerId, schemeType, isActive);
                var patients = ConvertDataTableToPatientList(dt);

                return Ok(ApiResponse<List<PatientResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Found {patients.Count} patient(s)",
                    patients
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<PatientResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error searching patients: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Check if mobile number exists
        /// </summary>
        [HttpGet("check-mobile/{mobileNo}")]
        public async Task<IActionResult> CheckMobileExists(string mobileNo, [FromQuery] int? excludePatientId = null)
        {
            try
            {
                bool exists = await PatientsDL.MobileNumberExistsAsync(mobileNo, excludePatientId);

                return Ok(new ApiResponse<object>(
                    exists ? ResponseStatus.AlreadyExists : ResponseStatus.Success,
                    exists ? "Mobile number already registered" : "Mobile number is available",
                    new { exists = exists, mobileNo = mobileNo }
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error checking mobile number: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get patient statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetPatientStatistics([FromQuery] int? companyId = null, [FromQuery] int? centerId = null)
        {
            try
            {
                int totalCount = 0;
                int activeCount = 0;

                if (centerId.HasValue)
                {
                    totalCount = await PatientsDL.GetPatientCountByCenterAsync(centerId.Value, false);
                    activeCount = await PatientsDL.GetPatientCountByCenterAsync(centerId.Value, true);
                }
                else if (companyId.HasValue)
                {
                    totalCount = await PatientsDL.GetPatientCountByCompanyAsync(companyId.Value, false);
                    activeCount = await PatientsDL.GetPatientCountByCompanyAsync(companyId.Value, true);
                }

                var statistics = new
                {
                    totalPatients = totalCount,
                    activePatients = activeCount,
                    inactivePatients = totalCount - activeCount
                };

                return Ok(ApiResponse<object>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Statistics retrieved successfully",
                    statistics
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving statistics: {ex.Message}"
                ));
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Register new patient
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RegisterPatient([FromBody] PatientRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.PatientName))
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Patient name is required"
                    ));
                }

                if (string.IsNullOrWhiteSpace(request.MobileNo))
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Mobile number is required"
                    ));
                }

                if (request.DateOfBirth >= DateTime.Today)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Invalid date of birth"
                    ));
                }

                // Check if mobile number already exists
                bool mobileExists = await PatientsDL.MobileNumberExistsAsync(request.MobileNo);
                if (mobileExists)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.AlreadyExists,
                        "Mobile number already registered with another patient"
                    ));
                }

                // Insert patient
                int patientId = await PatientsDL.InsertPatientAsync(
                    request.PatientName,
                    request.CompanyID,
                    request.CenterID,
                    request.MobileNo,
                    request.DateOfBirth,
                    request.SchemeType,
                    request.CreatedBy
                );

                // Get the newly created patient details
                var dtPatient = await PatientsDL.GetPatientByIdAsync(patientId);
                var patient = ConvertRowToPatient(dtPatient.Rows[0]);

                return Ok(ApiResponse<PatientResponse>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    $"Patient registered successfully with code: {patient.PatientCode}",
                    patient
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PatientResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error registering patient: {ex.Message}"
                ));
            }
        }

        #endregion

        #region PUT Endpoints

        /// <summary>
        /// Update patient details
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] PatientUpdateRequest request)
        {
            try
            {
                // Validate patient exists
                var dtExisting = await PatientsDL.GetPatientByIdAsync(id);
                if (dtExisting.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Patient not found"
                    ));
                }

                // Validate request
                if (string.IsNullOrWhiteSpace(request.PatientName))
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Patient name is required"
                    ));
                }

                // Check if mobile number already exists for another patient
                bool mobileExists = await PatientsDL.MobileNumberExistsAsync(request.MobileNo, id);
                if (mobileExists)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.AlreadyExists,
                        "Mobile number already registered with another patient"
                    ));
                }

                // Update patient
                int result = await PatientsDL.UpdatePatientAsync(
                    id,
                    request.PatientName,
                    request.CompanyID,
                    request.CenterID,
                    request.MobileNo,
                    request.DateOfBirth,
                    request.SchemeType,
                    request.IsActive,
                    request.ModifiedBy
                );

                if (result > 0)
                {
                    // Get updated patient details
                    var dtUpdated = await PatientsDL.GetPatientByIdAsync(id);
                    var patient = ConvertRowToPatient(dtUpdated.Rows[0]);

                    return Ok(ApiResponse<PatientResponse>.SuccessResponse(
                        ResponseStatus.DataUpdated,
                        "Patient updated successfully",
                        patient
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to update patient"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error updating patient: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Toggle patient status
        /// </summary>
        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> TogglePatientStatus(int id, [FromBody] int modifiedBy)
        {
            try
            {
                int result = await PatientsDL.TogglePatientStatusAsync(id, modifiedBy);

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Patient status updated successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.NotFound,
                    "Patient not found"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error updating patient status: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Increment dialysis cycles
        /// </summary>
        [HttpPut("{id}/increment-cycles")]
        public async Task<IActionResult> IncrementDialysisCycles(int id)
        {
            try
            {
                int result = await PatientsDL.IncrementDialysisCyclesAsync(id);

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Dialysis cycle incremented successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.NotFound,
                    "Patient not found"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error incrementing dialysis cycles: {ex.Message}"
                ));
            }
        }

        #endregion

        #region DELETE Endpoints

        /// <summary>
        /// Delete patient (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id, [FromQuery] int modifiedBy)
        {
            try
            {
                int result = await PatientsDL.SoftDeletePatientAsync(id, modifiedBy);

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Patient deleted successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.NotFound,
                    "Patient not found"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error deleting patient: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Helper Methods

        private PatientResponse ConvertRowToPatient(DataRow row)
        {
            return new PatientResponse
            {
                PatientID = Convert.ToInt32(row["PatientID"]),
                PatientCode = row["PatientCode"]?.ToString() ?? "",
                PatientName = row["PatientName"]?.ToString() ?? "",
                CompanyID = Convert.ToInt32(row["CompanyID"]),
                CompanyName = row["CompanyName"]?.ToString() ?? "",
                CompanyCode = row["CompanyCode"]?.ToString() ?? "",
                CenterID = Convert.ToInt32(row["CenterID"]),
                CenterName = row["CenterName"]?.ToString() ?? "",
                MobileNo = row["MobileNo"]?.ToString() ?? "",
                DateOfBirth = Convert.ToDateTime(row["DateOfBirth"]),
                Age = Convert.ToInt32(row["Age"]),
                SchemeType = row["SchemeType"] != DBNull.Value ? Convert.ToInt32(row["SchemeType"]) : null,
                SchemeTypeName = row["SchemeTypeName"]?.ToString() ?? "",
                DialysisCycles = Convert.ToInt32(row["DialysisCycles"]),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private List<PatientResponse> ConvertDataTableToPatientList(DataTable dt)
        {
            var patients = new List<PatientResponse>();
            foreach (DataRow row in dt.Rows)
            {
                patients.Add(ConvertRowToPatient(row));
            }
            return patients;
        }

        #endregion
    }
}
