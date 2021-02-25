using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    public class CommandBarItemHost : CommandBarItemBase, IContentControl, IContentPresenterHost
    {
        public static readonly StyledProperty<object> ContentProperty =
            ContentControl.ContentProperty.AddOwner<CommandBarItemHost>();

        public static readonly StyledProperty<IDataTemplate> ContentTemplateProperty =
            ContentControl.ContentTemplateProperty.AddOwner<CommandBarItemHost>();

        public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
            ContentControl.HorizontalAlignmentProperty.AddOwner<CommandBarItemHost>();

        public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
            ContentControl.VerticalContentAlignmentProperty.AddOwner<CommandBarItemHost>();

        [Content]
        public object Content { get => GetValue(ContentProperty); set => SetValue(ContentProperty, value); }

        public IDataTemplate ContentTemplate { get => GetValue(ContentTemplateProperty); set => SetValue(ContentTemplateProperty, value); }

        public HorizontalAlignment HorizontalContentAlignment 
        { 
            get => GetValue(HorizontalContentAlignmentProperty);
            set => SetValue(HorizontalContentAlignmentProperty, value); 
        }

        public VerticalAlignment VerticalContentAlignment
        {
            get => GetValue(VerticalContentAlignmentProperty);
            set => SetValue(VerticalContentAlignmentProperty, value);
        }

        IAvaloniaList<ILogical> IContentPresenterHost.LogicalChildren => LogicalChildren;

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ContentProperty)
            {
                if (change.OldValue.GetValueOrDefault() is ILogical old)
                {
                    LogicalChildren.Remove(old);
                }
                if (change.NewValue.GetValueOrDefault() is ILogical newC)
                {
                    LogicalChildren.Add(newC);
                }
            }
        }


        public bool RegisterContentPresenter(IContentPresenter presenter)
        {
            if (presenter.Name == "ContentPresenter")
                return true;

            return false;
        }
    }
}
