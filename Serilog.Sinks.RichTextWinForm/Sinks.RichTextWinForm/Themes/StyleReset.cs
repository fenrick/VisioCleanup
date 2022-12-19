// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StyleReset.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Themes;

/// <summary>The style reset.</summary>
internal readonly struct StyleReset : IDisposable, IEquatable<StyleReset>
{
    /// <summary>The output.</summary>
    private readonly RichTextBox output;

    /// <summary>Initialises a new instance of the <see cref="StyleReset"/> struct.</summary>
    /// <param name="output">The output.</param>
    internal StyleReset(RichTextBox output) => this.output = output;

    /// <summary>The dispose.</summary>
    public void Dispose() => RichTextTheme.Reset(this.output);

    /// <inheritdoc />
    public bool Equals(StyleReset other) => this.output.Equals(other.output);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is StyleReset other && this.Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => this.output.GetHashCode();
}
