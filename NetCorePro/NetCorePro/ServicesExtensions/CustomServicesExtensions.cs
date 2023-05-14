using Microsoft.Extensions.DependencyInjection;
using Netcore.IDAL;
using NetCore.AutoRegisterDi;
using NetCore.DAL;
using NetCore.Models;
using System.Reflection;
using System.Runtime.Loader;

namespace NetCorePro.ServicesExtensions
{
    public static class CustomServicesExtensions
    {
        /// <summary>
        /// 注册自定义服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection UseCustomServices(this IServiceCollection services)
        {
            #region 手动注册
            services.AddScoped<Isys_user,NetCore.DAL.sys_user>();
            //services.AddScoped<Netcore.IDAL.ISys_order_infoInfo,NetCore.DAL.Sys_order_infoInfo>();
            #endregion
            return services;
        }
        /// <summary>
        /// 注册配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<NetCore.Models.dbconfig>(config.GetSection("dbconfig")); 
            return services;
        }

        /// <summary>
        /// 程序集自动依赖注入
        /// </summary>
        /// <param name="services">服务实例</param>
        /// <param name="assemblyName">程序集名称,不带DLL</param>
        /// <param name="serviceLifetime">依赖注入的类型</param>
        public static void AddAssembly(this IServiceCollection services, string assemblyName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            if (services == null)
                throw new ArgumentNullException($"{nameof(services)}为空");
            if (String.IsNullOrEmpty(assemblyName))
                throw new ArgumentNullException($"{nameof(assemblyName)}为空");
            Assembly assembly = GetAssemblyByName(assemblyName);
            if (assembly == null)
                throw new DllNotFoundException($"{nameof(assembly)}.dll不存在");
            //找到当前程序集的类集合
            var types = assembly.GetTypes();
            //过滤筛选（是类文件，并且不是抽象类，不是泛型）
            var list = types.Where(o => o.IsClass && !o.IsAbstract && !o.IsGenericType).ToList();
            if (list == null && !list.Any())
                return;
            //遍历获取到的类
            foreach (var type in list)
            {
                //然后获取到类对应的接口
                var interfacesList = type.GetInterfaces();
                //校验接口存在则继续
                if (interfacesList == null || !interfacesList.Any())
                    continue;
                //获取到接口（第一个）
                var inter = interfacesList.First();
                switch (serviceLifetime)
                {
                    //根据条件，选择注册依赖的方法
                    case ServiceLifetime.Scoped:
                        //将获取到的接口和类注册进去
                        services.AddScoped(inter, type);
                        break;
                    case ServiceLifetime.Singleton:
                        services.AddScoped(inter, type);
                        break;
                    case ServiceLifetime.Transient:
                        services.AddTransient(inter, type);
                        break;
                }
            }
        }
        /// <summary>
        /// 通过程序集的名称加载程序集
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static Assembly GetAssemblyByName(string assemblyName)
        {
            return AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(assemblyName));
        }
    }
}
