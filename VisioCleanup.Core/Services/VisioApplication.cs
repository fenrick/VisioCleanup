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
        private static readonly ConcurrentDictionary<string, Master?> StencilCache = new();

        private readonly ILogger<VisioApplication> logger;

        private readonly List<Dictionary<string, object>> shapeUpdates = new();

        private readonly List<Dictionary<string, object>> toDrop = new();

        private Page? activePage;

        private Application? visioApplication;

        /// <summary>Initialises a new instance of the <see cref="VisioApplication" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        public VisioApplication(ILogger<VisioApplication> logger) => this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <inheritdoc />
        public int CalculateBaseSide(int visioId)
        {
            var shape = this.GetShape(visioId);

            var pinY = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormPinY);
            var locPinY = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormLocPinY);

            return DiagramShape.ConvertMeasurement(pinY - locPinY);
        }

        /// <inheritdoc />
        public int CalculateLeftSide(int visioId)
        {
            var shape = this.GetShape(visioId);

            var pinX = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormPinX);
            var locPinX = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormLocPinX);

            return DiagramShape.ConvertMeasurement(pinX - locPinX);
        }

        /// <inheritdoc />
        public int CalculateRightSide(int visioId)
        {
            var shape = this.GetShape(visioId);

            var pinX = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormPinX);
            var locPinX = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormLocPinX);
            var width = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormWidth);

            return DiagramShape.ConvertMeasurement((pinX - locPinX) + width);
        }

        /// <inheritdoc />
        public int CalculateTopSide(int visioId)
        {
            var shape = this.GetShape(visioId);

            var pinY = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormPinY);
            var locPinY = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormLocPinY);
            var height = GetCellValue(shape, VisSectionIndices.visSectionObject, VisRowIndices.visRowXFormOut, VisCellIndices.visXFormHeight);

            return DiagramShape.ConvertMeasurement((pinY - locPinY) + height);
        }

        /// <inheritdoc />
        public void Close()
        {
            this.logger.LogDebug("Clearing stencil cache.");
            StencilCache.Clear();

            this.logger.LogDebug("Releasing active page.");
            this.activePage = null;

            this.logger.LogDebug("Releasing visio application.");
            this.visioApplication = null;

            this.logger.LogDebug("Cleaning up.");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <inheritdoc />
        public void CompleteDrops()
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var dropCount = this.toDrop.Count;
            if (dropCount == 0)
            {
                return;
            }

            var objectsToInstance = new object[dropCount];
            var xyArray = new double[dropCount * 2];
            for (var i = 0; i < dropCount; i++)
            {
                var dropDetails = this.toDrop[i];
                objectsToInstance[i] = dropDetails["master"];
                xyArray[i * 2] = (double)dropDetails["x"];
                xyArray[(i * 2) + 1] = (double)dropDetails["y"];
            }

            this.visioApplication.ActivePage.DropMany(objectsToInstance, xyArray, out var idArray);

            if (idArray is null)
            {
                throw new InvalidOperationException("Unable to drop shapes.");
            }

            for (var i = 0; i < dropCount; i++)
            {
                var dropDetails = this.toDrop[i];
                if (dropDetails["shape"] is not DiagramShape shape)
                {
                    throw new InvalidOperationException("Unable to load shape.");
                }

                shape.VisioId = (short)idArray.GetValue(i)!;
                shape.ShapeType = ShapeType.Existing;

                var visioShape = this.GetShape(shape.VisioId);
                visioShape.Text = shape.ShapeText;

                this.UpdateShape(shape);
            }

            this.toDrop.Clear();
        }

        /// <inheritdoc />
        public void CompleteUpdates()
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var shapeUpdatesCount = this.shapeUpdates.Count;
            if (shapeUpdatesCount == 0)
            {
                this.logger.LogDebug("No changes found.");
                return;
            }

            this.logger.LogDebug("Updating shape.");

            // MAP THE REQUEST TO THE STRUCTURES VISIO EXPECTS
            var srcStream = new short[shapeUpdatesCount * 4];
            var unitsArray = new object[shapeUpdatesCount];
            var resultsArray = new object[shapeUpdatesCount];
            for (var i = 0; i < shapeUpdatesCount; i++)
            {
                var item = this.shapeUpdates[i];
                srcStream[(i * 4) + 0] = (short)item["sheetID"];
                srcStream[(i * 4) + 1] = (short)item["section"];
                srcStream[(i * 4) + 2] = (short)item["row"];
                srcStream[(i * 4) + 3] = (short)item["cell"];
                resultsArray[i] = item["result"];
                unitsArray[i] = item["unit"];
            }

            // EXECUTE THE REQUEST
            const short Flags = 0;
            this.visioApplication.ActivePage.SetResults(srcStream, unitsArray, resultsArray, Flags);

            this.shapeUpdates.Clear();
        }

        /// <inheritdoc />
        public void CreateShape(DiagramShape diagramShape)
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var newLocPinX = DiagramShape.ConvertMeasurement(diagramShape.Width() / 2);
            var newLocPinY = DiagramShape.ConvertMeasurement(diagramShape.Height() / 2);
            var newPinX = DiagramShape.ConvertMeasurement(diagramShape.LeftSide) + newLocPinX;
            var newPinY = DiagramShape.ConvertMeasurement(diagramShape.BaseSide) + newLocPinY;

            var shapeMaster = "Rectangle";

            if (diagramShape.Master is not null)
            {
                shapeMaster = diagramShape.Master;
            }

            var master = StencilCache.GetOrAdd(
                shapeMaster,
                key =>
                    {
                        // var documentStencil = this.visioApplication.ActiveDocument.Masters;
                        this.visioApplication.ActiveWindow.DockedStencils(out var stencilNames);
                        if (stencilNames is null || stencilNames.Length == 0)
                        {
                            return null;
                        }

                        foreach (var stencilName in stencilNames)
                        {
                            if (stencilName is null || stencilName.Equals(string.Empty))
                            {
                                continue;
                            }

                            var stencil = this.visioApplication.Documents[stencilName];
                            stencil.Masters.GetNames(out var masterNames);
                            if (masterNames is null || (masterNames.Length <= 0))
                            {
                                continue;
                            }

                            var result = (masterNames as string[])!.Contains(key);
                            if (result)
                            {
                                return stencil.Masters[key];
                            }
                        }

                        return null;
                    });

            if (master is null)
            {
                return;
            }

            this.toDrop.Add(new Dictionary<string, object> { { "shape", diagramShape }, { "master", master }, { "x", newPinX }, { "y", newPinY } });

            // var shape = this.visioApplication.ActivePage.Drop(master, newPinX, newPinY);
            // shape.Text = diagramShape.ShapeText;

            // diagramShape.VisioId = shape.ID;
            // diagramShape.ShapeType = ShapeType.Existing;

            // this.UpdateShape(diagramShape);
        }

        /// <inheritdoc />
        /// <exception cref="T:System.AggregateException">
        /// The exception that contains all the individual exceptions thrown on all
        /// threads.
        /// </exception>
        /// <exception cref="T:System.NullReferenceException">Shape is <see langword="null" />.</exception>
        public IEnumerable<int> GetChildren(int visioId)
        {
            var childrenIds = new List<int>();
            var parentShape = this.GetShape(visioId);

            var relation = (short)VisSpatialRelationCodes.visSpatialContain;
            var flags = (short)VisSpatialRelationFlags.visSpatialBackToFront;
            parentShape.SpatialNeighbors[relation, 0, flags].GetIDs(out var ids);

            // selection.GetIDs(out var selectionIDs);
            if (ids is null)
            {
                return childrenIds;
            }

            if (ids.Length == 0)
            {
                return childrenIds;
            }

            this.logger.LogDebug("Potential child shapes found: {CountOfSelection}", ids.Length);

            foreach (var potentialChildId in ids)
            {
                // check that immediate parent is the supplied shape.
                relation = (short)VisSpatialRelationCodes.visSpatialContainedIn;
                flags = (short)VisSpatialRelationFlags.visSpatialFrontToBack;

                var potentialChildShape = this.GetShape((int)potentialChildId);

                potentialChildShape.SpatialNeighbors[relation, 0, flags].GetIDs(out var parentIDs);

                if (parentIDs is null)
                {
                    continue;
                }

                if (parentIDs.Length <= 0)
                {
                    continue;
                }

                var primaryItemVisioId = parentIDs.GetValue(0);

                if (visioId.Equals(primaryItemVisioId))
                {
                    childrenIds.Add((int)potentialChildId);
                }
            }

            this.logger.LogDebug("Final child shapes found: {CountOfVisioIDs}", childrenIds.Count);

            return childrenIds;
        }

        /// <inheritdoc />
        public int GetPageLeftSide()
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var pageSheet = this.visioApplication.ActivePage.PageSheet;

            return DiagramShape.ConvertMeasurement(
                GetCellValue(pageSheet, VisSectionIndices.visSectionObject, VisRowIndices.visRowPrintProperties, VisCellIndices.visPrintPropertiesLeftMargin));
        }

        /// <inheritdoc />
        public int GetPageRightSide()
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var pageSheet = this.visioApplication.ActivePage.PageSheet;

            var rightMargin = GetCellValue(pageSheet, VisSectionIndices.visSectionObject, VisRowIndices.visRowPrintProperties, VisCellIndices.visPrintPropertiesRightMargin);
            var width = GetCellValue(pageSheet, VisSectionIndices.visSectionObject, VisRowIndices.visRowPage, VisCellIndices.visPageWidth);

            return DiagramShape.ConvertMeasurement(width - rightMargin);
        }

        /// <inheritdoc />
        public int GetPageTopSide()
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var pageSheet = this.visioApplication.ActivePage.PageSheet;

            var height = GetCellValue(pageSheet, VisSectionIndices.visSectionObject, VisRowIndices.visRowPage, VisCellIndices.visPageHeight);
            var topMargin = GetCellValue(pageSheet, VisSectionIndices.visSectionObject, VisRowIndices.visRowPage, VisCellIndices.visPrintPropertiesTopMargin);

            return DiagramShape.ConvertMeasurement(height - topMargin);
        }

        /// <inheritdoc />
        /// <exception cref="T:System.NullReferenceException">Shape is <see langword="null" />.</exception>
        public string GetShapeText(int visioId)
        {
            var shape = this.GetShape(visioId);
            return shape.Text;
        }

        /// <inheritdoc />
        public void Open()
        {
            try
            {
                this.logger.LogDebug("Opening connection to visio.");
                this.visioApplication = Marshal.GetActiveObject("Visio.Application") as Application ?? throw new InvalidOperationException();
                this.activePage = this.visioApplication.ActivePage;
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

            var listVisioIds = new List<int>();
            var activeWindow = this.visioApplication.ActiveWindow;
            if (activeWindow is null)
            {
                return listVisioIds.ToArray();
            }

            var selection = activeWindow.Selection;

            selection.GetIDs(out var ids);

            if (ids is null)
            {
                return listVisioIds.ToArray();
            }

            listVisioIds.AddRange(ids.Cast<int>());

            return listVisioIds.ToArray();
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

            this.shapeUpdates.Add(
                new Dictionary<string, object>
                    {
                        { "sheetID", (short)diagramShape.VisioId },
                        { "section", (short)VisSectionIndices.visSectionObject },
                        { "row", (short)VisRowIndices.visRowXFormOut },
                        { "cell", (short)VisCellIndices.visXFormWidth },
                        { "unit", VisUnitCodes.visMillimeters },
                        { "result", width },
                    });
            this.shapeUpdates.Add(
                new Dictionary<string, object>
                    {
                        { "sheetID", (short)diagramShape.VisioId },
                        { "section", (short)VisSectionIndices.visSectionObject },
                        { "row", (short)VisRowIndices.visRowXFormOut },
                        { "cell", (short)VisCellIndices.visXFormHeight },
                        { "unit", VisUnitCodes.visMillimeters },
                        { "result", height },
                    });
            this.shapeUpdates.Add(
                new Dictionary<string, object>
                    {
                        { "sheetID", (short)diagramShape.VisioId },
                        { "section", (short)VisSectionIndices.visSectionObject },
                        { "row", (short)VisRowIndices.visRowXFormOut },
                        { "cell", (short)VisCellIndices.visXFormPinX },
                        { "unit", VisUnitCodes.visMillimeters },
                        { "result", newPinX },
                    });
            this.shapeUpdates.Add(
                new Dictionary<string, object>
                    {
                        { "sheetID", (short)diagramShape.VisioId },
                        { "section", (short)VisSectionIndices.visSectionObject },
                        { "row", (short)VisRowIndices.visRowXFormOut },
                        { "cell", (short)VisCellIndices.visXFormPinY },
                        { "unit", VisUnitCodes.visMillimeters },
                        { "result", newPinY },
                    });
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
            this.visioApplication.ScreenUpdating = visualChanges ? (short)1 : (short)0;
            this.visioApplication.DeferRecalc = visualChanges ? (short)1 : (short)0;
        }

        private static double GetCellValue(IVShape shape, VisSectionIndices sectionIndex, VisRowIndices rowIndex, VisCellIndices cellIndex)
        {
            var shapeCell = shape.CellsSRC[(short)sectionIndex, (short)rowIndex, (short)cellIndex];
            return shapeCell.Result[VisUnitCodes.visMillimeters];
        }

        private Shape GetShape(int visioId)
        {
            if (this.visioApplication is null || this.activePage is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            return this.activePage.Shapes.ItemFromID[visioId];
        }
    }
}