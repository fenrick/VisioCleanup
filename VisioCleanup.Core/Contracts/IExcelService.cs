// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExcelService.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace VisioCleanup.Core.Contracts;

/// <inheritdoc />
/// <summary>The ExcelService interface.</summary>
public interface IExcelService : IProcessingService
{
    /// <summary>Async process excel data set and load into memory.</summary>
    void ProcessDataSet();
}
