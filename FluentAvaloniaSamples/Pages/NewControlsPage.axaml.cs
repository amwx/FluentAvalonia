using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using FluentAvaloniaSamples.Controls;
using FluentAvaloniaSamples.Services;
using FluentAvaloniaSamples.ViewModels;

namespace FluentAvaloniaSamples.Pages
{
    public partial class NewControlsPage : UserControl
    {
        public NewControlsPage()
        {
            InitializeComponent();

            DataContext = new NewControlsPageViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
