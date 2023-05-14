using NetCore.Models;
using Newtonsoft.Json;
using System.Security.Claims;
using Utils;

namespace NetCorePro.Midleware
{
    public class JwtTokenAuth
    {
        private IConfiguration _Configuration;
        /// <summary>
        /// 请求头，令牌验证头
        /// </summary>
        private string _acceptheader { get; set; }
        // 中间件一定要有一个next，将管道可以正常的走下去
        private readonly RequestDelegate _next;
        public JwtTokenAuth(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _Configuration = configuration;
            _acceptheader = _Configuration.GetSection("AcceptHeader").Value;
        }
        /// <summary>
        /// 验证授权
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public Task Invoke(HttpContext httpContext)
        {
            var headers = httpContext.Request.Headers;
            //检测是否包含'X-Token'请求头，如果不包含返回context进行下一个中间件，用于访问不需要认证的API
            if (!headers.ContainsKey(_acceptheader))//X-Token
            {
                return _next(httpContext);
            }
            var tokenStr = headers[_acceptheader];
            try
            {
                string jwtStr = tokenStr.ToString().Substring("Bearer ".Length).Trim();
                //验证缓存中是否存在该jwt字符串
                if (!BussinessMemoryCache.Exists(jwtStr))
                {
                    var code = 50000; var data = "Login timeout!";
                    httpContext.Response.ContentType = "text/plain; charset=utf-8";
                    return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new
                    {
                        code,
                        data,
                    }));
                    //return httpContext.Response.WriteAsync("非法请求");
                }
                TokenModel tm = ((TokenModel)BussinessMemoryCache.Get(jwtStr));
                //提取tokenModel中的Sub属性进行authorize认证
                List<Claim> lc = new List<Claim>();
                Claim c = new Claim(tm.Sub + "Type", tm.Sub);
                lc.Add(c);
                //*****如果sub为"admin"则赋予所有策略policy*****
                if (tm.Sub.Equals("admin"))
                {
                    lc.Add(new Claim("userType", "user"));
                }
                ClaimsIdentity identity = new ClaimsIdentity(lc);
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                httpContext.User = principal;
                return _next(httpContext);
            }
            catch (Exception)
            {
                httpContext.Response.ContentType = "text/plain; charset=utf-8";
                return httpContext.Response.WriteAsync("token验证异常");
            }
        }
    }
}
