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