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

        private Page? activePage;

        private Application? visioApplication;

        /// <summary>Initialises a new instance of the <see cref="VisioApplication" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="options">Application configuration.</param>
        public VisioApplication(ILogger<VisioApplication> logger, IOptions<AppConfig> options)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.appConfig = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

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

            var stencil = this.visioApplication.Documents["Basic.vss"];
            var stencilName = "Ellipse";

            // if (diagramShape.Stencil is not null)
            // {
            // stencilName = diagramShape.Stencil;
            // }
            var shape = this.visioApplication.ActivePage.Drop(stencil.Masters[stencilName], newPinX, newPinY);

            diagramShape.VisioId = shape.ID;
            diagramShape.ShapeType = ShapeType.Existing;

            this.UpdateShape(diagramShape);
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

            this.logger.LogDebug("Potential child shapes found: {CountOfSelection}", ids.Length);

            // selection.GetIDs(out var selectionIDs);
            if (ids.Length == 0)
            {
                return childrenIds;
            }

            foreach (var potentialChildId in ids)
            {
                // check that immediate parent is the supplied shape.
                relation = (short)VisSpatialRelationCodes.visSpatialContainedIn;
                flags = (short)VisSpatialRelationFlags.visSpatialFrontToBack;

                var potentialChildShape = this.GetShape((int)potentialChildId);

                potentialChildShape.SpatialNeighbors[relation, 0, flags].GetIDs(out var parentIDs);

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

            foreach (var child in diagramShape.Children)
            {
                this.SetForeground(child);
            }
        }

        /// <inheritdoc />
        public void UpdateShape(DiagramShape diagramShape)
        {
            if (this.visioApplication is null)
            {
                throw new InvalidOperationException("System not initialised.");
            }

            var shape = this.GetShape(diagramShape.VisioId);

            this.ChangeShape(VisioChanges(diagramShape), shape);
        }

        private static double GetCellValue(IVShape shape, VisSectionIndices sectionIndex, VisRowIndices rowIndex, VisCellIndices cellIndex)
        {
            var shapeCell = shape.CellsSRC[(short)sectionIndex, (short)rowIndex, (short)cellIndex];
            return shapeCell.Result[VisUnitCodes.visMillimeters];
        }

        private static Dictionary<string, object>[] VisioChanges(DiagramShape diagramShape)
        {
            var newLocPinX = DiagramShape.ConvertMeasurement(diagramShape.Width() / 2);
            var newLocPinY = DiagramShape.ConvertMeasurement(diagramShape.Height() / 2);
            var newPinX = DiagramShape.ConvertMeasurement(diagramShape.LeftSide) + newLocPinX;
            var newPinY = DiagramShape.ConvertMeasurement(diagramShape.BaseSide) + newLocPinY;

            var visioChanges = new List<Dictionary<string, object>>();

            var width = DiagramShape.ConvertMeasurement(diagramShape.Width());
            var height = DiagramShape.ConvertMeasurement(diagramShape.Height());

            visioChanges.Add(
                new Dictionary<string, object>
                    {
                        { "section", (short)VisSectionIndices.visSectionObject },
                        { "row", (short)VisRowIndices.visRowXFormOut },
                        { "cell", (short)VisCellIndices.visXFormWidth },
                        { "unit", VisUnitCodes.visMillimeters },
                        { "result", width },
                    });
            visioChanges.Add(
                new Dictionary<string, object>
                    {
                        { "section", (short)VisSectionIndices.visSectionObject },
                        { "row", (short)VisRowIndices.visRowXFormOut },
                        { "cell", (short)VisCellIndices.visXFormHeight },
                        { "unit", VisUnitCodes.visMillimeters },
                        { "result", height },
                    });
            visioChanges.Add(
                new Dictionary<string, object>
                    {
                        { "section", (short)VisSectionIndices.visSectionObject },
                        { "row", (short)VisRowIndices.visRowXFormOut },
                        { "cell", (short)VisCellIndices.visXFormPinX },
                        { "unit", VisUnitCodes.visMillimeters },
                        { "result", newPinX },
                    });
            visioChanges.Add(
                new Dictionary<string, object>
                    {
                        { "section", (short)VisSectionIndices.visSectionObject },
                        { "row", (short)VisRowIndices.visRowXFormOut },
                        { "cell", (short)VisCellIndices.visXFormPinY },
                        { "unit", VisUnitCodes.visMillimeters },
                        { "result", newPinY },
                    });

            return visioChanges.ToArray();
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
            var unitsArray = new object[items.Count];
            var resultsArray = new object[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                srcStream[(i * 3) + 0] = (short)item["section"];
                srcStream[(i * 3) + 1] = (short)item["row"];
                srcStream[(i * 3) + 2] = (short)item["cell"];
                resultsArray[i] = item["result"];
                unitsArray[i] = item["unit"];
            }

            // EXECUTE THE REQUEST
            const short Flags = 0;
            shape.SetResults(srcStream, unitsArray, resultsArray, Flags);

            // shape.SetFormulas(srcStream, resultsArray, Flags);
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