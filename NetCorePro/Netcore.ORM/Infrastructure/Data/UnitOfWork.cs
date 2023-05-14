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
using System;
using System.Data;

namespace DBUtility.Dapper.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IContext _context;

        public UnitOfWork(IContext context)
        {
            _context = context;
            _context.BeginTransaction();
        }

        public void SaveChanges()
        {
            if (!_context.IsTransactionStarted)
                throw new InvalidOperationException("Transaction have already been commited or disposed.");

            _context.Commit();
        }

        public void Dispose()
        {
            if (_context.IsTransactionStarted)
                _context.Rollback();
        }
    }
}
