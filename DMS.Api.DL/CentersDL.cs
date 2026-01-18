using System.Data;

namespace DMS.Api.DL
{
    public static class CentersDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region GET Operations

        /// <summary>
        /// Get all centers
        /// </summary>
        public static async Task<DataTable> GetAllCentersAsync()
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT c.*, comp.CompanyName 
                  FROM M_Centers c
                  INNER JOIN M_Companies comp ON c.CompanyID = comp.CompanyID
                  ORDER BY c.CenterName"
            );
            return dt;
        }

        /// <summary>
        /// Get all active centers
        /// </summary>
        public static async Task<DataTable> GetActiveCentersAsync()
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT c.*, comp.CompanyName 
                  FROM M_Centers c
                  INNER JOIN M_Companies comp ON c.CompanyID = comp.CompanyID
                  WHERE c.IsActive = 1
                  ORDER BY c.CenterName"
            );
            return dt;
        }

        /// <summary>
        /// Get center by ID
        /// </summary>
        public static async Task<DataTable> GetCenterByIdAsync(int centerId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT c.*, comp.CompanyName 
                  FROM M_Centers c
                  INNER JOIN M_Companies comp ON c.CompanyID = comp.CompanyID
                  WHERE c.CenterID = @centerId",
                "@centerId", centerId
            );
            return dt;
        }

        /// <summary>
        /// Get centers by company ID
        /// </summary>
        public static async Task<DataTable> GetCentersByCompanyIdAsync(int companyId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT c.*, comp.CompanyName 
                  FROM M_Centers c
                  INNER JOIN M_Companies comp ON c.CompanyID = comp.CompanyID
                  WHERE c.CompanyID = @companyId
                  ORDER BY c.CenterName",
                "@companyId", companyId
            );
            return dt;
        }

        /// <summary>
        /// Get active centers by company ID
        /// </summary>
        public static async Task<DataTable> GetActiveCentersByCompanyIdAsync(int companyId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT c.*, comp.CompanyName 
                  FROM M_Centers c
                  INNER JOIN M_Companies comp ON c.CompanyID = comp.CompanyID
                  WHERE c.CompanyID = @companyId AND c.IsActive = 1
                  ORDER BY c.CenterName",
                "@companyId", companyId
            );
            return dt;
        }

        /// <summary>
        /// Get center by name
        /// </summary>
        public static async Task<DataTable> GetCenterByNameAsync(string centerName)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT c.*, comp.CompanyName 
                  FROM M_Centers c
                  INNER JOIN M_Companies comp ON c.CompanyID = comp.CompanyID
                  WHERE c.CenterName = @centerName",
                "@centerName", centerName
            );
            return dt;
        }

        /// <summary>
        /// Search centers by name (partial match)
        /// </summary>
        public static async Task<DataTable> SearchCentersByNameAsync(string searchTerm)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT c.*, comp.CompanyName 
                  FROM M_Centers c
                  INNER JOIN M_Companies comp ON c.CompanyID = comp.CompanyID
                  WHERE c.CenterName LIKE @searchTerm
                  ORDER BY c.CenterName",
                "@searchTerm", $"%{searchTerm}%"
            );
            return dt;
        }

        /// <summary>
        /// Check if center name exists in company
        /// </summary>
        public static async Task<bool> CenterNameExistsInCompanyAsync(string centerName, int companyId, int? excludeCenterId = null)
        {
            string query = excludeCenterId.HasValue
                ? "SELECT COUNT(*) FROM M_Centers WHERE CenterName = @centerName AND CompanyID = @companyId AND CenterID != @centerId"
                : "SELECT COUNT(*) FROM M_Centers WHERE CenterName = @centerName AND CompanyID = @companyId";

            object[] parameters = excludeCenterId.HasValue
                ? new object[] { "@centerName", centerName, "@companyId", companyId, "@centerId", excludeCenterId.Value }
                : new object[] { "@centerName", centerName, "@companyId", companyId };

            var result = await _sqlHelper.ExecScalarAsync(query, parameters);
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Get center count by company
        /// </summary>
        public static async Task<int> GetCenterCountByCompanyAsync(int companyId, bool activeOnly = false)
        {
            string query = activeOnly
                ? "SELECT COUNT(*) FROM M_Centers WHERE CompanyID = @companyId AND IsActive = 1"
                : "SELECT COUNT(*) FROM M_Centers WHERE CompanyID = @companyId";

            var result = await _sqlHelper.ExecScalarAsync(query, "@companyId", companyId);
            return Convert.ToInt32(result);
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Insert new center
        /// </summary>
        public static async Task<int> InsertCenterAsync(
            int companyId,
            string centerName,
            string centerAddress,
            bool isActive,
            string createdBy)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                @"INSERT INTO M_Centers (CompanyID, CenterName, CenterAddress, IsActive, CreatedDate, CreatedBy)
                  VALUES (@companyId, @centerName, @centerAddress, @isActive, CURDATE(), @createdBy);
                  SELECT LAST_INSERT_ID();",
                "@companyId", companyId,
                "@centerName", centerName,
                "@centerAddress", centerAddress,
                "@isActive", isActive,
                "@createdBy", createdBy
            );
            return Convert.ToInt32(result);
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update center
        /// </summary>
        public static async Task<int> UpdateCenterAsync(
            int centerId,
            int companyId,
            string centerName,
            string centerAddress,
            bool isActive)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Centers 
                  SET CompanyID = @companyId,
                      CenterName = @centerName,
                      CenterAddress = @centerAddress,
                      IsActive = @isActive
                  WHERE CenterID = @centerId",
                "@centerId", centerId,
                "@companyId", companyId,
                "@centerName", centerName,
                "@centerAddress", centerAddress,
                "@isActive", isActive
            );
            return result;
        }

        /// <summary>
        /// Toggle center active status
        /// </summary>
        public static async Task<int> ToggleCenterStatusAsync(int centerId)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                "UPDATE M_Centers SET IsActive = NOT IsActive WHERE CenterID = @centerId",
                "@centerId", centerId
            );
            return result;
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Delete center (soft delete)
        /// </summary>
        public static async Task<int> SoftDeleteCenterAsync(int centerId)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                "UPDATE M_Centers SET IsActive = 0 WHERE CenterID = @centerId",
                "@centerId", centerId
            );
            return result;
        }

        /// <summary>
        /// Delete center permanently
        /// </summary>
        public static async Task<int> DeleteCenterAsync(int centerId)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                "DELETE FROM M_Centers WHERE CenterID = @centerId",
                "@centerId", centerId
            );
            return result;
        }

        #endregion

        #region Additional GET Operations

        /// <summary>
        /// Get center by user ID
        /// </summary>
        public static async Task<DataTable> GetCenterByUserIdAsync(int userId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT c.*, comp.CompanyName 
          FROM M_Centers c
          INNER JOIN M_Companies comp ON c.CompanyID = comp.CompanyID
          INNER JOIN M_Users u ON c.CenterID = u.CenterID
          WHERE u.UserID = @userId",
                "@userId", userId
            );
            return dt;
        }

        /// <summary>
        /// Get center by patient ID
        /// </summary>
        public static async Task<DataTable> GetCenterByPatientIdAsync(int patientId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT c.*, comp.CompanyName 
          FROM M_Centers c
          INNER JOIN M_Companies comp ON c.CompanyID = comp.CompanyID
          INNER JOIN M_Patients p ON c.CenterID = p.CenterID
          WHERE p.PatientID = @patientId",
                "@patientId", patientId
            );
            return dt;
        }

        /// <summary>
        /// Get company and center details by user ID
        /// </summary>
        public static async Task<DataTable> GetCompanyAndCenterByUserIdAsync(int userId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT c.*, 
                 comp.CompanyID, comp.CompanyName, comp.CompanyCode, 
                 comp.CompanyAddress, comp.CompanyLogo, comp.IsActive as CompanyIsActive
          FROM M_Centers c
          INNER JOIN M_Companies comp ON c.CompanyID = comp.CompanyID
          INNER JOIN M_Users u ON c.CenterID = u.CenterID AND comp.CompanyID = u.CompanyID
          WHERE u.UserID = @userId",
                "@userId", userId
            );
            return dt;
        }

        /// <summary>
        /// Get company and center details by patient ID
        /// </summary>
        public static async Task<DataTable> GetCompanyAndCenterByPatientIdAsync(int patientId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT c.*, 
                 comp.CompanyID, comp.CompanyName, comp.CompanyCode, 
                 comp.CompanyAddress, comp.CompanyLogo, comp.IsActive as CompanyIsActive
          FROM M_Centers c
          INNER JOIN M_Companies comp ON c.CompanyID = comp.CompanyID
          INNER JOIN M_Patients p ON c.CenterID = p.CenterID AND comp.CompanyID = p.CompanyID
          WHERE p.PatientID = @patientId",
                "@patientId", patientId
            );
            return dt;
        }

        #endregion

    }
}
