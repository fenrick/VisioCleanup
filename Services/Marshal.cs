// -----------------------------------------------------------------------
// <copyright file="Marshal.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Services
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;

    /// <summary>
    ///     Implementation of .Net Framework 4.6 System.Runtime.InteropServices.Marshal.GetActiveObject().
    /// </summary>
    internal static class Marshal
    {
        /// <summary>
        ///     Copy of System.Runtime.InteropServices.Marshal.GetActiveObject().
        /// </summary>
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
            catch (Exception)
            {
                CLSIDFromProgID(progId, out classId);
            }

            GetActiveObject(ref classId, IntPtr.Zero, out var obj);
            return obj;
        }

        /// <summary>
        ///     Release com object properly.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <exception cref="T:System.NullReferenceException"><paramref name="o" /> is <see langword="null" />.</exception>
        [SupportedOSPlatform("windows")]
        internal static void ReleaseObject(object? obj)
        {
            // Do not catch an exception from this.
            // You may want to remove these guards depending on
            // what you think the semantics should be.
            if (obj != null && System.Runtime.InteropServices.Marshal.IsComObject(obj))
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
            }

            // GC.Collect();
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
        private static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out object ppunk);
    }
}