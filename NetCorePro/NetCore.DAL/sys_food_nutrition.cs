using DBUtility.ORM.Dapper;
using Netcore.IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore.DAL
{
    public class sys_food_nutrition : Isys_food_nutrition
    {
        public bool IsExists(string id)
        {
            return DapperHelper.Get<Models.sys_food_nutrition>(new Models.sys_food_nutrition { food_id = id }) != null;
        }
        //public bool IsExists_Name(string name)
        //{
        //    return DapperHelper.Get<Models.sys_food_nutrition>(new Models.sys_food_nutrition { name = name }) != null;
        //}
        public Models.sys_food_nutrition Get(string id)
        {
            return DapperHelper.Get<Models.sys_food_nutrition, string>(id);
        }
        public Models.sys_food_nutrition GetSysUser(string userid)
        {
            return DapperHelper.GetSingleList(new Models.sys_food_nutrition() { food_id = userid }).FirstOrDefault();
        }

        public Models.sys_food_nutrition GetSysName(string name)
        {
            return DapperHelper.GetSingleList(new Models.sys_food_nutrition() { food_id = name }).FirstOrDefault();
        }
        public bool Save(Models.sys_food_nutrition food_Info, out string id)
        {
            bool flag = false;
            id = "";
            //菜品名称是否存在
          
                if (!IsExists(food_Info.food_id))
                {
                    flag = DapperHelper.Insert(food_Info);  //DapperHelper.Execute(SqlConstructor.Insert(tUSER));
                }
                else
                {
                    id = food_Info.food_id;
                    flag = DapperHelper.Update(food_Info); //DapperHelper.Execute(SqlConstructor.UpdatePlus(tUSER));
                }
            return flag;
        }
        public bool delete(string id)
        {
            #region 方法1:
            return DapperHelper.delete(new Models.sys_food_nutrition() { food_id = id });
            #endregion
        }

        public Models.sys_food_nutrition Get(int id)
        {
            throw new NotImplementedException();
        }

        public List<Models.sys_food_nutrition> GetFoodInfoList(Models.sys_food_nutrition model, int pageindex, int pagesize, int tota)
        {
            return DapperHelper.GetListByPage(model, "createtime", pageindex, pagesize, out tota);
        }
    }
}
