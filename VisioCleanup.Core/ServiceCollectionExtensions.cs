// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core
{
    using System;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models.Config;
    using VisioCleanup.Core.Resources;
    using VisioCleanup.Core.Services;

    /// <summary>The service collection extensions.</summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>The add visio cleanup core.</summary>
        /// <param name="serviceCollection">The <paramref name="serviceCollection" /> .</param>
        /// <param name="configuration">The <paramref name="configuration" /> .</param>
        /// <returns>The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" /> .</returns>
        public static IServiceCollection AddVisioCleanupCore(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            serviceCollection.Configure<AppConfig>(configuration.GetSection(en_AU.ServiceCollectionExtensions_AddVisioCleanupCore_VisioCleanup_Core));
            serviceCollection.AddSingleton<IVisioApplication, VisioApplication>();
            serviceCollection.AddSingleton<IExcelDataSource, ExcelDataSource>();
            serviceCollection.AddSingleton<ISqlServerDataSource, SqlServerDataSource>();
            serviceCollection.AddSingleton<IVisioService, VisioService>();
            serviceCollection.AddSingleton<IExcelService, ExcelService>();
            serviceCollection.AddSingleton<IDatabaseService, DatabaseService>();

            return serviceCollection;
        }
    }
}
