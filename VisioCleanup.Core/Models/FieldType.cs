// -----------------------------------------------------------------------
// <copyright file="FieldType.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Models;

/// <summary>Fields within an excel data table.</summary>
internal enum FieldType
{
    /// <summary><see cref="VisioCleanup.Core.Models.FieldType.ShapeText" /> field.</summary>
    ShapeText,

    /// <summary>Sorting field.</summary>
    SortValue,

    /// <summary>ShapeType field.</summary>
    ShapeType,
}
