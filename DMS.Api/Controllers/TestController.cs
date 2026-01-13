using Microsoft.AspNetCore.Mvc;
using DMS.Api.DL;
using System.Data;

namespace DMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var version = await TestDL.GetServerVersionAsync();

                return Ok(new
                {
                    success = true,
                    message = "MySQL connection successful",
                    version = version
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("ping")]
        public async Task<IActionResult> PingDatabase()
        {
            try
            {
                var isAlive = await TestDL.IsConnectionAliveAsync();

                return Ok(new
                {
                    success = isAlive,
                    message = isAlive ? "Database is alive" : "Database is not responding"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("database-info")]
        public async Task<IActionResult> GetDatabaseInfo()
        {
            try
            {
                var dt = await TestDL.GetDatabaseInfoAsync();

                if (dt.Rows.Count > 0)
                {
                    return Ok(new
                    {
                        success = true,
                        database = dt.Rows[0]["CurrentDatabase"]?.ToString(),
                        serverTime = dt.Rows[0]["ServerTime"]?.ToString(),
                        user = dt.Rows[0]["CurrentUser"]?.ToString()
                    });
                }

                return Ok(new { success = true, message = "Query executed but no data" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("tables")]
        public async Task<IActionResult> GetAllTables()
        {
            try
            {
                var dt = await TestDL.GetAllTablesAsync();

                var tables = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    tables.Add(new
                    {
                        tableName = row["TableName"]?.ToString(),
                        rowCount = row["RowCount"]?.ToString()
                    });
                }

                return Ok(new
                {
                    success = true,
                    count = tables.Count,
                    tables = tables
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("table-exists/{tableName}")]
        public async Task<IActionResult> CheckTableExists(string tableName)
        {
            try
            {
                var count = await TestDL.GetTableCountAsync(tableName);

                return Ok(new
                {
                    success = true,
                    tableName = tableName,
                    exists = count > 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("test-transaction")]
        public async Task<IActionResult> TestTransaction()
        {
            try
            {
                var result = await TestDL.TestTransactionAsync();

                return Ok(new
                {
                    success = true,
                    message = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
