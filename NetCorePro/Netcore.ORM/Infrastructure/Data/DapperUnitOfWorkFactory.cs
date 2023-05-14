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
using DBUtility.Dapper.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBUtility.Dapper.Infrastructure.Data
{
    public class DapperUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly DapperDBContext _context;

        public DapperUnitOfWorkFactory(DapperDBContext context)
        {
            _context = context;
        }

        public IUnitOfWork Create()
        {
            return new UnitOfWork(_context);
        }
    }
}
