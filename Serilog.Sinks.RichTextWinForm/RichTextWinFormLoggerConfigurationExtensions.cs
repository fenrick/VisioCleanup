// -----------------------------------------------------------------------
// <copyright file="RichTextWinFormLoggerConfigurationExtensions.cs" company="Jolyon Suthers">
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
    using Serilog.Sinks.RichTextWinForm;
    using Serilog.Sinks.RichTextWinForm.Output;
    using Serilog.Sinks.RichTextWinForm.Themes;

    /// <summary>Extends <see cref="LoggerConfiguration" /> with methods for configuring Rich Text Windows Forms logging.</summary>
    public static class RichTextWinFormLoggerConfigurationExtensions
    {
        private const string DefaultOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

        /// <summary>Writes log events to a <see cref="System.Windows.Forms.RichTextBox" /> .</summary>
        /// <param name="loggerSinkConfiguration">Logger sink configuration.</param>
        /// <param name="outputTemplate">
        ///     A message template describing the format used to write to the sink. The default is
        ///     "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}".
        /// </param>
        /// <param name="theme">The richTextTheme to apply to the styled output.</param>
        /// <param name="restrictedToMinimumLevel">
        ///     The minimum level for events passed through the sink. Ignored when
        ///     <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="levelSwitch">A <see langword="switch" /> allowing the pass-through minimum level to be changed at runtime.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="loggerSinkConfiguration" /> is null.</exception>
        /// <exception cref="System.ArgumentNullException">When <paramref name="outputTemplate" /> is null.</exception>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration RichTextWinForm(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string outputTemplate = DefaultOutputTemplate,
            RichTextTheme? theme = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IFormatProvider? formatProvider = null,
            LoggingLevelSwitch? levelSwitch = null)
        {
            if (loggerSinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(loggerSinkConfiguration));
            }

            if (outputTemplate is null)
            {
                throw new ArgumentNullException(nameof(outputTemplate));
            }

            var appliedTheme = theme ?? RichTextThemes.Default;

            OutputTemplateRenderer formatter = new(appliedTheme, outputTemplate, formatProvider);

            return loggerSinkConfiguration.Sink(new RichTextWinFormSink(formatter), restrictedToMinimumLevel, levelSwitch);
        }
    }
}