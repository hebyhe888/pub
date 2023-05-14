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
//�������ú��Զ���Ч
builder.WebHost.UseConfiguration(
    new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsetttings.json", optional: true)
    .Build());

// Add services to the container.
var basepath = AppContext.BaseDirectory;
var configuration = builder.Configuration;
//����������
builder.Services.AddCors(options =>
{
    options.AddPolicy("*", builder =>
    {
        //�ͻ��˲�Я��cookieʱ����������
        builder.AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials();
    });
});
//���������ķ���
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
    //***��֧��ʵ����ǿ��֤�Ƿ�ȱ�ٻ����***
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
}).AddJsonOptions(options =>
{
    //�յ��ֶ��Ƿ񷵻�
    options.JsonSerializerOptions.IgnoreNullValues = false;
    //***����ĵ�����������˵���в�������ĸ��Сд������***
    options.JsonSerializerOptions.PropertyNamingPolicy = null; //new LowercasePolicy(); //null;
});

builder.Services.AddMvc(options =>
{
    options.Filters.Add(typeof(ActionResultMiddleware));
    options.Filters.Add(typeof(ExceptionResultMidleware));
    options.Filters.Add(typeof(APIFilter));
}).AddNewtonsoftJson(options => {
    //������������������ĸ��Сд������
    //����ѭ������
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    //��ʹ���շ���ʽ��key
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    //����ʱ���ʽ
    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
});

builder.Services.AddControllers();
builder.Logging.AddLog4Net("log4net.config");//��Ҫ�����ļ�
//����Swagger
builder.AddSwaggerGenExt();

#region ��Ȩ���Ȩ
//�����ȨPolicy�ǻ��ڽ�ɫ�ģ���������Ϊuser & admin������Ҫ�����userType & admintype��ɫ
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("user", policy => policy.RequireClaim("userType").Build());
    options.AddPolicy("admin", policy => policy.RequireClaim("adminType").Build());
});
//�����֤���� JWT Token��֤
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
// ��������ܵ�(��ʱ����״̬�ſ������汾����)
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwaggerExt();
}

app.UseHttpsRedirection();

app.UseJwtTokenAuth();

app.UseAuthorization();

app.MapControllers();

app.Run();
