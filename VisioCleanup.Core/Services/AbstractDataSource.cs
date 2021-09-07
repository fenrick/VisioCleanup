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
    using System.Globalization;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using VisioCleanup.Core.Models;
    using VisioCleanup.Core.Models.Config;
    using VisioCleanup.Core.Resources;

    /// <summary>
    /// Abstract data source.
    /// </summary>
    public class AbstractDataSource
    {
        /// <summary>Initialises a new instance of the <see cref="AbstractDataSource" /> class.</summary>
        /// <param name="logger">Logging instance.</param>
        /// <param name="options">Application configuration settings.</param>
        protected AbstractDataSource(ILogger logger, IOptions<AppConfig> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.AppConfig = options.Value;
        }

        /// <summary>Gets application configuration.</summary>
        /// <value>Configuration.</value>
        protected AppConfig AppConfig { get; }

        /// <summary>Gets logging environment.</summary>
        /// <value>Logger.</value>
        protected ILogger Logger { get; }

        /// <summary>
        /// Create a new shape object.
        /// </summary>
        /// <param name="rowResult">Data set row.</param>
        /// <param name="allShapes">set of all shapes.</param>
        /// <param name="previousShape">Parent shape.</param>
        /// <returns>New shape object.</returns>
        protected DiagramShape? CreateShape(IReadOnlyDictionary<FieldType, string> rowResult, IDictionary<string, DiagramShape> allShapes, DiagramShape? previousShape)
        {
            if (rowResult == null)
            {
                throw new ArgumentNullException(nameof(rowResult));
            }

            if (allShapes == null)
            {
                throw new ArgumentNullException(nameof(allShapes));
            }

            var shapeType = rowResult.ContainsKey(FieldType.ShapeType) ? rowResult[FieldType.ShapeType] : string.Empty;
            var sortValue = rowResult.ContainsKey(FieldType.SortValue) ? rowResult[FieldType.SortValue] : null;
            var shapeText = rowResult.ContainsKey(FieldType.ShapeText) ? rowResult[FieldType.ShapeText] : string.Empty;

            if (string.IsNullOrEmpty(shapeText))
            {
                return previousShape;
            }

            var shapeIdentifier = string.Format(CultureInfo.CurrentCulture, en_AU.ShapeIdentifierFormat, previousShape?.ShapeIdentifier, shapeText, shapeType).Trim();

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
