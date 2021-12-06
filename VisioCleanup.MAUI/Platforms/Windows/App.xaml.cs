﻿// -----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VisioCleanup.MAUI.WinUI;

using Microsoft.Maui;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml;

/// <summary>Provides application-specific behavior to supplement the default Application class.</summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code executed, and as such
    /// is the logical equivalent of main() or WinMain().
    /// </summary>
    public App() => this.InitializeComponent();

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        Platform.OnLaunched(args);
    }
}
