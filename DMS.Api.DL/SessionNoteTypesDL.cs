using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class SessionNoteTypesDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region GET Operations

        /// <summary>
        /// Get all session note types
        /// </summary>
        public static async Task<DataTable> GetAllNoteTypesAsync(
            string? category = null,
            bool? mandatoryOnly = null,
            bool activeOnly = true)
        {
            string query = "SELECT * FROM M_Session_Note_Types WHERE 1=1";

            var parameters = new List<object>();

            if (activeOnly)
            {
                query += " AND IsActive = 1";
            }

            if (!string.IsNullOrEmpty(category))
            {
                query += " AND Category = @category";
                parameters.Add("@category");
                parameters.Add(category);
            }

            if (mandatoryOnly.HasValue)
            {
                query += " AND IsMandatory = @isMandatory";
                parameters.Add("@isMandatory");
                parameters.Add(mandatoryOnly.Value);
            }

            query += " ORDER BY DisplayOrder, NoteTypeName";

            return await _sqlHelper.ExecDataTableAsync(query, parameters.ToArray());
        }

        /// <summary>
        /// Get note type by ID
        /// </summary>
        public static async Task<DataTable> GetNoteTypeByIdAsync(int noteTypeId)
        {
            return await _sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_Session_Note_Types WHERE NoteTypeID = @noteTypeId",
                "@noteTypeId", noteTypeId
            );
        }

        /// <summary>
        /// Get mandatory note types
        /// </summary>
        public static async Task<DataTable> GetMandatoryNoteTypesAsync()
        {
            return await _sqlHelper.ExecDataTableAsync(
                @"SELECT * FROM M_Session_Note_Types 
                  WHERE IsMandatory = 1 AND IsActive = 1 
                  ORDER BY DisplayOrder"
            );
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Create session note type
        /// </summary>
        public static async Task<int> CreateNoteTypeAsync(
            string noteTypeName,
            string noteTypeCode,
            string? description,
            string? unitOfMeasure,
            bool isMandatory,
            bool isNumeric,
            decimal? minimumValue,  // Changed from minValue
            decimal? maximumValue,  // Changed from maxValue
            string? defaultValue,
            int displayOrder,
            string? category,
            int createdBy)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                @"INSERT INTO M_Session_Note_Types 
                  (NoteTypeName, NoteTypeCode, Description, UnitOfMeasure, IsMandatory, 
                   IsNumeric, MinimumValue, MaximumValue, DefaultValue, DisplayOrder, Category,
                   IsActive, CreatedDate, CreatedBy)
                  VALUES 
                  (@noteTypeName, @noteTypeCode, @description, @unitOfMeasure, @isMandatory,
                   @isNumeric, @minimumValue, @maximumValue, @defaultValue, @displayOrder, @category,
                   1, NOW(), @createdBy);
                  SELECT LAST_INSERT_ID();",
                "@noteTypeName", noteTypeName,
                "@noteTypeCode", noteTypeCode,
                "@description", description ?? (object)DBNull.Value,
                "@unitOfMeasure", unitOfMeasure ?? (object)DBNull.Value,
                "@isMandatory", isMandatory,
                "@isNumeric", isNumeric,
                "@minimumValue", minimumValue ?? (object)DBNull.Value,
                "@maximumValue", maximumValue ?? (object)DBNull.Value,
                "@defaultValue", defaultValue ?? (object)DBNull.Value,
                "@displayOrder", displayOrder,
                "@category", category ?? (object)DBNull.Value,
                "@createdBy", createdBy
            );

            return Convert.ToInt32(result);
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update note type
        /// </summary>
        public static async Task<int> UpdateNoteTypeAsync(
            int noteTypeId,
            string noteTypeName,
            string? description,
            string? unitOfMeasure,
            bool isMandatory,
            bool isNumeric,
            decimal? minimumValue,  // Changed from minValue
            decimal? maximumValue,  // Changed from maxValue
            string? defaultValue,
            int displayOrder,
            string? category,
            int modifiedBy)
        {
            return await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Session_Note_Types 
                  SET NoteTypeName = @noteTypeName,
                      Description = @description,
                      UnitOfMeasure = @unitOfMeasure,
                      IsMandatory = @isMandatory,
                      IsNumeric = @isNumeric,
                      MinimumValue = @minimumValue,
                      MaximumValue = @maximumValue,
                      DefaultValue = @defaultValue,
                      DisplayOrder = @displayOrder,
                      Category = @category,
                      ModifiedDate = NOW(),
                      ModifiedBy = @modifiedBy
                  WHERE NoteTypeID = @noteTypeId",
                "@noteTypeId", noteTypeId,
                "@noteTypeName", noteTypeName,
                "@description", description ?? (object)DBNull.Value,
                "@unitOfMeasure", unitOfMeasure ?? (object)DBNull.Value,
                "@isMandatory", isMandatory,
                "@isNumeric", isNumeric,
                "@minimumValue", minimumValue ?? (object)DBNull.Value,
                "@maximumValue", maximumValue ?? (object)DBNull.Value,
                "@defaultValue", defaultValue ?? (object)DBNull.Value,
                "@displayOrder", displayOrder,
                "@category", category ?? (object)DBNull.Value,
                "@modifiedBy", modifiedBy
            );
        }

        /// <summary>
        /// Toggle note type status
        /// </summary>
        public static async Task<int> ToggleNoteTypeStatusAsync(int noteTypeId, bool isActive, int modifiedBy)
        {
            return await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Session_Note_Types 
                  SET IsActive = @isActive,
                      ModifiedDate = NOW(),
                      ModifiedBy = @modifiedBy
                  WHERE NoteTypeID = @noteTypeId",
                "@noteTypeId", noteTypeId,
                "@isActive", isActive,
                "@modifiedBy", modifiedBy
            );
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Delete note type
        /// </summary>
        public static async Task<int> DeleteNoteTypeAsync(int noteTypeId)
        {
            // Check if any session notes exist with this type
            var count = await _sqlHelper.ExecScalarAsync(
                "SELECT COUNT(*) FROM T_Session_Notes WHERE NoteTypeID = @noteTypeId",
                "@noteTypeId", noteTypeId
            );

            if (Convert.ToInt32(count) > 0)
            {
                throw new InvalidOperationException("Cannot delete note type with existing session notes");
            }

            return await _sqlHelper.ExecNonQueryAsync(
                "DELETE FROM M_Session_Note_Types WHERE NoteTypeID = @noteTypeId",
                "@noteTypeId", noteTypeId
            );
        }

        #endregion
    }
}
