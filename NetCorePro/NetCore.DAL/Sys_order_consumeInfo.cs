using DBUtility.ORM.Dapper;
using Netcore.IDAL;
using NetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore.DAL
{
    public class Sys_order_consumeInfo:ISys_order_consumeInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsExists(string id)
        {
            return DapperHelper.Get<NetCore.Models.sys_order_consume, string>(id) != null;
        }
    }
}
