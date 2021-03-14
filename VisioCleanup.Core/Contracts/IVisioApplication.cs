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
        /// <summary>Calculate base side location.</summary>
        /// <param name="visioId">Visio shape id.</param>
        /// <returns>Base side.</returns>
        int CalculateBaseSide(int visioId);

        /// <summary>Calculate left side location.</summary>
        /// <param name="visioId">Visio shape id.</param>
        /// <returns>Left side.</returns>
        int CalculateLeftSide(int visioId);

        /// <summary>Calculate right side location.</summary>
        /// <param name="visioId">Visio shape id.</param>
        /// <returns>Right side.</returns>
        int CalculateRightSide(int visioId);

        /// <summary>Calculate top side location.</summary>
        /// <param name="visioId">Visio shape id.</param>
        /// <returns>Top side.</returns>
        int CalculateTopSide(int visioId);

        /// <summary>Close visio session and shutdown.</summary>
        void Close();

        /// <summary>Create new shape on Visio diagram.</summary>
        /// <param name="diagramShape">Shape to be created.</param>
        void CreateShape(DiagramShape diagramShape);

        /// <summary>Return an array of shapeIDs for children of the supplied shape id.</summary>
        /// <param name="visioId">Shape ID of the parent shape.</param>
        /// <returns>array of shape ids for children.</returns>
        IEnumerable<int> GetChildren(int visioId);

        /// <summary>Calculate the left hand side of the page.</summary>
        /// <returns>Integer representing it.</returns>
        int GetPageLeftSide();

        /// <summary>Calculate the right hand side of the page.</summary>
        /// <returns>Integer representing it.</returns>
        int GetPageRightSide();

        /// <summary>Calculate the top side of the page.</summary>
        /// <returns>Integer representing it.</returns>
        int GetPageTopSide();

        /// <summary>Obtains the current shape text for a shape.</summary>
        /// <param name="visioId">shape id.</param>
        /// <returns>shape text.</returns>
        string GetShapeText(int visioId);

        /// <summary>Open visio session.</summary>
        void Open();

        /// <summary>Return an array of visio ids that have been selected.</summary>
        /// <returns>Array of visio ids.</returns>
        int[] Selection();

        /// <summary>If shape exists on Visio diagram, move to foreground.</summary>
        /// <param name="diagramShape">Shape to be moved.</param>
        void SetForeground(DiagramShape diagramShape);

        /// <summary>Update shape on Visio diagram, moving, resizing, etc.</summary>
        /// <param name="diagramShape">Shape to be updated.</param>
        void UpdateShape(DiagramShape diagramShape);

        /// <summary>Change visio updating diagram.</summary>
        /// <param name="visualChanges">Value to change.</param>
        void VisualChanges(bool visualChanges);

        void CompleteDrops();

        void CompleteUpdates();
    }
}