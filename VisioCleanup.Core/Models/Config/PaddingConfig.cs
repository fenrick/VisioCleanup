// -----------------------------------------------------------------------
// <copyright file="PaddingConfig.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Models.Config
{
    /// <summary>The padding config.</summary>
    public class PaddingConfig
    {
        /// <summary>Gets or sets the bottom.</summary>
        /// <value>The bottom.</value>
        public int Bottom { get; set; }

        /// <summary>Gets or sets the horizontal spacing.</summary>
        /// <value>The horizontal spacing.</value>
        public int HorizontalSpacing { get; set; }

        /// <summary>Gets or sets the left.</summary>
        /// <value>The left.</value>
        public int Left { get; set; }

        /// <summary>Gets or sets the right.</summary>
        /// <value>The right.</value>
        public int Right { get; set; }

        /// <summary>Gets or sets the top.</summary>
        /// <value>The top.</value>
        public int Top { get; set; }

        /// <summary>Gets or sets the vertical spacing.</summary>
        /// <value>The vertical spacing.</value>
        public int VerticalSpacing { get; set; }
    }
}