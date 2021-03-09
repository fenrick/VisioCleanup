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
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models;
    using VisioCleanup.Core.Models.Config;

    /// <summary>Abstract implementation of common code for processing services.</summary>
    public abstract class AbstractProcessingService : IProcessingService
    {
        /// <summary>Initialises a new instance of the <see cref="AbstractProcessingService" /> class.</summary>
        /// <param name="logger">Logger.</param>
        /// <param name="options">Application configuration being passed in.</param>
        /// <param name="visioApplication">Visio Application engine.</param>
        protected AbstractProcessingService(ILogger logger, IOptions<AppConfig> options, IVisioApplication visioApplication)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.VisioApplication = visioApplication ?? throw new ArgumentNullException(nameof(visioApplication));
            this.AppConfig = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        public Collection<DiagramShape> AllShapes { get; protected set; } = new();

        /// <inheritdoc />
        public DiagramShape? MasterShape { get; protected set; }

        /// <summary>Gets application configuration.</summary>
        protected AppConfig AppConfig { get; }

        /// <summary>Gets logging environment.</summary>
        protected ILogger Logger { get; }

        /// <summary>Gets visio processing engine.</summary>
        protected IVisioApplication VisioApplication { get; }

        /// <inheritdoc />
        public async Task LayoutDataSet()
        {
            if (this.MasterShape is null)
            {
                return;
            }

            await Task.Run(
                () =>
                    {
                        var counter = 1;

                        do
                        {
                            this.Logger.LogDebug("Loop {count}", counter++);
                        }
                        while (this.MasterShape.CorrectDiagram());
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