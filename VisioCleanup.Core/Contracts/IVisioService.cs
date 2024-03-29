﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IVisioService.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace VisioCleanup.Core.Contracts;

/// <inheritdoc />
/// <summary>The VisioService interface.</summary>
public interface IVisioService : IProcessingService
{
    /// <summary>Load the visio object model.</summary>
    void LoadVisioObjectModel();
}
