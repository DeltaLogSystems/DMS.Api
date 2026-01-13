using System.Data;

namespace DMS.Api.DL
{
    public static class CompaniesDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region GET Operations

        /// <summary>
        /// Get company code by company ID
        /// </summary>
        public static async Task<string> GetCompanyCodeAsync(int companyId)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                "SELECT CompanyCode FROM M_Companies WHERE CompanyID = @companyId",
                "@companyId", companyId
            );
            return result?.ToString() ?? "";
        }


        /// <summary>
        /// Get all companies
        /// </summary>
        public static async Task<DataTable> GetAllCompaniesAsync()
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_Companies ORDER BY CompanyName"
            );
            return dt;
        }

        /// <summary>
        /// Get all active companies
        /// </summary>
        public static async Task<DataTable> GetActiveCompaniesAsync()
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_Companies WHERE IsActive = 1 ORDER BY CompanyName"
            );
            return dt;
        }

        /// <summary>
        /// Get company by ID
        /// </summary>
        public static async Task<DataTable> GetCompanyByIdAsync(int companyId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_Companies WHERE CompanyID = @companyId",
                "@companyId", companyId
            );
            return dt;
        }

        /// <summary>
        /// Get company by name
        /// </summary>
        public static async Task<DataTable> GetCompanyByNameAsync(string companyName)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_Companies WHERE CompanyName = @companyName",
                "@companyName", companyName
            );
            return dt;
        }

        /// <summary>
        /// Search companies by name (partial match)
        /// </summary>
        public static async Task<DataTable> SearchCompaniesByNameAsync(string searchTerm)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_Companies WHERE CompanyName LIKE @searchTerm ORDER BY CompanyName",
                "@searchTerm", $"%{searchTerm}%"
            );
            return dt;
        }

        /// <summary>
        /// Check if company name exists
        /// </summary>
        public static async Task<bool> CompanyNameExistsAsync(string companyName, int? excludeCompanyId = null)
        {
            string query = excludeCompanyId.HasValue
                ? "SELECT COUNT(*) FROM M_Companies WHERE CompanyName = @companyName AND CompanyID != @companyId"
                : "SELECT COUNT(*) FROM M_Companies WHERE CompanyName = @companyName";

            object?[] parameters = excludeCompanyId.HasValue
                ? new object[] { "@companyName", companyName, "@companyId", excludeCompanyId.Value }
                : new object[] { "@companyName", companyName };

            var result = await _sqlHelper.ExecScalarAsync(query, parameters);
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Get company count
        /// </summary>
        public static async Task<int> GetCompanyCountAsync(bool activeOnly = false)
        {
            string query = activeOnly
                ? "SELECT COUNT(*) FROM M_Companies WHERE IsActive = 1"
                : "SELECT COUNT(*) FROM M_Companies";

            var result = await _sqlHelper.ExecScalarAsync(query);
            return Convert.ToInt32(result);
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Insert new company
        /// </summary>
        public static async Task<int> InsertCompanyAsync(
            string companyName,
            string companyAddress,
            string companyLogo,
            bool isActive,
            string createdBy)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                @"INSERT INTO M_Companies (CompanyName, CompanyAddress, CompanyLogo, IsActive, CreatedDate, CreatedBy)
                  VALUES (@companyName, @companyAddress, @companyLogo, @isActive, CURDATE(), @createdBy);
                  SELECT LAST_INSERT_ID();",
                "@companyName", companyName,
                "@companyAddress", companyAddress,
                "@companyLogo", companyLogo,
                "@isActive", isActive,
                "@createdBy", createdBy
            );
            return Convert.ToInt32(result);
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update company
        /// </summary>
        public static async Task<int> UpdateCompanyAsync(
            int companyId,
            string companyName,
            string companyAddress,
            string companyLogo,
            bool isActive)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Companies 
                  SET CompanyName = @companyName,
                      CompanyAddress = @companyAddress,
                      CompanyLogo = @companyLogo,
                      IsActive = @isActive
                  WHERE CompanyID = @companyId",
                "@companyId", companyId,
                "@companyName", companyName,
                "@companyAddress", companyAddress,
                "@companyLogo", companyLogo,
                "@isActive", isActive
            );
            return result;
        }

        /// <summary>
        /// Toggle company active status
        /// </summary>
        public static async Task<int> ToggleCompanyStatusAsync(int companyId)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                "UPDATE M_Companies SET IsActive = NOT IsActive WHERE CompanyID = @companyId",
                "@companyId", companyId
            );
            return result;
        }

        /// <summary>
        /// Update company logo
        /// </summary>
        public static async Task<int> UpdateCompanyLogoAsync(int companyId, string companyLogo)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                "UPDATE M_Companies SET CompanyLogo = @companyLogo WHERE CompanyID = @companyId",
                "@companyId", companyId,
                "@companyLogo", companyLogo
            );
            return result;
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Delete company (soft delete - set IsActive to 0)
        /// </summary>
        public static async Task<int> SoftDeleteCompanyAsync(int companyId)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                "UPDATE M_Companies SET IsActive = 0 WHERE CompanyID = @companyId",
                "@companyId", companyId
            );
            return result;
        }

        /// <summary>
        /// Delete company permanently
        /// </summary>
        public static async Task<int> DeleteCompanyAsync(int companyId)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                "DELETE FROM M_Companies WHERE CompanyID = @companyId",
                "@companyId", companyId
            );
            return result;
        }

        #endregion

        #region Additional GET Operations

        /// <summary>
        /// Get company by user ID
        /// </summary>
        public static async Task<DataTable> GetCompanyByUserIdAsync(int userId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT c.* 
          FROM M_Companies c
          INNER JOIN M_Users u ON c.CompanyID = u.CompanyID
          WHERE u.UserID = @userId",
                "@userId", userId
            );
            return dt;
        }

        /// <summary>
        /// Get company by patient ID
        /// </summary>
        public static async Task<DataTable> GetCompanyByPatientIdAsync(int patientId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT c.* 
          FROM M_Companies c
          INNER JOIN M_Patients p ON c.CompanyID = p.CompanyID
          WHERE p.PatientID = @patientId",
                "@patientId", patientId
            );
            return dt;
        }

        #endregion

    }
}
