    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore.Models
{
    /// <summary>
    /// 公共页码参数
    /// </summary>
    //[DataContract]
    public class PageNavigation
    {
        //[DataMember(Order = 1)]
        public int PageSize { get; set; } = 10;
        //[DataMember(Order = 2)]
        public int pageno { get; set; } = 1;
    }
}
