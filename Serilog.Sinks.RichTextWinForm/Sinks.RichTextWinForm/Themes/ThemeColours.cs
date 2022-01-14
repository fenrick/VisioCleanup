// -----------------------------------------------------------------------
// <copyright file="ThemeColours.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Themes;

using System.Runtime.InteropServices;

/// <summary>Styling applied using the <see cref="System.ConsoleColor" />enumeration.</summary>
[StructLayout(LayoutKind.Auto)]
public readonly struct ThemeColours : IEquatable<ThemeColours>
{
    /// <summary>Gets the foreground color to apply.</summary>
    /// <value>Foreground color.</value>
    internal Color? Foreground { get; init; }

    /// <summary>Gets the background color to apply.</summary>
    /// <value>Background color.</value>
    internal Color? Background { get; init; }

    /// <summary>Are they equal.</summary>
    /// <param name="left">Left.</param>
    /// <param name="right">Right.</param>
    /// <returns>Equality.</returns>
    public static bool operator ==(ThemeColours left, ThemeColours right) => left.Equals(right);

    /// <summary>Are they not equal.</summary>
    /// <param name="left">Left.</param>
    /// <param name="right">Right.</param>
    /// <returns>Inequality.</returns>
    public static bool operator !=(ThemeColours left, ThemeColours right) => !(left == right);

    /// <inheritdoc />
    public override readonly bool Equals(object? obj) => obj is ThemeColours colours && this.Equals(colours);

    /// <inheritdoc />
    public readonly bool Equals(ThemeColours other)
    {
        if (!EqualityComparer<Color?>.Default.Equals(this.Foreground, other.Foreground))
        {
            return false;
        }

        return EqualityComparer<Color?>.Default.Equals(this.Background, other.Background);
    }

    /// <inheritdoc />
    public override readonly int GetHashCode() => this.Foreground.GetHashCode() + this.Background.GetHashCode();
}
