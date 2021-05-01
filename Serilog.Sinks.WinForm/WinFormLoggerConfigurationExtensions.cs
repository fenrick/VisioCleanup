// -----------------------------------------------------------------------
// <copyright file="WinFormLoggerConfigurationExtensions.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog
{
    using System;

    using Serilog.Configuration;
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Sinks.WinForm;
    using Serilog.Sinks.WinForm.Output;

    /// <summary>Extends <see cref="LoggerConfiguration" /> with methods for configuring Rich Text Windows Forms logging.</summary>
    public static class WinFormLoggerConfigurationExtensions
    {
        private const string DefaultOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

        /// <summary>Writes log events to a <see cref="System.Windows.Forms.TextBox" /> .</summary>
        /// <param name="loggerSinkConfiguration">Logger sink configuration.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="loggerSinkConfiguration" /> is null.</exception>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WinForm(
            this LoggerSinkConfiguration loggerSinkConfiguration) =>
            WinForm(loggerSinkConfiguration, DefaultOutputTemplate);

        /// <summary>Writes log events to a <see cref="System.Windows.Forms.TextBox" /> .</summary>
        /// <param name="loggerSinkConfiguration">Logger sink configuration.</param>
        /// <param name="outputTemplate">
        /// A message template describing the format used to write to the sink. The default is
        /// "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}".
        /// </param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="loggerSinkConfiguration" /> is null.</exception>
        /// <exception cref="System.ArgumentNullException">When <paramref name="outputTemplate" /> is null.</exception>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WinForm(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string outputTemplate) =>
            WinForm(loggerSinkConfiguration, outputTemplate, LevelAlias.Minimum, null, null);

        /// <summary>Writes log events to a <see cref="System.Windows.Forms.TextBox" /> .</summary>
        /// <param name="loggerSinkConfiguration">Logger sink configuration.</param>
        /// <param name="outputTemplate">
        /// A message template describing the format used to write to the sink. The default is
        /// "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}".
        /// </param>
        /// <param name="restrictedToMinimumLevel">
        /// The minimum level for events passed through the sink. Ignored when
        /// <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="levelSwitch">A <see langword="switch" /> allowing the pass-through minimum level to be changed at runtime.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="loggerSinkConfiguration" /> is null.</exception>
        /// <exception cref="System.ArgumentNullException">When <paramref name="outputTemplate" /> is null.</exception>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WinForm(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string outputTemplate,
            LogEventLevel restrictedToMinimumLevel,
            IFormatProvider? formatProvider,
            LoggingLevelSwitch? levelSwitch)
        {
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration));
            }

            if (outputTemplate is null)
            {
                throw new ArgumentNullException(nameof(outputTemplate));
            }

            OutputTemplateRenderer formatter = new(outputTemplate, formatProvider);

            return loggerSinkConfiguration.Sink(new WinFormSink(formatter), restrictedToMinimumLevel, levelSwitch);
        }
    }
}
