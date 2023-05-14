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
*│　版权所有：XXXX                                        │
*└──────────────────────────────────┘
*/
using DBUtility.Dapper.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace DBUtility.Dapper.Common
{
    public class SqlDbContext: DapperDBContext
    {
        public SqlDbContext(IOptions<DapperDBContextOptions> optionsAccessor) : base(optionsAccessor)
        {
        }
        protected override IDbConnection CreateConnection(string connectionString)
        {
            //IDbConnection conn = new MySqlConnection(connectionString);
            //return conn;
            return DBUtility.Dapper.Common.ConnectionFactory.GetSqlConnection();
        }
    }
}
