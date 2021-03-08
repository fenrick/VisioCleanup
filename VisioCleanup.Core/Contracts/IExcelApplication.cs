// -----------------------------------------------------------------------
// <copyright file="IExcelApplication.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Contracts
{
    /// <summary>
    /// Handle creation and management of excel interop objects.
    /// </summary>
    public interface IExcelApplication
    {
        /// <summary>Close excel session and shutdown.</summary>
        void Close();

        /// <summary>Open excel session.</summary>
        void Open();
    }
}