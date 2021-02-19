// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.UI
{
    using System;
    using System.Runtime.InteropServices;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using Serilog;

    using VisioCleanup.Core;
    using VisioCleanup.UI.Forms;

    using WindowsFormsGenericHost;

    /// <summary>Main execution point.</summary>
    [Guid("E259A812-31F7-4456-BD56-EEDA53E99D7E")]
    internal static class Program
    {
        /// <summary>The main entry point for the application.</summary>
        /// <param name="args">Arguments.</param>
        [STAThread]
        public static void Main(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args);
            hostBuilder.UseSerilog(ConfigureSerilog);
            hostBuilder.ConfigureServices(ConfigureServices);
            hostBuilder.UseWindowsFormsLifetime<MainForm>();

            using var host = hostBuilder.Build();
            host.Run();
        }

        /// <summary>Configure Serilog.</summary>
        /// <param name="hostBuilderContext">The host builder context.</param>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        private static void ConfigureSerilog(HostBuilderContext hostBuilderContext, LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration.ReadFrom.Configuration(hostBuilderContext.Configuration);
            loggerConfiguration.WriteTo.RichTextWinForm();
        }

        /// <summary>Configure services.</summary>
        /// <param name="hostBuilderContext">The host builder context.</param>
        /// <param name="serviceCollection">The service collection.</param>
        private static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection serviceCollection)
        {
            serviceCollection.AddVisioCleanupCore(hostBuilderContext.Configuration);
            serviceCollection.AddForms();
        }
    }
}