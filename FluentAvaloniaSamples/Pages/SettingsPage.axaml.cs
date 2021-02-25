﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FluentAvaloniaSamples.Pages
{
    public class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
