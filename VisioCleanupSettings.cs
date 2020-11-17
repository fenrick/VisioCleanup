// -----------------------------------------------------------------------
// <copyright file="VisioCleanupSettings.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup
{
    /// <summary>
    ///     Visio cleanup settings.
    /// </summary>
    public class VisioCleanupSettings
    {
        /// <summary>
        ///     Gets or sets Visio Height Field.
        /// </summary>
        public string? VisioHeightField { get; set; }

        /// <summary>
        ///     Gets or sets Visio LocPinX Field.
        /// </summary>
        public string? VisioLocPinXField { get; set; }

        /// <summary>
        ///     Gets or sets Visio LocPinY Field.
        /// </summary>
        public string? VisioLocPinYField { get; set; }

        /// <summary>
        ///     Gets or sets Visio PinX Field.
        /// </summary>
        public string? VisioPinXField { get; set; }

        /// <summary>
        ///     Gets or sets Visio PinY Field.
        /// </summary>
        public string? VisioPinYField { get; set; }

        /// <summary>
        ///     Gets or sets Visio Spacer.
        /// </summary>
        public double VisioSpacer { get; set; }

        /// <summary>
        ///     Gets or sets Visio Units.
        /// </summary>
        public string? VisioUnits { get; set; }

        /// <summary>
        ///     Gets or sets Visio Width field.
        /// </summary>
        public string? VisioWidthField { get; set; }
    }
}