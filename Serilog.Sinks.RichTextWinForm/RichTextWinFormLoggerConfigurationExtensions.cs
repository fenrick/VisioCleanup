﻿// -----------------------------------------------------------------------
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
        /// <exception cref="System.ArgumentNullException">When <paramref name="loggerSinkConfiguration" /> is null.</exception>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration RichTextWinForm(this LoggerSinkConfiguration loggerSinkConfiguration) => RichTextWinForm(loggerSinkConfiguration, DefaultOutputTemplate);

        /// <summary>Writes log events to a <see cref="System.Windows.Forms.RichTextBox" /> .</summary>
        /// <param name="loggerSinkConfiguration">Logger sink configuration.</param>
        /// <param name="outputTemplate">
        /// A message template describing the format used to write to the sink. The default is
        /// "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}".
        /// </param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="loggerSinkConfiguration" /> is null.</exception>
        /// <exception cref="System.ArgumentNullException">When <paramref name="outputTemplate" /> is null.</exception>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration RichTextWinForm(this LoggerSinkConfiguration loggerSinkConfiguration, string outputTemplate) =>
            RichTextWinForm(loggerSinkConfiguration, outputTemplate, RichTextThemes.Default);

        /// <summary>Writes log events to a <see cref="System.Windows.Forms.RichTextBox" /> .</summary>
        /// <param name="loggerSinkConfiguration">Logger sink configuration.</param>
        /// <param name="outputTemplate">
        /// A message template describing the format used to write to the sink. The default is
        /// "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}".
        /// </param>
        /// <param name="theme">The richTextTheme to apply to the styled output.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="loggerSinkConfiguration" /> is null.</exception>
        /// <exception cref="System.ArgumentNullException">When <paramref name="outputTemplate" /> is null.</exception>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration RichTextWinForm(this LoggerSinkConfiguration loggerSinkConfiguration, string outputTemplate, RichTextTheme theme) =>
            RichTextWinForm(loggerSinkConfiguration, outputTemplate, theme, LevelAlias.Minimum);

        /// <summary>Writes log events to a <see cref="System.Windows.Forms.RichTextBox" /> .</summary>
        /// <param name="loggerSinkConfiguration">Logger sink configuration.</param>
        /// <param name="outputTemplate">
        /// A message template describing the format used to write to the sink. The default is
        /// "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}".
        /// </param>
        /// <param name="restrictedToMinimumLevel">The minimum level for events passed through the sink.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="loggerSinkConfiguration" /> is null.</exception>
        /// <exception cref="System.ArgumentNullException">When <paramref name="outputTemplate" /> is null.</exception>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration RichTextWinForm(this LoggerSinkConfiguration loggerSinkConfiguration, string outputTemplate, LogEventLevel restrictedToMinimumLevel) =>
            RichTextWinForm(loggerSinkConfiguration, outputTemplate, RichTextThemes.Default, restrictedToMinimumLevel);

        /// <summary>Writes log events to a <see cref="System.Windows.Forms.RichTextBox" /> .</summary>
        /// <param name="loggerSinkConfiguration">Logger sink configuration.</param>
        /// <param name="outputTemplate">
        /// A message template describing the format used to write to the sink. The default is
        /// "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}".
        /// </param>
        /// <param name="theme">The richTextTheme to apply to the styled output.</param>
        /// <param name="restrictedToMinimumLevel">The minimum level for events passed through the sink.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="loggerSinkConfiguration" /> is null.</exception>
        /// <exception cref="System.ArgumentNullException">When <paramref name="outputTemplate" /> is null.</exception>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration RichTextWinForm(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string outputTemplate,
            RichTextTheme theme,
            LogEventLevel restrictedToMinimumLevel) =>
            RichTextWinForm(loggerSinkConfiguration, outputTemplate, theme, restrictedToMinimumLevel, null, null);

        /// <summary>Writes log events to a <see cref="System.Windows.Forms.RichTextBox" /> .</summary>
        /// <param name="loggerSinkConfiguration">Logger sink configuration.</param>
        /// <param name="outputTemplate">
        /// A message template describing the format used to write to the sink. The default is
        /// "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}".
        /// </param>
        /// <param name="theme">The richTextTheme to apply to the styled output.</param>
        /// <param name="restrictedToMinimumLevel">
        /// The minimum level for events passed through the sink. Ignored when
        /// <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="levelSwitch">A <see langword="switch" /> allowing the pass-through minimum level to be changed at runtime.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="loggerSinkConfiguration" /> is null.</exception>
        /// <exception cref="System.ArgumentNullException">When <paramref name="outputTemplate" /> is null.</exception>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration RichTextWinForm(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string outputTemplate,
            RichTextTheme theme,
            LogEventLevel restrictedToMinimumLevel,
            IFormatProvider? formatProvider,
            LoggingLevelSwitch? levelSwitch)
        {
            OutputTemplateRenderer formatter = new(theme, outputTemplate, formatProvider);

            return loggerSinkConfiguration.Sink(new RichTextWinFormSink(formatter), restrictedToMinimumLevel, levelSwitch);
        }
    }
}
