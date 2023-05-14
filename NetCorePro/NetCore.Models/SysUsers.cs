using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;

namespace NetCore.Models
{
    [Table("sys_user")]
    public class sys_user
    {
        [ExplicitKey]
        public string id { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        public string userid { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string name { get;set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string jobnumber { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public string avatar { get; set; }
        /// <summary>
        /// 所属组织(运营商、食堂、档口)
        /// </summary>
        public string department { get; set; }
        /// <summary>
        /// 所属组织(运营商、食堂、档口)名称
        /// </summary>
        public string departmentname { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public string mobile { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
        public string password { get; set; }
        /// <summary>
        /// 用户角色（0系统管理员、1、运营商 2、食堂管理者3、食堂作业员 4、档口管理者 5、档口作业员）
        /// </summary>
        public int? userrole { get; set; }
        /// <summary>
        /// 是否有效(默认值1)
        /// </summary>
        public int? active { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public string createtime { get; set; }
        public string creater { get; set; }
        public string updatetime { get; set; }
        public string updater { get;set; }
        public string tenantCode { get; set; }
        /// <summary>
        /// 用户主体唯一码
        /// </summary>
        public string union_id { get; set; }
    }
}
