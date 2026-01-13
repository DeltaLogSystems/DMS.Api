using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.DL
{
    public static class TestDL
    {
        private static MySQLHelper _sqlHelper = null;

        static TestDL()
        {
            _sqlHelper = new MySQLHelper();
        }

        /// <summary>
        /// Tests MySQL connection and returns server version
        /// </summary>
        public static async Task<string> GetServerVersionAsync()
        {
            var result = await _sqlHelper.ExecScalarAsync("SELECT VERSION()");
            return result?.ToString() ?? "Unknown";
        }

        /// <summary>
        /// Gets current database information
        /// </summary>
        public static async Task<DataTable> GetDatabaseInfoAsync()
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                "SELECT DATABASE() as CurrentDatabase, NOW() as ServerTime, USER() as CurrentUser"
            );
            return dt;
        }

        /// <summary>
        /// Gets list of all tables in current database
        /// </summary>
        public static async Task<DataTable> GetAllTablesAsync()
        {
            var dt = await _sqlHelper.ExecDataTableAsync(
                "SELECT table_name as TableName, table_rows as RowCount " +
                "FROM information_schema.tables " +
                "WHERE table_schema = DATABASE() " +
                "ORDER BY table_name"
            );
            return dt;
        }

        /// <summary>
        /// Tests parameterized query
        /// </summary>
        public static async Task<int> GetTableCountAsync(string tableName)
        {
            var result = await _sqlHelper.ExecScalarAsync(
                "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = @tableName",
                "@tableName", tableName
            );
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Tests transaction
        /// </summary>
        public static async Task<string> TestTransactionAsync()
        {
            try
            {
                await _sqlHelper.BeginTransactionAsync();

                await _sqlHelper.ExecNonQueryAsync("CREATE TEMPORARY TABLE test_temp (id INT, name VARCHAR(50))");

                await _sqlHelper.ExecNonQueryAsync(
                    "INSERT INTO test_temp (id, name) VALUES (@id, @name)",
                    "@id", 1,
                    "@name", "Test Transaction"
                );

                var count = await _sqlHelper.ExecScalarAsync("SELECT COUNT(*) FROM test_temp");

                await _sqlHelper.RollbackAsync();

                return $"Transaction test successful. Inserted {count} record(s) and rolled back.";
            }
            catch (Exception ex)
            {
                await _sqlHelper.RollbackAsync();
                throw new Exception($"Transaction test failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if database connection is alive
        /// </summary>
        public static async Task<bool> IsConnectionAliveAsync()
        {
            try
            {
                await _sqlHelper.ExecScalarAsync("SELECT 1");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
