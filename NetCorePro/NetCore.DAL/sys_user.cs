using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBUtility.ORM.Dapper;
using Netcore.IDAL;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Bcpg;

namespace NetCore.DAL
{
    public class sys_user:Isys_user
    {
        public bool IsExists(string id)
        {
            return DapperHelper.Get<Models.sys_user>(new Models.sys_user { id = id }) != null;
        }
        public Models.sys_user Get(string id)
        {
            return DapperHelper.Get<Models.sys_user,string>(id);
        }
        public Models.sys_user GetSysUser(string userid)
        {
            return DapperHelper.GetSingleList(new Models.sys_user() { userid = userid }).FirstOrDefault();
        }
        public bool Save(Models.sys_user tUSER)
        {
            bool flag = false;
            if (!string.IsNullOrEmpty(tUSER.password))
            {
                tUSER.password = Utils.DESEncrypt.Encrypt(tUSER.password);
            }
            if (!IsExists(tUSER.id))
            {
                flag = DapperHelper.Insert(tUSER);  //DapperHelper.Execute(SqlConstructor.Insert(tUSER));
            }
            else
            {
                flag = DapperHelper.Update(tUSER); //DapperHelper.Execute(SqlConstructor.UpdatePlus(tUSER));
            }
            return flag;
        }
        public bool delete(string id)
        {
            #region 方法1:
            return DapperHelper.delete(new Models.sys_user() { id = id });
            #endregion
        }
        public List<Models.sys_user> GetTUSERs(Models.sys_user model, int pageindex, int pagesize, out int totalrecords)
        {
            return DapperHelper.GetListByPage(model, "userid", pageindex, pagesize, out totalrecords);
        }
    }
}
