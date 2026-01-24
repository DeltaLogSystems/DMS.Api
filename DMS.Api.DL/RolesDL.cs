using System.Data;

namespace DMS.Api.DL
{
    public static class RolesDL
    {
        // Removed static shared MySQLHelper to fix concurrency issues

        // Each method creates its own instance for thread-safety

        #region GET Operations

        /// <summary>
        /// Get all roles
        /// </summary>
        public static async Task<DataTable> GetAllRolesAsync()
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_Roles ORDER BY RoleName"
            );
            return dt;
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        public static async Task<DataTable> GetRoleByIdAsync(int roleId)
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_Roles WHERE RoleID = @roleId",
                "@roleId", roleId
            );
            return dt;
        }

        /// <summary>
        /// Get role by name
        /// </summary>
        public static async Task<DataTable> GetRoleByNameAsync(string roleName)
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_Roles WHERE RoleName = @roleName",
                "@roleName", roleName
            );
            return dt;
        }

        /// <summary>
        /// Search roles by name (partial match)
        /// </summary>
        public static async Task<DataTable> SearchRolesByNameAsync(string searchTerm)
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_Roles WHERE RoleName LIKE @searchTerm ORDER BY RoleName",
                "@searchTerm", $"%{searchTerm}%"
            );
            return dt;
        }

        /// <summary>
        /// Check if role name exists
        /// </summary>
        public static async Task<bool> RoleNameExistsAsync(string roleName, int? excludeRoleId = null)
        {
            using var sqlHelper = new MySQLHelper();
            string query = excludeRoleId.HasValue
                ? "SELECT COUNT(*) FROM M_Roles WHERE RoleName = @roleName AND RoleID != @roleId"
                : "SELECT COUNT(*) FROM M_Roles WHERE RoleName = @roleName";

            object[] parameters = excludeRoleId.HasValue
                ? new object[] { "@roleName", roleName, "@roleId", excludeRoleId.Value }
                : new object[] { "@roleName", roleName };

            var result = await sqlHelper.ExecScalarAsync(query, parameters);
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Get role count
        /// </summary>
        public static async Task<int> GetRoleCountAsync()
        {
            using var sqlHelper = new MySQLHelper();
            var result = await sqlHelper.ExecScalarAsync("SELECT COUNT(*) FROM M_Roles");
            return Convert.ToInt32(result);
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Insert new role
        /// </summary>
        public static async Task<int> InsertRoleAsync(string roleName)
        {
            using var sqlHelper = new MySQLHelper();
            var result = await sqlHelper.ExecScalarAsync(
                @"INSERT INTO M_Roles (RoleName)
                  VALUES (@roleName);
                  SELECT LAST_INSERT_ID();",
                "@roleName", roleName
            );
            return Convert.ToInt32(result);
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update role
        /// </summary>
        public static async Task<int> UpdateRoleAsync(int roleId, string roleName)
        {
            using var sqlHelper = new MySQLHelper();
            var result = await sqlHelper.ExecNonQueryAsync(
                "UPDATE M_Roles SET RoleName = @roleName WHERE RoleID = @roleId",
                "@roleId", roleId,
                "@roleName", roleName
            );
            return result;
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Delete role
        /// </summary>
        public static async Task<int> DeleteRoleAsync(int roleId)
        {
            using var sqlHelper = new MySQLHelper();
            var result = await sqlHelper.ExecNonQueryAsync(
                "DELETE FROM M_Roles WHERE RoleID = @roleId",
                "@roleId", roleId
            );
            return result;
        }

        #endregion
    }
}
