using Microsoft.AspNetCore.Mvc;
using DMS.Api.DL;
using DMS.Api.Shared;
using System.Data;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MastersController : ControllerBase
    {
        #region Scheme Types

        /// <summary>
        /// Get all scheme types
        /// </summary>
        [HttpGet("scheme-types")]
        public async Task<IActionResult> GetAllSchemeTypes([FromQuery] bool activeOnly = true)
        {
            try
            {
                var dt = await SchemeTypesDL.GetAllSchemeTypesAsync(activeOnly);
                var schemeTypes = ConvertDataTableToSchemeTypeList(dt);

                return Ok(ApiResponse<List<SchemeTypeResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {schemeTypes.Count} scheme type(s)",
                    schemeTypes
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<SchemeTypeResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving scheme types: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get scheme type by ID
        /// </summary>
        [HttpGet("scheme-types/{id}")]
        public async Task<IActionResult> GetSchemeTypeById(int id)
        {
            try
            {
                var dt = await SchemeTypesDL.GetSchemeTypeByIdAsync(id);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<SchemeTypeResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Scheme type not found"
                    ));
                }

                var schemeType = ConvertRowToSchemeType(dt.Rows[0]);

                return Ok(ApiResponse<SchemeTypeResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Scheme type retrieved successfully",
                    schemeType
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SchemeTypeResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving scheme type: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Add new scheme type
        /// </summary>
        [HttpPost("scheme-types")]
        public async Task<IActionResult> AddSchemeType([FromBody] SchemeTypeRequest request)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(request.SchemeTypeName))
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Scheme type name is required"
                    ));
                }

                // Check if name exists
                bool exists = await SchemeTypesDL.SchemeTypeNameExistsAsync(request.SchemeTypeName);
                if (exists)
                {
                    return Ok(ApiResponse<int>.ErrorResponse(
                        ResponseStatus.AlreadyExists,
                        "Scheme type name already exists"
                    ));
                }

                // Insert
                int schemeTypeId = await SchemeTypesDL.InsertSchemeTypeAsync(
                    request.SchemeTypeName,
                    request.Description,
                    request.CreatedBy
                );

                return Ok(ApiResponse<int>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    "Scheme type added successfully",
                    schemeTypeId
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error adding scheme type: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Update scheme type
        /// </summary>
        [HttpPut("scheme-types/{id}")]
        public async Task<IActionResult> UpdateSchemeType(int id, [FromBody] SchemeTypeRequest request)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(request.SchemeTypeName))
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Scheme type name is required"
                    ));
                }

                // Check if exists
                var dtExisting = await SchemeTypesDL.GetSchemeTypeByIdAsync(id);
                if (dtExisting.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Scheme type not found"
                    ));
                }

                // Check if name exists for another record
                bool nameExists = await SchemeTypesDL.SchemeTypeNameExistsAsync(request.SchemeTypeName, id);
                if (nameExists)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.AlreadyExists,
                        "Scheme type name already exists"
                    ));
                }

                // Update
                int result = await SchemeTypesDL.UpdateSchemeTypeAsync(
                    id,
                    request.SchemeTypeName,
                    request.Description
                );

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Scheme type updated successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    "Failed to update scheme type"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error updating scheme type: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Toggle scheme type status
        /// </summary>
        [HttpPut("scheme-types/{id}/toggle-status")]
        public async Task<IActionResult> ToggleSchemeTypeStatus(int id)
        {
            try
            {
                int result = await SchemeTypesDL.ToggleSchemeTypeStatusAsync(id);

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Scheme type status updated successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.NotFound,
                    "Scheme type not found"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error updating scheme type status: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Delete scheme type
        /// </summary>
        [HttpDelete("scheme-types/{id}")]
        public async Task<IActionResult> DeleteSchemeType(int id)
        {
            try
            {
                int result = await SchemeTypesDL.SoftDeleteSchemeTypeAsync(id);

                if (result > 0)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        "Scheme type deleted successfully"
                    ));
                }

                return Ok(ApiResponse.ErrorResponse(
                    ResponseStatus.NotFound,
                    "Scheme type not found"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error deleting scheme type: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Roles

        /// <summary>
        /// Get all roles
        /// </summary>
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var dt = await RolesDL.GetAllRolesAsync();
                var roles = ConvertDataTableToRoleList(dt);

                return Ok(ApiResponse<List<RoleResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {roles.Count} role(s)",
                    roles
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<RoleResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving roles: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        [HttpGet("roles/{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            try
            {
                var dt = await RolesDL.GetRoleByIdAsync(id);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<RoleResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Role not found"
                    ));
                }

                var role = ConvertRowToRole(dt.Rows[0]);

                return Ok(ApiResponse<RoleResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Role retrieved successfully",
                    role
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<RoleResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving role: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Companies

        /// <summary>
        /// Get all companies (for dropdown)
        /// </summary>
        [HttpGet("companies")]
        public async Task<IActionResult> GetAllCompanies([FromQuery] bool activeOnly = true)
        {
            try
            {
                var dt = activeOnly
                    ? await CompaniesDL.GetActiveCompaniesAsync()
                    : await CompaniesDL.GetAllCompaniesAsync();

                var companies = ConvertDataTableToCompanyList(dt);

                return Ok(ApiResponse<List<CompanyResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {companies.Count} company(ies)",
                    companies
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<CompanyResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving companies: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Centers

        /// <summary>
        /// Get centers by company (for dropdown)
        /// </summary>
        [HttpGet("centers/by-company/{companyId}")]
        public async Task<IActionResult> GetCentersByCompany(int companyId, [FromQuery] bool activeOnly = true)
        {
            try
            {
                var dt = activeOnly
                    ? await CentersDL.GetActiveCentersByCompanyIdAsync(companyId)
                    : await CentersDL.GetCentersByCompanyIdAsync(companyId);

                var centers = ConvertDataTableToCenterList(dt);

                return Ok(ApiResponse<List<CenterResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {centers.Count} center(s)",
                    centers
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<CenterResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving centers: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get all centers (for dropdown)
        /// </summary>
        [HttpGet("centers")]
        public async Task<IActionResult> GetAllCenters([FromQuery] bool activeOnly = true)
        {
            try
            {
                var dt = activeOnly
                    ? await CentersDL.GetActiveCentersAsync()
                    : await CentersDL.GetAllCentersAsync();

                var centers = ConvertDataTableToCenterList(dt);

                return Ok(ApiResponse<List<CenterResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {centers.Count} center(s)",
                    centers
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<CenterResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving centers: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Helper Methods

        private SchemeTypeResponse ConvertRowToSchemeType(DataRow row)
        {
            return new SchemeTypeResponse
            {
                SchemeTypeID = Convert.ToInt32(row["SchemeTypeID"]),
                SchemeTypeName = row["SchemeTypeName"]?.ToString() ?? "",
                Description = row["Description"]?.ToString() ?? "",
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private List<SchemeTypeResponse> ConvertDataTableToSchemeTypeList(DataTable dt)
        {
            var schemeTypes = new List<SchemeTypeResponse>();
            foreach (DataRow row in dt.Rows)
            {
                schemeTypes.Add(ConvertRowToSchemeType(row));
            }
            return schemeTypes;
        }

        private RoleResponse ConvertRowToRole(DataRow row)
        {
            return new RoleResponse
            {
                RoleID = Convert.ToInt32(row["RoleID"]),
                RoleName = row["RoleName"]?.ToString() ?? ""
            };
        }

        private List<RoleResponse> ConvertDataTableToRoleList(DataTable dt)
        {
            var roles = new List<RoleResponse>();
            foreach (DataRow row in dt.Rows)
            {
                roles.Add(ConvertRowToRole(row));
            }
            return roles;
        }

        private CompanyResponse ConvertRowToCompany(DataRow row)
        {
            return new CompanyResponse
            {
                CompanyID = Convert.ToInt32(row["CompanyID"]),
                CompanyName = row["CompanyName"]?.ToString() ?? "",
                CompanyCode = row["CompanyCode"]?.ToString() ?? "",
                CompanyAddress = row["CompanyAddress"]?.ToString() ?? "",
                IsActive = Convert.ToBoolean(row["IsActive"])
            };
        }

        private List<CompanyResponse> ConvertDataTableToCompanyList(DataTable dt)
        {
            var companies = new List<CompanyResponse>();
            foreach (DataRow row in dt.Rows)
            {
                companies.Add(ConvertRowToCompany(row));
            }
            return companies;
        }

        private CenterResponse ConvertRowToCenter(DataRow row)
        {
            return new CenterResponse
            {
                CenterID = Convert.ToInt32(row["CenterID"]),
                CenterName = row["CenterName"]?.ToString() ?? "",
                CenterAddress = row["CenterAddress"]?.ToString() ?? "",
                CompanyID = Convert.ToInt32(row["CompanyID"]),
                CompanyName = row["CompanyName"]?.ToString() ?? "",
                IsActive = Convert.ToBoolean(row["IsActive"])
            };
        }

        private List<CenterResponse> ConvertDataTableToCenterList(DataTable dt)
        {
            var centers = new List<CenterResponse>();
            foreach (DataRow row in dt.Rows)
            {
                centers.Add(ConvertRowToCenter(row));
            }
            return centers;
        }

        #endregion

        #region Company and Center by User/Patient

        /// <summary>
        /// Get company details by user ID
        /// </summary>
        [HttpGet("company/by-user/{userId}")]
        public async Task<IActionResult> GetCompanyByUserId(int userId)
        {
            try
            {
                var dt = await CompaniesDL.GetCompanyByUserIdAsync(userId);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<CompanyResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Company not found for this user"
                    ));
                }

                var company = ConvertRowToCompany(dt.Rows[0]);

                return Ok(ApiResponse<CompanyResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Company details retrieved successfully",
                    company
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CompanyResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving company details: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get company details by patient ID
        /// </summary>
        [HttpGet("company/by-patient/{patientId}")]
        public async Task<IActionResult> GetCompanyByPatientId(int patientId)
        {
            try
            {
                var dt = await CompaniesDL.GetCompanyByPatientIdAsync(patientId);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<CompanyResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Company not found for this patient"
                    ));
                }

                var company = ConvertRowToCompany(dt.Rows[0]);

                return Ok(ApiResponse<CompanyResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Company details retrieved successfully",
                    company
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CompanyResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving company details: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get center details by user ID
        /// </summary>
        [HttpGet("center/by-user/{userId}")]
        public async Task<IActionResult> GetCenterByUserId(int userId)
        {
            try
            {
                var dt = await CentersDL.GetCenterByUserIdAsync(userId);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<CenterResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Center not found for this user"
                    ));
                }

                var center = ConvertRowToCenter(dt.Rows[0]);

                return Ok(ApiResponse<CenterResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Center details retrieved successfully",
                    center
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CenterResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving center details: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get center details by patient ID
        /// </summary>
        [HttpGet("center/by-patient/{patientId}")]
        public async Task<IActionResult> GetCenterByPatientId(int patientId)
        {
            try
            {
                var dt = await CentersDL.GetCenterByPatientIdAsync(patientId);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<CenterResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Center not found for this patient"
                    ));
                }

                var center = ConvertRowToCenter(dt.Rows[0]);

                return Ok(ApiResponse<CenterResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Center details retrieved successfully",
                    center
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CenterResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving center details: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get company and center details by user ID (combined)
        /// </summary>
        [HttpGet("company-center/by-user/{userId}")]
        public async Task<IActionResult> GetCompanyCenterByUserId(int userId)
        {
            try
            {
                var dt = await CentersDL.GetCompanyAndCenterByUserIdAsync(userId);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<CompanyCenterResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Company and center details not found for this user"
                    ));
                }

                var row = dt.Rows[0];
                var response = new CompanyCenterResponse
                {
                    Company = new CompanyInfo
                    {
                        CompanyID = Convert.ToInt32(row["CompanyID"]),
                        CompanyName = row["CompanyName"]?.ToString() ?? "",
                        CompanyCode = row["CompanyCode"]?.ToString() ?? "",
                        CompanyAddress = row["CompanyAddress"]?.ToString() ?? "",
                        CompanyLogo = row["CompanyLogo"]?.ToString() ?? "",
                        IsActive = Convert.ToBoolean(row["CompanyIsActive"])
                    },
                    Center = new CenterInfo
                    {
                        CenterID = Convert.ToInt32(row["CenterID"]),
                        CenterName = row["CenterName"]?.ToString() ?? "",
                        CenterAddress = row["CenterAddress"]?.ToString() ?? "",
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    }
                };

                return Ok(ApiResponse<CompanyCenterResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Company and center details retrieved successfully",
                    response
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CompanyCenterResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving company and center details: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get company and center details by patient ID (combined)
        /// </summary>
        [HttpGet("company-center/by-patient/{patientId}")]
        public async Task<IActionResult> GetCompanyCenterByPatientId(int patientId)
        {
            try
            {
                var dt = await CentersDL.GetCompanyAndCenterByPatientIdAsync(patientId);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<CompanyCenterResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Company and center details not found for this patient"
                    ));
                }

                var row = dt.Rows[0];
                var response = new CompanyCenterResponse
                {
                    Company = new CompanyInfo
                    {
                        CompanyID = Convert.ToInt32(row["CompanyID"]),
                        CompanyName = row["CompanyName"]?.ToString() ?? "",
                        CompanyCode = row["CompanyCode"]?.ToString() ?? "",
                        CompanyAddress = row["CompanyAddress"]?.ToString() ?? "",
                        CompanyLogo = row["CompanyLogo"]?.ToString() ?? "",
                        IsActive = Convert.ToBoolean(row["CompanyIsActive"])
                    },
                    Center = new CenterInfo
                    {
                        CenterID = Convert.ToInt32(row["CenterID"]),
                        CenterName = row["CenterName"]?.ToString() ?? "",
                        CenterAddress = row["CenterAddress"]?.ToString() ?? "",
                        IsActive = Convert.ToBoolean(row["IsActive"])
                    }
                };

                return Ok(ApiResponse<CompanyCenterResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Company and center details retrieved successfully",
                    response
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CompanyCenterResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving company and center details: {ex.Message}"
                ));
            }
        }

        #endregion

        #region Appointment Status

        /// <summary>
        /// Get all appointment statuses
        /// </summary>
        [HttpGet("appointment-statuses")]
        public async Task<IActionResult> GetAllAppointmentStatuses([FromQuery] bool activeOnly = true)
        {
            try
            {
                var dt = await AppointmentStatusDL.GetAllAppointmentStatusesAsync(activeOnly);
                var statuses = ConvertDataTableToAppointmentStatusList(dt);

                return Ok(ApiResponse<List<AppointmentStatusResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {statuses.Count} appointment status(es)",
                    statuses
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AppointmentStatusResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving appointment statuses: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get appointment status by ID
        /// </summary>
        [HttpGet("appointment-statuses/{id}")]
        public async Task<IActionResult> GetAppointmentStatusById(int id)
        {
            try
            {
                var dt = await AppointmentStatusDL.GetAppointmentStatusByIdAsync(id);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<AppointmentStatusResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Appointment status not found"
                    ));
                }

                var status = ConvertRowToAppointmentStatus(dt.Rows[0]);

                return Ok(ApiResponse<AppointmentStatusResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Appointment status retrieved successfully",
                    status
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AppointmentStatusResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving appointment status: {ex.Message}"
                ));
            }
        }

        #endregion

        // Add these helper methods at the end of MastersController

        private AppointmentStatusResponse ConvertRowToAppointmentStatus(DataRow row)
        {
            return new AppointmentStatusResponse
            {
                StatusID = Convert.ToInt32(row["StatusID"]),
                StatusName = row["StatusName"]?.ToString() ?? "",
                StatusColor = row["StatusColor"]?.ToString() ?? ""
            };
        }

        private List<AppointmentStatusResponse> ConvertDataTableToAppointmentStatusList(DataTable dt)
        {
            var statuses = new List<AppointmentStatusResponse>();
            foreach (DataRow row in dt.Rows)
            {
                statuses.Add(ConvertRowToAppointmentStatus(row));
            }
            return statuses;
        }


        #region Asset Types

        /// <summary>
        /// Get all asset types
        /// </summary>
        [HttpGet("asset-types")]
        public async Task<IActionResult> GetAllAssetTypes([FromQuery] bool activeOnly = true)
        {
            try
            {
                var dt = await AssetTypesDL.GetAllAssetTypesAsync(activeOnly);
                var assetTypes = ConvertDataTableToAssetTypeList(dt);

                return Ok(ApiResponse<List<AssetTypeResponse>>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    $"Retrieved {assetTypes.Count} asset type(s)",
                    assetTypes
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AssetTypeResponse>>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving asset types: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get asset type by ID
        /// </summary>
        [HttpGet("asset-types/{id}")]
        public async Task<IActionResult> GetAssetTypeById(int id)
        {
            try
            {
                var dt = await AssetTypesDL.GetAssetTypeByIdAsync(id);

                if (dt.Rows.Count == 0)
                {
                    return Ok(ApiResponse<AssetTypeResponse>.ErrorResponse(
                        ResponseStatus.NotFound,
                        "Asset type not found"
                    ));
                }

                var assetType = ConvertRowToAssetType(dt.Rows[0]);

                return Ok(ApiResponse<AssetTypeResponse>.SuccessResponse(
                    ResponseStatus.DataRetrieved,
                    "Asset type retrieved successfully",
                    assetType
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AssetTypeResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving asset type: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Create new asset type
        /// </summary>
        [HttpPost("asset-types")]
        public async Task<IActionResult> CreateAssetType([FromBody] AssetTypeRequest request)
        {
            try
            {
                int assetTypeId = await AssetTypesDL.CreateAssetTypeAsync(
                    request.AssetTypeName,
                    request.AssetTypeCode,
                    request.Description,
                    request.RequiresMaintenance,
                    request.MaintenanceIntervalDays,
                    request.CreatedBy
                );

                return Ok(ApiResponse<int>.SuccessResponse(
                    ResponseStatus.DataSaved,
                    "Asset type created successfully",
                    assetTypeId
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error creating asset type: {ex.Message}"
                ));
            }
        }

        #endregion

        // Helper methods
        private AssetTypeResponse ConvertRowToAssetType(DataRow row)
        {
            return new AssetTypeResponse
            {
                AssetTypeID = Convert.ToInt32(row["AssetTypeID"]),
                AssetTypeName = row["AssetTypeName"]?.ToString() ?? "",
                AssetTypeCode = row["AssetTypeCode"]?.ToString() ?? "",
                Description = row["Description"]?.ToString(),
                RequiresMaintenance = Convert.ToBoolean(row["RequiresMaintenance"]),
                MaintenanceIntervalDays = Convert.ToInt32(row["MaintenanceIntervalDays"]),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private List<AssetTypeResponse> ConvertDataTableToAssetTypeList(DataTable dt)
        {
            var assetTypes = new List<AssetTypeResponse>();
            foreach (DataRow row in dt.Rows)
            {
                assetTypes.Add(ConvertRowToAssetType(row));
            }
            return assetTypes;
        }


    }
}
