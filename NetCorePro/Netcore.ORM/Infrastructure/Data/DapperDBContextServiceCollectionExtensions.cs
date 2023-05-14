/*命名空间: IPolarDBDataAccess
*
* 功 能： N/A
* 类 名： Class1
* Author   : HebyHe
* Created  : 21 - 09 - 2022 13:40:21
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
*  Copyright (c) 2022 bluestar Corporation.All rights reserved.
*┌──────────────────────────────────┐
*│　此技术信息为本公司机密信息，未经本公司书面同意禁止向第三方披露．　│
*│　版权所有：蓝星科技有限公司                                        │
*└──────────────────────────────────┘
*/
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBUtility.Dapper.Infrastructure.Data
{
    public static class DapperDBContextServiceCollectionExtensions
    {
        public static IServiceCollection AddDapperDBContext<T>(this IServiceCollection services,
            Action<DapperDBContextOptions> setupAction) where T : DapperDBContext
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddOptions();
            services.Configure(setupAction);
            services.AddScoped<DapperDBContext, T>();
            services.AddScoped<IUnitOfWorkFactory, DapperUnitOfWorkFactory>();
            return services;
        }
    }
}
