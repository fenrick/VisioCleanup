﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProcessingService.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace VisioCleanup.Core.Contracts;

using VisioCleanup.Core.Models;

/// <summary>Implementers store and process diagram shapes for representing in visio.</summary>
public interface IProcessingService
{
    /// <summary>Gets collection of all diagram shapes.</summary>
    /// <value>all diagram spaces.</value>
    ICollection<DiagramShape> AllShapes { get; }

    /// <summary>Gets the digram shape at the top of the tree.</summary>
    /// <value>Master shape.</value>
    DiagramShape? MasterShape { get; }

    /// <summary>Lays outs the diagramshapes based on appconfig.</summary>
    void LayoutDataSet();

    /// <summary>Draws new data set onto visio.</summary>
    void UpdateVisio();
}
