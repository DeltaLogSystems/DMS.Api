using System.Data;

namespace DMS.Api.DL
{
    public static class AssetsDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region Asset Code Generation

        /// <summary>
        /// Generate unique asset code
        /// </summary>
        public static async Task<string> GenerateAssetCodeAsync(int assetTypeId, int centerId)
        {
            // Get asset type code
            var dtType = await _sqlHelper.ExecDataTableAsync(
                "SELECT AssetTypeCode FROM M_Asset_Types WHERE AssetTypeID = @assetTypeId",
                "@assetTypeId", assetTypeId
            );

            if (dtType.Rows.Count == 0)
            {
                throw new Exception("Asset type not found");
            }

            string typeCode = dtType.Rows[0]["AssetTypeCode"]?.ToString() ?? "AST";

            // Get center code (first 3 letters)
            var dtCenter = await _sqlHelper.ExecDataTableAsync(
                "SELECT CenterName FROM M_Centers WHERE CenterID = @centerId",
                "@centerId", centerId
            );

            string centerCode = dtCenter.Rows.Count > 0
                ? dtCenter.Rows[0]["CenterName"]?.ToString()?.Substring(0, Math.Min(3, dtCenter.Rows[0]["CenterName"]?.ToString()?.Length ?? 0)).ToUpper() ?? "CTR"
                : "CTR";

            // Get last number
            var lastNumber = await _sqlHelper.ExecScalarAsync(
                @"SELECT MAX(CAST(SUBSTRING(AssetCode, -4) AS UNSIGNED)) 
                  FROM M_Assets 
                  WHERE AssetType = @assetTypeId AND CenterID = @centerId",
                "@assetTypeId", assetTypeId,
                "@centerId", centerId
            );

            int nextNumber = lastNumber != DBNull.Value && lastNumber != null
                ? Convert.ToInt32(lastNumber) + 1
                : 1;

            // Format: DM-NEP-0001 (DialysisMachine-Nephro-0001)
            return $"{typeCode}-{centerCode}-{nextNumber:D4}";
        }

        #endregion

        #region GET Operations

        /// <summary>
        /// Get all assets
        /// </summary>
        public static async Task<DataTable> GetAllAssetsAsync(int? centerId = null, bool? activeOnly = null)
        {
            string query = @"SELECT a.*, 
                                    at.AssetTypeName,
                                    c.CenterName,
                                    comp.CompanyName
                             FROM M_Assets a
                             INNER JOIN M_Asset_Types at ON a.AssetType = at.AssetTypeID
                             INNER JOIN M_Centers c ON a.CenterID = c.CenterID
                             INNER JOIN M_Companies comp ON a.CompanyID = comp.CompanyID
                             WHERE 1=1";

            var parameters = new List<object>();

            if (centerId.HasValue)
            {
                query += " AND a.CenterID = @centerId";
                parameters.Add("@centerId");
                parameters.Add(centerId.Value);
            }

            if (activeOnly.HasValue)
            {
                query += " AND a.IsActive = @isActive";
                parameters.Add("@isActive");
                parameters.Add(activeOnly.Value);
            }

            query += " ORDER BY a.AssetCode";

            return await _sqlHelper.ExecDataTableAsync(query, parameters.ToArray());
        }

        /// <summary>
        /// Get asset by ID
        /// </summary>
        public static async Task<DataTable> GetAssetByIdAsync(int assetId)
        {
            return await _sqlHelper.ExecDataTableAsync(
                @"SELECT a.*, 
                         at.AssetTypeName, at.RequiresMaintenance, at.MaintenanceIntervalDays,
                         c.CenterName,
                         comp.CompanyName
                  FROM M_Assets a
                  INNER JOIN M_Asset_Types at ON a.AssetType = at.AssetTypeID
                  INNER JOIN M_Centers c ON a.CenterID = c.CenterID
                  INNER JOIN M_Companies comp ON a.CompanyID = comp.CompanyID
                  WHERE a.AssetID = @assetId",
                "@assetId", assetId
            );
        }

