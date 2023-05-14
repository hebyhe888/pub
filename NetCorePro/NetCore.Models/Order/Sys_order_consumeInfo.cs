using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Dapper.Contrib.Extensions;

namespace NetCore.Models
{
    [Table("sys_order_consume")]
    public class sys_order_consume
    {
        [ExplicitKey]
        public string cost_id
        {
            get;set;
        }
        public string order_id
        {
            get; set;
        }
        /// <summary>
        /// 消费金额
        /// </summary>
        public decimal payment_amount
        {
            get; set;
        }
        public string payment_way
        {
            get; set;
        }
        public string payment_time
        {
            get; set;
        }
        public int payment_type
        {
            get; set;
        }
        public decimal return_amt
        {
            get; set;
        }
        public decimal status
        {
            get; set;
        }
    }
}