using System.Data;

namespace DMS.Api.DL
{
    public static class MaintenanceDL
    {
        // Removed static shared MySQLHelper to fix concurrency issues

        // Each method creates its own instance for thread-safety

        #region GET Operations

        /// <summary>
        /// Get all maintenance records
        /// </summary>
        public static async Task<DataTable> GetAllMaintenanceAsync(int? assetId = null, int? centerId = null)
        {
            using var sqlHelper = new MySQLHelper();
            string query = @"SELECT m.*, 
                                    a.AssetCode, a.AssetName, a.CenterID,
                                    c.CenterName,
                                    at.AssetTypeName
                             FROM T_Asset_Maintenance m
                             INNER JOIN M_Assets a ON m.AssetID = a.AssetID
                             INNER JOIN M_Centers c ON a.CenterID = c.CenterID
                             INNER JOIN M_Asset_Types at ON a.AssetType = at.AssetTypeID
                             WHERE 1=1";

            var parameters = new List<object>();

            if (assetId.HasValue)
            {
                query += " AND m.AssetID = @assetId";
                parameters.Add("@assetId");
                parameters.Add(assetId.Value);
            }

            if (centerId.HasValue)
            {
                query += " AND a.CenterID = @centerId";
                parameters.Add("@centerId");
                parameters.Add(centerId.Value);
            }

            query += " ORDER BY m.MaintenanceDate DESC";

            return await sqlHelper.ExecDataTableAsync(query, parameters.ToArray());
        }

        /// <summary>
        /// Get maintenance by ID
        /// </summary>
        public static async Task<DataTable> GetMaintenanceByIdAsync(int maintenanceId)
        {
            using var sqlHelper = new MySQLHelper();
            return await sqlHelper.ExecDataTableAsync(
                @"SELECT m.*, 
                         a.AssetCode, a.AssetName,
                         at.AssetTypeName
                  FROM T_Asset_Maintenance m
                  INNER JOIN M_Assets a ON m.AssetID = a.AssetID
                  INNER JOIN M_Asset_Types at ON a.AssetType = at.AssetTypeID
                  WHERE m.MaintenanceID = @maintenanceId",
                "@maintenanceId", maintenanceId
            );
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Create maintenance record and update asset
        /// </summary>
        public static async Task<int> CreateMaintenanceAsync(
            int assetId,
            DateTime maintenanceDate,
            string maintenanceType,
            string? description,
            string? technicianName,
            decimal? cost,
            DateTime? nextMaintenanceDate,
            int createdBy)
        {
            using var sqlHelper = new MySQLHelper();
            try
            {
                await sqlHelper.BeginTransactionAsync();

                // Insert maintenance record
                var result = await sqlHelper.ExecScalarAsync(
                    @"INSERT INTO T_Asset_Maintenance 
                      (AssetID, MaintenanceDate, MaintenanceType, Description, 
                       TechnicianName, Cost, NextMaintenanceDate, Status, CreatedDate, CreatedBy)
                      VALUES 
                      (@assetId, @maintenanceDate, @maintenanceType, @description,
                       @technicianName, @cost, @nextMaintenanceDate, 'Completed', NOW(), @createdBy);
                      SELECT LAST_INSERT_ID();",
                    "@assetId", assetId,
                    "@maintenanceDate", maintenanceDate,
                    "@maintenanceType", maintenanceType,
                    "@description", description ?? (object)DBNull.Value,
                    "@technicianName", technicianName ?? (object)DBNull.Value,
                    "@cost", cost ?? (object)DBNull.Value,
                    "@nextMaintenanceDate", nextMaintenanceDate ?? (object)DBNull.Value,
                    "@createdBy", createdBy
                );

                // Update asset maintenance dates
                await sqlHelper.ExecNonQueryAsync(
                    @"UPDATE M_Assets 
                      SET LastMaintenanceDate = @maintenanceDate,
                          NextMaintenanceDate = @nextMaintenanceDate,
                          ModifiedDate = NOW(),
                          ModifiedBy = @createdBy
                      WHERE AssetID = @assetId",
                    "@assetId", assetId,
                    "@maintenanceDate", maintenanceDate,
                    "@nextMaintenanceDate", nextMaintenanceDate ?? (object)DBNull.Value,
                    "@createdBy", createdBy
                );

                await sqlHelper.CommitAsync();
                return Convert.ToInt32(result);
            }
            catch
            {
                await sqlHelper.RollbackAsync();
                throw;
            }
        }

        #endregion
    }
}
