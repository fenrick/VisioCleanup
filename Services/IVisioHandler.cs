// -----------------------------------------------------------------------
// <copyright file="IVisioHandler.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Services
{
    using System;
    using System.Collections.Generic;
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
        Task<Corners> CalculateCornersAsync(int shapeId);

        /// <summary>
        ///     Close visio session and shutdown.
        /// </summary>
        void Close();

        /// <summary>
        ///     Return an array of shapeIDs for children of the supplied shape id.
        /// </summary>
        /// <param name="shapeId">Shape ID of the parent shape.</param>
        /// <returns>array of shape ids for children.</returns>
        Task<int[]> GetChildrenAsync(int shapeId);

        /// <summary>
        ///     Open visio session.
        /// </summary>
        Task OpenAsync();

        /// <summary>
        ///     Find shapes in visio diagram and change their location and size to match diagramShapes.
        /// </summary>
        /// <param name="diagramShape">Internal structure for modelling visio shapes.</param>
        Task ReDrawShapesAsync(DiagramShape diagramShape);

        /// <summary>
        ///     Returns primary item of the current selection.
        /// </summary>
        /// <returns>shape id of primary item.</returns>
        Task<int> SelectionPrimaryItemAsync();
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
        public async Task<Corners> CalculateCornersAsync(int shapeId)
        {
            var corners = default(Corners);

            var shape = await this.GetShapeAsync(shapeId).ConfigureAwait(false);

            if (shape is not null)
            {
                corners.LeftSide = shape.Cells[this.settings.VisioPinXField].Result[this.settings.VisioUnits]
                                   - shape.Cells[this.settings.VisioLocPinXField].Result[this.settings.VisioUnits];
                corners.BottomSide = shape.Cells[this.settings.VisioPinYField].Result[this.settings.VisioUnits]
                                     - shape.Cells[this.settings.VisioLocPinYField].Result[this.settings.VisioUnits];

                corners.RightSide = corners.LeftSide
                                    + shape.Cells[this.settings.VisioWidthField].Result[this.settings.VisioUnits];
                corners.TopSide = corners.BottomSide + shape.Cells[this.settings.VisioHeightField]
                                      .Result[this.settings.VisioUnits];
            }

            return corners;
        }

        /// <inheritdoc />
        public void Close()
        {
        }

        /// <inheritdoc />
        public async Task<int[]> GetChildrenAsync(int shapeId)
        {
            var shapeIDs = new List<int>();

            var parentShape = await this.GetShapeAsync(shapeId).ConfigureAwait(false);

            var selection = parentShape.SpatialNeighbors[
                (short)VisSpatialRelationCodes.visSpatialContain,
                0,
                (short)VisSpatialRelationFlags.visSpatialBackToFront];

            this.logger.LogDebug($"Child shapes found: {selection.Count}");

            foreach (var child in selection)
            {
                if (child is Shape childShape)
                {
                    shapeIDs.Add(childShape.ID);
                }
            }

            return shapeIDs.ToArray();
        }

        /// <inheritdoc />
        public async Task OpenAsync()
        {
            this.logger.LogDebug("Opening connection to visio.");
            this.visioApplication = Marshal.GetActiveObject("Visio.Application") as Application
                                    ?? throw new InvalidOperationException();
        }

        /// <inheritdoc />
        public async Task ReDrawShapesAsync(DiagramShape diagramShape)
        {
            foreach (var childDiagramShape in diagramShape.Children)
            {
                await this.ReDrawShapesAsync(childDiagramShape).ConfigureAwait(false);
            }

            var shape = await this.GetShapeAsync(diagramShape.VisioId).ConfigureAwait(false);
            this.logger.LogDebug($"Redrawing shape: {shape.Name}");

            /*
                                         calculate values
                                        left + locPinX = pinX
                                        bottom + locPinY = pinY
                                        right - left = width
                                        top - bottom = height
                                        */
            shape.Cells[this.settings.VisioWidthField].Result[this.settings.VisioUnits] =
                diagramShape.Corners.RightSide - diagramShape.Corners.LeftSide;
            shape.Cells[this.settings.VisioHeightField].Result[this.settings.VisioUnits] =
                diagramShape.Corners.TopSide - diagramShape.Corners.BottomSide;
            shape.Cells[this.settings.VisioPinXField].Result[this.settings.VisioUnits] =
                diagramShape.Corners.LeftSide + shape.Cells[this.settings.VisioLocPinXField]
                    .Result[this.settings.VisioUnits];
            shape.Cells[this.settings.VisioPinYField].Result[this.settings.VisioUnits] =
                diagramShape.Corners.BottomSide + shape.Cells[this.settings.VisioLocPinYField]
                    .Result[this.settings.VisioUnits];

            var newCorners = await this.CalculateCornersAsync(diagramShape.VisioId).ConfigureAwait(false);

            diagramShape.Corners = newCorners;
        }

        /// <inheritdoc />
        public async Task<int> SelectionPrimaryItemAsync()
        {
            Shape? primaryItem = null;
            primaryItem = this.visioApplication.ActiveWindow.Selection.PrimaryItem;

            return primaryItem?.ID ?? 0;
        }

        private async Task<Shape> GetShapeAsync(int shapeId)
        {
            var activePage = this.visioApplication.ActivePage;
            var shape = activePage.Shapes.ItemFromID[shapeId];
            return shape;
        }
    }
}