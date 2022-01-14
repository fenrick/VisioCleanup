// -----------------------------------------------------------------------
// <copyright file="Marshal.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core;

using System.Security;

using Serilog;

/// <summary>Implementation of .Net Framework 4.6 System.Runtime.InteropServices.Marshal.GetActiveObject().</summary>
internal static class Marshal
{
    /// <summary>Copy of System.Runtime.InteropServices.Marshal.GetActiveObject().</summary>
    /// <param name="progId">String program identifier.</param>
    /// <returns>Object.</returns>
    [SecurityCritical] // auto-generated_required
    internal static object GetActiveObject(string progId)
    {
        Guid classId;

        // Call CLSIDFromProgIDEx first then fall back on CLSIDFromProgID if
        // CLSIDFromProgIDEx doesn't exist.
        try
        {
            NativeMethods.CLSIDFromProgIDEx(progId, out classId);
        }
        catch (Exception e)
        {
            Log.Error(e, "Unable to find CLSIDFromProgIDEx");

            // When you catch an exception you should throw exception or at least log error
            NativeMethods.CLSIDFromProgID(progId, out classId);

            throw;
        }

        NativeMethods.GetActiveObject(ref classId, IntPtr.Zero, out var obj);
        return obj;
    }
}
