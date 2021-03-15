// -----------------------------------------------------------------------
// <copyright file="ExcelService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;
    using VisioCleanup.Core.Models.Config;

    /// <summary>The excel service.</summary>
    public class ExcelService : AbstractProcessingService, IExcelService
    {
        private readonly IExcelApplication excelApplication;

        private int maxRight;

        /// <summary>Initialises a new instance of the <see cref="ExcelService" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="visioApplication">Visio application handler.</param>
        /// <param name="excelApplication">Excel application handler.</param>
        /// <param name="options">Application configuration being passed in.</param>
        public ExcelService(ILogger<ExcelService> logger, IVisioApplication visioApplication, IExcelApplication excelApplication, IOptions<AppConfig> options)
            : base(logger, options, visioApplication) =>
            this.excelApplication = excelApplication ?? throw new ArgumentNullException(nameof(excelApplication));

        /// <inheritdoc />
        public new async Task LayoutDataSet()
        {
            await Task.Run(
                () =>
                    {
                        try
                        {
                            this.VisioApplication.Open();

                            this.maxRight = this.VisioApplication.GetPageRightSide() - DiagramShape.ConvertMeasurement(this.AppConfig.SidePanelWidth);
                        }
                        finally
                        {
                            this.VisioApplication.Close();
                        }

                        if (this.MasterShape is null)
                        {
                            throw new InvalidOperationException("Need to load shapes first.");
                        }

                        // initiate a base layout.
                        this.MasterShape.CorrectDiagram();

                        // look for overruns.
                        this.ChopDown(this.MasterShape);
                    });
        }

        /// <inheritdoc />
        public async Task ProcessDataSet()
        {
            await Task.Run(
                () =>
                    {
                        try
                        {
                            // open connection to excel
                            this.excelApplication.Open();

                            // open connection to visio
                            this.VisioApplication.Open();

                            // retrieve records
                            this.AllShapes = this.excelApplication.RetrieveRecords();

                            if (this.AllShapes.Count == 0)
                            {
                                return;
                            }

                            this.MasterShape = new DiagramShape(0) { ShapeText = "FAKE MASTER", ShapeType = ShapeType.FakeShape };

                            this.MasterShape.LeftSide = this.VisioApplication.GetPageLeftSide();
                            this.MasterShape.TopSide = this.VisioApplication.GetPageTopSide() - DiagramShape.ConvertMeasurement(this.AppConfig.HeaderHeight);

                            foreach (var shape in this.AllShapes.Where(shape => !shape.HasParent()))
                            {
                                this.MasterShape.AddChildShape(shape);
                            }

                            // need to set children relationships.
                            this.SortChildren(this.MasterShape);

                            this.AllShapes.Add(this.MasterShape);
                        }
                        finally
                        {
                            this.VisioApplication.Close();

                            this.excelApplication.Close();
                        }
                    });
        }

        private void ChopDown(DiagramShape diagramShape)
        {
            // check shape is too wide.
            if (diagramShape.RightSide < (this.maxRight + DiagramShape.ConvertMeasurement(this.AppConfig.Right)))
            {
                return;
            }

            // find overlapping child
            DiagramShape? overlapChild = null;
            foreach (var shape1 in diagramShape.Children.OrderBy(shape => shape.LeftSide))
            {
                if (shape1.RightSide >= this.maxRight)
                {
                    overlapChild = shape1;
                    break;
                }
            }

            if ((this.maxRight - diagramShape.LeftSide) <= (DiagramShape.ConvertMeasurement(this.AppConfig.Width) * 2))
            {
                if (diagramShape.Left is not null)
                {
                    // set to null to be processed later.
                    overlapChild = null;
                }
                else
                {
                    return;
                }
            }

            // if has children, then loop.
            if (overlapChild is not null && (overlapChild.Children.Count > 0))
            {
                do
                {
                    // chop down.
                    this.ChopDown(overlapChild);

                    // correct diagram.
                    diagramShape.CorrectDiagram();

                    // find sides.
                    diagramShape.FindNeighbours();
                }
                while (overlapChild.RightSide >= this.maxRight);

                this.ChopDown(diagramShape);
                return;
            }

            if (overlapChild is null)
            {
                overlapChild = diagramShape;
            }

            // if no shape to left, then move parent.
            while (overlapChild!.Left is null)
            {
                overlapChild = overlapChild.ParentShape;

                // if we've run out of parents, then we're probably as good as can be.
                if (overlapChild is null)
                {
                    return;
                }
            }

            // find far left shape.
            var shape = overlapChild;
            while (shape.Left is not null)
            {
                shape = shape.Left;
            }

            if (shape == overlapChild)
            {
                return;
            }

            // do we have nothing above us?
            if (overlapChild.Above is not null)
            {
                throw new InvalidOperationException("this shouldn't happen.");
            }

            // clear one side.
            if (overlapChild.Left is not null)
            {
                overlapChild.Left.Right = null;
            }

            // set above
            shape.Below = overlapChild;
        }

        private void SortChildren(DiagramShape diagramShape)
        {
            foreach (var child in diagramShape.Children)
            {
                if (child.Children.Count > 0)
                {
                    this.SortChildren(child);
                }
            }

            var children = diagramShape.Children.OrderByDescending(shape => shape.TotalChildrenCount()).ToList();

            for (var i = 0; i < children.Count; i++)
            {
                if (children.Count > (i + 1))
                {
                    children[i].Right = children[i + 1];
                }
            }
        }
    }
}