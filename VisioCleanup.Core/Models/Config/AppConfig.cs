// -----------------------------------------------------------------------
// <copyright file="AppConfig.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Models.Config
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    /// <summary>The app config.</summary>
    public class AppConfig
    {
        /// <summary>Gets or sets the bottom.</summary>
        /// <value>The bottom.</value>
        public double Base
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the database catalog.</summary>
        public string? DatabaseCatalog
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the list of database queries.</summary>
        public List<Dictionary<string, string>>? DatabaseQueries
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the database server name.</summary>
        public string? DatabaseServer
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the field label format.</summary>
        /// <value>The field label format.</value>
        public string? FieldLabelFormat
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the header height.</summary>
        /// <value>The header height.</value>
        public double HeaderHeight
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the height.</summary>
        /// <value>The height.</value>
        public double Height
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the horizontal spacing.</summary>
        /// <value>The horizontal spacing.</value>
        public double HorizontalSpacing
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the left.</summary>
        /// <value>The left.</value>
        public double Left
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the max number of box lines.</summary>
        public double? MaxBoxLines
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the right.</summary>
        /// <value>The right.</value>
        public double Right
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the shape type label format.</summary>
        /// <value>The shape type label format.</value>
        public string? ShapeTypeLabelFormat
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the side panel width.</summary>
        /// <value>The side panel width.</value>
        public double SidePanelWidth
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the sort field label format.</summary>
        /// <value>The sort field label format.</value>
        public string? SortFieldLabelFormat
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the top.</summary>
        /// <value>The top.</value>
        public double Top
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the vertical spacing.</summary>
        /// <value>The vertical spacing.</value>
        public double VerticalSpacing
        {
            get;
            [UsedImplicitly]
            set;
        }

        /// <summary>Gets or sets the width.</summary>
        /// <value>The width.</value>
        public double Width
        {
            get;
            [UsedImplicitly]
            set;
        }
    }
}