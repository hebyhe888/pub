/*命名空间: IPolarDBDataAccess
*
* 功 能： N/A
* 类 名： Class1
* Author   : HebyHe
* Created  : 21 - 09 - 2022 13:40:21
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
*  Copyright (c) 2022 bluestar Corporation.All rights reserved.
*┌──────────────────────────────────┐
*│　此技术信息为本公司机密信息，未经本公司书面同意禁止向第三方披露．　│
*│　版权所有：xxxxx                                        │
*└──────────────────────────────────┘
*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NetCore.Models;
using Newtonsoft.Json;

namespace NetCorePro.CustomFilter
{
    public class ActionResultMiddleware : ActionFilterAttribute
    {
        /// <summary>
        /// 控制器中加入了该属性的方法中代码执行之前该方法(可以用于权限效验)
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var objectResult = context.Result as ObjectResult;
            if (objectResult != null && objectResult.Value is BaseResultModel)
            {
                BaseResultModel model = (BaseResultModel)objectResult.Value;
                context.Result = new OkObjectResult(new BaseResultModel(code: 20000, message: model.Message, result: model.Result));//多属性输出
            }
            else
            {
                context.Result = new OkObjectResult(new BaseResultModel(code: 20000, message: null, result: objectResult.Value));
            }
        }
        /// <summary>
        /// 控制器中加入了该属性的方法执行完成后才执行该方法
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
