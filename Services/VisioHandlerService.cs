// -----------------------------------------------------------------------
// <copyright file="VisioHandlerService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Office.Interop.Visio;

    using VisioCleanup.Objects;

    /// <inheritdoc />
    internal class VisioHandlerService : IVisioHandler
    {
        private readonly ILogger<VisioHandlerService> logger;

        private readonly VisioCleanupSettings settings;

        private List<Task>? tasks;

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
        /// <exception cref="T:System.NullReferenceException">Shape is <see langword="null" />.</exception>
        public Corners CalculateCorners(int shapeId)
        {
            Shape? shape = null;
            try
            {
                var corners = default(Corners);

                shape = this.GetShape(shapeId);

                corners.LeftSide = (int)(Math.Round(
                                             shape.Cells[this.settings.VisioPinXField].Result[this.settings.VisioUnits]
                                             - shape.Cells[this.settings.VisioLocPinXField].Result[this.settings.VisioUnits],
                                             3,
                                             MidpointRounding.AwayFromZero) * 1000);
                corners.BottomSide = (int)(Math.Round(
                                               shape.Cells[this.settings.VisioPinYField].Result[this.settings.VisioUnits]
                                               - shape.Cells[this.settings.VisioLocPinYField].Result[this.settings.VisioUnits],
                                               3,
                                               MidpointRounding.AwayFromZero) * 1000);

                corners.RightSide = (int)(Math.Round(
                                              corners.LeftSide + shape.Cells[this.settings.VisioWidthField].Result[this.settings.VisioUnits],
                                              3,
                                              MidpointRounding.AwayFromZero) * 1000);
                corners.TopSide = (int)(Math.Round(
                                            corners.BottomSide + shape.Cells[this.settings.VisioHeightField].Result[this.settings.VisioUnits],
                                            3,
                                            MidpointRounding.AwayFromZero) * 1000);

                return corners;
            }
            finally
            {
                Marshal.ReleaseObject(shape);
            }
        }

        /// <inheritdoc />
        /// <exception cref="T:System.NullReferenceException">Visio application is <see langword="null" />.</exception>
        public void Close()
        {
            Marshal.ReleaseObject(this.visioApplication);
        }

        /// <inheritdoc />
        /// <exception cref="T:System.AggregateException">
        ///     The exception that contains all the individual exceptions thrown on all
        ///     threads.
        /// </exception>
        /// TODO: Needs to be refactored.
        /// <exception cref="T:System.NullReferenceException">Shape is <see langword="null" />.</exception>
        public IEnumerable<int> GetChildren(int parentShapeId)
        {
            Shape? parentShape = null;
            Selection? selection = null;
            try
            {
                var shapeIDs = new List<int>();

                parentShape = this.GetShape(parentShapeId);

                var relation = (short)VisSpatialRelationCodes.visSpatialContain;
                var flags = (short)VisSpatialRelationFlags.visSpatialBackToFront;
                selection = parentShape.SpatialNeighbors[relation, 0, flags];

                this.logger.LogDebug("Potential child shapes found: {CountOfSelection}", selection.Count);

                // selection.GetIDs(out var selectionIDs);
                if (selection.Count == 0)
                {
                    return shapeIDs;
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

                    var primaryItemShapeId = parentSelection.PrimaryItem.ID;

                    if (parentShapeId.Equals(primaryItemShapeId))
                    {
                        shapeIDs.Add(shape.ID);
                    }
                }

                this.logger.LogDebug("Final child shapes found: {CountOfShapeIDs}", shapeIDs.Count);

                return shapeIDs;
            }
            finally
            {
                Marshal.ReleaseObject(parentShape);
                Marshal.ReleaseObject(selection);
            }
        }

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">System not initialised.</exception>
        /// TODO: needs refactoring
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

                var pageWidth = (int)(Math.Round(pageSheet.Cells["PageWidth"].Result[this.settings.VisioUnits], 3, MidpointRounding.AwayFromZero) * 1000);
                var pageHeight = (int)(Math.Round(pageSheet.Cells["PageHeight"].Result[this.settings.VisioUnits], 3, MidpointRounding.AwayFromZero) * 1000);
                var pageLeftMargin = (int)(Math.Round(pageSheet.Cells["PageLeftMargin"].Result[this.settings.VisioUnits], 3, MidpointRounding.AwayFromZero) * 1000);
                var pageTopMargin = (int)(Math.Round(pageSheet.Cells["PageTopMargin"].Result[this.settings.VisioUnits], 3, MidpointRounding.AwayFromZero) * 1000);
                var pageRightMargin = (int)(Math.Round(pageSheet.Cells["PageRightMargin"].Result[this.settings.VisioUnits], 3, MidpointRounding.AwayFromZero) * 1000);
                var pageBottomMargin = (int)(Math.Round(pageSheet.Cells["PageBottomMargin"].Result[this.settings.VisioUnits], 3, MidpointRounding.AwayFromZero) * 1000);

                corners.LeftSide = pageLeftMargin;
                corners.BottomSide = pageBottomMargin;
                var horizontalMargins = pageLeftMargin + pageRightMargin;
                corners.RightSide = pageWidth - (horizontalMargins + sidePanelWidth);
                var verticalMargins = pageTopMargin + pageBottomMargin;
                corners.TopSide = pageHeight - (verticalMargins + headerHeight);

                return corners;
            }
            finally
            {
                Marshal.ReleaseObject(pageSheet);
            }
        }

        /// <inheritdoc />
        /// <exception cref="T:System.NullReferenceException">Shape is <see langword="null" />.</exception>
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
            catch (COMException)
            {
                this.logger.LogDebug("Visio not running, time to open it.");
                this.visioApplication = new Application();
            }
        }

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">System not initialised.</exception>
        /// TODO: needs refactoring.
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

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">System not initialised.</exception>
        /// TODO: needs refactoring
        /// <exception cref="T:System.NullReferenceException">Shape is <see langword="null" />.</exception>
        public async Task UpdateVisio(DiagramShape diagramShape)
        {
            this.tasks = new List<Task>();

            this.UpdateVisioInternal(diagramShape);
            foreach (var task in this.tasks)
            {
                task.Start();
            }

            await Task.WhenAll(this.tasks);
        }

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">System not initialised.</exception>
        public void VisualChanges(bool visualChanges)
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            this.visioApplication.ShowChanges = visualChanges;
            this.visioApplication.UndoEnabled = visualChanges;
            this.visioApplication.ScreenUpdating = visualChanges ? 1 : 0;
        }

        private void ChangeShape(IReadOnlyList<Dictionary<string, object>> items, IVShape shape)
        {
            if (items.Count == 0)
            {
                this.logger.LogDebug("No changes found.");
                return;
            }

            this.logger.LogDebug("Updating shape.");

            // MAP THE REQUEST TO THE STRUCTURES VISIO EXPECTS
            var srcStream = new short[items.Count * 3];
            var formulaObjects = new object[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                srcStream[(i * 3) + 0] = (short)item["section"];
                srcStream[(i * 3) + 1] = (short)item["row"];
                srcStream[(i * 3) + 2] = (short)item["cell"];
                formulaObjects[i] = item["formula"];
            }

            // EXECUTE THE REQUEST
            const short Flags = 0;
            shape.SetFormulas(srcStream, formulaObjects, Flags);
        }

        private Shape DropShape(DiagramShape diagramShape)
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            double newLocPinX = (diagramShape.Corners.Width() * 0.5) / 1000;
            double newLocPinY = (diagramShape.Corners.Height() * 0.5) / 1000;
            double newPinX = (diagramShape.Corners.LeftSide + newLocPinX) / 1000;
            double newPinY = (diagramShape.Corners.BottomSide + newLocPinY) / 1000;

            var stencil = this.visioApplication.Documents["Basic.vss"];
            var stencilName = "Ellipse";

            if (diagramShape.Stencil is not null)
            {
                stencilName = diagramShape.Stencil;
            }

            var shape = this.visioApplication.ActivePage.Drop(stencil.Masters[stencilName], newPinX, newPinY);

            diagramShape.VisioId = shape.ID;
            diagramShape.ShapeType = ShapeType.Existing;
            shape.SendToBack();
            return shape;
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

        private void UpdateVisioInternal(DiagramShape diagramShape)
        {
            foreach (var childDiagramShape in diagramShape.Children)
            {
                this.UpdateVisioInternal(childDiagramShape);
            }

            var task = new Task(
                () =>
                    {
                        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                        Shape? shape = null;
                        switch (diagramShape.ShapeType)
                        {
                            case ShapeType.NewShape:
                                this.logger.LogDebug("Dropping new shape: {Shape}", diagramShape);
                                shape = this.DropShape(diagramShape);
                                goto case ShapeType.Existing;
                            case ShapeType.Existing:
                                this.logger.LogDebug("Checking shape: {Shape}", diagramShape);
                                shape ??= this.GetShape(diagramShape.VisioId);

                                this.ChangeShape(this.VisioChanges(diagramShape), shape);
                                shape.Text = diagramShape.ShapeText;

                                // diagramShape.Corners = this.CalculateCorners(diagramShape.VisioId);
                                break;
                            case ShapeType.FakeShape:
                                // we don't draw this!
                                this.logger.LogDebug("Skipping fake shape: {Shape}", diagramShape);
                                break;
                        }
                    });

            this.tasks.Add(task);
        }

        private Dictionary<string, object>[] VisioChanges(DiagramShape diagramShape)
        {
            double newLocPinX = ((double) diagramShape.Corners.Width() * 0.5) / 1000;
            double newLocPinY = ((double) diagramShape.Corners.Height() * 0.5) / 1000;
            double newPinX = ((double) diagramShape.Corners.LeftSide + newLocPinX) / 1000;
            double newPinY = ((double) diagramShape.Corners.BottomSide + newLocPinY) / 1000;
            
            var visioChanges = new List<Dictionary<string, object>>();

            double width = (double) diagramShape.Corners.Width() / 1000;
            double height = (double) diagramShape.Corners.Height() / 1000;
            
            visioChanges.Add(
                new Dictionary<string, object>
                    {
                        { "section", (short)VisSectionIndices.visSectionObject },
                        { "row", (short)VisRowIndices.visRowXFormOut },
                        { "cell", (short)VisCellIndices.visXFormWidth },
                        { "formula", $"{width} {this.settings.VisioUnits}" },
                    });
            visioChanges.Add(
                new Dictionary<string, object>
                    {
                        { "section", (short)VisSectionIndices.visSectionObject },
                        { "row", (short)VisRowIndices.visRowXFormOut },
                        { "cell", (short)VisCellIndices.visXFormHeight },
                        { "formula", $"{height} {this.settings.VisioUnits}" },
                    });
            visioChanges.Add(
                new Dictionary<string, object>
                    {
                        { "section", (short)VisSectionIndices.visSectionObject },
                        { "row", (short)VisRowIndices.visRowXFormOut },
                        { "cell", (short)VisCellIndices.visXFormPinX },
                        { "formula", $"{newPinX} {this.settings.VisioUnits}" },
                    });
            visioChanges.Add(
                new Dictionary<string, object>
                    {
                        { "section", (short)VisSectionIndices.visSectionObject },
                        { "row", (short)VisRowIndices.visRowXFormOut },
                        { "cell", (short)VisCellIndices.visXFormPinY },
                        { "formula", $"{newPinY} {this.settings.VisioUnits}" },
                    });
            
            return visioChanges.ToArray();
        }
    }
}