using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NetCore.Models;

namespace NetCorePro.CustomFilter
{
    public class ExceptionResultMidleware:ExceptionFilterAttribute
    {
        private readonly ILoggerFactory _loggerFactory;
        public ExceptionResultMidleware(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public override void OnException(ExceptionContext context)
        {
            if (!context.ExceptionHandled)
            {
                var ip = context.HttpContext.Connection.RemoteIpAddress.ToString();
                var msg = $"发生异常，请联系管理员:[路由:{context.HttpContext.Request.Path} {context.Exception.Message}]";
                #region 新的输出方式
                #endregion
                #region 旧的输出方式
                context.Result= new OkObjectResult(new BaseResultModel(code: 60000, message: context.Exception.Message));//保持与原有一致(不用改前端输出)
                #endregion
                Task.Factory.StartNew(() =>
                {
                    _loggerFactory.CreateLogger<ExceptionResultMidleware>().LogError($"主机IP:{ip},堆栈信息:{context.Exception.StackTrace},异常描述:{context.Exception.Message}");
                });
                context.ExceptionHandled = true;
            }
        }
        public override Task OnExceptionAsync(ExceptionContext context)
        {
            return base.OnExceptionAsync(context);
        }
    }
}
