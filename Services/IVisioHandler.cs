// -----------------------------------------------------------------------
// <copyright file="IVisioHandler.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Services
{
    using System.Collections.Generic;

    using VisioCleanup.Objects;

    /// <summary>
    ///     Manipulate visio objects.
    /// </summary>
    internal interface IVisioHandler
    {
        /// <summary>
        ///     Calculate the corners of a shape.
        /// </summary>
        /// <param name="shapeId">Shape ID for the shape.</param>
        /// <returns>Corners of a shape.</returns>
        Corners CalculateCorners(int shapeId);

        /// <summary>
        ///     Close visio session and shutdown.
        /// </summary>
        void Close();

        /// <summary>
        ///     Return an array of shapeIDs for children of the supplied shape id.
        /// </summary>
        /// <param name="parentShapeId">Shape ID of the parent shape.</param>
        /// <returns>array of shape ids for children.</returns>
        IEnumerable<int> GetChildren(int parentShapeId);

        /// <summary>
        ///     Get size of the page.
        /// </summary>
        /// <param name="headerHeight">Header size.</param>
        /// <param name="sidePanelWidth">Side panel size.</param>
        /// <returns>Corners size.</returns>
        Corners GetPageSize(double headerHeight, double sidePanelWidth);

        /// <summary>
        ///     Obtains the current shape text for a shape.
        /// </summary>
        /// <param name="shapeId">shape id.</param>
        /// <returns>shape text.</returns>
        string GetShapeText(int shapeId);

        /// <summary>
        ///     Open visio session.
        /// </summary>
        void Open();

        /// <summary>
        ///     Find shapes in visio diagram and change their location and size to match diagramShapes.
        /// </summary>
        /// <param name="diagramShape">Internal structure for modelling visio shapes.</param>
        void ReDrawShapes(DiagramShape diagramShape);

        /// <summary>
        ///     Return an array of visio ids that have been selected.
        /// </summary>
        /// <returns>Array of visio ids.</returns>
        int[] Selection();

        /// <summary>
        ///     Change visio updating diagram.
        /// </summary>
        /// <param name="visualChanges">Value to change.</param>
        void VisualChanges(bool visualChanges);
    }
}