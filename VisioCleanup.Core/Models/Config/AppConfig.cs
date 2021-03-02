// -----------------------------------------------------------------------
// <copyright file="AppConfig.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Models.Config
{
    /// <summary>The app config.</summary>
    public class AppConfig
    {
        /// <summary>Gets or sets the bottom.</summary>
        /// <value>The bottom.</value>
        public int Base { get; set; }

        /// <summary>Gets or sets the field label format.</summary>
        /// <value>The field label format.</value>
        public string? FieldLabelFormat { get; set; }

        /// <summary>Gets or sets the header height.</summary>
        /// <value>The header height.</value>
        public int HeaderHeight { get; set; }

        /// <summary>Gets or sets the height.</summary>
        /// <value>The height.</value>
        public int Height { get; set; }

        /// <summary>Gets or sets the horizontal spacing.</summary>
        /// <value>The horizontal spacing.</value>
        public int HorizontalSpacing { get; set; }

        /// <summary>Gets or sets the left.</summary>
        /// <value>The left.</value>
        public int Left { get; set; }

        /// <summary>Gets or sets the right.</summary>
        /// <value>The right.</value>
        public int Right { get; set; }

        /// <summary>Gets or sets the shape type label format.</summary>
        /// <value>The shape type label format.</value>
        public string? ShapeTypeLabelFormat { get; set; }

        /// <summary>Gets or sets the side panel width.</summary>
        /// <value>The side panel width.</value>
        public int SidePanelWidth { get; set; }

        /// <summary>Gets or sets the sort field label format.</summary>
        /// <value>The sort field label format.</value>
        public string? SortFieldLabelFormat { get; set; }

        /// <summary>Gets or sets the top.</summary>
        /// <value>The top.</value>
        public int Top { get; set; }

        /// <summary>Gets or sets the vertical spacing.</summary>
        /// <value>The vertical spacing.</value>
        public int VerticalSpacing { get; set; }

        /// <summary>Gets or sets the width.</summary>
        /// <value>The width.</value>
        public int Width { get; set; }
    }
}