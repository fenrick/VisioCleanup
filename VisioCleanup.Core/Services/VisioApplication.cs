// -----------------------------------------------------------------------
// <copyright file="VisioApplication.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    using Microsoft.Extensions.Logging;
    using Microsoft.Office.Interop.Visio;

    using VisioCleanup.Core.Contracts;

    using Marshal = VisioCleanup.Core.Marshal;

    /// <inheritdoc />
    public class VisioApplication : IVisioApplication
    {
        private readonly ILogger<VisioApplication> logger;

        private Application? visioApplication;

        /// <summary>
        /// Initialises a new instance of the <see cref="VisioApplication"/> class.
        /// </summary>
        /// <param name="logger">Logging instance.</param>
        public VisioApplication(ILogger<VisioApplication> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public void Close()
        {
            this.logger.LogDebug("Releasing visio application.");
            Marshal.ReleaseObject(this.visioApplication);
            this.visioApplication = null;
        }

        /// <inheritdoc />
        public void Open()
        {
            try
            {
                this.logger.LogDebug("Opening connection to visio.");
                this.visioApplication = Marshal.GetActiveObject("Visio.Application") as Application ?? throw new InvalidOperationException();
            }
            catch (COMException)
            {
                this.logger.LogDebug("Visio not running, time to open it.");
                this.visioApplication = new Application();
            }
        }

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">System not initialised.</exception>
        /// <exception cref="T:System.NullReferenceException">Visio object is <see langword="null" />.</exception>
        public int[] Selection()
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            Window? activeWindow = null;
            Selection? selection = null;
            try
            {
                var listShapeIds = new List<int>();
                activeWindow = this.visioApplication.ActiveWindow;
                if (activeWindow is null)
                {
                    return listShapeIds.ToArray();
                }

                selection = activeWindow.Selection;

                selection.GetIDs(out var ids);

                if (ids is null)
                {
                    return listShapeIds.ToArray();
                }

                listShapeIds.AddRange(ids.Cast<int>());

                return listShapeIds.ToArray();
            }
            finally
            {
                Marshal.ReleaseObject(activeWindow);
                Marshal.ReleaseObject(selection);
            }
        }
    }
}