// -----------------------------------------------------------------------
// <copyright file="IVisioApplication.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Contracts
{
    using System.Collections.Generic;

    /// <summary>
    /// Handle creation and management of visio.
    /// </summary>
    public interface IVisioApplication
    {
        /// <summary>
        /// Calculate base side location.
        /// </summary>
        /// <param name="visioId">Visio shape id.</param>
        /// <returns>Base side.</returns>
        int CalclateBaseSide(int visioId);

        /// <summary>
        /// Calculate left side location.
        /// </summary>
        /// <param name="visioId">Visio shape id.</param>
        /// <returns>Left side.</returns>
        int CalclateLeftSide(int visioId);

        /// <summary>
        /// Calculate right side location.
        /// </summary>
        /// <param name="visioId">Visio shape id.</param>
        /// <returns>Right side.</returns>
        int CalclateRightSide(int visioId);

        /// <summary>
        /// Calculate top side location.
        /// </summary>
        /// <param name="visioId">Visio shape id.</param>
        /// <returns>Top side.</returns>
        int CalclateTopSide(int visioId);

        /// <summary>
        ///     Close visio session and shutdown.
        /// </summary>
        void Close();

        /// <summary>
        ///     Return an array of shapeIDs for children of the supplied shape id.
        /// </summary>
        /// <param name="visioId">Shape ID of the parent shape.</param>
        /// <returns>array of shape ids for children.</returns>
        IEnumerable<int> GetChildren(int visioId);

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