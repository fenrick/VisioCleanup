// -----------------------------------------------------------------------
// <copyright file="IDataSource.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Contracts;

using System.Collections.Generic;

using VisioCleanup.Core.Models;

/// <summary>Handle creation and management of excel interop objects.</summary>
public interface IDataSource
{
    /// <summary>Gets the name of data source.</summary>
    /// <value>String representation of the name of a data source.</value>
    string Name { get; }

    /// <summary><see cref="Close" /> excel session and shutdown.</summary>
    void Close();

    /// <summary><see cref="Open" /> excel session.</summary>
    void Open();

    /// <summary>Retrieve records based on parameter.</summary>
    /// <param name="parameter">Parameter.</param>
    /// <returns>Collection of diagram shapes.</returns>
    IEnumerable<DiagramShape> RetrieveRecords(string parameter);
}
