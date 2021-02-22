// -----------------------------------------------------------------------
// <copyright file="IVisioApplication.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Contracts
{
    using VisioCleanup.Core.Models;

    /// <summary>
    /// Handle creation and management of visio.
    /// </summary>
    public interface IVisioApplication
    {
        /// <summary>
        ///     Calculate the corners of a shape.
        /// </summary>
        /// <param name="visioId">Shape ID for the shape.</param>
        /// <returns>Corners of a shape.</returns>
        Corners CalculateCorners(int visioId);

        /// <summary>
        ///     Close visio session and shutdown.
        /// </summary>
        void Close();

        /// <summary>
        ///     Get size of the page.
        /// </summary>
        /// <param name="headerHeight">Header size.</param>
        /// <param name="sidePanelWidth">Side panel size.</param>
        /// <returns>Corners size.</returns>
        Corners GetPageSize(int headerHeight, int sidePanelWidth);

        /// <summary>
        ///     Obtains the current shape text for a shape.
        /// </summary>
        /// <param name="visioId">shape id.</param>
        /// <returns>shape text.</returns>
        string GetShapeText(int visioId);

        /// <summary>
        ///     Open visio session.
        /// </summary>
        void Open();

        /// <summary>
        ///     Return an array of visio ids that have been selected.
        /// </summary>
        /// <returns>Array of visio ids.</returns>
        int[] Selection();
    }
}