        /// <summary>
        /// Get assets by type
        /// </summary>
        public static async Task<DataTable> GetAssetsByTypeAsync(int assetTypeId, int? centerId = null, bool activeOnly = true)
        {
            string query = @"SELECT a.*, 
                                    at.AssetTypeName,
                                    c.CenterName,
                                    comp.CompanyName
                             FROM M_Assets a
                             INNER JOIN M_Asset_Types at ON a.AssetType = at.AssetTypeID
                             INNER JOIN M_Centers c ON a.CenterID = c.CenterID
                             INNER JOIN M_Companies comp ON a.CompanyID = comp.CompanyID
                             WHERE a.AssetType = @assetTypeId";

            var parameters = new List<object> { "@assetTypeId", assetTypeId };

            if (centerId.HasValue)
            {
                query += " AND a.CenterID = @centerId";
                parameters.Add("@centerId");
                parameters.Add(centerId.Value);
            }

            if (activeOnly)
            {
                query += " AND a.IsActive = 1";
            }

            query += " ORDER BY a.AssetCode";

            return await _sqlHelper.ExecDataTableAsync(query, parameters.ToArray());
        }

        /// <summary>
        /// Get assets requiring maintenance
        /// </summary>
        public static async Task<DataTable> GetAssetsRequiringMaintenanceAsync(int? centerId = null, int daysThreshold = 7)
        {
            string query = @"SELECT a.*, 
                                    at.AssetTypeName,
                                    c.CenterName,
                                    DATEDIFF(a.NextMaintenanceDate, CURDATE()) as DaysUntilMaintenance
                             FROM M_Assets a
                             INNER JOIN M_Asset_Types at ON a.AssetType = at.AssetTypeID
                             INNER JOIN M_Centers c ON a.CenterID = c.CenterID
                             WHERE a.IsActive = 1
                             AND a.NextMaintenanceDate IS NOT NULL
                             AND DATEDIFF(a.NextMaintenanceDate, CURDATE()) <= @daysThreshold";

            var parameters = new List<object> { "@daysThreshold", daysThreshold };

            if (centerId.HasValue)
            {
                query += " AND a.CenterID = @centerId";
                parameters.Add("@centerId");
                parameters.Add(centerId.Value);
            }

            query += " ORDER BY a.NextMaintenanceDate";

            return await _sqlHelper.ExecDataTableAsync(query, parameters.ToArray());
        }

