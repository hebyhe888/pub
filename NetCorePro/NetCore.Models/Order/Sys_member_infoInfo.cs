using System;

using Dapper.Contrib.Extensions;

namespace NetCore.Models
{

    /// <summary>
    /// </summary>

    [Table("sys_member_info")]
    public class sys_member_info
    {
        [ExplicitKey]
        public int id
        {
            get;set;
        }

        public string userid
        {
            get; set;
        }

        public string username
        {
            get; set;
        }

        public string avatarurl
        {
            get; set;
        }

        public int gender
        {
            get; set;
        }

        public string city
        {
            get; set;
        }

        public string province
        {
            get; set;
        }

        public string country
        {
            get; set;
        }

        public string mobile
        {
            get; set;
        }

        public string followdate
        {
            get; set;
        }

        public int paymentway
        {
            get; set;
        }

        public string groupname
        {
            get; set;
        }

        public string openid
        {
            get; set;
        }

        public int active
        {
            get; set;
        }

        public string fullname
        {
            get; set;
        }

        public int usertype
        {
            get; set;
        }

    }
}