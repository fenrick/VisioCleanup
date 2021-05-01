// -----------------------------------------------------------------------
// <copyright file="IVisioService.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Contracts
{
    using System.Threading.Tasks;

    /// <inheritdoc />
    /// <summary>The VisioService interface.</summary>
    public interface IVisioService : IProcessingService
    {
        /// <summary>Load the visio object model.</summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task" /> representing the result of the asynchronous operation.</returns>
        Task LoadVisioObjectModel();
    }
}
