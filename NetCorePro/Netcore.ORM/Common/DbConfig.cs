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
using System.Data;
using System.Data.OleDb;
using System.Collections.Concurrent;
using Oracle.ManagedDataAccess.Client;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration;

namespace DBUtility.Dapper.Common
{
    /// <summary>
    /// 数据库配置
    /// </summary>
    public class ConnectionFactory
    {
        /// <summary>
        /// 数据库连接字符串缓存
        /// </summary>
        public static IDbConnection GetSqlConnection(ConnType connType = ConnType.MYSQL)
        {
            IDbConnection conn = default(IDbConnection);
            switch (connType)
            {
                case ConnType.MYSQL:
                    {
                        conn = new MySqlConnection(Utils.AppSettingHelper._MySqlConnectionString);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            conn.Open();
            return conn;
        }
    }
    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum ConnType
    {
        POLARDB = 1,
        ORACLE = 2,
        MYSQL = 3,
        SQLSERVER = 4,
        HANA = 5
    }
}
