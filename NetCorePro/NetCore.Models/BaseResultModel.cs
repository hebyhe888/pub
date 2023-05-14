using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore.Models
{
    /// <summary>
    /// 响应结果实体
    /// </summary>
    public class BaseResultModel
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="result"></param>
        /// <param name="returnStatus"></param>
        public BaseResultModel(int? code = null, string message = null,
            object result = null, ReturnStatus returnStatus = ReturnStatus.Success)
        {
            this.Code = code;
            this.Result = result;
            this.Message = message;
            this.ReturnStatus = returnStatus;
        }
        /// <summary>
        /// 返回代码
        /// </summary>
        public int? Code { get; set; }
        /// <summary>
        /// 返回信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 返回结果
        /// </summary>

        public object Result { get; set; }
        /// <summary>
        /// 返回状态[1:成功;0:失败;2:确认继续;3:错误]
        /// </summary>
        public ReturnStatus ReturnStatus { get; set; }
    }
    /// <summary>
    /// 返回状态
    /// </summary>
    public enum ReturnStatus
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 1,
        /// <summary>
        /// 失败
        /// </summary>
        Fail = 0,
        /// <summary>
        /// 确认继续？
        /// </summary>
        ConfirmIsContinue = 2,
        /// <summary>
        /// 错误
        /// </summary>
        Error = 3
    }
}
