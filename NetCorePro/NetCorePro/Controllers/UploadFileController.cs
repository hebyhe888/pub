using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Utils;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace aussco_shop.Controllers
{
    /// <summary>
    /// 文件上传
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Consumes("application/json", "multipart/form-data")]//此处为新增
    [Authorize(Policy = "admin")]
    public class UploadFileController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public UploadFileController(IHostingEnvironment hostingEnvironment)
        {
            this._hostingEnvironment = hostingEnvironment;
        }
        /// <summary>
        /// 上传单个图片
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost]
        //[AllowAnonymous]
        public async Task<IActionResult> OnPostPhotoAsync(IFormFile formFile)
        {
            var ImgBasePath = string.Empty;
            long size = formFile.Length;
            if(size/1024>500)
            {
                throw new Exception("上传图片不能超过500K");
            }
            if (formFile.Length > 0)
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                string contentRootPath = _hostingEnvironment.ContentRootPath;
                String childPath = "/" + DateTime.Now.ToString("yyyy-MM-dd") + "/";

                //var extendname = formFile.FileName.Substring(formFile.FileName.LastIndexOf('.'));
                //var newfilename = DateTime.Now.ToString("yyyyMMddHHmmssfff") + extendname;//最终文件名
                //var tempnewfilename = "temp" + newfilename;//临时文件名

                string FilePath = webRootPath + childPath;
                DirectoryInfo di = new DirectoryInfo(FilePath);
                if (!di.Exists) { di.Create(); }
                using (var stream = System.IO.File.Create(Path.Combine(FilePath, formFile.FileName )))//formFile.FileName
                {
                    await formFile.CopyToAsync(stream);
                    stream.Flush();
                }
                //// 生成缩略图
                //if (System.IO.File.Exists(FilePath + tempnewfilename))
                //{
                //    ImageHelper.GenThumbnail(FilePath + tempnewfilename, FilePath + newfilename, 1024, 1024);
                //    //删除临时文件
                //    System.IO.File.Delete(FilePath + tempnewfilename);
                //}
                ImgBasePath = childPath + formFile.FileName;//formFile.FileName
            }
            return Ok(new { size, ImgBasePath });
        }
        /// <summary>
        /// 批量上传图片
        /// </summary>
        /// <param name="files"></param>
        /// <param name="pkey"></param>
        /// <returns></returns>
        [HttpPost("BatchUploadPhotos")]
        public async Task<IActionResult> UploadPhotosAsync(IFormFileCollection files,string pkey)
        {
            long size = files.Sum(f => f.Length);
            var childPath = "/" + DateTime.Now.ToString("yyyy-MM-dd") + "/";
            var fileFolder = Path.Combine(_hostingEnvironment.WebRootPath, childPath);
            if (!Directory.Exists(fileFolder))
                Directory.CreateDirectory(fileFolder);
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine(fileFolder, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }
            return Ok(new { count = files.Count, size });
        }
        /// <summary>
        /// *****上传文件服务*****
        /// </summary>
        /// <param name="formCollection"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UploadFiles([FromForm]IFormCollection formCollection)
        {
            Dictionary<string, string> diclist = new Dictionary<string, string>();
            string webRootPath = _hostingEnvironment.WebRootPath;
            FormFileCollection filelist = (FormFileCollection)formCollection.Files;
            long size = filelist.Sum(f => f.Length);
            if (size / filelist.Count / 1024 > 2048)
            {
                throw new Exception("上传单张图片不能超过2M.");
            }
            foreach (IFormFile file in filelist)
            {
                string childPath = "/" + DateTime.Now.ToString("yyyy-MM-dd") + "/";
                string FilePath = webRootPath + childPath;
                var extendname = file.FileName.Substring(file.FileName.LastIndexOf('.'));
                var newfilename = DateTime.Now.ToString("yyyyMMddHHmmssfff") + extendname;//最终文件名
                var tempnewfilename = "temp" + newfilename;//临时文件名
                DirectoryInfo di = new DirectoryInfo(FilePath);
                if (!di.Exists)
                {
                    di.Create();
                }
                using (FileStream fs = System.IO.File.Create(FilePath + tempnewfilename))
                {

                    diclist.Add(file.FileName, childPath + newfilename);
                    // 复制文件
                    await file.CopyToAsync(fs);
                    // 清空缓冲区数据
                    fs.Flush();
                }
                // 生成缩略图
                if (System.IO.File.Exists(FilePath + tempnewfilename))
                {
                    ImageHelper.GenThumbnail(FilePath + tempnewfilename, FilePath + newfilename, 1024, 1024);
                    //删除临时文件
                    System.IO.File.Delete(FilePath + tempnewfilename);
                }
            }
            return Ok(new { count = filelist.Count, size, diclist });
        }
    }
}