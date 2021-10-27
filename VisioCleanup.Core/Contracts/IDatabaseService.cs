// -----------------------------------------------------------------------
// <copyright file="IDatabaseService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Contracts;

using System.Threading.Tasks;

/// <inheritdoc />
/// <summary>The IDatabase interface.</summary>
public interface IDatabaseService : IProcessingService
{
    /// <summary>Load dataset based on database query, from iserver reporting database.</summary>
    /// <param name="sqlCommand">sql to execute.</param>
    /// <returns>Async task.</returns>
    Task ProcessDataSet(string sqlCommand);
}
