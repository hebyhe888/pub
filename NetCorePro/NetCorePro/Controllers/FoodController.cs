using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Netcore.IDAL;
using NetCore.DAL;
using NetCore.Models;

namespace NetCorePro.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FoodController : ControllerBase
    {
        private Isys_food_info _Ifood { get; set; }
        public FoodController(Isys_food_info ifood)
        {
            this._Ifood = ifood;
        }
        /// <summary>
        /// 保存修改菜品信息
        /// </summary>
        /// <param name="Food"></param>
        /// <returns></returns>
        [HttpPost]
#if DEBUG
        [AllowAnonymous]
#endif
        public IActionResult Save([FromBody] NetCore.Models.sys_food_info FoodInfo)
        {
            var id = "0";
            var status= _Ifood.Save(FoodInfo, out id);
            return Ok(new { status, id });
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
#if DEBUG
        [AllowAnonymous]
#endif
        public IActionResult Delete(string id)
        {
            var status = _Ifood.delete(id);
            return Ok(new { status });
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="FoodInfo"></param>
        /// <returns></returns>
        [HttpPost]
#if DEBUG
        [AllowAnonymous]
#endif
        public IActionResult GetFoodInfoList([FromBody] NetCore.Models.sys_food_info param)
        {
            var totalrecords = 0;
            var list = _Ifood.GetFoodInfoList(param, ref totalrecords);
            return Ok(new { totalrecords, list });
        }
    }
}
