using NetCorePro.ActionFilter;
using DBUtility.Dapper.Common;
using DBUtility.Dapper.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Netcore.ORM.Infrastructure.Data;
using NetCorePro.ServicesExtensions;
using ServiceStack;
using ServiceStack.Text;
using NetCorePro.CustomFilter;
using System.Reflection;
using System.Runtime.CompilerServices;
using NetCorePro.Midleware;
using Newtonsoft.Json.Serialization;
using Google.Protobuf.WellKnownTypes;
using NetCore.Models;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
//改了配置后自动生效
builder.WebHost.UseConfiguration(
    new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsetttings.json", optional: true)
    .Build());

// Add services to the container.
var basepath = AppContext.BaseDirectory;
var configuration = builder.Configuration;
//允许跨域访问
builder.Services.AddCors(options =>
{
    options.AddPolicy("*", builder =>
    {
        //客户端部携带cookie时，可以配置
        builder.AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials();
    });
});
//加入上下文服务
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

//Register class for DI
//builder.Services.UseCustomServices();
builder.Services.AddAssembly(configuration.GetSection("Assembly").Value, serviceLifetime: ServiceLifetime.Transient);
builder.Services.AddConfig(builder.Configuration);


builder.Services.AddDapperDBContext<SqlDbContext>(options =>
{
    options.Configuration = Utils.AppSettingHelper._MySqlConnectionString;
});

builder.Services.AddControllers(options =>
{
    //***不支持实体类强验证是否缺少或多余***
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
}).AddJsonOptions(options =>
{
    //空的字段是否返回
    options.JsonSerializerOptions.IgnoreNullValues = false;
    //***解决文档中样例参数说明中参数首字母变小写的问题***
    options.JsonSerializerOptions.PropertyNamingPolicy = null; //new LowercasePolicy(); //null;
});

builder.Services.AddMvc(options =>
{
    options.Filters.Add(typeof(ActionResultMiddleware));
    options.Filters.Add(typeof(ExceptionResultMidleware));
    options.Filters.Add(typeof(APIFilter));
}).AddNewtonsoftJson(options => {
    //解决输入输出参数首字母变小写的问题
    //忽略循环引用
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    //不使用驼峰样式的key
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    //设置时间格式
    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
});

builder.Services.AddControllers();
builder.Logging.AddLog4Net("log4net.config");//需要配置文件
//加入Swagger
builder.AddSwaggerGenExt();

#region 授权与鉴权
//添加授权Policy是基于角色的，策略名称为user & admin，策略要求具有userType & admintype角色
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("user", policy => policy.RequireClaim("userType").Build());
    options.AddPolicy("admin", policy => policy.RequireClaim("adminType").Build());
});
//添加认证方法 JWT Token认证
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
});
#endregion
var app = builder.Build();

// Configure the HTTP request pipeline.
// 配置请求管道(暂时调试状态放开生产版本公开)
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwaggerExt();
}

app.UseHttpsRedirection();

app.UseJwtTokenAuth();

app.UseAuthorization();

app.MapControllers();

app.Run();
