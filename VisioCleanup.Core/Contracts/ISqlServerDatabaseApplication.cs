// -----------------------------------------------------------------------
// <copyright file="ISqlServerDatabaseApplication.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Contracts
{
    using System.Collections.Generic;

    using VisioCleanup.Core.Models;

    /// <summary>The SqlServerDatabaseApplication interface.</summary>
    public interface ISqlServerDatabaseApplication
    {
        /// <summary>Close database connection and cleanup objects.</summary>
        void Close();

        /// <summary><see cref="Open" /> connection to iserver reporting database.</summary>
        void Open();

        /// <summary>Retrieve database records based on sql query.</summary>
        /// <param name="sqlCommand">Query.</param>
        /// <returns>Collection of diagram shapes.</returns>
        IEnumerable<DiagramShape> RetrieveRecords(string sqlCommand);
    }
}