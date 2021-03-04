// -----------------------------------------------------------------------
// <copyright file="Casing.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Rendering
{
    internal static class Casing
    {
        /// <summary>
        /// Apply upper or lower casing to <paramref name="value" /> when <paramref name="format" /> is provided. Returns
        /// <paramref name="value" /> when no or invalid format provided.
        /// </summary>
        /// <param name="value">Provided string for formatting.</param>
        /// <param name="format">Format string.</param>
        /// <returns>The provided <paramref name="value" /> with formatting applied.</returns>
        public static string Format(string value, string? format = null)
        {
            return format switch
                {
                    "u" => value.ToUpperInvariant(),
                    "w" => value.ToLowerInvariant(),
                    _ => value,
                };
        }
    }
}