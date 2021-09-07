// -----------------------------------------------------------------------
// <copyright file="AbstractDataSource.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Services
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using VisioCleanup.Core.Models;
    using VisioCleanup.Core.Models.Config;
    using VisioCleanup.Core.Resources;

    public abstract class AbstractDataSource
    {
        /// <summary>Initialises a new instance of the <see cref="AbstractDataSource" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="options">Application configuration settings.</param>
        public AbstractDataSource(ILogger logger, IOptions<AppConfig> options)
        {
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.AppConfig = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>Gets application configuration.</summary>
        /// <value>Configuration.</value>
        protected AppConfig AppConfig { get; }

        /// <summary>Gets logging environment.</summary>
        /// <value>Logger.</value>
        protected ILogger Logger { get; }

        protected DiagramShape? CreateShape(IReadOnlyDictionary<FieldType, string> rowResult, IDictionary<string, DiagramShape> allShapes, DiagramShape? previousShape)
        {
            var shapeType = rowResult.ContainsKey(FieldType.ShapeType) ? rowResult[FieldType.ShapeType] : string.Empty;
            var sortValue = rowResult.ContainsKey(FieldType.SortValue) ? rowResult[FieldType.SortValue] : null;
            var shapeText = rowResult.ContainsKey(FieldType.ShapeText) ? rowResult[FieldType.ShapeText] : string.Empty;

            if (string.IsNullOrEmpty(shapeText))
            {
                return previousShape;
            }

            var shapeIdentifier = string.Format(en_AU.ShapeIdentifierFormat, previousShape?.ShapeIdentifier, shapeText, shapeType).Trim();

            if (!allShapes.ContainsKey(shapeIdentifier))
            {
                this.Logger.LogDebug(en_AU.ExcelApplication_CreateShape_Creating_shape_for___ShapeText_, shapeText);
                allShapes.Add(
                    shapeIdentifier,
                    new DiagramShape(0)
                        {
                            ShapeText = shapeText,
                            ShapeType = ShapeType.NewShape,
                            SortValue = sortValue,
                            Master = shapeType,
                            ShapeIdentifier = shapeIdentifier,
                        });
            }

            var shape = allShapes[shapeIdentifier];

            previousShape?.AddChildShape(shape);
            return shape;
        }
    }
}
