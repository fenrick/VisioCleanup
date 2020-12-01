// -----------------------------------------------------------------------
// <copyright file="MyTree.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Services
{
    using System.Collections.Generic;

    /// <summary>
    ///     Tree structure.
    /// </summary>
    /// <typeparam name="TValue">Object in tree.</typeparam>
    public class MyTree<TValue> : HashSet<MyTree<TValue>>
    {
        /// <summary>
        ///     Gets or sets value.
        /// </summary>
        public TValue Value { get; set; } = default!;
    }
}