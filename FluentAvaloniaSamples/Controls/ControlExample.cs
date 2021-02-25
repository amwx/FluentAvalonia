using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvaloniaSamples
{
    public class ControlExample : HeaderedContentControl
    {
        public static readonly StyledProperty<string> XamlSourceProperty =
            AvaloniaProperty.Register<ControlExample, string>("XamlSource");

        public static readonly StyledProperty<string> CSharpSourceProperty =
            AvaloniaProperty.Register<ControlExample, string>("CSharpSource");

        public static readonly StyledProperty<string> UsageNotesProperty =
            AvaloniaProperty.Register<ControlExample, string>("UsageNotes");

        public static readonly StyledProperty<IControl> OptionsProperty =
            AvaloniaProperty.Register<ControlExample, IControl>("Options");

        public string XamlSource
        {
            get => GetValue(XamlSourceProperty);
            set => SetValue(XamlSourceProperty, value);
        }

        public string CSharpSource
        {
            get => GetValue(CSharpSourceProperty);
            set => SetValue(CSharpSourceProperty, value);
        }

        public string UsageNotes
        {
            get => GetValue(UsageNotesProperty);
            set => SetValue(UsageNotesProperty, value);
        }

        public IControl Options
        {
            get => GetValue(OptionsProperty);
            set => SetValue(OptionsProperty, value);
        }
    }
}
