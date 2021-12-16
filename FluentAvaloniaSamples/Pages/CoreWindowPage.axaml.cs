using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvaloniaSamples.Pages.NVSamplePages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace FluentAvaloniaSamples.Pages
{
    public class CoreWindowPage : UserControl
    {
        public CoreWindowPage()
        {
            InitializeComponent();

            DescriptionText = GetAssemblyResource("CoreWindowInfo.txt");

            DataContext = this;
        }

      
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public string DescriptionText { get; set; }

        protected string GetAssemblyResource(string name)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            using (var stream = assets.Open(new Uri($"avares://FluentAvaloniaSamples/DescriptionTexts/{name}")))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
