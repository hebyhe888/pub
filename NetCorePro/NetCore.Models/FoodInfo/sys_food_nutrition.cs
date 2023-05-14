using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;

namespace NetCore.Models
{
    [Table("sys_food_nutrition")]
    public class sys_food_nutrition
    {
        /// <summary>
        /// 菜品Id
        /// </summary>
        [ExplicitKey]
        public string food_id { get; set; }

        /// <summary>
        /// 其他热量(卡)
        /// </summary>
        public decimal other_calories { get; set; }

        /// <summary>
        /// 蛋白质(G)
        /// </summary>
        public decimal protein { get; set; }

        /// <summary>
        /// 碳水化合物(G)
        /// </summary>
        public decimal carbohydrate { get; set; }

        /// <summary>
        /// 脂肪(G)
        /// </summary>
        public decimal fat { get; set; }

        /// <summary>
        /// 纤维素(G)
        /// </summary>
        public decimal cellulose { get; set; }


    }
}
