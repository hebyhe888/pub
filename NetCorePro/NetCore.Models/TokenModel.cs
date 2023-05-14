﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore.Models
{
    /// 令牌类
    /// </summary>
    public class TokenModel
    {
        /// <summary>
        /// 
        /// </summary>
        public TokenModel()
        {
            this.Uid = "0";
        }
        /// <summary>
        /// 用户Id
        /// </summary>
        [Required(ErrorMessage = "ID不能为空")]
        public string Uid { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string Uname { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string UNickname { get; set; }
        /// <summary>
        /// 身份
        /// </summary>
        public string Sub { get; set; }
    }
}
