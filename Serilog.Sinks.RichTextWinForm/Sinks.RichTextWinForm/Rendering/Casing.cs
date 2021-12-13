// -----------------------------------------------------------------------
// <copyright file="Casing.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Rendering;

using System;

/// <summary>Set the case of a string.</summary>
internal static class Casing
{
    /// <summary>
    /// Apply upper or lower casing to <paramref name="value" /> when <paramref name="formatString" /> is provided.
    /// Returns <paramref name="value" /> when no or invalid <paramref name="formatString" /> provided.
    /// </summary>
    /// <param name="value">Provided string for formatting.</param>
    /// <param name="formatString"><see cref="Casing.Format" /> string.</param>
    /// <exception cref="ArgumentNullException">Empty value string.</exception>
    /// <returns>The provided <paramref name="value" /> with formatting applied.</returns>
    internal static string Format(string value, string? formatString)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (formatString is null)
        {
            return value;
        }

        if (string.Equals(formatString, "u", StringComparison.Ordinal))
        {
            return value.ToUpperInvariant();
        }

#pragma warning disable S4040 // Strings should be normalized to uppercase
        return string.Equals(formatString, "w", StringComparison.Ordinal) ? value.ToLowerInvariant() : value;
#pragma warning restore S4040 // Strings should be normalized to uppercase
    }
}
