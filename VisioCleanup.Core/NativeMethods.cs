﻿// -----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core;

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;

internal static class NativeMethods
{
    [DllImport("ole32.dll", PreserveSig = false)]
    [ResourceExposure(ResourceScope.None)]
    [SecurityCritical] // auto-generated
    internal static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

    [DllImport("ole32.dll", PreserveSig = false)]
    [ResourceExposure(ResourceScope.None)]
    [SecurityCritical] // auto-generated
    internal static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

    [DllImport("oleaut32.dll", PreserveSig = false)]
    [ResourceExposure(ResourceScope.None)]
    [SecurityCritical] // auto-generated
#pragma warning disable GCop216 // A method named `{0}` is expected return a value. If it's meant to be void, then use a verb other than `Get` such as Read, Download, Sync, ...
    internal static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out object ppunk);
#pragma warning restore GCop216 // A method named `{0}` is expected return a value. If it's meant to be void, then use a verb other than `Get` such as Read, Download, Sync, ...
}
