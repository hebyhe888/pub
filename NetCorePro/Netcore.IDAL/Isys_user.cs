using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Netcore.IDAL
{
    public interface Isys_user
    {
        NetCore.Models.sys_user Get(string id);
        bool Save(NetCore.Models.sys_user tUSER);
        NetCore.Models.sys_user GetSysUser(string id);

        bool delete(string id);

        List<NetCore.Models.sys_user> GetTUSERs(NetCore.Models.sys_user model, int pageindex, int pagesize, out int totalrecords);
    }
}
