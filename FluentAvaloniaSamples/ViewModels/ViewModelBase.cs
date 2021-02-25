using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Avalonia;
using Avalonia.Platform;
using ReactiveUI;

namespace FluentAvaloniaSamples.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected string GetAssemblyResource(string name)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            using (var stream = assets.Open(new Uri($"avares://FluentAvaloniaSamples/DescriptionTexts/{name}")))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        protected bool RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }

    }
}
