﻿// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models.Config;
    using VisioCleanup.Core.Services;

    /// <summary>The service collection extensions.</summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>The add visio cleanup core.</summary>
        /// <param name="serviceCollection">The <paramref name="serviceCollection" />.</param>
        /// <param name="configuration">The <paramref name="configuration" />.</param>
        /// <returns>The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" /> .</returns>
        public static IServiceCollection AddVisioCleanupCore(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            // serviceCollection.AddHostedService<VisioCleanupService>();
            return serviceCollection.Configure<AppConfig>(configuration.GetSection("VisioCleanup:Core")).AddSingleton<IVisioService, VisioService>()
                .AddSingleton<IExcelService, ExcelService>().AddSingleton<IVisioApplication, VisioApplication>();
        }
    }
}