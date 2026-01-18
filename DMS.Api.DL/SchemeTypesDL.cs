using System.Data;

namespace DMS.Api.DL
{
    public static class SchemeTypesDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region GET Operations

        /// <summary>
        /// Get all scheme types
        /// </summary>
        public static async Task<DataTable> GetAllSchemeTypesAsync(bool activeOnly = false)
        {
            string query = activeOnly
                ? "SELECT * FROM M_SchemeTypes WHERE IsActive = 1 ORDER BY SchemeTypeName"
                : "SELECT * FROM M_SchemeTypes ORDER BY SchemeTypeName";

            var dt = await _sqlHelper.ExecDataTableAsync(query);
            return dt;
        }

        /// <summary>
        /// Get scheme type by ID
        /// </summary>
        public static async Task<DataTable> GetSchemeTypeByIdAsync(int schemeTypeId)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_SchemeTypes WHERE SchemeTypeID = @schemeTypeId",
                "@schemeTypeId", schemeTypeId
            );
            return dt;
        }

        /// <summary>
        /// Get scheme type by name
        /// </summary>
        public static async Task<DataTable> GetSchemeTypeByNameAsync(string schemeTypeName)
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_SchemeTypes WHERE SchemeTypeName = @schemeTypeName",
                "@schemeTypeName", schemeTypeName
            );
            return dt;
        }

        /// <summary>
        /// Check if scheme type name exists
        /// </summary>
        public static async Task<bool> SchemeTypeNameExistsAsync(string schemeTypeName, int? excludeSchemeTypeId = null)
        {
            string query = excludeSchemeTypeId.HasValue
                ? "SELECT COUNT(*) FROM M_SchemeTypes WHERE SchemeTypeName = @schemeTypeName AND SchemeTypeID != @schemeTypeId"
                : "SELECT COUNT(*) FROM M_SchemeTypes WHERE SchemeTypeName = @schemeTypeName";

            object[] parameters = excludeSchemeTypeId.HasValue
                ? new object[] { "@schemeTypeName", schemeTypeName, "@schemeTypeId", excludeSchemeTypeId.Value }
                : new object[] { "@schemeTypeName", schemeTypeName };

            var result = await _sqlHelper.ExecScalarAsync(query, parameters);
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Get scheme type count
        /// </summary>
        public static async Task<int> GetSchemeTypeCountAsync(bool activeOnly = false)
        {
            string query = activeOnly
                ? "SELECT COUNT(*) FROM M_SchemeTypes WHERE IsActive = 1"
                : "SELECT COUNT(*) FROM M_SchemeTypes";

            var result = await _sqlHelper.ExecScalarAsync(query);
            return Convert.ToInt32(result);
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Insert new scheme type
        /// </summary>
        public static async Task<int> InsertSchemeTypeAsync(
            string schemeTypeName,
            string description,
            string createdBy)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                @"INSERT INTO M_SchemeTypes (SchemeTypeName, Description, IsActive, CreatedDate, CreatedBy)
                  VALUES (@schemeTypeName, @description, 1, CURDATE(), @createdBy);
                  SELECT LAST_INSERT_ID();",
                "@schemeTypeName", schemeTypeName,
                "@description", description ?? "",
                "@createdBy", createdBy
            );
            return Convert.ToInt32(result);
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update scheme type
        /// </summary>
        public static async Task<int> UpdateSchemeTypeAsync(
            int schemeTypeId,
            string schemeTypeName,
            string description)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_SchemeTypes 
                  SET SchemeTypeName = @schemeTypeName,
                      Description = @description
                  WHERE SchemeTypeID = @schemeTypeId",
                "@schemeTypeId", schemeTypeId,
                "@schemeTypeName", schemeTypeName,
                "@description", description ?? ""
            );
            return result;
        }

        /// <summary>
        /// Toggle scheme type active status
        /// </summary>
        public static async Task<int> ToggleSchemeTypeStatusAsync(int schemeTypeId)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                "UPDATE M_SchemeTypes SET IsActive = NOT IsActive WHERE SchemeTypeID = @schemeTypeId",
                "@schemeTypeId", schemeTypeId
            );
            return result;
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Soft delete scheme type
        /// </summary>
        public static async Task<int> SoftDeleteSchemeTypeAsync(int schemeTypeId)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                "UPDATE M_SchemeTypes SET IsActive = 0 WHERE SchemeTypeID = @schemeTypeId",
                "@schemeTypeId", schemeTypeId
            );
            return result;
        }

        /// <summary>
        /// Permanently delete scheme type
        /// </summary>
        public static async Task<int> DeleteSchemeTypeAsync(int schemeTypeId)
        {
            var result = await _sqlHelper.ExecNonQueryAsync(
                "DELETE FROM M_SchemeTypes WHERE SchemeTypeID = @schemeTypeId",
                "@schemeTypeId", schemeTypeId
            );
            return result;
        }

        #endregion
    }
}
