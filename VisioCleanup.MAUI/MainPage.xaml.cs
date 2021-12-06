// -----------------------------------------------------------------------
// <copyright file="MainPage.xaml.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.MAUI;

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

public partial class MainPage : ContentPage
{
    private int count = 0;

    public MainPage() => this.InitializeComponent();

    private void OnCounterClicked(object sender, EventArgs e)
    {
        this.count++;
        this.CounterLabel.Text = $"Current count: {this.count}";

        SemanticScreenReader.Announce(this.CounterLabel.Text);
    }
}
