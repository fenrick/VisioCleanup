﻿// -----------------------------------------------------------------------
// <copyright file="IVisioHandler.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Services
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Office.Interop.Visio;

    using VisioCleanup.Objects;

    /// <summary>
    ///     Manipulate visio objects.
    /// </summary>
    internal interface IVisioHandler
    {
        /// <summary>
        ///     Calculate the corners of a shape.
        /// </summary>
        /// <param name="shapeId">Shape ID for the shape.</param>
        /// <returns>Corners of a shape.</returns>
        Corners CalculateCorners(int shapeId);

        /// <summary>
        ///     Close visio session and shutdown.
        /// </summary>
        void Close();

        void CreateDocument();

        /// <summary>
        ///     Return an array of shapeIDs for children of the supplied shape id.
        /// </summary>
        /// <param name="shapeId">Shape ID of the parent shape.</param>
        /// <returns>array of shape ids for children.</returns>
        Task<IEnumerable<int>> GetChildrenAsync(int shapeId);

        Corners GetPageSize(double headerHeight, double sidepanelWidth);

        /// <summary>
        ///     Obtains the current shape text for a shape.
        /// </summary>
        /// <param name="shapeId">shape id.</param>
        /// <returns>shape text.</returns>
        string GetShapeText(int shapeId);

        /// <summary>
        ///     Open visio session.
        /// </summary>
        void Open();

        /// <summary>
        ///     Find shapes in visio diagram and change their location and size to match diagramShapes.
        /// </summary>
        /// <param name="diagramShape">Internal structure for modelling visio shapes.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task ReDrawShapesAsync(DiagramShape diagramShape);

        /// <summary>
        ///     Return an array of visio ids that have been selected.
        /// </summary>
        /// <returns>Array of visio ids.</returns>
        int[] Selection();

        /// <summary>
        ///     Returns primary item of the current selection.
        /// </summary>
        /// <returns>shape id of primary item.</returns>
        int SelectionPrimaryItem();
    }

    /// <inheritdoc />
    internal class VisioHandlerService : IVisioHandler
    {
        private readonly ILogger<VisioHandlerService> logger;

        private readonly VisioCleanupSettings settings;

        private Application? visioApplication;

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
        public Corners CalculateCorners(int shapeId)
        {
            Shape? shape = null;
            try
            {
                var corners = default(Corners);

                shape = this.GetShape(shapeId);

                corners.LeftSide = shape.Cells[this.settings.VisioPinXField].Result[this.settings.VisioUnits]
                                   - shape.Cells[this.settings.VisioLocPinXField].Result[this.settings.VisioUnits];
                corners.BottomSide = shape.Cells[this.settings.VisioPinYField].Result[this.settings.VisioUnits]
                                     - shape.Cells[this.settings.VisioLocPinYField].Result[this.settings.VisioUnits];

                corners.RightSide = corners.LeftSide + shape.Cells[this.settings.VisioWidthField].Result[this.settings.VisioUnits];
                corners.TopSide = corners.BottomSide + shape.Cells[this.settings.VisioHeightField].Result[this.settings.VisioUnits];

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
            Marshal.ReleaseObject(this.visioApplication);
        }

        public void CreateDocument()
        {
            this.visioApplication.Documents.Add(string.Empty);
            this.visioApplication.Documents.OpenEx("Basic.vss", (short)VisOpenSaveArgs.visOpenDocked);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<int>> GetChildrenAsync(int shapeId)
        {
            Shape? parentShape = null;
            Selection? selection = null;
            try
            {
                var shapeIDs = new List<int>();

                parentShape = this.GetShape(shapeId);

                var relation = (short)VisSpatialRelationCodes.visSpatialContain;
                var flags = (short)VisSpatialRelationFlags.visSpatialBackToFront;
                selection = parentShape.SpatialNeighbors[relation, 0, flags];

                this.logger.LogDebug("Potential child shapes found: {CountOfSelection}", selection.Count);

                selection.GetIDs(out Array selectionIDs);
                foreach (int shapeID in selectionIDs)
                {
                    await Task.Run(
                        () =>
                            {
                                // check that immediate parent is the supplied shape.
                                relation = (short)VisSpatialRelationCodes.visSpatialContainedIn;
                                flags = (short)VisSpatialRelationFlags.visSpatialFrontToBack;
                                Shape? childShape = null;
                                Selection? parentSelection = null;
                                try
                                {
                                    childShape = this.GetShape(shapeID);
                                    parentSelection = childShape.SpatialNeighbors[relation, 0, flags];
                                    if (parentSelection.Count > 0)
                                    {
                                        var primaryItemShapeId = parentSelection.PrimaryItem.ID;

                                        if (shapeId.Equals(primaryItemShapeId))
                                        {
                                            shapeIDs.Add(childShape.ID);
                                        }
                                    }
                                }
                                finally
                                {
                                    Marshal.ReleaseObject(childShape);
                                    Marshal.ReleaseObject(parentSelection);
                                }
                            }).ConfigureAwait(false);
                }

                this.logger.LogDebug("Final child shapes found: {CountOfShapeIDs}", shapeIDs.Count);

                return shapeIDs.ToArray();
            }
            finally
            {
                Marshal.ReleaseObject(parentShape);
                Marshal.ReleaseObject(selection);
            }
        }

        public Corners GetPageSize(double headerHeight, double sidepanelWidth)
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

                var pageWidth = pageSheet.Cells["PageWidth"].Result[this.settings.VisioUnits];
                var pageHeight = pageSheet.Cells["PageHeight"].Result[this.settings.VisioUnits];
                var pageLeftMargin = pageSheet.Cells["PageLeftMargin"].Result[this.settings.VisioUnits];
                var pageTopMargin = pageSheet.Cells["PageTopMargin"].Result[this.settings.VisioUnits];
                var pageRightMargin = pageSheet.Cells["PageRightMargin"].Result[this.settings.VisioUnits];
                var pageBottomMargin = pageSheet.Cells["PageBottomMargin"].Result[this.settings.VisioUnits];

                corners.LeftSide = pageLeftMargin;
                corners.BottomSide = pageBottomMargin;
                corners.RightSide = pageWidth - (pageLeftMargin + pageRightMargin + sidepanelWidth);
                corners.TopSide = pageHeight - (pageTopMargin + pageBottomMargin + headerHeight);

                return corners;
            }
            finally
            {
                Marshal.ReleaseObject(pageSheet);
            }
        }

        /// <inheritdoc />
        public string GetShapeText(int shapeId)
        {
            Shape? shape = null;
            try
            {
                shape = this.GetShape(shapeId);
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
            catch (COMException e)
            {
                this.logger.LogDebug("Visio not running, time to open it.");
                this.visioApplication = new Application();
            }
        }

        /// <inheritdoc />
        public async Task ReDrawShapesAsync(DiagramShape diagramShape)
        {
            foreach (var childDiagramShape in diagramShape.Children)
            {
                await this.ReDrawShapesAsync(childDiagramShape).ConfigureAwait(false);
            }

            await Task.Run(
                () =>
                    {
                        switch (diagramShape.ShapeType)
                        {
                            case ShapeType.NewShape:
                                {
                                    // todo: find master shape
                                    var stencil = this.visioApplication.Documents["Basic.vss"];
                                    var shape = this.visioApplication.ActivePage.Drop(stencil.Masters["Rounded Rectangle"], 1, 1);

                                    diagramShape.VisioId = shape.ID;
                                    diagramShape.ShapeType = ShapeType.Existing;
                                    shape.SendToBack();

                                    // execute standard movement code.
                                    goto case ShapeType.Existing;
                                }

                            case ShapeType.Existing:
                                {
                                    Shape? shape = null;
                                    try
                                    {
                                        shape = this.GetShape(diagramShape.VisioId);

                                        // width = right - left
                                        var currentWidth = Math.Round(
                                            shape.Cells[this.settings.VisioWidthField].Result[this.settings.VisioUnits], 3, MidpointRounding.AwayFromZero);
                                        var newWidth = Math.Round(diagramShape.Corners.RightSide - diagramShape.Corners.LeftSide, 3, MidpointRounding.AwayFromZero);
                                        if (!currentWidth.Equals(newWidth))
                                        {
                                            this.logger.LogDebug("Changing width: {Shape} - {OldValue},{NewValue}", diagramShape, currentWidth, newWidth);
                                            shape.Cells[this.settings.VisioWidthField].Result[this.settings.VisioUnits] = newWidth;
                                        }

                                        // height = top - bottom
                                        var currentHeight = Math.Round(
                                            shape.Cells[this.settings.VisioHeightField].Result[this.settings.VisioUnits], 3, MidpointRounding.AwayFromZero);
                                        var newHeight = Math.Round(diagramShape.Corners.TopSide - diagramShape.Corners.BottomSide, 3, MidpointRounding.AwayFromZero);
                                        if (!currentHeight.Equals(newHeight))
                                        {
                                            this.logger.LogDebug("Changing height: {Shape} - {OldValue},{NewValue}", diagramShape, currentHeight, newHeight);
                                            shape.Cells[this.settings.VisioHeightField].Result[this.settings.VisioUnits] = newHeight;
                                        }

                                        // pinX = left + locPinX
                                        var currentPinX = Math.Round(shape.Cells[this.settings.VisioPinXField].Result[this.settings.VisioUnits], 3, MidpointRounding.AwayFromZero);
                                        var newPinX = Math.Round(
                                            diagramShape.Corners.LeftSide + Math.Round(
                                                shape.Cells[this.settings.VisioLocPinXField].Result[this.settings.VisioUnits], 3, MidpointRounding.AwayFromZero), 3,
                                            MidpointRounding.AwayFromZero);

                                        if (!currentPinX.Equals(newPinX))
                                        {
                                            this.logger.LogDebug("Changing PinX: {Shape} - {OldValue},{NewValue}", diagramShape, currentPinX, newPinX);
                                            shape.Cells[this.settings.VisioPinXField].Result[this.settings.VisioUnits] = newPinX;
                                        }

                                        // pinY = bottom + locPinY
                                        var currentPinY = Math.Round(shape.Cells[this.settings.VisioPinYField].Result[this.settings.VisioUnits], 3, MidpointRounding.AwayFromZero);
                                        var newPinY = Math.Round(
                                            diagramShape.Corners.BottomSide + Math.Round(
                                                shape.Cells[this.settings.VisioLocPinYField].Result[this.settings.VisioUnits], 3, MidpointRounding.AwayFromZero), 3,
                                            MidpointRounding.AwayFromZero);

                                        if (!currentPinY.Equals(newPinY))
                                        {
                                            this.logger.LogDebug("Changing PinY: {Shape} - {OldValue},{NewValue}", diagramShape, currentPinY, newPinY);
                                            shape.Cells[this.settings.VisioPinYField].Result[this.settings.VisioUnits] = newPinY;
                                        }

                                        // shape text
                                        if (!shape.Text.Equals(diagramShape.ShapeText))
                                        {
                                            shape.Text = diagramShape.ShapeText;
                                        }

                                        var newCorners = this.CalculateCorners(diagramShape.VisioId);

                                        if (!newCorners.Equals(diagramShape.Corners))
                                        {
                                            this.ReDrawShapesAsync(diagramShape).ConfigureAwait(false);
                                        }
                                        else
                                        {
                                            diagramShape.Corners = newCorners;
                                        }
                                    }
                                    finally
                                    {
                                        Marshal.ReleaseObject(shape);
                                    }

                                    break;
                                }
                        }
                    }).ConfigureAwait(false);
        }

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">System not initialised.</exception>
        /// <exception cref="T:System.OverflowException">
        ///     The array is multidimensional and contains more than
        ///     <see cref="F:System.Int32.MaxValue" /> elements.
        /// </exception>
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
                if (activeWindow is not null)
                {
                    selection = activeWindow.Selection;

                    selection.GetIDs(out Array ids);

                    foreach (var id in ids)
                    {
                        listShapeIds.Add((int)id);
                    }
                }

                return listShapeIds.ToArray();
            }
            finally
            {
                Marshal.ReleaseObject(activeWindow);
                Marshal.ReleaseObject(selection);
            }
        }

        /// <exception cref="InvalidOperationException">System not initialised.</exception>
        /// <inheritdoc />
        public int SelectionPrimaryItem()
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            Selection? selection = null;
            Shape? primaryItem = null;
            try
            {
                selection = this.visioApplication.ActiveWindow.Selection;
                primaryItem = selection.PrimaryItem;

                return primaryItem.ID;
            }
            finally
            {
                Marshal.ReleaseObject(selection);
                Marshal.ReleaseObject(primaryItem);
            }
        }

        private Shape GetShape(int shapeId)
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            Page? activePage = null;
            try
            {
                activePage = this.visioApplication.ActivePage;
                var shape = activePage.Shapes.ItemFromID[shapeId];
                return shape;
            }
            finally
            {
                Marshal.ReleaseObject(activePage);
            }
        }
    }
}