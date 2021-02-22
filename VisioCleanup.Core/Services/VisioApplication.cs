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
    using Microsoft.Extensions.Options;
    using Microsoft.Office.Interop.Visio;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;
    using VisioCleanup.Core.Models.Config;

    using Marshal = VisioCleanup.Core.Marshal;

    /// <inheritdoc />
    public class VisioApplication : IVisioApplication
    {
        private readonly AppConfig appConfig;

        private readonly ILogger<VisioApplication> logger;

        private Application? visioApplication;

        /// <summary>
        /// Initialises a new instance of the <see cref="VisioApplication"/> class.
        /// </summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="options">Application configuration.</param>
        public VisioApplication(ILogger<VisioApplication> logger, IOptions<AppConfig> options)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.appConfig = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        /// <exception cref="T:System.NullReferenceException">Shape is <see langword="null" />.</exception>
        public Corners CalculateCorners(int visioId)
        {
            Shape? shape = null;
            try
            {
                var corners = default(Corners);

                shape = this.GetShape(visioId);

                var pinX = shape.Cells[this.appConfig.PinXField].Result[this.appConfig.Units];
                var locPinX = shape.Cells[this.appConfig.LocPinXField].Result[this.appConfig.Units];
                var pinY = shape.Cells[this.appConfig.PinYField].Result[this.appConfig.Units];
                var locPinY = shape.Cells[this.appConfig.LocPinYField].Result[this.appConfig.Units];
                var width = shape.Cells[this.appConfig.WidthField].Result[this.appConfig.Units];
                var height = shape.Cells[this.appConfig.HeightField].Result[this.appConfig.Units];

                corners.Left = Corners.ConvertMeasurement(pinX - locPinX);
                corners.Base = Corners.ConvertMeasurement(pinY - locPinY);
                corners.Right = corners.Left + Corners.ConvertMeasurement(width);
                corners.Top = corners.Base + Corners.ConvertMeasurement(height);

                return corners;
            }
            finally
            {
                Marshal.ReleaseObject(shape);
            }
        }

        /// <inheritdoc />
        public void Close()
        {
            this.logger.LogDebug("Releasing visio application.");
            Marshal.ReleaseObject(this.visioApplication);
            this.visioApplication = null;
        }

        /// <inheritdoc />
        /// <exception cref="T:System.AggregateException">
        ///     The exception that contains all the individual exceptions thrown on all
        ///     threads.
        /// </exception>
        /// <exception cref="T:System.NullReferenceException">Shape is <see langword="null" />.</exception>
        public IEnumerable<int> GetChildren(int visioId)
        {
            Shape? parentShape = null;
            Selection? selection = null;
            var childrenIds = new List<int>();
            try
            {
                parentShape = this.GetShape(visioId);

                var relation = (short)VisSpatialRelationCodes.visSpatialContain;
                var flags = (short)VisSpatialRelationFlags.visSpatialBackToFront;
                selection = parentShape.SpatialNeighbors[relation, 0, flags];

                this.logger.LogDebug("Potential child shapes found: {CountOfSelection}", selection.Count);

                // selection.GetIDs(out var selectionIDs);
                if (selection.Count == 0)
                {
                    return childrenIds;
                }

                foreach (Shape shape in selection)
                {
                    // check that immediate parent is the supplied shape.
                    relation = (short)VisSpatialRelationCodes.visSpatialContainedIn;
                    flags = (short)VisSpatialRelationFlags.visSpatialFrontToBack;
                    Selection parentSelection = shape.SpatialNeighbors[relation, 0, flags];
                    if (parentSelection.Count <= 0)
                    {
                        continue;
                    }

                    var primaryItemVisioId = parentSelection.PrimaryItem.ID;

                    if (visioId.Equals(primaryItemVisioId))
                    {
                        childrenIds.Add(shape.ID);
                    }
                }

                this.logger.LogDebug("Final child shapes found: {CountOfVisioIDs}", childrenIds.Count);
            }
            catch (COMException e)
            {
                this.logger.LogError("COM Exception: {Exception}", e);
            }
            finally
            {
                Marshal.ReleaseObject(parentShape);
                Marshal.ReleaseObject(selection);
            }

            return childrenIds;
        }

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">System not initialised.</exception>
        /// <exception cref="T:System.NullReferenceException">page sheet is <see langword="null" />.</exception>
        public Corners GetPageSize(int headerHeight, int sidePanelWidth)
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            Shape? pageSheet = null;
            try
            {
                var corners = default(Corners);

                pageSheet = this.visioApplication.ActivePage.PageSheet;

                var pageWidth = pageSheet.Cells["PageWidth"].Result[this.appConfig.Units];
                var pageHeight = pageSheet.Cells["PageHeight"].Result[this.appConfig.Units];
                var pageLeftMargin = pageSheet.Cells["PageLeftMargin"].Result[this.appConfig.Units];
                var pageTopMargin = pageSheet.Cells["PageTopMargin"].Result[this.appConfig.Units];
                var pageRightMargin = pageSheet.Cells["PageRightMargin"].Result[this.appConfig.Units];
                var pageBottomMargin = pageSheet.Cells["PageBottomMargin"].Result[this.appConfig.Units];

                corners.Left = Corners.ConvertMeasurement(pageLeftMargin);
                corners.Base = Corners.ConvertMeasurement(pageBottomMargin);

                var horizontalMargins = pageLeftMargin + pageRightMargin;
                corners.Right = Corners.ConvertMeasurement(pageWidth - horizontalMargins) - sidePanelWidth;

                var verticalMargins = pageTopMargin + pageBottomMargin;
                corners.Top = Corners.ConvertMeasurement(pageHeight - verticalMargins) - headerHeight;

                return corners;
            }
            finally
            {
                Marshal.ReleaseObject(pageSheet);
            }
        }

        /// <inheritdoc />
        /// <exception cref="T:System.NullReferenceException">Shape is <see langword="null" />.</exception>
        public string GetShapeText(int visioId)
        {
            Shape? shape = null;
            try
            {
                shape = this.GetShape(visioId);
                return shape.Text;
            }
            finally
            {
                Marshal.ReleaseObject(shape);
            }
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
                var listVisioIds = new List<int>();
                activeWindow = this.visioApplication.ActiveWindow;
                if (activeWindow is null)
                {
                    return listVisioIds.ToArray();
                }

                selection = activeWindow.Selection;

                selection.GetIDs(out var ids);

                if (ids is null)
                {
                    return listVisioIds.ToArray();
                }

                listVisioIds.AddRange(ids.Cast<int>());

                return listVisioIds.ToArray();
            }
            finally
            {
                Marshal.ReleaseObject(activeWindow);
                Marshal.ReleaseObject(selection);
            }
        }

        private Shape GetShape(int visioId)
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            Page? activePage = null;
            try
            {
                activePage = this.visioApplication.ActivePage;
                var shape = activePage.Shapes.ItemFromID[visioId];
                return shape;
            }
            finally
            {
                Marshal.ReleaseObject(activePage);
            }
        }
    }
}