// -----------------------------------------------------------------------
// <copyright file="VisioApplication.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Office.Interop.Visio;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;

    using Marshal = VisioCleanup.Core.Marshal;

    /// <inheritdoc />
    public class VisioApplication : IVisioApplication
    {
        private readonly ILogger<VisioApplication> logger;

        private readonly ConcurrentDictionary<int, IVShape> shapeCache = new();

        private readonly ConcurrentDictionary<string, IVMaster?> stencilCache = new();

        private IVPage? activePage;

        private Application? visioApplication;

        /// <summary>Initialises a new instance of the <see cref="VisioApplication" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        public VisioApplication(ILogger<VisioApplication> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.activePage = null;
            this.visioApplication = null;
        }

        /// <inheritdoc />
        public void Close()
        {
            this.logger.LogDebug("Clearning shape cache");
            this.shapeCache.Clear();

            this.logger.LogDebug("Clearing stencil cache");
            this.stencilCache.Clear();

            this.logger.LogDebug("Releasing active page");
            this.activePage = null;

            this.logger.LogDebug("Releasing visio application");
            this.visioApplication = null;
        }

        /// <inheritdoc />
        public void CreateShape(DiagramShape diagramShape)
        {
            var newLocPinX = DiagramShape.ConvertMeasurement(diagramShape.Width() / 2);
            var newLocPinY = DiagramShape.ConvertMeasurement(diagramShape.Height() / 2);
            var newPinX = DiagramShape.ConvertMeasurement(diagramShape.LeftSide) + newLocPinX;
            var newPinY = DiagramShape.ConvertMeasurement(diagramShape.BaseSide) + newLocPinY;

            var shapeMaster = "Rectangle";

            if (!string.IsNullOrEmpty(diagramShape.Master))
            {
                shapeMaster = diagramShape.Master;
            }

            var master = this.stencilCache.GetOrAdd(shapeMaster, this.StencilValueFactory);

            if (master is null)
            {
                this.logger.LogError("Unable to find matching stencil: {StencilName}", shapeMaster);
                throw new InvalidOperationException("Unable to find matching stencil.");
            }

            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var shape = this.visioApplication.ActivePage.Drop(master, newPinX, newPinY);

            diagramShape.VisioId = shape.ID;
            diagramShape.ShapeType = ShapeType.Existing;

            var visioShape = this.GetShape(diagramShape.VisioId);
            visioShape.Text = diagramShape.ShapeText;

            this.UpdateShape(diagramShape);
        }

        /// <inheritdoc />
        public int GetPageLeftSide()
        {
            if (this.visioApplication is null || this.activePage is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var pageSheet = this.activePage.PageSheet;

            return DiagramShape.ConvertMeasurement(
                GetCellValue(pageSheet, VisSectionIndices.visSectionObject, VisRowIndices.visRowPrintProperties, VisCellIndices.visPrintPropertiesLeftMargin));
        }

        /// <inheritdoc />
        public int GetPageRightSide()
        {
            if (this.visioApplication is null || this.activePage is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var pageSheet = this.activePage.PageSheet;

            var rightMargin = GetCellValue(pageSheet, VisSectionIndices.visSectionObject, VisRowIndices.visRowPrintProperties, VisCellIndices.visPrintPropertiesRightMargin);
            var width = GetCellValue(pageSheet, VisSectionIndices.visSectionObject, VisRowIndices.visRowPage, VisCellIndices.visPageWidth);

            return DiagramShape.ConvertMeasurement(width - rightMargin);
        }

        /// <inheritdoc />
        public int GetPageTopSide()
        {
            if (this.visioApplication is null || this.activePage is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var pageSheet = this.activePage.PageSheet;

            var height = GetCellValue(pageSheet, VisSectionIndices.visSectionObject, VisRowIndices.visRowPage, VisCellIndices.visPageHeight);
            var topMargin = GetCellValue(pageSheet, VisSectionIndices.visSectionObject, VisRowIndices.visRowPage, VisCellIndices.visPrintPropertiesTopMargin);

            return DiagramShape.ConvertMeasurement(height - topMargin);
        }

        /// <inheritdoc />
        public void Open()
        {
            try
            {
                this.logger.LogDebug("Opening connection to visio");
                this.visioApplication = Marshal.GetActiveObject("Visio.Application") as Application ?? throw new InvalidOperationException("Visio must be running.");
                this.activePage = this.visioApplication.ActivePage;

                this.stencilCache.Clear();
                this.shapeCache.Clear();
            }
            catch (COMException)
            {
                throw new InvalidOperationException();
            }
        }

        /// <inheritdoc />
        public IEnumerable<DiagramShape> RetrieveShapes()
        {
            if (this.visioApplication?.ActiveWindow is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            Dictionary<int, DiagramShape> allShapes = new();

            // get selection.
            var selection = this.visioApplication.ActiveWindow.Selection;
            this.LoadShapeCache();

            this.logger.LogDebug("Found {Count} selected shapes", selection.Count);
            foreach (var selected in selection.Cast<Shape>().Where(selected => selected is not null))
            {
                this.logger.LogDebug("Processing shape: {ShapeID} - {ShapeText}", selected.ID, selected.Text);

                var sheetId = selected.ID;
                if (allShapes.ContainsKey(sheetId))
                {
                    this.logger.LogDebug("Shape already processed");
                    continue;
                }

                // create new shape.
                DiagramShape diagramShape = new(sheetId)
                                                {
                                                    ShapeText = selected.Text,
                                                    ShapeType = ShapeType.Existing,
                                                    LeftSide = this.CalculateLeftSide(sheetId),
                                                    RightSide = this.CalculateRightSide(sheetId),
                                                    TopSide = this.CalculateTopSide(sheetId),
                                                    BaseSide = this.CalculateBaseSide(sheetId),
                                                };
                this.logger.LogDebug("Adding shape to collection");
                allShapes.Add(sheetId, diagramShape);
            }

            this.logger.LogDebug("Processed a total of {Count} shapes", allShapes.Count);

            // generate final collection
            Collection<DiagramShape> shapes = new();
            foreach (var (_, value) in allShapes)
            {
                shapes.Add(value);
            }

            return shapes;
        }

        /// <inheritdoc />
        public void SetForeground(DiagramShape diagramShape)
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            if (diagramShape is null)
            {
                throw new ArgumentNullException(nameof(diagramShape));
            }

            if (diagramShape.ShapeType == ShapeType.Existing)
            {
                var shape = this.GetShape(diagramShape.VisioId);
                shape.BringToFront();
            }

            Parallel.ForEach(diagramShape.Children, this.SetForeground);
        }

        /// <inheritdoc />
        public void UpdateShape(DiagramShape diagramShape)
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var newLocPinX = DiagramShape.ConvertMeasurement(diagramShape.Width() / 2);
            var newLocPinY = DiagramShape.ConvertMeasurement(diagramShape.Height() / 2);
            var newPinX = DiagramShape.ConvertMeasurement(diagramShape.LeftSide) + newLocPinX;
            var newPinY = DiagramShape.ConvertMeasurement(diagramShape.BaseSide) + newLocPinY;

            var width = DiagramShape.ConvertMeasurement(diagramShape.Width());
            var height = DiagramShape.ConvertMeasurement(diagramShape.Height());

            var updates = new List<Dictionary<string, object>>
                              {
                                  new()
                                      {
                                          { "sheetID", diagramShape.VisioId },
                                          { "section", (short)VisSectionIndices.visSectionObject },
                                          { "row", (short)VisRowIndices.visRowXFormOut },
                                          { "cell", (short)VisCellIndices.visXFormWidth },
                                          { "unit", VisUnitCodes.visMillimeters },
                                          { "result", width },
                                      },
                                  new()
                                      {
                                          { "sheetID", diagramShape.VisioId },
                                          { "section", (short)VisSectionIndices.visSectionObject },
                                          { "row", (short)VisRowIndices.visRowXFormOut },
                                          { "cell", (short)VisCellIndices.visXFormHeight },
                                          { "unit", VisUnitCodes.visMillimeters },
                                          { "result", height },
                                      },
                                  new()
                                      {
                                          { "sheetID", diagramShape.VisioId },
                                          { "section", (short)VisSectionIndices.visSectionObject },
                                          { "row", (short)VisRowIndices.visRowXFormOut },
                                          { "cell", (short)VisCellIndices.visXFormPinX },
                                          { "unit", VisUnitCodes.visMillimeters },
                                          { "result", newPinX },
                                      },
                                  new()
                                      {
                                          { "sheetID", diagramShape.VisioId },
                                          { "section", (short)VisSectionIndices.visSectionObject },
                                          { "row", (short)VisRowIndices.visRowXFormOut },
                                          { "cell", (short)VisCellIndices.visXFormPinY },
                                          { "unit", VisUnitCodes.visMillimeters },
                                          { "result", newPinY },
                                      },
                              };

            // MAP THE REQUEST TO THE STRUCTURES VISIO EXPECTS
            const int SrcStreamFields = 3;
            var srcStream = new short[updates.Count * SrcStreamFields];
            var unitsArray = new object[updates.Count];
            var resultsArray = new object[updates.Count];
            for (var i = 0; i < updates.Count; i++)
            {
                var item = updates[i];
                var srcStreamTracker = 0;

                srcStream[(i * SrcStreamFields) + srcStreamTracker] = Convert.ToInt16(item["section"]);
                srcStreamTracker++;
                srcStream[(i * SrcStreamFields) + srcStreamTracker] = Convert.ToInt16(item["row"]);
                srcStreamTracker++;
                srcStream[(i * SrcStreamFields) + srcStreamTracker] = Convert.ToInt16(item["cell"]);
                resultsArray[i] = item["result"];
                unitsArray[i] = item["unit"];
            }

            // EXECUTE THE REQUEST
            const short Flags = 0;
            try
            {
                this.GetShape(diagramShape.VisioId).SetResults(srcStream, unitsArray, resultsArray, Flags);
            }
            catch (COMException e)
            {
                this.logger.LogError(e, "Error occured during updating {Shape}", diagramShape);
            }
        }

        /// <exception cref="System.InvalidOperationException">System not initialised.</exception>
        /// <inheritdoc />
        public void VisualChanges(bool state)
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            this.visioApplication.ShowChanges = state;
            this.visioApplication.UndoEnabled = state;
            this.visioApplication.ScreenUpdating = state ? (short)1 : (short)0;
            this.visioApplication.DeferRecalc = state ? (short)1 : (short)0;
        }

        private static double GetCellValue(IVShape shape, VisSectionIndices sectionIndex, VisRowIndices rowIndex, VisCellIndices cellIndex)
        {
            var shapeCell = shape.CellsSRC[(short)sectionIndex, (short)rowIndex, (short)cellIndex];
            return shapeCell.Result[VisUnitCodes.visMillimeters];
        }

        private int CalculateBaseSide(int visioId)
        {
            var shape = this.GetShape(visioId);

            var pinY = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormPinY);
            var locPinY = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormLocPinY);

            return DiagramShape.ConvertMeasurement(pinY - locPinY);
        }

        private int CalculateLeftSide(int visioId)
        {
            var shape = this.GetShape(visioId);

            var pinX = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormPinX);
            var locPinX = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormLocPinX);

            return DiagramShape.ConvertMeasurement(pinX - locPinX);
        }

        private int CalculateRightSide(int visioId)
        {
            var shape = this.GetShape(visioId);

            var pinX = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormPinX);
            var locPinX = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormLocPinX);
            var width = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormWidth);

            return DiagramShape.ConvertMeasurement((pinX - locPinX) + width);
        }

        private int CalculateTopSide(int visioId)
        {
            var shape = this.GetShape(visioId);

            var pinY = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormPinY);
            var locPinY = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormLocPinY);
            var height = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormHeight);

            return DiagramShape.ConvertMeasurement((pinY - locPinY) + height);
        }

        private IVShape GetShape(int visioId)
        {
            if (this.visioApplication is null || this.activePage is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            return this.shapeCache.GetOrAdd(visioId, sheetId => this.activePage.Shapes.ItemFromID[sheetId]);
        }

        private void LoadShapeCache()
        {
            if (this.visioApplication is null || this.activePage is null)
            {
                throw new InvalidOperationException("Initialise system first");
            }

            // clear cache
            this.shapeCache.Clear();

            var shapes = this.activePage.Shapes;
            foreach (var element in shapes)
            {
                if (element is not Shape shape)
                {
                    continue;
                }

                this.shapeCache.TryAdd(shape.ID, shape);
            }
        }

        private IVMaster? StencilValueFactory(string key)
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("Initialise system first.");
            }

            var documentStencil = this.visioApplication.ActiveDocument.Masters;
            if (documentStencil.Count > 0)
            {
                documentStencil.GetNames(out var masterNames);
                if (masterNames is string[] strings)
                {
                    var result = strings.Contains(key);
                    if (result)
                    {
                        return documentStencil[key];
                    }
                }
            }

            this.visioApplication.ActiveWindow.DockedStencils(out var stencilNames);
            if (stencilNames is null)
            {
                return null;
            }

            foreach (var stencil in from object? stencilName in stencilNames where stencilName?.Equals(string.Empty) == false select this.visioApplication.Documents[stencilName])
            {
                stencil.Masters.GetNames(out var names);
                if (names is not string[] nameArray)
                {
                    continue;
                }

                var result = nameArray.Contains(key);
                if (result)
                {
                    return stencil.Masters[key];
                }
            }

            return null;
        }
    }
}
