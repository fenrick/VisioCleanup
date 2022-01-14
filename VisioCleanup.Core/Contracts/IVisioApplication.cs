// -----------------------------------------------------------------------
// <copyright file="IVisioApplication.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Contracts;

using VisioCleanup.Core.Models;

/// <summary>Handle creation and management of visio.</summary>
public interface IVisioApplication
{
    /// <summary>Gets the left hand side of the page.</summary>
    /// <value>position of left hand side of page.</value>
    int PageLeftSide { get; }

    /// <summary>Gets the right hand side of the page.</summary>
    /// <value>position of right hand side of page.</value>
    int PageRightSide { get; }

    /// <summary>Gets the top side of the page.</summary>
    /// <value>position of top of the page.</value>
    int PageTopSide { get; }

    /// <summary><see cref="Close" /> visio session and shutdown.</summary>
    void Close();

    /// <summary>Create new shape on Visio diagram.</summary>
    /// <param name="diagramShape">Shape to be created.</param>
    void CreateShape(DiagramShape diagramShape);

    /// <summary><see cref="Open" /> visio session.</summary>
    void Open();

    /// <summary>Retrieve hierarchy of shapes from Visio.</summary>
    /// <returns>Enumerable of DiagramShapes.</returns>
    IEnumerable<DiagramShape> RetrieveShapes();

    /// <summary>Update shape on Visio diagram, moving, resizing, etc.</summary>
    /// <param name="diagramShape">Shape to be updated.</param>
    void UpdateShape(DiagramShape diagramShape);

    /// <summary>Change visio updating diagram.</summary>
    /// <param name="state">Value to change.</param>
    void VisualChanges(bool state);
}
