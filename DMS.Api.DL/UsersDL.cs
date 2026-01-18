using System.Data;

namespace DMS.Api.DL
{
    public static class UsersDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region GET Operations

        /// <summary>
        /// Get all users
        /// </summary>
        public static async Task<DataTable> GetAllUsersAsync()
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT u.*, 
                         comp.CompanyName, 
                         c.CenterName, 
                         r.RoleName
                  FROM M_Users u
                  INNER JOIN M_Companies comp ON u.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON u.CenterID = c.CenterID
                  INNER JOIN M_Roles r ON u.UserRole = r.RoleID
                  ORDER BY u.FirstName, u.LastName"
            );
            return dt;
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public static async Task<DataTable> GetUserByIdAsync(int userId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT u.*, 
                         comp.CompanyName, 
                         c.CenterName, 
                         r.RoleName
                  FROM M_Users u
                  INNER JOIN M_Companies comp ON u.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON u.CenterID = c.CenterID
                  INNER JOIN M_Roles r ON u.UserRole = r.RoleID
                  WHERE u.UserID = @userId",
                "@userId", userId
            );
            return dt;
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        public static async Task<DataTable> GetUserByUsernameAsync(string username)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT u.*, 
                         comp.CompanyName, 
                         c.CenterName, 
                         r.RoleName
                  FROM M_Users u
                  INNER JOIN M_Companies comp ON u.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON u.CenterID = c.CenterID
                  INNER JOIN M_Roles r ON u.UserRole = r.RoleID
                  WHERE u.UserName = @username",
                "@username", username
            );
            return dt;
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        public static async Task<DataTable> GetUserByEmailAsync(string email)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT u.*, 
                         comp.CompanyName, 
                         c.CenterName, 
                         r.RoleName
                  FROM M_Users u
                  INNER JOIN M_Companies comp ON u.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON u.CenterID = c.CenterID
                  INNER JOIN M_Roles r ON u.UserRole = r.RoleID
                  WHERE u.EmailID = @email",
                "@email", email
            );
            return dt;
        }

        /// <summary>
        /// Get users by company ID
        /// </summary>
        public static async Task<DataTable> GetUsersByCompanyIdAsync(int companyId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT u.*, 
                         comp.CompanyName, 
                         c.CenterName, 
                         r.RoleName
                  FROM M_Users u
                  INNER JOIN M_Companies comp ON u.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON u.CenterID = c.CenterID
                  INNER JOIN M_Roles r ON u.UserRole = r.RoleID
                  WHERE u.CompanyID = @companyId
                  ORDER BY u.FirstName, u.LastName",
                "@companyId", companyId
            );
            return dt;
        }

        /// <summary>
        /// Get users by center ID
        /// </summary>
        public static async Task<DataTable> GetUsersByCenterIdAsync(int centerId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT u.*, 
                         comp.CompanyName, 
                         c.CenterName, 
                         r.RoleName
                  FROM M_Users u
                  INNER JOIN M_Companies comp ON u.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON u.CenterID = c.CenterID
                  INNER JOIN M_Roles r ON u.UserRole = r.RoleID
                  WHERE u.CenterID = @centerId
                  ORDER BY u.FirstName, u.LastName",
                "@centerId", centerId
            );
            return dt;
        }

        /// <summary>
        /// Get users by role ID
        /// </summary>
        public static async Task<DataTable> GetUsersByRoleIdAsync(int roleId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT u.*, 
                         comp.CompanyName, 
                         c.CenterName, 
                         r.RoleName
                  FROM M_Users u
                  INNER JOIN M_Companies comp ON u.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON u.CenterID = c.CenterID
                  INNER JOIN M_Roles r ON u.UserRole = r.RoleID
                  WHERE u.UserRole = @roleId
                  ORDER BY u.FirstName, u.LastName",
                "@roleId", roleId
            );
            return dt;
        }

        /// <summary>
        /// Get super users
        /// </summary>
        public static async Task<DataTable> GetSuperUsersAsync()
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT u.*, 
                         comp.CompanyName, 
                         c.CenterName, 
                         r.RoleName
                  FROM M_Users u
                  INNER JOIN M_Companies comp ON u.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON u.CenterID = c.CenterID
                  INNER JOIN M_Roles r ON u.UserRole = r.RoleID
                  WHERE u.IsSuperUser = 1
                  ORDER BY u.FirstName, u.LastName"
            );
            return dt;
        }

        /// <summary>
        /// Search users by name (partial match)
        /// </summary>
        public static async Task<DataTable> SearchUsersByNameAsync(string searchTerm)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT u.*, 
                         comp.CompanyName, 
                         c.CenterName, 
                         r.RoleName
                  FROM M_Users u
                  INNER JOIN M_Companies comp ON u.CompanyID = comp.CompanyID
                  INNER JOIN M_Centers c ON u.CenterID = c.CenterID
                  INNER JOIN M_Roles r ON u.UserRole = r.RoleID
                  WHERE CONCAT(u.FirstName, ' ', u.LastName) LIKE @searchTerm
                  ORDER BY u.FirstName, u.LastName",
                "@searchTerm", $"%{searchTerm}%"
            );
            return dt;
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        public static async Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null)
        {
            string query = excludeUserId.HasValue
                ? "SELECT COUNT(*) FROM M_Users WHERE UserName = @username AND UserID != @userId"
                : "SELECT COUNT(*) FROM M_Users WHERE UserName = @username";

            object[] parameters = excludeUserId.HasValue
                ? new object[] { "@username", username, "@userId", excludeUserId.Value }
                : new object[] { "@username", username };

            var result = await _sqlHelper.ExecScalarAsync(query, parameters);
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        public static async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
        {
            string query = excludeUserId.HasValue
                ? "SELECT COUNT(*) FROM M_Users WHERE EmailID = @email AND UserID != @userId"
                : "SELECT COUNT(*) FROM M_Users WHERE EmailID = @email";

            object[] parameters = excludeUserId.HasValue
                ? new object[] { "@email", email, "@userId", excludeUserId.Value }
                : new object[] { "@email", email };

            var result = await _sqlHelper.ExecScalarAsync(query, parameters);
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Validate user credentials (for login)
        /// </summary>
        public static async Task<DataTable> ValidateUserAsync(string username, string password)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT u.*, 
                         comp.CompanyName, 
                         c.CenterName, 
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
        /// Get user count by company
        /// </summary>
        public static async Task<int> GetUserCountByCompanyAsync(int companyId)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                "SELECT COUNT(*) FROM M_Users WHERE CompanyID = @companyId",
                "@companyId", companyId
            );
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Get user count by center
        /// </summary>
        public static async Task<int> GetUserCountByCenterAsync(int centerId)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                "SELECT COUNT(*) FROM M_Users WHERE CenterID = @centerId",
                "@centerId", centerId
            );
            return Convert.ToInt32(result);
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Insert new user
        /// </summary>
        public static async Task<int> InsertUserAsync(
            string firstName,
            string lastName,
            string emailId,
            string mobileNo,
            string userName,
            string password,
            int companyId,
            int centerId,
            int userRole,
            bool isSuperUser,
            string createdBy)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                @"INSERT INTO M_Users (FirstName, LastName, EmailID, MobileNo, UserName, Password, 
                                       CompanyID, CenterID, UserRole, IsSuperUser, 
                                       CreatedDate, CreatedBy, ModifiedBy, ModifiedDate)
                  VALUES (@firstName, @lastName, @emailId, @mobileNo, @userName, @password,
                          @companyId, @centerId, @userRole, @isSuperUser,
                          CURDATE(), @createdBy, @createdBy, CURDATE());
                  SELECT LAST_INSERT_ID();",
                "@firstName", firstName,
                "@lastName", lastName,
                "@emailId", emailId,
                "@mobileNo", mobileNo,
                "@userName", userName,
                "@password", password,
                "@companyId", companyId,
                "@centerId", centerId,
                "@userRole", userRole,
                "@isSuperUser", isSuperUser,
                "@createdBy", createdBy
            );
            return Convert.ToInt32(result);
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update user
        /// </summary>
        public static async Task<int> UpdateUserAsync(
            int userId,
            string firstName,
            string lastName,
            string emailId,
            string mobileNo,
            string userName,
            int companyId,
            int centerId,
            int userRole,
            bool isSuperUser,
            string modifiedBy)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Users 
                  SET FirstName = @firstName,
                      LastName = @lastName,
                      EmailID = @emailId,
                      MobileNo = @mobileNo,
                      UserName = @userName,
                      CompanyID = @companyId,
                      CenterID = @centerId,
                      UserRole = @userRole,
                      IsSuperUser = @isSuperUser,
                      ModifiedBy = @modifiedBy,
                      ModifiedDate = CURDATE()
                  WHERE UserID = @userId",
                "@userId", userId,
                "@firstName", firstName,
                "@lastName", lastName,
                "@emailId", emailId,
                "@mobileNo", mobileNo,
                "@userName", userName,
                "@companyId", companyId,
                "@centerId", centerId,
                "@userRole", userRole,
                "@isSuperUser", isSuperUser,
                "@modifiedBy", modifiedBy
            );
            return result;
        }

        /// <summary>
        /// Update user password
        /// </summary>
        public static async Task<int> UpdatePasswordAsync(int userId, string newPassword, string modifiedBy)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Users 
                  SET Password = @newPassword,
                      ModifiedBy = @modifiedBy,
                      ModifiedDate = CURDATE()
                  WHERE UserID = @userId",
                "@userId", userId,
                "@newPassword", newPassword,
                "@modifiedBy", modifiedBy
            );
            return result;
        }

        /// <summary>
        /// Update user password by username
        /// </summary>
        public static async Task<int> UpdatePasswordByUsernameAsync(string username, string newPassword)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Users 
                  SET Password = @newPassword,
                      ModifiedDate = CURDATE()
                  WHERE UserName = @username",
                "@username", username,
                "@newPassword", newPassword
            );
            return result;
        }

        /// <summary>
        /// Toggle super user status
        /// </summary>
        public static async Task<int> ToggleSuperUserStatusAsync(int userId, string modifiedBy)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Users 
                  SET IsSuperUser = NOT IsSuperUser,
                      ModifiedBy = @modifiedBy,
                      ModifiedDate = CURDATE()
                  WHERE UserID = @userId",
                "@userId", userId,
                "@modifiedBy", modifiedBy
            );
            return result;
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Delete user
        /// </summary>
        public static async Task<int> DeleteUserAsync(int userId)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                "DELETE FROM M_Users WHERE UserID = @userId",
                "@userId", userId
            );
            return result;
        }

        #endregion

        #region Authentication Methods

        /// <summary>
        /// Authenticate user with comprehensive validation
        /// </summary>
        public static async Task<DataTable> AuthenticateUserAsync(string username, string password)
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
        /// Check if user exists by username
        /// </summary>
        public static async Task<bool> UserExistsByUsernameAsync(string username)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                "SELECT COUNT(*) FROM M_Users WHERE UserName = @username",
                "@username", username
            );
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Get user active status
        /// </summary>
        public static async Task<bool> IsUserActiveAsync(int userId)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                "SELECT IsActive FROM M_Users WHERE UserID = @userId",
                "@userId", userId
            );
            return result != null && Convert.ToBoolean(result);
        }

        /// <summary>
        /// Toggle user active status
        /// </summary>
        public static async Task<int> ToggleUserStatusAsync(int userId, string modifiedBy)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Users 
          SET IsActive = NOT IsActive,
              ModifiedBy = @modifiedBy,
              ModifiedDate = CURDATE()
          WHERE UserID = @userId",
                "@userId", userId,
                "@modifiedBy", modifiedBy
            );
            return result;
        }

        /// <summary>
        /// Update user active status
        /// </summary>
        public static async Task<int> UpdateUserStatusAsync(int userId, bool isActive, string modifiedBy)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Users 
          SET IsActive = @isActive,
              ModifiedBy = @modifiedBy,
              ModifiedDate = CURDATE()
          WHERE UserID = @userId",
                "@userId", userId,
                "@isActive", isActive,
                "@modifiedBy", modifiedBy
            );
            return result;
        }

        #endregion

    }
}
