// -----------------------------------------------------------------------
// <copyright file="IVisioHandler.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Services
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Office.Interop.Visio;

    /// <summary>
    ///     Manipulate visio objects.
    /// </summary>
    internal interface IVisioHandler
    {
        /// <summary>
        ///     Close visio session and shutdown.
        /// </summary>
        void Close();

        /// <summary>
        ///     Open visio session.
        /// </summary>
        void Open();
    }

    /// <inheritdoc />
    internal class VisioHandlerService : IVisioHandler
    {
        private readonly ILogger<VisioHandlerService> logger;

        private readonly VisioCleanupSettings settings;

        private Application visioApplication;

        /// <summary>
        ///     Initialises a new instance of the <see cref="VisioHandlerService" /> class.
        /// </summary>
        /// <param name="settings">Settings object.</param>
        /// <param name="logger">Logging object.</param>
        public VisioHandlerService(IOptions<VisioCleanupSettings> settings, ILogger<VisioHandlerService> logger)
        {
            this.settings = settings.Value;
            this.logger = logger;

            this.logger.LogDebug("Starting Visio Handler");
        }

        /// <inheritdoc />
        public void Close()
        {
        }

        /// <inheritdoc />
        public void Open()
        {
            this.visioApplication = Marshal.GetActiveObject("Visio.Application") as Application;

            var sel = this.visioApplication.ActiveWindow.Selection;

            this.logger.LogDebug($"Document: {this.visioApplication.ActiveDocument.Name}");
            this.logger.LogDebug($"Page: {this.visioApplication.ActiveDocument.Pages[1].Name}");
            this.logger.LogDebug($"Selection count: {sel.Count}");
        }
    }
}