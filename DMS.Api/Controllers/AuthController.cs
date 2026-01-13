using Microsoft.AspNetCore.Mvc;
using DMS.Api.DL;
using DMS.Api.Shared;
using System.Data;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// User login endpoint
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return Ok(ApiResponse<LoginResponse>.ErrorResponse(
                        ResponseStatus.ValidationError,
                        "Username and password are required"
                    ));
                }

                // Step 1: Check if user exists
                bool userExists = await AuthenticationDL.UsernameExistsAsync(request.Username);

                if (!userExists)
                {
                    return Ok(ApiResponse<LoginResponse>.ErrorResponse(
                        ResponseStatus.UserNotFound,
                        "User not found. Please check your username"
                    ));
                }

                // Step 2: Validate credentials and get user details
                DataTable dtUser = await AuthenticationDL.ValidateLoginAsync(request.Username, request.Password);

                if (dtUser.Rows.Count == 0)
                {
                    return Ok(ApiResponse<LoginResponse>.ErrorResponse(
                        ResponseStatus.InvalidCredentials,
                        "Invalid password. Please try again"
                    ));
                }

                // Step 3: Check user active status
                DataRow userRow = dtUser.Rows[0];
                bool userIsActive = Convert.ToBoolean(userRow["UserIsActive"]);

                if (!userIsActive)
                {
                    return Ok(ApiResponse<LoginResponse>.ErrorResponse(
                        ResponseStatus.UserInactive,
                        "Your account is inactive. Please contact administrator"
                    ));
                }

                // Step 4: Check company active status
                bool companyIsActive = Convert.ToBoolean(userRow["CompanyIsActive"]);

                if (!companyIsActive)
                {
                    return Ok(ApiResponse<LoginResponse>.ErrorResponse(
                        ResponseStatus.CompanyInactive,
                        "Your company account is inactive. Please contact support"
                    ));
                }

                // Step 5: Check center active status
                bool centerIsActive = Convert.ToBoolean(userRow["CenterIsActive"]);

                if (!centerIsActive)
                {
                    return Ok(ApiResponse<LoginResponse>.ErrorResponse(
                        ResponseStatus.CenterInactive,
                        "Your center is inactive. Please contact administrator"
                    ));
                }

                // Step 6: All validations passed - Create login response
                var loginResponse = new LoginResponse
                {
                    UserId = Convert.ToInt32(userRow["UserID"]),
                    FirstName = userRow["FirstName"]?.ToString() ?? "",
                    LastName = userRow["LastName"]?.ToString() ?? "",
                    Email = userRow["EmailID"]?.ToString() ?? "",
                    MobileNo = userRow["MobileNo"]?.ToString() ?? "",
                    Username = userRow["UserName"]?.ToString() ?? "",
                    CompanyId = Convert.ToInt32(userRow["CompanyID"]),
                    CompanyName = userRow["CompanyName"]?.ToString() ?? "",
                    CenterId = Convert.ToInt32(userRow["CenterID"]),
                    CenterName = userRow["CenterName"]?.ToString() ?? "",
                    RoleId = Convert.ToInt32(userRow["UserRole"]),
                    RoleName = userRow["RoleName"]?.ToString() ?? "",
                    IsSuperUser = Convert.ToBoolean(userRow["IsSuperUser"]),
                    IsActive = userIsActive
                };

                // Log successful login (optional - for future audit)
                await AuthenticationDL.LogLoginActivityAsync(loginResponse.UserId, true);

                return Ok(ApiResponse<LoginResponse>.SuccessResponse(
                    ResponseStatus.LoginSuccess,
                    $"Welcome back, {loginResponse.FirstName}!",
                    loginResponse
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<LoginResponse>.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"An error occurred during login: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Check if username exists (for registration/validation)
        /// </summary>
        [HttpGet("check-username/{username}")]
        public async Task<IActionResult> CheckUsername(string username)
        {
            try
            {
                bool exists = await AuthenticationDL.UsernameExistsAsync(username);

                return Ok(new ApiResponse<object>(
                    exists ? ResponseStatus.AlreadyExists : ResponseStatus.Success,
                    exists ? "Username already exists" : "Username is available",
                    new { exists = exists, username = username }
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error checking username: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Logout endpoint (placeholder for future token invalidation)
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Future: Invalidate JWT token or session here
                await Task.CompletedTask;

                return Ok(ApiResponse.SuccessResponse("Logged out successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error during logout: {ex.Message}"
                ));
            }
        }

        /// <summary>
        /// Get user status details
        /// </summary>
        [HttpGet("user-status/{username}")]
        public async Task<IActionResult> GetUserStatus(string username)
        {
            try
            {
                var dtStatus = await AuthenticationDL.GetUserWithStatusAsync(username);

                if (dtStatus.Rows.Count == 0)
                {
                    return Ok(ApiResponse.ErrorResponse(
                        ResponseStatus.UserNotFound,
                        "User not found"
                    ));
                }

                var row = dtStatus.Rows[0];
                var statusInfo = new
                {
                    userId = Convert.ToInt32(row["UserID"]),
                    username = row["UserName"]?.ToString(),
                    userIsActive = Convert.ToBoolean(row["UserIsActive"]),
                    companyId = Convert.ToInt32(row["CompanyID"]),
                    companyName = row["CompanyName"]?.ToString(),
                    companyIsActive = Convert.ToBoolean(row["CompanyIsActive"]),
                    centerId = Convert.ToInt32(row["CenterID"]),
                    centerName = row["CenterName"]?.ToString(),
                    centerIsActive = Convert.ToBoolean(row["CenterIsActive"])
                };

                return Ok(ApiResponse<object>.SuccessResponse(
                    statusInfo,
                    "User status retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResponse(
                    ResponseStatus.InternalServerError,
                    $"Error retrieving user status: {ex.Message}"
                ));
            }
        }
    }
}
