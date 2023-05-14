using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netcore.IDAL
{
    public interface Isys_food_nutrition
    {
        NetCore.Models.sys_food_nutrition Get(string id);
        bool Save(NetCore.Models.sys_food_nutrition food_Info, out string id);
        NetCore.Models.sys_food_nutrition GetSysUser(string id);

        bool delete(string id);


        List<NetCore.Models.sys_food_nutrition> GetFoodInfoList(NetCore.Models.sys_food_nutrition model, int pageindex, int pagesize, int tota);
    }
}
