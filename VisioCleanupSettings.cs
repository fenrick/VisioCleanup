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
        ///     Gets or sets bottom padding.
        /// </summary>
        public int BottomPadding { get; set; }

        /// <summary>
        ///     Gets or sets Excel Header Format.
        /// </summary>
        public string? ExcelFieldLabelFormat { get; set; }

        public string? ExcelShapeTypeLabelFormat { get; set; }

        public string? ExcelSortFieldLabelFormat { get; set; }

        public int HeaderHeight { get; set; }

        /// <summary>
        ///     Gets or sets left padding.
        /// </summary>
        public int LeftPadding { get; set; }

        /// <summary>
        ///     Gets or sets right padding.
        /// </summary>
        public int RightPadding { get; set; }

        public int SidePanelWidth { get; set; }

        /// <summary>
        ///     Gets or sets top padding.
        /// </summary>
        public int TopPadding { get; set; }

        /// <summary>
        ///     Gets or sets ultimate shape height.
        /// </summary>
        public int UltimateShapeHeight { get; set; }

        /// <summary>
        ///     Gets or sets ultimate shape width.
        /// </summary>
        public int UltimateShapeWidth { get; set; }

        /// <summary>
        ///     Gets or sets Visio Height Field.
        /// </summary>
        public string? VisioHeightField { get; set; }

        /// <summary>
        ///     Gets or sets Visio Horizontal Spacer.
        /// </summary>
        public int VisioHorizontalSpacer { get; set; }

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
        ///     Gets or sets Visio Units.
        /// </summary>
        public string? VisioUnits { get; set; }

        /// <summary>
        ///     Gets or sets Visio Vertical Spacer.
        /// </summary>
        public int VisioVerticalSpacer { get; set; }

        /// <summary>
        ///     Gets or sets Visio Width field.
        /// </summary>
        public string? VisioWidthField { get; set; }
    }
}