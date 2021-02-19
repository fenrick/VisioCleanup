// -----------------------------------------------------------------------
// <copyright file="VisioFieldsConfig.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Models.Config
{
    /// <summary>The visio fields config.</summary>
    public class VisioFieldsConfig
    {
        /// <summary>Gets or sets the height field.</summary>
        /// <value>The height field.</value>
        public string? HeightField { get; set; }

        /// <summary>Gets or sets the loc pin x field.</summary>
        /// <value>The loc pin x field.</value>
        public string? LocPinXField { get; set; }

        /// <summary>Gets or sets the loc pin y field.</summary>
        /// <value>The loc pin y field.</value>
        public string? LocPinYField { get; set; }

        /// <summary>Gets or sets the pin x field.</summary>
        /// <value>The pin x field.</value>
        public string? PinXField { get; set; }

        /// <summary>Gets or sets the pin y field.</summary>
        /// <value>The pin y field.</value>
        public string? PinYField { get; set; }

        /// <summary>Gets or sets the units.</summary>
        /// <value>The units.</value>
        public string? Units { get; set; }

        /// <summary>Gets or sets the width field.</summary>
        /// <value>The width field.</value>
        public string? WidthField { get; set; }
    }
}