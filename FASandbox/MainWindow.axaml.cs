using System;
using System.Numerics;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using FluentAvalonia.UI.Controls;

namespace FASandbox;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

#if DEBUG
        this.AttachDevTools();
#endif
        DataContext = new MainWindowViewModel();

        _aniBorder = this.FindControl<Border>("Border1");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // START DIALOG ANIMATION
    public async void ShowDialogClick(object sender, RoutedEventArgs args)
    {
        var cd = new ContentDialog
        {
            Title = "Test",
            PrimaryButtonText = "Close"
        };

        await cd.ShowAsync();
    }


    // COMPOSITION ANIMATION ISSUE
    public void OpenAnimRun(object sender, RoutedEventArgs args)
    {
        if (_expandAnimation == null)
        {
            var compositor = ElementComposition.GetElementVisual(_aniBorder).Compositor;

            _expandAnimation = compositor.CreateVector3KeyFrameAnimation();

            _expandAnimation.SetScalarParameter("Width", (float)_aniBorder.Bounds.Width);
            _expandAnimation.SetScalarParameter("Height", (float)_aniBorder.Bounds.Height);
            _expandEasingFunction = new SplineEasing(0.1, 0.9, 0.2, 1);

            _expandAnimation.InsertExpressionKeyFrame(0.0f, "Vector3(Min(0.01, 20.0 / Width), Min(0.01, 20.0 / Height), 1.0)");
            (_expandAnimation as Vector3KeyFrameAnimation).InsertKeyFrame(1f, Vector3.One, _expandEasingFunction);
            _expandAnimation.Duration = TimeSpan.FromMilliseconds(250);
            _expandAnimation.Target = "Scale";
        }

        ElementComposition.GetElementVisual(_aniBorder).StartAnimationGroup(_expandAnimation);
    }

    public void CloseAnimRun(object sender, RoutedEventArgs args)
    {
        if (_contractAnimation == null)
        {
            var compositor = ElementComposition.GetElementVisual(_aniBorder).Compositor;

            _contractAnimation = compositor.CreateVector3KeyFrameAnimation();

            _contractAnimation.SetScalarParameter("Width", (float)_aniBorder.Bounds.Width);
            _contractAnimation.SetScalarParameter("Height", (float)_aniBorder.Bounds.Height);
            _contractEasingFunction = new SplineEasing(0.1, 0.9, 0.2, 1);

            (_contractAnimation as Vector3KeyFrameAnimation).InsertKeyFrame(0f, Vector3.One);
            _contractAnimation.InsertExpressionKeyFrame(1.0f, "Vector3(20.0 / Width, 20.0 / Height, 1.0)", (Easing)_contractEasingFunction);
            _contractAnimation.Duration = TimeSpan.FromMilliseconds(250);
            _contractAnimation.Target = "Scale";
        }

        ElementComposition.GetElementVisual(_aniBorder).StartAnimationGroup(_contractAnimation);
    }

    public void TeachingTipOpen(object sender, RoutedEventArgs args)
    {
        var tt = this.FindControl<TeachingTip>("TeachingTip1");
        tt.IsOpen = !tt.IsOpen;
    }

    private KeyFrameAnimation _expandAnimation;
    private IEasing _expandEasingFunction;
    private IEasing _contractEasingFunction;
    private KeyFrameAnimation _contractAnimation;
    private Border _aniBorder;
}
