// -----------------------------------------------------------------------
// <copyright file="RichTextThemeStyle.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Themes
{
    using System;
    using System.ComponentModel;

    /// <summary>Elements styled by a console theme.</summary>
#pragma warning disable CA1027
    public enum RichTextThemeStyle
#pragma warning restore CA1027
    {
        /// <summary>Prominent text, generally content within an event's message.</summary>
        Text,

        /// <summary>Boilerplate text, for example items specified in an output template.</summary>
        SecondaryText,

        /// <summary>
        /// De-emphasized text, for example literal text in output templates and punctuation used when writing structured
        /// data.
        /// </summary>
        TertiaryText,

        /// <summary>Output demonstrating some kind of configuration issue, e.g. an invalid message template token.</summary>
        Invalid,

        /// <summary>The built-in <see langword="null" /> value.</summary>
        Null,

        /// <summary>Property and type names.</summary>
        Name,

        /// <summary>Strings.</summary>
        String,

        /// <summary>Numbers.</summary>
        Number,

        /// <summary>Boolean values.</summary>
        Boolean,

        /// <summary>All other scalar values, e.g. <see cref="System.Guid" /> instances.</summary>
        Scalar,

        /// <summary>Unrecognized literal values, e.g. <see cref="System.Guid" /> instances.</summary>
        [Obsolete("Use RichTextThemeStyle.Scalar instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        Object = Scalar,

        /// <summary>Level indicator.</summary>
        LevelVerbose,

        /// <summary>Level indicator.</summary>
        LevelDebug,

        /// <summary>Level indicator.</summary>
        LevelInformation,

        /// <summary>Level indicator.</summary>
        LevelWarning,

        /// <summary>Level indicator.</summary>
        LevelError,

        /// <summary>Level indicator.</summary>
        LevelFatal,
    }
}