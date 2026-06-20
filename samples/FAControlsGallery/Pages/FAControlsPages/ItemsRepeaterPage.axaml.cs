using Avalonia.Controls;
using FAControlsGallery.ViewModels;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Pages;

public partial class ItemsRepeaterPage : ControlsPageBase
{
    public ItemsRepeaterPage()
    {
        InitializeComponent();

        DataContext = new ItemsRepeaterPageViewModel();
        TargetType = typeof(FAItemsRepeater);

        StackLayoutSpacingNB.ValueChanged += StackLayoutSpacingNBValueChanged;
        FlowOrientationCB.SelectionChanged += FlowOrientationCBSelectionChanged;
    }

    private void FlowOrientationCBSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FlowOrientationCB.SelectedIndex == 0)
        {
            FlowLayoutScrollViewer.Classes.Remove("horizontal");
        }
        else
        {
            FlowLayoutScrollViewer.Classes.Add("horizontal");
        }
    }

    private void StackLayoutSpacingNBValueChanged(FANumberBox sender, FANumberBoxValueChangedEventArgs args)
    {
        if (Repeater1.Layout is FAStackLayout sl)
        {
            sl.Spacing = args.NewValue;
        }
    }
   
}
