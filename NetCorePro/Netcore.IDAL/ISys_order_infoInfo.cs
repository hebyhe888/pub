using NetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Netcore.IDAL
{
    public interface ISys_order_infoInfo
    {
        bool Save(List<sys_order_info> _Order_Infos);

        NetCore.Models.sys_order_info Get_Order_Info(string id);
    }
}
