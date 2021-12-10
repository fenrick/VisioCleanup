// -----------------------------------------------------------------------
// <copyright file="IDataSource.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Contracts;

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
    /// <param name="masterShape">Master shape to add all found shapes to.</param>
    void RetrieveRecords(string parameter, DiagramShape masterShape);
}
