using Microsoft.Extensions.Configuration.Json;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace NetCorePro.ServicesExtensions
{
    public static class AddSwaggerGenExtensions
    {
        public static IConfiguration configuration { get; set; }
        static AddSwaggerGenExtensions()
        {
            string Path = "appsettings.json";
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .Add(new JsonConfigurationSource
                {
                    Path = Path,
                    Optional = false,
                    ReloadOnChange = true
                })
                .Build();// 这样的话，可以直接读目录里的json文件，而不是 bin 文件夹下的，所以不用修改复制属性
        }
        public static void AddSwaggerGenExt(this WebApplicationBuilder builder)
        {
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Version = configuration.GetSection("SystemInfo:version").Value,
                    Title = configuration.GetSection("SystemInfo:Title").Value,
                    Description = configuration.GetSection("SystemInfo:Description").Value,
                    Contact = new OpenApiContact
                    {
                        Name = configuration.GetSection("SystemInfo:ContactName:Name").Value,
                    },
                    License = new OpenApiLicense
                    {
                        Name = configuration.GetSection("SystemInfo:License:Name").Value,
                    }
                });
                //添加安全要求
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference=new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },new List<string>()
                    }
                });
                //添加安全定义
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT授权直接在下框中输入Bearer {token}（注意两者之间是一个空格）\"",
                    Name = configuration.GetSection("AcceptHeader").Value,//X-Token
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });
                //显示注释
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), true);
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "NetCore.Models.xml"),true);
            });
        }

        public static void UseSwaggerExt(this WebApplication app)
        {
            app.UseSwagger(options =>
            {
                options.SerializeAsV2 = true;
            });
            app.UseSwaggerUI(options =>
            {
                //options.SwaggerEndpoint("/swagger/v1/swagger.json", configuration.GetSection("SystemInfo:version").Value);
                //options.RoutePrefix = string.Empty;
                options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);//DocExpansion设置为none可折叠所有方法
                options.DefaultModelExpandDepth(-1);//可不显示Models
            });
        }
    }
}
