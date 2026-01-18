using System.Data;
using MySqlConnector;
using DMS.Api.Shared;

namespace DMS.Api.DL
{
    public class MySQLHelper : IDisposable, IAsyncDisposable
    {
        private readonly string _connString;
        private MySqlConnection? _conn;
        private MySqlTransaction? _trans;
        private bool _disposed;

        /// <summary>
        /// Constructor using global connection string from Variables
        /// </summary>
        public MySQLHelper()
        {
            _connString = Variables.ConnectionString ?? throw new InvalidOperationException("Connection string not set in Variables.");
            InitializeConnection();
        }

        /// <summary>
        /// Constructor with custom connection string (optional)
        /// </summary>
        public MySQLHelper(string connectionString)
        {
            _connString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            InitializeConnection();
        }

        private void InitializeConnection()
        {
            _conn = new MySqlConnection(_connString);
        }

        private async Task EnsureConnectionOpenAsync()
        {
            if (_conn == null) throw new ObjectDisposedException(nameof(MySQLHelper));
            if (_conn.State != ConnectionState.Open)
            {
                await _conn.OpenAsync();
            }
        }

        public MySqlCommand CreateCommand(string qry, CommandType type, params object[] args)
        {
            if (_conn == null) throw new ObjectDisposedException(nameof(MySQLHelper));

            var cmd = new MySqlCommand(qry, _conn)
            {
                CommandType = type,
                Transaction = _trans
            };

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is string parameterName && i < args.Length - 1)
                {
                    cmd.Parameters.AddWithValue(parameterName, args[++i]);
                }
                else if (args[i] is MySqlParameter parameter)
                {
                    cmd.Parameters.Add(parameter);
                }
                else
                {
                    throw new ArgumentException("Invalid number or type of arguments supplied");
                }
            }
            return cmd;
        }

        #region Exec Members

        public async Task<int> ExecNonQueryAsync(string qry, params object[] args)
        {
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(qry, CommandType.Text, args);
            try
            {
                var result = await cmd.ExecuteNonQueryAsync();
                if (_trans != null) await CommitAsync();
                return result;
            }
            catch
            {
                if (_trans != null) await RollbackAsync();
                throw;
            }
        }

        public async Task<int> ExecNonQueryProcAsync(string proc, params object[] args)
        {
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(proc, CommandType.StoredProcedure, args);
            try
            {
                cmd.CommandTimeout = 0;
                var result = await cmd.ExecuteNonQueryAsync();
                if (_trans != null) await CommitAsync();
                return result;
            }
            catch
            {
                if (_trans != null) await RollbackAsync();
                throw;
            }
        }

        public async Task<object?> ExecScalarAsync(string qry, params object[] args)
        {
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(qry, CommandType.Text, args);
            return await cmd.ExecuteScalarAsync();
        }

        public async Task<object?> ExecScalarProcAsync(string proc, params object[] args)
        {
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(proc, CommandType.StoredProcedure, args);
            return await cmd.ExecuteScalarAsync();
        }

        public async Task<MySqlDataReader> ExecDataReaderAsync(string qry, params object[] args)
        {
            await EnsureConnectionOpenAsync();
            var cmd = CreateCommand(qry, CommandType.Text, args);
            return await cmd.ExecuteReaderAsync();
        }

        public async Task<MySqlDataReader> ExecDataReaderProcAsync(string proc, params object[] args)
        {
            await EnsureConnectionOpenAsync();
            var cmd = CreateCommand(proc, CommandType.StoredProcedure, args);
            return await cmd.ExecuteReaderAsync();
        }

        public async Task<DataTable> ExecDataTableAsync(string qry, params object[] args)
        {
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(qry, CommandType.Text, args);
            using var reader = await cmd.ExecuteReaderAsync();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public async Task<DataTable> ExecDataTableProcAsync(string proc, params object[] args)
        {
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(proc, CommandType.StoredProcedure, args);
            using var reader = await cmd.ExecuteReaderAsync();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public async Task<DataSet> ExecDataSetAsync(string qry, params object[] args)
        {
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(qry, CommandType.Text, args);
            using var reader = await cmd.ExecuteReaderAsync();
            var ds = new DataSet();
            var dt = new DataTable();
            dt.Load(reader);
            ds.Tables.Add(dt);
            return ds;
        }

        public async Task<DataSet> ExecDataSetProcAsync(string proc, params object[] args)
        {
            await EnsureConnectionOpenAsync();
            using var cmd = CreateCommand(proc, CommandType.StoredProcedure, args);
            using var reader = await cmd.ExecuteReaderAsync();
            var ds = new DataSet();
            var dt = new DataTable();
            dt.Load(reader);
            ds.Tables.Add(dt);
            return ds;
        }

        #endregion

        #region Transaction Members

        public async Task<MySqlTransaction> BeginTransactionAsync()
        {
            await EnsureConnectionOpenAsync();
            if (_trans != null)
            {
                await _trans.RollbackAsync();
                _trans = null;
            }
            _trans = await _conn!.BeginTransactionAsync();
            return _trans;
        }

        public async Task CommitAsync()
        {
            if (_trans != null)
            {
                await _trans.CommitAsync();
                await _trans.DisposeAsync();
                _trans = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_trans != null)
            {
                await _trans.RollbackAsync();
                await _trans.DisposeAsync();
                _trans = null;
            }
        }

        #endregion

        #region IDisposable & IAsyncDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _trans?.Dispose();
                _conn?.Dispose();
                _trans = null;
                _conn = null;
            }
            _disposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            if (_trans != null)
            {
                await _trans.DisposeAsync();
                _trans = null;
            }
            if (_conn != null)
            {
                await _conn.DisposeAsync();
                _conn = null;
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
