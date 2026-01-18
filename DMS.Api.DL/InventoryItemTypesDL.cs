using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class InventoryItemTypesDL
    {
        private static MySQLHelper _sqlHelper = new MySQLHelper();

        #region GET Operations

        /// <summary>
        /// Get all inventory item types
        /// </summary>
        public static async Task<DataTable> GetAllItemTypesAsync(bool activeOnly = true)
        {
            string query = activeOnly
                ? "SELECT * FROM M_Inventory_Item_Types WHERE IsActive = 1 ORDER BY ItemTypeName"
                : "SELECT * FROM M_Inventory_Item_Types ORDER BY ItemTypeName";

            return await _sqlHelper.ExecDataTableAsync(query);
        }

        /// <summary>
        /// Get item type by ID
        /// </summary>
        public static async Task<DataTable> GetItemTypeByIdAsync(int itemTypeId)
        {
            return await _sqlHelper.ExecDataTableAsync(
                "SELECT * FROM M_Inventory_Item_Types WHERE ItemTypeID = @itemTypeId",
                "@itemTypeId", itemTypeId
            );
        }

        #endregion

        #region INSERT Operations

        /// <summary>
        /// Create new item type
        /// </summary>
        public static async Task<int> CreateItemTypeAsync(
            string itemTypeName,
            string itemTypeCode,
            string? description,
            int createdBy)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                @"INSERT INTO M_Inventory_Item_Types 
                  (ItemTypeName, ItemTypeCode, Description, IsActive, CreatedDate, CreatedBy)
                  VALUES 
                  (@itemTypeName, @itemTypeCode, @description, 1, NOW(), @createdBy);
                  SELECT LAST_INSERT_ID();",
                "@itemTypeName", itemTypeName,
                "@itemTypeCode", itemTypeCode,
                "@description", description ?? (object)DBNull.Value,
                "@createdBy", createdBy
            );

            return Convert.ToInt32(result);
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Update item type
        /// </summary>
        public static async Task<int> UpdateItemTypeAsync(
            int itemTypeId,
            string itemTypeName,
            string? description,
            int modifiedBy)
        {
            return await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Inventory_Item_Types 
                  SET ItemTypeName = @itemTypeName,
                      Description = @description,
                      ModifiedDate = NOW(),
                      ModifiedBy = @modifiedBy
                  WHERE ItemTypeID = @itemTypeId",
                "@itemTypeId", itemTypeId,
                "@itemTypeName", itemTypeName,
                "@description", description ?? (object)DBNull.Value,
                "@modifiedBy", modifiedBy
            );
        }

        /// <summary>
        /// Toggle item type status
        /// </summary>
        public static async Task<int> ToggleItemTypeStatusAsync(int itemTypeId, bool isActive, int modifiedBy)
        {
            return await _sqlHelper.ExecNonQueryAsync(
                @"UPDATE M_Inventory_Item_Types 
                  SET IsActive = @isActive,
                      ModifiedDate = NOW(),
                      ModifiedBy = @modifiedBy
                  WHERE ItemTypeID = @itemTypeId",
                "@itemTypeId", itemTypeId,
                "@isActive", isActive,
                "@modifiedBy", modifiedBy
            );
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Delete item type
        /// </summary>
        public static async Task<int> DeleteItemTypeAsync(int itemTypeId)
        {
            // Check if any items exist with this type
            var count = await _sqlHelper.ExecScalarAsync(
                "SELECT COUNT(*) FROM M_Inventory_Items WHERE ItemTypeID = @itemTypeId",
                "@itemTypeId", itemTypeId
            );

            if (Convert.ToInt32(count) > 0)
            {
                throw new InvalidOperationException("Cannot delete item type with existing inventory items");
            }

            return await _sqlHelper.ExecNonQueryAsync(
                "DELETE FROM M_Inventory_Item_Types WHERE ItemTypeID = @itemTypeId",
                "@itemTypeId", itemTypeId
            );
        }

        #endregion
    }
}
