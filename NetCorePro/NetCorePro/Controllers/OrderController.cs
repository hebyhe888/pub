using DBUtility.Dapper.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Netcore.IDAL;
using NetCore.Models;
using NetCorePro.Midleware;
using System.Runtime.InteropServices;

namespace NetCorePro.Controllers
{
    /// <summary>
    /// 订单
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Policy = "user")]
    public class OrderController : ControllerBase
    {
        private ISys_order_infoInfo _order_info { get; set; }
        public OrderController(ISys_order_infoInfo order_info)
        {
            _order_info = order_info;
        }
        /// <summary>
        /// 订单保存
        /// </summary>
        /// <param name="_Order_Infos"></param>
        /// <returns></returns>
        [HttpPost]
#if DEBUG
        [AllowAnonymous]
#endif
        public IActionResult Save([FromBody]List<sys_order_info> _Order_Infos)
        {
            return Ok(_order_info.Save(_Order_Infos));
        }
        /// <summary>
        /// 返回订单单笔记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
#if DEBUG
        [AllowAnonymous]
#endif
        public IActionResult  Get_Order_Info(string id)
        {
            return Ok(_order_info.Get_Order_Info(id));
        }
    }
}
