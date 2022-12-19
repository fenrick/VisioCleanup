// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractDataSource.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace VisioCleanup.Core.Services;

using System.Globalization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using VisioCleanup.Core.Models;
using VisioCleanup.Core.Models.Config;

/// <summary>Abstract data source.</summary>
public class AbstractDataSource
{
    /// <summary>Initialises a new instance of the <see cref="AbstractDataSource"/> class.</summary>
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

    /// <summary>Gets logging environment.</summary>
    /// <value>Logger.</value>
    internal ILogger Logger { get; }

    /// <summary>Gets application configuration.</summary>
    /// <value>Configuration.</value>
    protected AppConfig AppConfig { get; }

    /// <summary>Create a new shape object.</summary>
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

        var shapeType = rowResult[FieldType.ShapeType];
        var shapeText = rowResult[FieldType.ShapeText];
        var sortValue = rowResult[FieldType.SortValue];
        var calculatedSortValue = false;

        if (string.IsNullOrWhiteSpace(sortValue))
        {
            sortValue = shapeText;
            calculatedSortValue = true;
        }

        if (string.IsNullOrWhiteSpace(shapeText))
        {
            return null;
        }

        var shapeIdentifier = string.Format(CultureInfo.CurrentCulture, "{0} {1}:{2}", previousShape?.ShapeIdentifier, shapeText, shapeType).Trim();

        if (!allShapes.ContainsKey(shapeIdentifier))
        {
            this.Logger.LogDebug("Creating shape for: {ShapeText}", shapeText);
            allShapes.Add(
                shapeIdentifier,
                new DiagramShape(0)
                {
                    ShapeText = shapeText,
                    ShapeType = ShapeType.NewShape,
                    SortValue = sortValue,
                    Master = shapeType,
                    ShapeIdentifier = shapeIdentifier,
                    HasCalculatedSortValue = calculatedSortValue,
                });
        }

        var shape = allShapes[shapeIdentifier];

        previousShape?.AddChildShape(shape);
        return shape;
    }
}
