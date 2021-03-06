// -----------------------------------------------------------------------
// <copyright file="AbstractProcessingService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;

    /// <summary>Abstract implementation of common code for processing services.</summary>
    public abstract class AbstractProcessingService : IProcessingService
    {
        /// <summary>Initialises a new instance of the <see cref="AbstractProcessingService" /> class.</summary>
        /// <param name="logger">Logger.</param>
        /// <param name="visioApplication">Visio Application engine.</param>
        protected AbstractProcessingService(ILogger logger, IVisioApplication visioApplication)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.VisioApplication = visioApplication ?? throw new ArgumentNullException(nameof(visioApplication));
        }

        /// <inheritdoc />
        public Collection<DiagramShape> AllShapes { get; protected set; } = new();

        /// <inheritdoc />
        public DiagramShape? MasterShape { get; protected set; }

        /// <summary>Gets logging environment.</summary>
        protected ILogger Logger { get; }

        /// <summary>Gets visio processing engine.</summary>
        protected IVisioApplication VisioApplication { get; }

        /// <inheritdoc />
        public async Task LayoutDataSet()
        {
            await Task.Run(
                () =>
                    {
                        bool result;

                        do
                        {
                            result = false;

                            // step 1 - resize shapes without children
                            foreach (var diagramShape in this.AllShapes.Where(diagramShape => !diagramShape.HasChildren()))
                            {
                                if (diagramShape.ResizeShape())
                                {
                                    result = true;
                                }
                            }

                            // step 2 - move top left child shape into position
                            foreach (var diagramShape in this.AllShapes.Where(
                                diagramShape => (diagramShape.ShapeType != ShapeType.FakeShape) && diagramShape.Left is null && diagramShape.Above is null))
                            {
                                if (diagramShape.AlignToParent())
                                {
                                    result = true;
                                }
                            }

                            continue;

                            // step 3 - fix spacing for shapes
                            foreach (var diagramShape in this.AllShapes)
                            {
                                if (diagramShape.FixSpacing())
                                {
                                    result = true;
                                }
                            }

                            // step 4 - align shapes
                            foreach (var diagramShape in this.AllShapes.Where(diagramShape => (diagramShape.ShapeType != ShapeType.FakeShape)))
                            {
                                if (diagramShape.FixAlignment())
                                {
                                    result = true;
                                }
                            }
                        }
                        while (false);
                        
                        // step 5 - fix size of shapes (including those with children)
                        foreach (var diagramShape in this.AllShapes)
                        {
                            if (diagramShape.ResizeShape())
                            {
                                result = true;
                            }
                        }

                        return;
                    });
        }

        /// <inheritdoc />
        public async Task UpdateVisio()
        {
            if (this.MasterShape is null)
            {
                throw new ArgumentNullException(nameof(this.MasterShape));
            }

            await Task.Run(
                () =>
                    {
                        try
                        {
                            this.VisioApplication.Open();

                            // update each shape
                            foreach (var diagramShape in this.AllShapes)
                            {
                                switch (diagramShape.ShapeType)
                                {
                                    case ShapeType.NewShape:
                                        this.Logger.LogDebug("Dropping new shape: {Shape}", diagramShape);
                                        this.VisioApplication.CreateShape(diagramShape);
                                        break;
                                    case ShapeType.Existing:
                                        this.Logger.LogDebug("Checking shape: {Shape}", diagramShape);
                                        this.VisioApplication.UpdateShape(diagramShape);
                                        break;
                                    case ShapeType.FakeShape:
                                        // we don't draw this!
                                        this.Logger.LogDebug("Skipping fake shape: {Shape}", diagramShape);
                                        break;
                                    default:
                                        throw new InvalidOperationException("ShapeType not matched.");
                                }
                            }

                            // iterate down the tree setting shapes to the foreground
                            this.VisioApplication.SetForeground(this.MasterShape);
                        }
                        finally
                        {
                            this.VisioApplication.Close();
                        }
                    });
        }
    }
}