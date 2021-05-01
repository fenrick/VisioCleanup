// -----------------------------------------------------------------------
// <copyright file="IExcelApplication.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Contracts
{
    using System.Collections.Generic;

    using VisioCleanup.Core.Models;

    /// <summary>Handle creation and management of excel interop objects.</summary>
    public interface IExcelApplication
    {
        /// <summary><see cref="Close" /> excel session and shutdown.</summary>
        void Close();

        /// <summary><see cref="Open" /> excel session.</summary>
        void Open();

        /// <summary>Retrieve records from excel.</summary>
        /// <returns>Collection of diagram shapes.</returns>
        IEnumerable<DiagramShape> RetrieveRecords();
    }
}