        /// <summary>
        /// Get available assets for specific time slot
        /// </summary>
        public static async Task<DataTable> GetAvailableAssetsAsync(
            int centerId,
            int assetTypeId,
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            return await _sqlHelper.ExecDataTableAsync(
                @"SELECT a.*, at.AssetTypeName
                  FROM M_Assets a
                  INNER JOIN M_Asset_Types at ON a.AssetType = at.AssetTypeID
                  WHERE a.CenterID = @centerId
                  AND a.AssetType = @assetTypeId
                  AND a.IsActive = 1
                  AND a.AssetID NOT IN (
                      SELECT AssetID FROM T_Asset_Assignments
                      WHERE AssignedDate = @date
                      AND Status = 'Active'
                      AND (
                          (@startTime >= AssignedTime AND @startTime < ADDTIME(AssignedTime, SEC_TO_TIME(SessionDuration * 60)))
                          OR (@endTime > AssignedTime AND @endTime <= ADDTIME(AssignedTime, SEC_TO_TIME(SessionDuration * 60)))
                          OR (@startTime <= AssignedTime AND @endTime >= ADDTIME(AssignedTime, SEC_TO_TIME(SessionDuration * 60)))
                      )
                  )
                  ORDER BY a.AssetCode",
                "@centerId", centerId,
                "@assetTypeId", assetTypeId,
                "@date", date.Date,
                "@startTime", startTime,
                "@endTime", endTime
            );
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Create new asset
        /// </summary>
        public static async Task<int> CreateAssetAsync(
            string assetName,
            int assetType,
            string? serialNo,
            string? modelNo,
            string? manufacturer,
            DateTime? purchaseDate,
            decimal? purchaseCost,
            DateTime? warrantyExpiryDate,
            int centerId,
            int companyId,
            int createdBy)
        {
            try
            {
                await _sqlHelper.BeginTransactionAsync();

                // Generate asset code
                string assetCode = await GenerateAssetCodeAsync(assetType, centerId);

                // Calculate initial maintenance date if required
                DateTime? nextMaintenanceDate = null;
                var dtType = await _sqlHelper.ExecDataTableAsync(
                    "SELECT RequiresMaintenance, MaintenanceIntervalDays FROM M_Asset_Types WHERE AssetTypeID = @assetTypeId",
                    "@assetTypeId", assetType
                );

                if (dtType.Rows.Count > 0 && Convert.ToBoolean(dtType.Rows[0]["RequiresMaintenance"]))
                {
                    int intervalDays = Convert.ToInt32(dtType.Rows[0]["MaintenanceIntervalDays"]);
                    nextMaintenanceDate = (purchaseDate ?? DateTime.Today).AddDays(intervalDays);
                }

                // Insert asset
                var result = await _sqlHelper.ExecScalarAsync(
                    @"INSERT INTO M_Assets 
                      (AssetCode, AssetName, AssetType, SerialNo, ModelNo, Manufacturer,
                       PurchaseDate, PurchaseCost, WarrantyExpiryDate, CenterID, CompanyID,
                       NextMaintenanceDate, IsActive, CreatedDate, CreatedBy)
                      VALUES 
                      (@assetCode, @assetName, @assetType, @serialNo, @modelNo, @manufacturer,
                       @purchaseDate, @purchaseCost, @warrantyExpiryDate, @centerId, @companyId,
                       @nextMaintenanceDate, 1, NOW(), @createdBy);
                      SELECT LAST_INSERT_ID();",
                    "@assetCode", assetCode,
                    "@assetName", assetName,
                    "@assetType", assetType,
                    "@serialNo", serialNo ?? (object)DBNull.Value,
                    "@modelNo", modelNo ?? (object)DBNull.Value,
                    "@manufacturer", manufacturer ?? (object)DBNull.Value,
                    "@purchaseDate", purchaseDate ?? (object)DBNull.Value,
                    "@purchaseCost", purchaseCost ?? (object)DBNull.Value,
                    "@warrantyExpiryDate", warrantyExpiryDate ?? (object)DBNull.Value,
                    "@centerId", centerId,
                    "@companyId", companyId,
                    "@nextMaintenanceDate", nextMaintenanceDate ?? (object)DBNull.Value,
                    "@createdBy", createdBy
                );

                await _sqlHelper.CommitAsync();
                return Convert.ToInt32(result);
            }
            catch
            {
                await _sqlHelper.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update asset details
        /// </summary>
        public static async Task<int> UpdateAssetAsync(
            int assetId,
            string assetName,
            string? serialNo,
            string? modelNo,
            string? manufacturer,
            DateTime? purchaseDate,
            decimal? purchaseCost,
            DateTime? warrantyExpiryDate,
            int modifiedBy)
        {
            return await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Assets 
                  SET AssetName = @assetName,
                      SerialNo = @serialNo,
                      ModelNo = @modelNo,
                      Manufacturer = @manufacturer,
                      PurchaseDate = @purchaseDate,
                      PurchaseCost = @purchaseCost,
                      WarrantyExpiryDate = @warrantyExpiryDate,
                      ModifiedDate = NOW(),
                      ModifiedBy = @modifiedBy
                  WHERE AssetID = @assetId",
                "@assetId", assetId,
                "@assetName", assetName,
                "@serialNo", serialNo ?? (object)DBNull.Value,
                "@modelNo", modelNo ?? (object)DBNull.Value,
                "@manufacturer", manufacturer ?? (object)DBNull.Value,
                "@purchaseDate", purchaseDate ?? (object)DBNull.Value,
                "@purchaseCost", purchaseCost ?? (object)DBNull.Value,
                "@warrantyExpiryDate", warrantyExpiryDate ?? (object)DBNull.Value,
                "@modifiedBy", modifiedBy
            );
        }

        /// <summary>
        /// Update asset status (Active/Inactive)
        /// </summary>
        public static async Task<int> UpdateAssetStatusAsync(
            int assetId,
            bool isActive,
            string? reason,
            DateTime? expectedActiveDate,
            int modifiedBy)
        {
            try
            {
                await _sqlHelper.BeginTransactionAsync();

                // Get current status
                var dtCurrent = await _sqlHelper.ExecDataTableAsync(
                    "SELECT IsActive FROM M_Assets WHERE AssetID = @assetId",
                    "@assetId", assetId
                );

                if (dtCurrent.Rows.Count == 0)
                {
                    throw new Exception("Asset not found");
                }

                bool currentStatus = Convert.ToBoolean(dtCurrent.Rows[0]["IsActive"]);

                // Update asset status
                var result = await _sqlHelper.ExecNonQueryAsync(
                    @"UPDATE M_Assets 
                      SET IsActive = @isActive,
                          InactiveReason = @reason,
                          InactiveDate = @inactiveDate,
                          ExpectedActiveDate = @expectedActiveDate,
                          ModifiedDate = NOW(),
                          ModifiedBy = @modifiedBy
                      WHERE AssetID = @assetId",
                    "@assetId", assetId,
                    "@isActive", isActive,
                    "@reason", reason ?? (object)DBNull.Value,
                    "@inactiveDate", !isActive ? (object)DateTime.Now : DBNull.Value,
                    "@expectedActiveDate", expectedActiveDate ?? (object)DBNull.Value,
                    "@modifiedBy", modifiedBy
                );

                // Log status change in history
                if (currentStatus != isActive)
                {
                    await _sqlHelper.ExecNonQueryAsync(
                        @"INSERT INTO T_Asset_Status_History 
                          (AssetID, PreviousStatus, NewStatus, Reason, ExpectedActiveDate, ChangedDate, ChangedBy)
                          VALUES 
                          (@assetId, @previousStatus, @newStatus, @reason, @expectedActiveDate, NOW(), @changedBy)",
                        "@assetId", assetId,
                        "@previousStatus", currentStatus,
                        "@newStatus", isActive,
                        "@reason", reason ?? (object)DBNull.Value,
                        "@expectedActiveDate", expectedActiveDate ?? (object)DBNull.Value,
                        "@changedBy", modifiedBy
                    );
                }

                await _sqlHelper.CommitAsync();
                return result;
            }
            catch
            {
                await _sqlHelper.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Delete asset (if no history exists)
        /// </summary>
        public static async Task<int> DeleteAssetAsync(int assetId)
        {
            try
            {
                await _sqlHelper.BeginTransactionAsync();

                // Check if asset has any assignments
                var assignmentCount = await _sqlHelper.ExecScalarAsync(
                    "SELECT COUNT(*) FROM T_Asset_Assignments WHERE AssetID = @assetId",
                    "@assetId", assetId
                );

                if (Convert.ToInt32(assignmentCount) > 0)
                {
                    throw new InvalidOperationException("Cannot delete asset with existing assignments");
                }

                // Delete maintenance history
                await _sqlHelper.ExecNonQueryAsync(
                    "DELETE FROM T_Asset_Maintenance WHERE AssetID = @assetId",
                    "@assetId", assetId
                );

                // Delete status history
                await _sqlHelper.ExecNonQueryAsync(
                    "DELETE FROM T_Asset_Status_History WHERE AssetID = @assetId",
                    "@assetId", assetId
                );

                // Delete asset
                var result = await _sqlHelper.ExecNonQueryAsync(
                    "DELETE FROM M_Assets WHERE AssetID = @assetId",
                    "@assetId", assetId
                );

                await _sqlHelper.CommitAsync();
                return result;
            }
            catch
            {
                await _sqlHelper.RollbackAsync();
                throw;
            }
        }

        #endregion
    }
}
