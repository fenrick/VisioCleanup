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
        /// <summary>Gets or sets the defaults.</summary>
        /// <value>The defaults.</value>
        public DefaultsConfig? Defaults { get; set; }

        /// <summary>Gets or sets the excel.</summary>
        /// <value>The excel.</value>
        public ExcelConfig? Excel { get; set; }

        /// <summary>Gets or sets the padding.</summary>
        /// <value>The padding.</value>
        public PaddingConfig? Padding { get; set; }

        /// <summary>Gets or sets the page.</summary>
        /// <value>The page.</value>
        public PageConfig? Page { get; set; }

        /// <summary>Gets or sets the visio fields.</summary>
        /// <value>The visio fields.</value>
        public VisioFieldsConfig? VisioFields { get; set; }
    }
}