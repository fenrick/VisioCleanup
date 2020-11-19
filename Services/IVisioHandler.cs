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
        Corners CalculateCorners(int shapeId);

        /// <summary>
        ///     Close visio session and shutdown.
        /// </summary>
        void Close();

        /// <summary>
        ///     Return an array of shapeIDs for children of the supplied shape id.
        /// </summary>
        /// <param name="shapeId">Shape ID of the parent shape.</param>
        /// <returns>array of shape ids for children.</returns>
        Task<IEnumerable<int>> GetChildrenAsync(int shapeId);

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
            var corners = default(Corners);

            var shape = this.GetShape(shapeId);

            corners.LeftSide = shape.Cells[this.settings.VisioPinXField].Result[this.settings.VisioUnits]
                               - shape.Cells[this.settings.VisioLocPinXField].Result[this.settings.VisioUnits];
            corners.BottomSide = shape.Cells[this.settings.VisioPinYField].Result[this.settings.VisioUnits]
                                 - shape.Cells[this.settings.VisioLocPinYField].Result[this.settings.VisioUnits];

            corners.RightSide = corners.LeftSide
                                + shape.Cells[this.settings.VisioWidthField].Result[this.settings.VisioUnits];
            corners.TopSide = corners.BottomSide
                              + shape.Cells[this.settings.VisioHeightField].Result[this.settings.VisioUnits];

            return corners;
        }

        /// <inheritdoc />
        public void Close()
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<int>> GetChildrenAsync(int shapeId)
        {
            var shapeIDs = new List<int>();

            var parentShape = this.GetShape(shapeId);

            var selection = parentShape.SpatialNeighbors[
                (short)VisSpatialRelationCodes.visSpatialContain,
                0,
                (short)VisSpatialRelationFlags.visSpatialBackToFront];

            this.logger.LogDebug(
                "Potential child shapes found: {CountOfSelection}",
                selection.Count);

            // var tasks = new List<Task>();
            foreach (var child in selection)
            {
                if (!(child is Shape childShape))
                {
                    continue;
                }

                await Task.Run(
                    () =>
                        {
                            // check that immediate parent is the supplied shape.
                            var parentSelection = childShape.SpatialNeighbors[
                                (short)VisSpatialRelationCodes.visSpatialContainedIn,
                                0,
                                (short)VisSpatialRelationFlags.visSpatialFrontToBack];

                            if (parentShape.Equals(parentSelection.PrimaryItem))
                            {
                                shapeIDs.Add(childShape.ID);
                            }
                        }).ConfigureAwait(false);

                // tasks.Add(t);
            }

            // Task.WaitAll(tasks.ToArray());
            this.logger.LogDebug(
                "Final child shapes found: {CountOfShapeIDs}",
                shapeIDs.Count);

            return shapeIDs.ToArray();
        }

        /// <inheritdoc />
        public string GetShapeText(int shapeId)
        {
            var shape = this.GetShape(shapeId);
            return shape.Text;
        }

        /// <inheritdoc />
        public void Open()
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

            switch (diagramShape.ShapeType)
            {
                case ShapeType.Existing:
                    {
                        await Task.Run(
                            () =>
                                {
                                    var shape = this.GetShape(diagramShape.VisioId);
                                    this.logger.LogDebug(
                                        "Redrawing shape: {ShapeName}",
                                        shape.Name);

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

                                    shape.Text = diagramShape.ShapeText;

                                    var newCorners = this.CalculateCorners(diagramShape.VisioId);

                                    diagramShape.Corners = newCorners;
                                }).ConfigureAwait(false);
                        break;
                    }
            }
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

            var selection = this.visioApplication.ActiveWindow.Selection;

            selection.GetIDs(out Array ids);
            var listShapeIds = new List<int>(ids.Length);

            foreach (var id in ids)
            {
                listShapeIds.Add((int)id);
            }

            return listShapeIds.ToArray();
        }

        /// <exception cref="InvalidOperationException">System not initialised.</exception>
        /// <inheritdoc />
        public int SelectionPrimaryItem()
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var selection = this.visioApplication.ActiveWindow.Selection;
            Shape primaryItem = selection.PrimaryItem;

            return primaryItem.ID;
        }

        private Shape GetShape(int shapeId)
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var activePage = this.visioApplication.ActivePage;
            var shape = activePage.Shapes.ItemFromID[shapeId];
            return shape;
        }
    }
}