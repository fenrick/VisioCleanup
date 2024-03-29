﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppConfig.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace VisioCleanup.Core.Models.Config;

/// <summary>The app config.</summary>
public class AppConfig
{
    /// <summary>Gets or sets the bottom.</summary>
    /// <value>The bottom.</value>
    public double Base
    {
        get;
        set;
    }

    /// <summary>Gets or sets the database catalog.</summary>
    /// <value>Database catalog.</value>
    public string? DatabaseCatalog
    {
        get;
        set;
    }

    /// <summary>Gets or sets the list of database queries.</summary>
    /// <value>Database queries.</value>
    public IReadOnlyCollection<Dictionary<string, string>>? DatabaseQueries
    {
        get;
        set;
    }

    /// <summary>Gets or sets the database server name.</summary>
    /// <value>Database server.</value>
    public string? DatabaseServer
    {
        get;
        set;
    }

    /// <summary>Gets or sets the field label format.</summary>
    /// <value>The field label format.</value>
    public string? FieldLabelFormat
    {
        get;
        set;
    }

    /// <summary>Gets or sets the header height.</summary>
    /// <value>The header height.</value>
    public double HeaderHeight
    {
        get;
        set;
    }

    /// <summary>Gets or sets the height.</summary>
    /// <value>The height.</value>
    public double Height
    {
        get;
        set;
    }

    /// <summary>Gets or sets the horizontal spacing.</summary>
    /// <value>The horizontal spacing.</value>
    public double HorizontalSpacing
    {
        get;
        set;
    }

    /// <summary>Gets or sets the left.</summary>
    /// <value>The left.</value>
    public double Left
    {
        get;
        set;
    }

    /// <summary>Gets or sets the max number of box lines.</summary>
    /// <value>maximum box lines.</value>
    public double? MaxBoxLines
    {
        get;
        set;
    }

    /// <summary>Gets or sets the right.</summary>
    /// <value>The right.</value>
    public double Right
    {
        get;
        set;
    }

    /// <summary>Gets or sets the shape type label format.</summary>
    /// <value>The shape type label format.</value>
    public string? ShapeTypeLabelFormat
    {
        get;
        set;
    }

    /// <summary>Gets or sets the side panel width.</summary>
    /// <value>The side panel width.</value>
    public double SidePanelWidth
    {
        get;
        set;
    }

    /// <summary>Gets or sets the sort field label format.</summary>
    /// <value>The sort field label format.</value>
    public string? SortFieldLabelFormat
    {
        get;
        set;
    }

    /// <summary>Gets or sets the top.</summary>
    /// <value>The top.</value>
    public double Top
    {
        get;
        set;
    }

    /// <summary>Gets or sets the vertical spacing.</summary>
    /// <value>The vertical spacing.</value>
    public double VerticalSpacing
    {
        get;
        set;
    }

    /// <summary>Gets or sets the width.</summary>
    /// <value>The width.</value>
    public double Width
    {
        get;
        set;
    }
}
