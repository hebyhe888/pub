using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using Dapper.Contrib.Extensions;

namespace NetCore.Models
{

    /// <summary>
    /// </summary>

    [Table("sys_order_info")]
    public class sys_order_info
    {
        public sys_order_info()
        {
            this._Order_Consumes = new List<sys_order_consume>();
        }
        [ExplicitKey]
        public string id
        {
            get;
            set;
        }
        /// <summary>
        /// 订单日期
        /// </summary>
        public string orderdate
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string platecode
        {
            get;
            set;
        }

        public string userid
        {
            get;
            set;
        }
        public string username
        {
            get;
            set;
        }
        public int owner
        {
            get;
            set;
        }
        public string ownername
        {
            get;
            set;
        }
        public string plateway
        {
            get;
            set;
        }
        public string foods
        {
            get;
            set;
        }
        public int state
        {
            get;
            set;
        }
        public decimal order_amt
        {
            get;
            set;
        }
        public decimal preferential_amt
        {
            get;
            set;
        }
        public decimal payment_amt
        {
            get;
            set;
        }	
        public decimal return_amt
        {
            get;
            set;
        }
        public int returnstate
        {
            get;
            set;
        }
        public int executionstate
        {
            get;
            set;
        }
        public int ordertype
        {
            get;
            set;
        }
        public string mealdate
        {
            get;
            set;
        }
        public int mealaddress_id
        {
            get;
            set;
        }

        public string remark
        {
            get;
            set;
        }
        public int isprint
        {
            get;
            set;
        }
        [Write(false)]
        public List<sys_order_consume> _Order_Consumes { get; set; }
        [Write(false)]
        public sys_member_info _Member_Info { get; set; }
    }
}