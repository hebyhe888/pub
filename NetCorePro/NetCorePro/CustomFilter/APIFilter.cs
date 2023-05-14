using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System;

namespace NetCorePro.ActionFilter
{
    /// <summary>
        /// API白名单过滤器
        /// </summary>
    public class APIFilter : ActionFilterAttribute
    {
        private IConfiguration _configuration { get; set; }
        public APIFilter(IConfiguration configuration) { 
           this._configuration = configuration;
        }
        /// <summary>
        /// 控制器中加了该属性的方法中代码执行之前该方法。
        /// 所以可以用做权限校验。
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var localIp = context.HttpContext.Connection.LocalIpAddress.ToString();
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress.ToString();
            var whiteIp = _configuration.GetSection("AllowedHosts").Value.ToString();
            if (!string.IsNullOrEmpty(whiteIp))
            {
                List<string> whiteIpList = whiteIp.Split(',').ToList();
                if (!whiteIpList.Contains("*") && !whiteIpList.Contains(remoteIp) && !whiteIpList.Contains(localIp))
                {
                    context.HttpContext.Response.StatusCode = 401;
                    context.Result = new JsonResult(new { code = 401, msg = "非法IP" });
                }
            }
            base.OnActionExecuting(context);
        }
        /// <summary>
        /// 控制器中加了该属性的方法执行完成后才会来执行该方法。
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }
        /// <summary>
        /// 控制器中加了该属性的方法执行完成后才会来执行该方法。比OnActionExecuted()方法还晚执行。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            return base.OnResultExecutionAsync(context, next);
        }
    }
}
