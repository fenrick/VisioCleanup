// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

using System;

[assembly: CLSCompliant(true)]

namespace VisioCleanup
{
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using Serilog;

    using VisioCleanup.Services;

    /// <summary>
    ///     Main program.
    /// </summary>
    internal class Program
    {
        /// <summary>Main entry point.</summary>
        /// <param name="args">command line arguments.</param>
        /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args).RunConsoleAsync();
        }

        /// <summary>The create host builder.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="IHostBuilder" />.</returns>
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureLogging((hostingContext, logging) => { SerilogConfiguration(logging, hostingContext); }).ConfigureServices(
                (hostingContext, services) =>
                    {
                        services.Configure<VisioCleanupSettings>(hostingContext.Configuration.GetSection("VisioCleanupSettings"));
                        services.AddSingleton<IVisioHandler, VisioHandlerService>();
                        services.AddSingleton<IExcelHandler, ExcelHandlerService>();
                        services.AddHostedService<VisioCleanupService>();
                    });
        }

        private static void SerilogConfiguration(ILoggingBuilder logging, HostBuilderContext hostingContext)
        {
            logging.ClearProviders();

            var loggerConfiguration = new LoggerConfiguration();
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);

            var logger = loggerConfiguration.CreateLogger();

            logging.AddSerilog(logger, true);
        }
    }
}