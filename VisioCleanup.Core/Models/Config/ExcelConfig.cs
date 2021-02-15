// -----------------------------------------------------------------------
// <copyright file="ExcelConfig.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Models.Config
{
    /// <summary>The excel config.</summary>
    public class ExcelConfig
    {
        /// <summary>Gets or sets the field label format.</summary>
        /// <value>The field label format.</value>
        public string FieldLabelFormat { get; set; }

        /// <summary>Gets or sets the shape type label format.</summary>
        /// <value>The shape type label format.</value>
        public string ShapeTypeLabelFormat { get; set; }

        /// <summary>Gets or sets the sort field label format.</summary>
        /// <value>The sort field label format.</value>
        public string SortFieldLabelFormat { get; set; }
    }
}