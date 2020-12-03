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
        /// <exception cref="T:System.NullReferenceException">Visio application is <see langword="null" />.</exception>
        public void Close()
        {
            Marshal.ReleaseObject(this.visioApplication);
        }

        /*
                /// <inheritdoc />
                /// <exception cref="T:System.InvalidOperationException">System not initialised.</exception>
                public void CreateDocument()
                {
                    if (this.visioApplication is null)
                    {
                        throw new InvalidOperationException("System not initialised.");
                    }
        
                    this.visioApplication.Documents.Add(string.Empty);
                    this.visioApplication.Documents.OpenEx("Basic.vss", (short)VisOpenSaveArgs.visOpenDocked);
                }
        */

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

                selection.GetIDs(out var selectionIDs);

                var selections = new List<int>();
                if (selectionIDs is null)
                {
                    return selections;
                }

                selections.AddRange(selectionIDs.Cast<int>());

                Parallel.ForEach(
                    selections, childShapeId =>
                        {
                            // check that immediate parent is the supplied shape.
                            relation = (short)VisSpatialRelationCodes.visSpatialContainedIn;
                            flags = (short)VisSpatialRelationFlags.visSpatialFrontToBack;
                            Shape? childShape = null;
                            Selection? parentSelection = null;
                            try
                            {
                                childShape = this.GetShape(childShapeId);
                                parentSelection = childShape.SpatialNeighbors[relation, 0, flags];
                                if (parentSelection.Count > 0)
                                {
                                    var primaryItemShapeId = parentSelection.PrimaryItem.ID;

                                    if (parentShapeId.Equals(primaryItemShapeId))
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
                        });

                this.logger.LogDebug("Final child shapes found: {CountOfShapeIDs}", shapeIDs.Count);

                return shapeIDs.ToArray();
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
        public Corners GetPageSize(double headerHeight, double sidePanelWidth)
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
        /// TODO: needs refactoring
        /// <exception cref="T:System.NullReferenceException">Shape is <see langword="null" />.</exception>
        public void ReDrawShapes(DiagramShape diagramShape)
        {
            foreach (var childDiagramShape in diagramShape.Children)
            {
                this.ReDrawShapes(childDiagramShape);
            }

            switch (diagramShape.ShapeType)
            {
                case ShapeType.NewShape:
                    this.DropShape(diagramShape);
                    goto case ShapeType.Existing;
                case ShapeType.Existing:
                    Shape? shape = null;
                    try
                    {
                        shape = this.GetShape(diagramShape.VisioId);
                        ChangeShape(this.visioChanges(diagramShape), shape);
                        shape.Text = diagramShape.ShapeText;
                        diagramShape.Corners = this.CalculateCorners(diagramShape.VisioId);
                    }
                    finally
                    {
                        Marshal.ReleaseObject(shape);
                    }

                    break;
                case ShapeType.FakeShape:
                    break;
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
                if (activeWindow is not null)
                {
                    selection = activeWindow.Selection;

                    selection.GetIDs(out var ids);

                    if (ids is null)
                    {
                        return listShapeIds.ToArray();
                    }

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

        /*
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
        */

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">System not initialised.</exception>
        public void VisualChanges(bool visualChanges)
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            this.visioApplication.ShowChanges = visualChanges;
            this.visioApplication.ScreenUpdating = (short)(visualChanges ? 1 : 0);
        }

        private static void ChangeShape(Dictionary<string, object>[] items, Shape shape)
        {
            // MAP THE REQUEST TO THE STRUCTURES VISIO EXPECTS
            var srcStream = new short[items.Length * 3];
            var formulaObjects = new object[items.Length];
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                srcStream[(i * 3) + 0] = (short)item["section"];
                srcStream[(i * 3) + 1] = (short)item["row"];
                srcStream[(i * 3) + 2] = (short)item["cell"];
                formulaObjects[i] = item["formula"];
            }

            // EXECUTE THE REQUEST
            short flags = 0;
            shape.SetFormulas(srcStream, formulaObjects, flags);
        }

        private void DropShape(DiagramShape diagramShape)
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var newLocPinX = Math.Round(diagramShape.Corners.Width() * 0.5, 3, MidpointRounding.AwayFromZero);
            var newLocPinY = Math.Round(diagramShape.Corners.Height() * 0.5, 3, MidpointRounding.AwayFromZero);
            var newPinX = Math.Round(diagramShape.Corners.LeftSide + newLocPinX, 3, MidpointRounding.AwayFromZero);
            var newPinY = Math.Round(diagramShape.Corners.BottomSide + newLocPinY, 3, MidpointRounding.AwayFromZero);

            var stencil = this.visioApplication.Documents["Basic.vss"];
            var shape = this.visioApplication.ActivePage.Drop(stencil.Masters["Rectangle"], newPinX, newPinY);

            diagramShape.VisioId = shape.ID;
            diagramShape.ShapeType = ShapeType.Existing;
            shape.SendToBack();
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

        private Dictionary<string, object>[] visioChanges(DiagramShape diagramShape)
        {
            var newLocPinX = Math.Round(diagramShape.Corners.Width() * 0.5, 3, MidpointRounding.AwayFromZero);
            var newLocPinY = Math.Round(diagramShape.Corners.Height() * 0.5, 3, MidpointRounding.AwayFromZero);
            var newPinX = Math.Round(diagramShape.Corners.LeftSide + newLocPinX, 3, MidpointRounding.AwayFromZero);
            var newPinY = Math.Round(diagramShape.Corners.BottomSide + newLocPinY, 3, MidpointRounding.AwayFromZero);

            return new[]
                       {
                           new Dictionary<string, object>
                               {
                                   { "section", (short)VisSectionIndices.visSectionObject },
                                   { "row", (short)VisRowIndices.visRowXFormOut },
                                   { "cell", (short)VisCellIndices.visXFormWidth },
                                   { "formula", $"{diagramShape.Corners.Width()} {this.settings.VisioUnits}" },
                               },
                           new Dictionary<string, object>
                               {
                                   { "section", (short)VisSectionIndices.visSectionObject },
                                   { "row", (short)VisRowIndices.visRowXFormOut },
                                   { "cell", (short)VisCellIndices.visXFormHeight },
                                   { "formula", $"{diagramShape.Corners.Height()} {this.settings.VisioUnits}" },
                               },
                           new Dictionary<string, object>
                               {
                                   { "section", (short)VisSectionIndices.visSectionObject },
                                   { "row", (short)VisRowIndices.visRowXFormOut },
                                   { "cell", (short)VisCellIndices.visXFormPinX },
                                   { "formula", $"{newPinX} {this.settings.VisioUnits}" },
                               },
                           new Dictionary<string, object>
                               {
                                   { "section", (short)VisSectionIndices.visSectionObject },
                                   { "row", (short)VisRowIndices.visRowXFormOut },
                                   { "cell", (short)VisCellIndices.visXFormPinY },
                                   { "formula", $"{newPinY} {this.settings.VisioUnits}" },
                               },
                       };
        }
    }
}