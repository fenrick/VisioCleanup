﻿// -----------------------------------------------------------------------
// <copyright file="Marshal.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core
{
    using System;
    using System.Security;

    using Serilog;

    using VisioCleanup.Core.Resources;

    /// <summary>Implementation of .Net Framework 4.6 System.Runtime.InteropServices.Marshal.GetActiveObject().</summary>
    internal static class Marshal
    {
        /// <summary>Copy of System.Runtime.InteropServices.Marshal.GetActiveObject().</summary>
        /// <param name="progId">String program identifier.</param>
        /// <returns>Object.</returns>
        [SecurityCritical] // auto-generated_required
        public static object GetActiveObject(string progId)
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
                Log.Error(e, en_AU.Marshal_GetActiveObject_Unable_to_find_CLSIDFromProgIDEx);

                // When you catch an exception you should throw exception or at least log error
                NativeMethods.CLSIDFromProgID(progId, out classId);
            }

            NativeMethods.GetActiveObject(ref classId, IntPtr.Zero, out var obj);
            return obj;
        }
    }
}
