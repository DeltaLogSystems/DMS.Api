using System.Data;

namespace DMS.Api.DL
{
    public static class CenterConfigurationDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region GET Operations

        /// <summary>
        /// Get all center configurations
        /// </summary>
        public static async Task<DataTable> GetAllConfigurationsAsync()
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT cc.*, c.CenterName, c.CompanyID, comp.CompanyName
                  FROM L_Center_Configuration cc
                  INNER JOIN M_Centers c ON cc.CenterID = c.CenterID
                  INNER JOIN M_Companies comp ON c.CompanyID = comp.CompanyID
                  ORDER BY c.CenterName"
            );
            return dt;
        }

        /// <summary>
        /// Get configuration by ID
        /// </summary>
        public static async Task<DataTable> GetConfigurationByIdAsync(int configurationId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT cc.*, c.CenterName, c.CompanyID, comp.CompanyName
                  FROM L_Center_Configuration cc
                  INNER JOIN M_Centers c ON cc.CenterID = c.CenterID
                  INNER JOIN M_Companies comp ON c.CompanyID = comp.CompanyID
                  WHERE cc.ConfigurationID = @configurationId",
                "@configurationId", configurationId
            );
            return dt;
        }

        /// <summary>
        /// Get configuration by center ID
        /// </summary>
        public static async Task<DataTable> GetConfigurationByCenterIdAsync(int centerId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                @"SELECT cc.*, c.CenterName, c.CompanyID, comp.CompanyName
                  FROM L_Center_Configuration cc
                  INNER JOIN M_Centers c ON cc.CenterID = c.CenterID
                  INNER JOIN M_Companies comp ON c.CompanyID = comp.CompanyID
                  WHERE cc.CenterID = @centerId",
                "@centerId", centerId
            );
            return dt;
        }

        /// <summary>
        /// Check if center has configuration
        /// </summary>
        public static async Task<bool> CenterHasConfigurationAsync(int centerId)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                "SELECT COUNT(*) FROM L_Center_Configuration WHERE CenterID = @centerId",
                "@centerId", centerId
            );
            return Convert.ToInt32(result) > 0;
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Insert center configuration
        /// </summary>
        public static async Task<int> InsertConfigurationAsync(
            int centerId,
            decimal? machineSessionHours,
            bool isFixedHoursForSession)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                @"INSERT INTO L_Center_Configuration (CenterID, MachineSessionHours, IsFixedHoursForSession)
                  VALUES (@centerId, @machineSessionHours, @isFixedHoursForSession);
                  SELECT LAST_INSERT_ID();",
                "@centerId", centerId,
                "@machineSessionHours", machineSessionHours.HasValue ? (object)machineSessionHours.Value : DBNull.Value,
                "@isFixedHoursForSession", isFixedHoursForSession
            );
            return Convert.ToInt32(result);
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update configuration
        /// </summary>
        public static async Task<int> UpdateConfigurationAsync(
            int configurationId,
            decimal? machineSessionHours,
            bool isFixedHoursForSession)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE L_Center_Configuration 
                  SET MachineSessionHours = @machineSessionHours,
                      IsFixedHoursForSession = @isFixedHoursForSession
                  WHERE ConfigurationID = @configurationId",
                "@configurationId", configurationId,
                "@machineSessionHours", machineSessionHours.HasValue ? (object)machineSessionHours.Value : DBNull.Value,
                "@isFixedHoursForSession", isFixedHoursForSession
            );
            return result;
        }

        /// <summary>
        /// Update configuration by center ID
        /// </summary>
        public static async Task<int> UpdateConfigurationByCenterIdAsync(
            int centerId,
            decimal? machineSessionHours,
            bool isFixedHoursForSession)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE L_Center_Configuration 
                  SET MachineSessionHours = @machineSessionHours,
                      IsFixedHoursForSession = @isFixedHoursForSession
                  WHERE CenterID = @centerId",
                "@centerId", centerId,
                "@machineSessionHours", machineSessionHours.HasValue ? (object)machineSessionHours.Value : DBNull.Value,
                "@isFixedHoursForSession", isFixedHoursForSession
            );
            return result;
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Delete configuration
        /// </summary>
        public static async Task<int> DeleteConfigurationAsync(int configurationId)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                "DELETE FROM L_Center_Configuration WHERE ConfigurationID = @configurationId",
                "@configurationId", configurationId
            );
            return result;
        }

        /// <summary>
        /// Delete configuration by center ID
        /// </summary>
        public static async Task<int> DeleteConfigurationByCenterIdAsync(int centerId)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                "DELETE FROM L_Center_Configuration WHERE CenterID = @centerId",
                "@centerId", centerId
            );
            return result;
        }

        #endregion
    }
}
