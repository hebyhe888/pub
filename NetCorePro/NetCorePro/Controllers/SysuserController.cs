using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Netcore.IDAL;
using NetCore.Models;
using NetCorePro.Midleware;

namespace NetCorePro.Controllers
{
    /// <summary>
    /// 用户信息管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Policy ="user")]
    public class SysuserController : ControllerBase
    {
        private Isys_user _Isys_User { get; set; }
        public SysuserController(Isys_user isys_User)
        {
            this._Isys_User= isys_User;
        }
        /// <summary>
        /// 获取单笔用户记录
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        [HttpGet]
#if DEBUG
        //[AllowAnonymous]
#endif
        public IActionResult GetSysUser(string id)
        {
            return Ok(_Isys_User.Get(id));
        }
        /// <summary>
        /// 保存用户数据
        /// </summary>
        /// <param name="tUSER"></param>
        /// <returns></returns>
        [HttpPost]
#if DEBUG
        //[AllowAnonymous]
#endif
        [AllowAnonymous]
        public IActionResult Save([FromBody]NetCore.Models.sys_user tUSER)
        {
            return Ok(_Isys_User.Save(tUSER));
        }
        /// <summary>
        /// 删除单笔用户记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
#if DEBUG
        [AllowAnonymous]
#endif
        public IActionResult delete(string id)
        {
            return Ok(_Isys_User.delete(id));
        }
        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="model"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        [HttpPut]
        public IActionResult GetSysUsers([FromBody]NetCore.Models.sys_user model, int pageindex, int pagesize)
        {
            var totalrecords = 0;
            var list = _Isys_User.GetTUSERs(model, pageindex, pagesize,out totalrecords);
            return Ok(new { totalrecords,list });
        }
        /// <summary>
        /// 用户授权
        /// </summary>
        /// <param name="usercode"></param>
        /// <param name="userpass"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult CreateJwt(string usercode, string userpass)
        {
            var tuser = _Isys_User.GetSysUser(usercode);
            TokenModel tm = default(TokenModel);
            var jwt = string.Empty;
            if (tuser != null)
            {
                if (Utils.DESEncrypt.Encrypt(userpass).Equals(tuser.password))
                {
                    tm = new TokenModel()
                    {
                        Uid = new Guid().ToString(),
                        Uname = tuser.name,
                        Sub = tuser.userrole.Equals("0") ? "admin" : "user"
                    };
                }
                jwt = JwtTokenIssue.IssueJWT(tm, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
            }
            return Ok(jwt);
        }
    }
}
