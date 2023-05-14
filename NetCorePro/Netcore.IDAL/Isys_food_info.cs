using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Netcore.IDAL
{
    public interface Isys_food_info
    {
        NetCore.Models.sys_food_info Get(string id);
        bool Save(NetCore.Models.sys_food_info food_Info, out string id);
        NetCore.Models.sys_food_info GetSysUser(string id);

        bool delete(string id);


        List<NetCore.Models.sys_food_info> GetFoodInfoList(NetCore.Models.sys_food_info model, ref int totalrecords);
    }
}
