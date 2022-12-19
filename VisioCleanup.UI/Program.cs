// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

[assembly: CLSCompliant(true)]

namespace VisioCleanup.UI;

using System.Runtime.InteropServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using VisioCleanup.Core;
using VisioCleanup.UI.Forms;

using WindowsFormsLifetime;

/// <summary>Main execution point.</summary>
[Guid("E259A812-31F7-4456-BD56-EEDA53E99D7E")]
public static class Program
{
    /// <summary>The main entry point for the application.</summary>
    /// <param name="args">Arguments.</param>
    [STAThread]
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    /// <summary>Configure services.</summary>
    /// <param name="hostBuilderContext">The host builder context.</param>
    /// <param name="serviceCollection">The service collection.</param>
    private static void ConfigureApplicationServices(HostBuilderContext hostBuilderContext, IServiceCollection serviceCollection) =>
        serviceCollection.AddVisioCleanupCore(hostBuilderContext.Configuration);

    /// <summary>Configure Serilog.</summary>
    /// <param name="hostBuilderContext">The host builder context.</param>
    /// <param name="loggerConfiguration">The logger configuration.</param>
    private static void ConfigureSerilog(HostBuilderContext hostBuilderContext, LoggerConfiguration loggerConfiguration) =>
        loggerConfiguration.ReadFrom.Configuration(hostBuilderContext.Configuration);

    /// <summary>The create host builder.</summary>
    /// <param name="args">The args.</param>
    /// <returns>The <see cref="IHostBuilder"/>.</returns>
    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var hostBuilder = Host.CreateDefaultBuilder(args);
        hostBuilder.UseSerilog(ConfigureSerilog);
        hostBuilder.ConfigureServices(ConfigureApplicationServices);
        hostBuilder.UseWindowsFormsLifetime<MainForm>(
            options =>
                {
                    options.HighDpiMode = HighDpiMode.PerMonitorV2;
                    options.EnableVisualStyles = true;
                    options.CompatibleTextRenderingDefault = false;
                    options.SuppressStatusMessages = false;
                    options.EnableConsoleShutdown = true;
                });
        return hostBuilder;
    }
}
