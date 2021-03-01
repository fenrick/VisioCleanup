// -----------------------------------------------------------------------
// <copyright file="AbstractProcessingService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;

    /// <summary>
    /// Abstract implementation of common code for processing services.
    /// </summary>
    public abstract class AbstractProcessingService : IProcessingService
    {
        protected ILogger logger;

        protected IVisioApplication visioApplication;

        /// <inheritdoc />
        public Collection<DiagramShape> AllShapes { get; protected set; } = new();

        /// <inheritdoc />
        public DiagramShape? MasterShape { get; protected set; }

        /// <inheritdoc />
        public async Task LayoutDataSet()
        {
            await Task.Run(
                () =>
                    {
                        // step 1 - resize shapes without children
                        foreach (var diagramShape in this.AllShapes.Where(diagramShape => !diagramShape.HasChildren()))
                        {
                            diagramShape.ResizeShape();
                        }

                        // step 2 - fix spacing for shapes without children

                        // step 3 - fix size of shapes with children

                        // step 4 - fix spacing for shapes with children

                        // step 5
                    });
        }

        /// <inheritdoc />
        public async Task UpdateVisio()
        {
            await Task.Run(
                () =>
                    {
                        try
                        {
                            this.visioApplication.Open();

                            // update each shape
                            foreach (var diagramShape in this.AllShapes)
                            {
                                switch (diagramShape.ShapeType)
                                {
                                    case ShapeType.NewShape:
                                        this.logger.LogDebug("Dropping new shape: {Shape}", diagramShape);
                                        this.visioApplication.CreateShape(diagramShape);
                                        goto case ShapeType.Existing;
                                    case ShapeType.Existing:
                                        this.logger.LogDebug("Checking shape: {Shape}", diagramShape);
                                        this.visioApplication.UpdateShape(diagramShape);
                                        break;
                                    case ShapeType.FakeShape:
                                        // we don't draw this!
                                        this.logger.LogDebug("Skipping fake shape: {Shape}", diagramShape);
                                        break;
                                }
                            }

                            // iterate down the tree setting shapes to the foreground
                            this.visioApplication.SetForeground(this.MasterShape);
                        }
                        finally
                        {
                            this.visioApplication.Close();
                        }
                    });
        }
    }
}