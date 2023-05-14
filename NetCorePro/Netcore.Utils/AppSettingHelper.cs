using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class AppSettingHelper
    {
        public static IConfiguration _Configuration { get; set; }
        public static string _MySqlConnectionString { get; set; }
        static AppSettingHelper()
        {
            string path = "appsettings.json";
            _Configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .Add(new JsonConfigurationSource
                 {
                     Path = path,
                     Optional = false,
                     ReloadOnChange = true
                 })
                 .Build();// 这样的话，可以直接读目录里的json文件，而不是 bin 文件夹下的，所以不用修改复制属性
            var database = _Configuration.GetSection("dbconfig:database").Value;
            var server = _Configuration.GetSection("dbconfig:server").Value;
            var uid = _Configuration.GetSection("dbconfig:UserID").Value;
            var pwd = _Configuration.GetSection("dbconfig:Password").Value;
            var port = _Configuration.GetSection("dbconfig:port").Value;
            _MySqlConnectionString = $"server={server};database={database};uid={uid};pwd={pwd};port={port};";
        }
    }
}
