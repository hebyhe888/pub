using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBUtility.ORM.Dapper;
using NetCore.Models;
using ServiceStack;
using Netcore.IDAL;

namespace NetCore.DAL
{
    public class Sys_order_infoInfo:ISys_order_infoInfo
    {
        private ISys_order_consumeInfo _Order_ConsumeInfo { get; set; }
        public Sys_order_infoInfo(ISys_order_consumeInfo sys_Order_Consume)
        {
            this._Order_ConsumeInfo= sys_Order_Consume;
        }
        /// <summary>
        /// 订单是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsExists(string id)
        {
            return DapperHelper.Get<sys_order_info,string>(id)!=null;
        }
        public bool Save(sys_order_info _Order_Info)
        {
            bool result = false;
            if(!IsExists(_Order_Info.id))
            {
                result = DapperHelper.Insert<sys_order_info>(_Order_Info);
            }
            else
            {
                result=DapperHelper.Update<sys_order_info>(_Order_Info);
            }
            return result;
        }
        /// <summary>
        /// 保存订单记录
        /// </summary>
        /// <param name="_Order_Infos"></param>
        /// <returns></returns>
        public bool Save(List<sys_order_info> _Order_Infos)
        {
            bool status = false;
            List<string> sqls = new List<string>();
            if(_Order_Infos!= null && _Order_Infos.Count>0) 
            {
                _Order_Infos.ForEach(line =>
                {
                    #region 订单主表
                    if (!IsExists(line.id))
                    {
                        sqls.Add(Utils.SqlConstructor.Insert<sys_order_info>(line));
                    }
                    else
                    {
                        sqls.Add(Utils.SqlConstructor.UpdatePlus<sys_order_info>(line));
                    }
                    #endregion
                    #region 消费表
                    if (line._Order_Consumes!=null && line._Order_Consumes.Count>0)
                    {
                        line._Order_Consumes.ForEach(row =>
                        {
                            if(!_Order_ConsumeInfo.IsExists(row.cost_id))
                            {
                                sqls.Add(Utils.SqlConstructor.Insert<sys_order_consume>(row));
                            }
                            else
                            {
                                sqls.Add(Utils.SqlConstructor.UpdatePlus<sys_order_consume>(row));
                            }
                        });
                    }
                    #endregion
                });
            }
            status = DapperHelper.ExecuteTransaction(sqls);
            return status;  
        }
        /// <summary>
        /// 返回订单单笔记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Models.sys_order_info Get_Order_Info(string id)
        {
            var sql = $"select a.*,b.*from sys_order_info a left outer join sys_order_consume  b  on a.id=b.order_id where a.id='{id}'";
            Dictionary<string, Models.sys_order_info> lookup = new Dictionary<string, sys_order_info>();
            DapperHelper.Query<Models.sys_order_info, Models.sys_order_consume,
                Models.sys_order_info>(sql, (a, b) =>
            {
                Models.sys_order_info temporder = null;
                if(!lookup.TryGetValue(a.id,out temporder))
                {
                    temporder = a;
                    lookup.Add(a.id,temporder);
                }
                if(temporder._Order_Consumes==null)
                    temporder._Order_Consumes= new List<sys_order_consume>();
                Models.sys_order_consume tempconsume = 
                temporder._Order_Consumes.Find(line => line.order_id == b.order_id && line.cost_id == b.cost_id);
                if(tempconsume == null)
                {
                    tempconsume = b;
                    temporder._Order_Consumes.Add(tempconsume);
                }
                return a;
            }, splitOn: "cost_id").ToList();
            return lookup.Count > 0 ? lookup.Values.FirstOrDefault() : null;
        }

    }
}
