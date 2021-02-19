// -----------------------------------------------------------------------
// <copyright file="ColouredStringWriter.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.TextWriter
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    // This class implements a text writer that writes to a string buffer and allows
    // the resulting sequence of characters to be presented as a string.
    public class ColouredStringWriter : TextWriter
    {
        private static volatile UnicodeEncoding? s_encoding;

        private readonly StringBuilder _sb;

        private bool _isOpen;

        // Constructs a new StringWriter. A new StringBuilder is automatically
        // created and associated with the new StringWriter.
        public ColouredStringWriter()
            : this(new StringBuilder(), CultureInfo.CurrentCulture)
        {
        }

        public ColouredStringWriter(IFormatProvider? formatProvider)
            : this(new StringBuilder(), formatProvider)
        {
        }

        // Constructs a new StringWriter that writes to the given StringBuilder.
        public ColouredStringWriter(StringBuilder sb)
            : this(sb, CultureInfo.CurrentCulture)
        {
        }

        public ColouredStringWriter(StringBuilder sb, IFormatProvider? formatProvider)
            : base(formatProvider)
        {
            if (sb == null)
            {
                throw new ArgumentNullException(nameof(sb), "Buffer cannot be null.");
            }

            this._sb = sb;
            this._isOpen = true;
        }

        public override Encoding Encoding
        {
            get
            {
                if (s_encoding == null)
                {
                    s_encoding = new UnicodeEncoding(false, false);
                }

                return s_encoding;
            }
        }

        public override void Close()
        {
            this.Dispose(true);
        }

        public override Task FlushAsync()
        {
            return Task.CompletedTask;
        }

        // Returns the underlying StringBuilder. This is either the StringBuilder
        // that was passed to the constructor, or the StringBuilder that was
        // automatically created.
        public virtual StringBuilder GetStringBuilder()
        {
            return this._sb;
        }

        // Returns a string containing the characters written to this TextWriter so far.
        public override string ToString()
        {
            return this._sb.ToString();
        }

        // Writes a character to the underlying string buffer.
        public override void Write(char value)
        {
            if (!this._isOpen)
            {
                throw new ObjectDisposedException(null, "Cannot write to a closed TextWriter.");
            }

            this._sb.Append(value);
        }

        // Writes a range of a character array to the underlying string buffer.
        // This method will write count characters of data into this
        // StringWriter from the buffer character array starting at position
        // index.
        public override void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null.");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Non-negative number required.");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Non-negative number required.");
            }

            if ((buffer.Length - index) < count)
            {
                throw new ArgumentException(
                    "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
            }

            if (!this._isOpen)
            {
                throw new ObjectDisposedException(null, "Cannot write to a closed TextWriter.");
            }

            this._sb.Append(buffer, index, count);
        }

        public override void Write(ReadOnlySpan<char> buffer)
        {
            if (this.GetType() != typeof(StringWriter))
            {
                // This overload was added after the Write(char[], ...) overload, and so in case
                // a derived type may have overridden it, we need to delegate to it, which the base does.
                base.Write(buffer);
                return;
            }

            if (!this._isOpen)
            {
                throw new ObjectDisposedException(null, "Cannot write to a closed TextWriter.");
            }

            this._sb.Append(buffer);
        }

        // Writes a string to the underlying string buffer. If the given string is
        // null, nothing is written.
        public override void Write(string? value)
        {
            if (!this._isOpen)
            {
                throw new ObjectDisposedException(null, "Cannot write to a closed TextWriter.");
            }

            if (value != null)
            {
                this._sb.Append(value);
            }
        }

        public override void Write(StringBuilder? value)
        {
            if (this.GetType() != typeof(StringWriter))
            {
                // This overload was added after the Write(char[], ...) overload, and so in case
                // a derived type may have overridden it, we need to delegate to it, which the base does.
                base.Write(value);
                return;
            }

            if (!this._isOpen)
            {
                throw new ObjectDisposedException(null, "Cannot write to a closed TextWriter.");
            }

            this._sb.Append(value);
        }

        public override Task WriteAsync(char value)
        {
            this.Write(value);
            return Task.CompletedTask;
        }

        public override Task WriteAsync(string? value)
        {
            this.Write(value);
            return Task.CompletedTask;
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            this.Write(buffer, index, count);
            return Task.CompletedTask;
        }

        public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            this.Write(buffer.Span);
            return Task.CompletedTask;
        }

        public override Task WriteAsync(StringBuilder? value, CancellationToken cancellationToken = default)
        {
            if (this.GetType() != typeof(StringWriter))
            {
                // This overload was added after the WriteAsync(char[], ...) overload, and so in case
                // a derived type may have overridden it, we need to delegate to it, which the base does.
                return base.WriteAsync(value, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            if (!this._isOpen)
            {
                throw new ObjectDisposedException(null, "Cannot write to a closed TextWriter.");
            }

            this._sb.Append(value);
            return Task.CompletedTask;
        }

        public override void WriteLine(ReadOnlySpan<char> buffer)
        {
            if (this.GetType() != typeof(StringWriter))
            {
                // This overload was added after the WriteLine(char[], ...) overload, and so in case
                // a derived type may have overridden it, we need to delegate to it, which the base does.
                base.WriteLine(buffer);
                return;
            }

            if (!this._isOpen)
            {
                throw new ObjectDisposedException(null, "Cannot write to a closed TextWriter.");
            }

            this._sb.Append(buffer);
            this.WriteLine();
        }

        public override void WriteLine(StringBuilder? value)
        {
            if (this.GetType() != typeof(StringWriter))
            {
                // This overload was added after the WriteLine(char[], ...) overload, and so in case
                // a derived type may have overridden it, we need to delegate to it, which the base does.
                base.WriteLine(value);
                return;
            }

            if (!this._isOpen)
            {
                throw new ObjectDisposedException(null, "Cannot write to a closed TextWriter.");
            }

            this._sb.Append(value);
            this.WriteLine();
        }

        public override Task WriteLineAsync(char value)
        {
            this.WriteLine(value);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(string? value)
        {
            this.WriteLine(value);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(StringBuilder? value, CancellationToken cancellationToken = default)
        {
            if (this.GetType() != typeof(StringWriter))
            {
                // This overload was added after the WriteLineAsync(char[], ...) overload, and so in case
                // a derived type may have overridden it, we need to delegate to it, which the base does.
                return base.WriteLineAsync(value, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            if (!this._isOpen)
            {
                throw new ObjectDisposedException(null, "Cannot write to a closed TextWriter.");
            }

            this._sb.Append(value);
            this.WriteLine();
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            this.WriteLine(buffer, index, count);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            this.WriteLine(buffer.Span);
            return Task.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            // Do not destroy _sb, so that we can extract this after we are
            // done writing (similar to MemoryStream's GetBuffer & ToArray methods)
            this._isOpen = false;
            base.Dispose(disposing);
        }
    }
}