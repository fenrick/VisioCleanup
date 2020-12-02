// -----------------------------------------------------------------------
// <copyright file="IExcelHandler.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Services
{
    /// <summary>
    ///     Interface for excel handler.
    /// </summary>
    internal interface IExcelHandler
    {
        /// <summary>
        ///     Close connection to excel.
        /// </summary>
        void Close();

        /// <summary>
        ///     Open connection to excel.
        /// </summary>
        void Open();

        /// <summary>
        ///     Retrieve records from excel.
        /// </summary>
        /// <returns>Tree of results.</returns>
        MyTree<string> RetrieveRecords();
    }
}