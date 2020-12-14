// -----------------------------------------------------------------------
// <copyright file="MyTree.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Services
{
    using System.Collections.Concurrent;

    /// <summary>
    ///     Tree structure.
    /// </summary>
    /// <typeparam name="TValue">Object in tree.</typeparam>
#pragma warning disable 8714
    public class MyTree<TValue> : ConcurrentDictionary<TValue, MyTree<TValue>>
#pragma warning restore 8714
    {
        /// <summary>
        ///     Gets or ses the shape text.
        /// </summary>
        public string? ShapeText { get; set; }

        /// <summary>
        ///     Gets or sets the shape type.
        /// </summary>
        public string? ShapeType { get; set; }

        /// <summary>
        ///     Gets or sets a sort value.
        /// </summary>
        public string? SortValue { get; set; }
    }
}