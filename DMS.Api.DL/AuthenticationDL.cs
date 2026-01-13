using System.Data;

namespace DMS.Api.DL
{
    /// <summary>
    /// Data Layer for Authentication operations
    /// </summary>
    public static class AuthenticationDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        /// <summary>
        /// Validate user login with all necessary checks
        /// </summary>
        public static async Task<DataTable> ValidateLoginAsync(string username, string password)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT u.UserID, u.FirstName, u.LastName, u.EmailID, u.MobileNo, 
                         u.UserName, u.CompanyID, u.CenterID, u.UserRole, 
                         u.IsSuperUser, u.IsActive as UserIsActive,
                         comp.CompanyName, comp.IsActive as CompanyIsActive,
                         c.CenterName, c.IsActive as CenterIsActive,
                         r.RoleName
                  FROM M_Users u
                  INNER JOIN M_Companies comp ON u.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON u.CenterID = c.CenterID
                  INNER JOIN M_Roles r ON u.UserRole = r.RoleID
                  WHERE u.UserName = @username AND u.Password = @password",
                "@username", username,
                "@password", password
            );
            return dt;
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        public static async Task<bool> UsernameExistsAsync(string username)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                "SELECT COUNT(*) FROM M_Users WHERE UserName = @username",
                "@username", username
            );
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Get user with company and center status
        /// </summary>
        public static async Task<DataTable> GetUserWithStatusAsync(string username)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT u.UserID, u.UserName, u.IsActive as UserIsActive,
                         comp.CompanyID, comp.CompanyName, comp.IsActive as CompanyIsActive,
                         c.CenterID, c.CenterName, c.IsActive as CenterIsActive
                  FROM M_Users u
                  INNER JOIN M_Companies comp ON u.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON u.CenterID = c.CenterID
                  WHERE u.UserName = @username",
                "@username", username
            );
            return dt;
        }

        /// <summary>
        /// Log user login activity (for future audit trail)
        /// </summary>
        public static async Task<int> LogLoginActivityAsync(int userId, bool isSuccessful, string ipAddress = "")
        {
            // This is a placeholder for future audit logging
            // You can create a login_audit table later if needed
            await Task.CompletedTask;
            return 1;
        }
    }
}
