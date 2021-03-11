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

                        // do
                        {
                            // initiate a base layout.
                            this.MasterShape.CorrectDiagram();

                            // look for overruns.
                            this.ChopDown(this.MasterShape);
                        }
                        // while (this.MasterShape.Width() > this.maxRight);
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
            if (diagramShape.Width() <= this.maxRight)
            {
                return;
            }

            // find overlapping child
            var overlapChild = diagramShape.Children.FirstOrDefault(shape => (shape.LeftSide < this.maxRight) && (shape.RightSide >= this.maxRight));

            // if has children, then loop.
            if (overlapChild is not null && overlapChild.Children.Count > 0)
            {
                this.ChopDown(overlapChild);
                return;
            }

            // if no shape to left, then move parent.
            if (overlapChild?.Left is null)
            {
                overlapChild = diagramShape;
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

            // resize parent
            overlapChild.ParentShape!.CorrectDiagram();

            // find sides.
            overlapChild.ParentShape!.FindNeighbours();

            // loop parent again
            this.ChopDown(overlapChild.ParentShape);
        }

        private void SortChildren(DiagramShape diagramShape)
        {
            foreach (var child in diagramShape.Children)
            {
                this.SortChildren(child);
            }

            for (var i = 0; i < diagramShape.Children.Count; i++)
            {
                if (diagramShape.Children.Count > (i + 1))
                {
                    diagramShape.Children[i].Right = diagramShape.Children[i + 1];
                }
            }
        }
    }
}