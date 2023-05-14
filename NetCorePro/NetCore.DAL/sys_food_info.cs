using Dapper;
using DBUtility.Dapper.Common;
using DBUtility.ORM.Dapper;
using Netcore.IDAL;
using NetCore.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Slapper.AutoMapper;

namespace NetCore.DAL
{
    public class sys_food_info : Isys_food_info
    {
        public bool IsExists(string id)
        {
            return DapperHelper.Get<Models.sys_food_info>(new Models.sys_food_info { id = id }) != null;
        }
        //public bool IsExists_Name(string name)
        //{
        //    return DapperHelper.Get<Models.sys_food_info>(new Models.sys_food_info { name = name }) != null;
        //}
        public Models.sys_food_info Get(string id)
        {
            return DapperHelper.Get<Models.sys_food_info, string>(id);
        }
        public Models.sys_food_info GetSysUser(string userid)
        {
            return DapperHelper.GetSingleList(new Models.sys_food_info() { id = userid }).FirstOrDefault();
        }

        public Models.sys_food_info GetSysName(string name)
        {
            return DapperHelper.GetSingleList(new Models.sys_food_info() { name = name }).FirstOrDefault();
        }
        public bool Save(Models.sys_food_info food_Info,out string id)
        {
            bool flag = false;
            id = "0";
            //菜品名称是否存在
            
            if (!IsExists(food_Info.id))
                {
                if (GetSysName(food_Info.name) == null) flag = DapperHelper.Insert(food_Info);
                food_Info.sys_food_nutrition.food_id = GetSysName(food_Info.name).id;
                DapperHelper.Insert(food_Info.sys_food_nutrition);
            }
                else
                {
                    
                    flag = DapperHelper.Update(food_Info);
                    food_Info.sys_food_nutrition.food_id= food_Info.id;
                    DapperHelper.Update(food_Info.sys_food_nutrition);
                }
            
            return flag;
        }
        public bool delete(string id)
        {
            #region 方法1:
            return DapperHelper.delete(new Models.sys_food_info() { id = id });
            #endregion
        }

        public Models.sys_food_info Get(int id)
        {
            throw new NotImplementedException();
        }

        public List<Models.sys_food_info> GetFoodInfoList(Models.sys_food_info model, ref int totalrecords)
        {
            using (IDbConnection connection = ConnectionFactory.GetSqlConnection())
            {
                StringBuilder sqlbuilder = new StringBuilder();
                sqlbuilder.Append($"select * from sys_food_info slam inner join sys_food_nutrition slal on slam.id = slal.food_id where slam.name='{model.name}' order by slam.createtime desc");
                var lookup = new Dictionary<string, Models.sys_food_info>();
                connection.Query<Models.sys_food_info, Models.sys_food_nutrition,
                    Models.sys_food_info>(sqlbuilder.ToString(), (slam, slal) =>
                    {
                        slam.id = slal.food_id;
                        return slam;
                    }, splitOn: "food_id");
                totalrecords = lookup.Values.AsList().Count;
                return lookup.Values.AsList().Skip((model.page.pageno - 1) * model.page.PageSize).Take(model.page.PageSize).ToList();
            }
        }
    }
}
