using System.Data;

namespace DMS.Api.DL
{
    public static class AssetTypesDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region GET Operations

        /// <summary>
        /// Get all asset types
        /// </summary>
        public static async Task<DataTable> GetAllAssetTypesAsync(bool activeOnly = true)
        {
            string query = activeOnly
                ? "SELECT * FROM M_Asset_Types WHERE IsActive = 1 ORDER BY AssetTypeName"
                : "SELECT * FROM M_Asset_Types ORDER BY AssetTypeName";

            return await _sqlHelper.ExecDataTableAsync(query);
        }

        /// <summary>
        /// Get asset type by ID
        /// </summary>
        public static async Task<DataTable> GetAssetTypeByIdAsync(int assetTypeId)
        {
            return await _sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_Asset_Types WHERE AssetTypeID = @assetTypeId",
                "@assetTypeId", assetTypeId
            );
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Create new asset type
        /// </summary>
        public static async Task<int> CreateAssetTypeAsync(
            string assetTypeName,
            string assetTypeCode,
            string? description,
            bool requiresMaintenance,
            int maintenanceIntervalDays,
            int createdBy)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                @"INSERT INTO M_Asset_Types 
                  (AssetTypeName, AssetTypeCode, Description, RequiresMaintenance, 
                   MaintenanceIntervalDays, IsActive, CreatedDate, CreatedBy)
                  VALUES 
                  (@assetTypeName, @assetTypeCode, @description, @requiresMaintenance,
                   @maintenanceIntervalDays, 1, NOW(), @createdBy);
                  SELECT LAST_INSERT_ID();",
                "@assetTypeName", assetTypeName,
                "@assetTypeCode", assetTypeCode,
                "@description", description ?? (object)DBNull.Value,
                "@requiresMaintenance", requiresMaintenance,
                "@maintenanceIntervalDays", maintenanceIntervalDays,
                "@createdBy", createdBy
            );

            return Convert.ToInt32(result);
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update asset type
        /// </summary>
        public static async Task<int> UpdateAssetTypeAsync(
            int assetTypeId,
            string assetTypeName,
            string? description,
            bool requiresMaintenance,
            int maintenanceIntervalDays,
            int modifiedBy)
        {
            return await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Asset_Types 
                  SET AssetTypeName = @assetTypeName,
                      Description = @description,
                      RequiresMaintenance = @requiresMaintenance,
                      MaintenanceIntervalDays = @maintenanceIntervalDays,
                      ModifiedDate = NOW(),
                      ModifiedBy = @modifiedBy
                  WHERE AssetTypeID = @assetTypeId",
                "@assetTypeId", assetTypeId,
                "@assetTypeName", assetTypeName,
                "@description", description ?? (object)DBNull.Value,
                "@requiresMaintenance", requiresMaintenance,
                "@maintenanceIntervalDays", maintenanceIntervalDays,
                "@modifiedBy", modifiedBy
            );
        }

        /// <summary>
        /// Toggle asset type active status
        /// </summary>
        public static async Task<int> ToggleAssetTypeStatusAsync(int assetTypeId, bool isActive, int modifiedBy)
        {
            return await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Asset_Types 
                  SET IsActive = @isActive,
                      ModifiedDate = NOW(),
                      ModifiedBy = @modifiedBy
                  WHERE AssetTypeID = @assetTypeId",
                "@assetTypeId", assetTypeId,
                "@isActive", isActive,
                "@modifiedBy", modifiedBy
            );
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Delete asset type (if no assets exist)
        /// </summary>
        public static async Task<int> DeleteAssetTypeAsync(int assetTypeId)
        {
            // Check if any assets exist with this type
            var count = await _sqlHelper.ExecScalarAsync(
                "SELECT COUNT(*) FROM M_Assets WHERE AssetType = @assetTypeId",
                "@assetTypeId", assetTypeId
            );

            if (Convert.ToInt32(count) > 0)
            {
                throw new InvalidOperationException("Cannot delete asset type with existing assets");
            }

            return await _sqlHelper.ExecNonQueryAsync(
                "DELETE FROM M_Asset_Types WHERE AssetTypeID = @assetTypeId",
                "@assetTypeId", assetTypeId
            );
        }

        #endregion
    }
}
