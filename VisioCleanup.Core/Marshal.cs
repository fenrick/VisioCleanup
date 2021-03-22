// -----------------------------------------------------------------------
// <copyright file="Marshal.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;

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
                CLSIDFromProgIDEx(progId, out classId);
            }
            catch
            {
                // When you catch an exception you should throw exception or at least log error
                CLSIDFromProgID(progId, out classId);
            }

            GetActiveObject(ref classId, IntPtr.Zero, out var obj);
            return obj;
        }

        [DllImport("ole32.dll", PreserveSig = false)]
        [ResourceExposure(ResourceScope.None)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical] // auto-generated
        private static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

        [DllImport("ole32.dll", PreserveSig = false)]
        [ResourceExposure(ResourceScope.None)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical] // auto-generated
        private static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

        [DllImport("oleaut32.dll", PreserveSig = false)]
        [ResourceExposure(ResourceScope.None)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical] // auto-generated
#pragma warning disable GCop216 // A method named `{0}` is expected return a value. If it's meant to be void, then use a verb other than `Get` such as Read, Download, Sync, ...
        private static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out object ppunk);
#pragma warning restore GCop216 // A method named `{0}` is expected return a value. If it's meant to be void, then use a verb other than `Get` such as Read, Download, Sync, ...
    }
}