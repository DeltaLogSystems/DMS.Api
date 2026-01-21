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
        // Removed static shared MySQLHelper to fix concurrency issues

        // Each method creates its own instance for thread-safety

        /// <summary>
        /// Tests MySQL connection and returns server version
        /// </summary>
        public static async Task<string> GetServerVersionAsync()
        {
            using var sqlHelper = new MySQLHelper();
            var result = await sqlHelper.ExecScalarAsync("SELECT VERSION()");
            return result?.ToString() ?? "Unknown";
        }

        /// <summary>
        /// Gets current database information
        /// </summary>
        public static async Task<DataTable> GetDatabaseInfoAsync()
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
                "SELECT DATABASE() as CurrentDatabase, NOW() as ServerTime, USER() as CurrentUser"
            );
            return dt;
        }

        /// <summary>
        /// Gets list of all tables in current database
        /// </summary>
        public static async Task<DataTable> GetAllTablesAsync()
        {
            using var sqlHelper = new MySQLHelper();
            var dt = await sqlHelper.ExecDataTableAsync(
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
            using var sqlHelper = new MySQLHelper();
            var result = await sqlHelper.ExecScalarAsync(
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
            using var sqlHelper = new MySQLHelper();
            try
            {
                await sqlHelper.BeginTransactionAsync();

                await sqlHelper.ExecNonQueryAsync("CREATE TEMPORARY TABLE test_temp (id INT, name VARCHAR(50))");

                await sqlHelper.ExecNonQueryAsync(
                    "INSERT INTO test_temp (id, name) VALUES (@id, @name)",
                    "@id", 1,
                    "@name", "Test Transaction"
                );

                var count = await sqlHelper.ExecScalarAsync("SELECT COUNT(*) FROM test_temp");

                await sqlHelper.RollbackAsync();

                return $"Transaction test successful. Inserted {count} record(s) and rolled back.";
            }
            catch (Exception ex)
            {
                await sqlHelper.RollbackAsync();
                throw new Exception($"Transaction test failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if database connection is alive
        /// </summary>
        public static async Task<bool> IsConnectionAliveAsync()
        {
            using var sqlHelper = new MySQLHelper();
            try
            {
                await sqlHelper.ExecScalarAsync("SELECT 1");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
