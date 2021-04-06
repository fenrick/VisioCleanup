// -----------------------------------------------------------------------
// <copyright file="IVisioApplication.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Contracts
{
    using System.Collections.Generic;

    using VisioCleanup.Core.Models;

    /// <summary>Handle creation and management of visio.</summary>
    public interface IVisioApplication
    {
        /// <summary><see cref="Close" /> visio session and shutdown.</summary>
        void Close();

        /// <summary>Execute the dropping of shapes onto the visio page.</summary>
        void CompleteDrops();

        /// <summary>Execute the updating of shapes onto the visio page.</summary>
        void CompleteUpdates();

        /// <summary>Create new shape on Visio diagram.</summary>
        /// <param name="diagramShape">Shape to be created.</param>
        void CreateShape(DiagramShape diagramShape);

        /// <summary>Calculate the left hand side of the page.</summary>
        /// <returns>Integer representing it.</returns>
        int GetPageLeftSide();

        /// <summary>Calculate the right hand side of the page.</summary>
        /// <returns>Integer representing it.</returns>
        int GetPageRightSide();

        /// <summary>Calculate the top side of the page.</summary>
        /// <returns>Integer representing it.</returns>
        int GetPageTopSide();

        /// <summary><see cref="Open" /> visio session.</summary>
        void Open();

        /// <summary>Retrieve hierachy of shapes from Visio.</summary>
        /// <returns>Enumerable of DiagramShapes.</returns>
        IEnumerable<DiagramShape> RetrieveShapes();

        /// <summary>If shape exists on Visio diagram, move to foreground.</summary>
        /// <param name="diagramShape">Shape to be moved.</param>
        void SetForeground(DiagramShape diagramShape);

        /// <summary>Update shape on Visio diagram, moving, resizing, etc.</summary>
        /// <param name="diagramShape">Shape to be updated.</param>
        void UpdateShape(DiagramShape diagramShape);

        /// <summary>Change visio updating diagram.</summary>
        /// <param name="visualChanges">Value to change.</param>
        void VisualChanges(bool visualChanges);
    }
}