﻿// -----------------------------------------------------------------------
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

        private int convertedAppConfigRight;

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
                        int maxRight;

                        try
                        {
                            this.VisioApplication.Open();

                            maxRight = this.VisioApplication.GetPageRightSide() - DiagramShape.ConvertMeasurement(this.AppConfig.SidePanelWidth);
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
                        this.convertedAppConfigRight = DiagramShape.ConvertMeasurement(this.AppConfig.Right);

                        // look for overruns.
                        this.ChopDown(this.MasterShape, maxRight);
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

                            this.MasterShape = new DiagramShape(0)
                                                   {
                                                       ShapeText = "FAKE MASTER",
                                                       ShapeType = ShapeType.FakeShape,
                                                       LeftSide = this.VisioApplication.GetPageLeftSide(),
                                                       TopSide = this.VisioApplication.GetPageTopSide() - DiagramShape.ConvertMeasurement(this.AppConfig.HeaderHeight),
                                                   };

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

        private void ChopDown(DiagramShape diagramShape, int maxRight)
        {
            this.Logger.LogDebug("Checking {Shape} for fit.", diagramShape);

            // check shape is too wide.
            if (diagramShape.RightSide < maxRight)
            {
                this.Logger.LogDebug("We fit in the space supplied.");
                return;
            }

            // remove internal padding.
            var internalMaxRight = maxRight - this.convertedAppConfigRight;
            this.Logger.LogDebug("Internal max right: {MaxRight}", internalMaxRight);

            // find overlapping child
            var overlapChild = diagramShape.Children.OrderBy(childShape => childShape.LeftSide).FirstOrDefault(childShape => childShape.RightSide >= internalMaxRight);

            // do we have an child to move
            if (overlapChild is null)
            {
                // no overlap child - needs more work.
                throw new InvalidOperationException("this shouldn't happen.");
            }

            this.Logger.LogDebug("Found overlapping shape: {Shape}", overlapChild);

            // does this have children?
            if (overlapChild.Children.Count > 0)
            {
                do
                {
                    this.Logger.LogDebug("Can we chop this shape up?");
                    this.ChopDown(overlapChild, internalMaxRight);

                    this.Logger.LogDebug("Correcting {Shape}", diagramShape);
                    diagramShape.CorrectDiagram();

                    this.Logger.LogDebug("Checking neighbours for {Shape}", diagramShape);
                    diagramShape.FindNeighbours();
                }
                while (overlapChild.RightSide >= internalMaxRight);

                // process this shape again, just incase.
                this.ChopDown(diagramShape, maxRight);

                return;
            }

            while (overlapChild.Left is null)
            {
                if (overlapChild.ParentShape is null)
                {
                    // can't fix the top shape.
                    return;
                }

                // bounce up a shape.
                overlapChild = overlapChild.ParentShape;

                // increase max right accordingly.
                internalMaxRight += this.convertedAppConfigRight;
            }

            // we shouldn't have something above.
            if (overlapChild.Above is not null)
            {
                throw new InvalidOperationException("this shouldn't happen.");
            }

            // look for new above shape.
            var shape = overlapChild;
            while (shape.Left is not null)
            {
                // shape to the left
                shape = shape.Left;
            }

            // can't overlap with ourselves.
            if (shape == overlapChild)
            {
                return;
            }

            // wipe current relationships
            overlapChild.Left.Right = null;

            // set new above (indirectly)
            shape.Below = overlapChild;

            // fix other relationships.
            diagramShape.FindNeighbours();
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