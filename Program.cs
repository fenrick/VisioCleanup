// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using Serilog;
    using Serilog.Core;

    internal class Program
    {
        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args).RunConsoleAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
               .ConfigureLogging((hostingContext, logging) =>
            {
                logging.ClearProviders();

                var loggerConfiguration = new LoggerConfiguration();
                loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);

                Logger logger = loggerConfiguration.CreateLogger();

                logging.AddSerilog(logger, true);
            }).ConfigureServices((hostingContext, services) => { });
    }
}
