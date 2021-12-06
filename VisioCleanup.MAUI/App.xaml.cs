// -----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.MAUI;

using Microsoft.Maui.Controls;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();

        this.MainPage = new MainPage();
    }
}
