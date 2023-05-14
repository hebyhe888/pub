/*命名空间: IPolarDBDataAccess
*
* 功 能： N/A
* 类 名： Class1
* Author   : HebyHe
* Created  : 21 - 09 - 2022 13:40:21
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
*  Copyright (c) 2022 bluestar Corporation.All rights reserved.
*┌──────────────────────────────────┐
*│　此技术信息为本公司机密信息，未经本公司书面同意禁止向第三方披露．　│
*│　版权所有：蓝星科技有限公司                                        │
*└──────────────────────────────────┘
*/
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Options;
using Netcore.ORM.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DBUtility.Dapper.Infrastructure.Data
{
    public abstract class DapperDBContext : IContext
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private int? _commandTimeout = null;
        private readonly DapperDBContextOptions _options;

        public bool IsTransactionStarted { get; private set; }

        protected abstract IDbConnection CreateConnection(string connectionString);

        protected DapperDBContext(IOptions<DapperDBContextOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value;

            _connection = CreateConnection(_options.Configuration);
            if (_connection.State == ConnectionState.Closed)
                _connection.Open(); //modify at 2023-04-27

            DebugPrint("Connection started.");
        }

        #region Transaction

        public void BeginTransaction()
        {
            if (IsTransactionStarted)
                throw new InvalidOperationException("Transaction is already started.");

            _transaction = _connection.BeginTransaction();
            IsTransactionStarted = true;

            DebugPrint("Transaction started.");
        }

        public void Commit()
        {
            if (!IsTransactionStarted)
                throw new InvalidOperationException("No transaction started.");

            _transaction.Commit();
            _transaction = null;

            IsTransactionStarted = false;

            DebugPrint("Transaction committed.");
        }

        public void Rollback()
        {
            if (!IsTransactionStarted)
                throw new InvalidOperationException("No transaction started.");

            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;

            IsTransactionStarted = false;

            DebugPrint("Transaction rollbacked and disposed.");
        }

        #endregion Transaction

        #region Dapper.Contrib.Extensions

        public async Task<T> GetAsync<T>(int id) where T : class, new()
        {
            return await _connection.GetAsync<T>(id, _transaction, _commandTimeout);
        }

        public async Task<T> GetAsync<T>(string id) where T : class, new()
        {
            return await _connection.GetAsync<T>(id, _transaction, _commandTimeout);
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class, new()
        {
            return await _connection.GetAllAsync<T>();
        }

        public long Insert<T>(T model) where T : class, new()
        {
            return _connection.Insert<T>(model, _transaction, _commandTimeout);
        }

        public async Task<int> InsertAsync<T>(T model) where T : class, new()
        {
            return await _connection.InsertAsync<T>(model, _transaction, _commandTimeout);
        }
        public bool Update<T>(T model) where T : class, new()
        {
            return _connection.Update<T>(model, _transaction, _commandTimeout);
        }

        public async Task<bool> UpdateAsync<T>(T model) where T : class, new()
        {
            return await _connection.UpdateAsync<T>(model, _transaction, _commandTimeout);
        }

        public async Task<Page<T>> PageAsync<T>(long pageIndex, long pageSize, string sql, object param = null)
        {
            DapperPage.BuildPageQueries((pageIndex - 1) * pageSize, pageSize, sql, out string sqlCount, out string sqlPage);

            var result = new Page<T>
            {
                CurrentPage = pageIndex,
                ItemsPerPage = pageSize,
                TotalItems = await _connection.ExecuteScalarAsync<long>(sqlCount, param)
            };
            result.TotalPages = result.TotalItems / pageSize;

            if ((result.TotalItems % pageSize) != 0)
                result.TotalPages++;

            result.Items = await _connection.QueryAsync<T>(sqlPage, param);
            return result;
        }


        #endregion


        #region Dapper Execute & Query


        public int ExecuteScalar(string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            return _connection.ExecuteScalar<int>(sql, param, _transaction, _commandTimeout, commandType);
        }

        public async Task<int> ExecuteScalarAsync(string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            return await _connection.ExecuteScalarAsync<int>(sql, param, _transaction, _commandTimeout, commandType);
        }
        public int Execute(string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            return _connection.Execute(sql, param, _transaction, _commandTimeout, commandType);
        }

        public async Task<int> ExecuteAsync(string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            return await _connection.ExecuteAsync(sql, param, _transaction, _commandTimeout, commandType);
        }

        public IEnumerable<T> Query<T>(string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            return _connection.Query<T>(sql, param, _transaction, true, _commandTimeout, commandType);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            return await _connection.QueryAsync<T>(sql, param, _transaction, _commandTimeout, commandType);
        }

        public T QueryFirstOrDefault<T>(string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            return _connection.QueryFirstOrDefault<T>(sql, param, _transaction, _commandTimeout, commandType);
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            return await _connection.QueryFirstOrDefaultAsync<T>(sql, param, _transaction, _commandTimeout, commandType);
        }
        public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id", CommandType commandType = CommandType.Text)
        {
            return _connection.Query(sql, map, param, _transaction, true, splitOn, _commandTimeout, commandType);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id", CommandType commandType = CommandType.Text)
        {
            return await _connection.QueryAsync(sql, map, param, _transaction, true, splitOn, _commandTimeout, commandType);
        }

        public async Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            return await _connection.QueryMultipleAsync(sql, param, _transaction, _commandTimeout, commandType);
        }

        #endregion Dapper Execute & Query

        public void Dispose()
        {
            if (IsTransactionStarted)
                Rollback();

            _connection.Close();
            _connection.Dispose();
            _connection = null;

            DebugPrint("Connection closed and disposed.");
        }

        private void DebugPrint(string message)
        {
#if DEBUG
            Debug.Print(">>> UnitOfWorkWithDapper - Thread {0}: {1}", Thread.CurrentThread.ManagedThreadId, message);
#endif
        }
    }
}
