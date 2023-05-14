using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;

namespace NetCore.Models
{
    [Table("sys_food_info")]
    public class sys_food_info
    {
        /// <summary>
        /// 菜品Id
        /// </summary>
        [ExplicitKey]
        public string id { get; set; }

        /// <summary>
        /// 菜品编码
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string spec { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string unit { get; set; }

        /// <summary>
        /// 分类Id
        /// </summary>
        public int? typeid { get; set; }

        /// <summary>
        /// 分类名
        /// </summary>
        public string typename { get; set; }

        /// <summary>
        /// 所属者
        /// </summary>
        public int? owner { get; set; }

        /// <summary>
        /// 所属者名称
        /// </summary>
        public string ownername { get; set; }

        /// <summary>
        /// 成本价
        /// </summary>
        public decimal costprice { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal price { get; set; }

        /// <summary>
        /// 毛利率
        /// </summary>
        public decimal grossmargin { get; set; }

        /// <summary>
        /// 提醒量
        /// </summary>
        public decimal beststock { get; set; }

        /// <summary>
        /// 备注说明
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// 菜品图片
        /// </summary>
        public string pic { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public int? active { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string createtime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string creater { get; set; }

        /// <summary>
        /// 关联食材
        /// </summary>
        public string ingredients { get; set; }

        [Write(false)]
        public sys_food_nutrition sys_food_nutrition { get; set; }

        [Write(false)]
        public PageNavigation page { get; set; }
    }
}
