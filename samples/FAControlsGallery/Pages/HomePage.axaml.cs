using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using FAControlsGallery.ViewModels;

namespace FAControlsGallery.Pages;
public partial class HomePage : UserControl
{
    public HomePage()
    {
        InitializeComponent();
        DataContext = new HomePageViewModel();

        AddHandler(Button.ClickEvent, OnNavButtonClick);

        ItemsControl.Loaded += ItemsControlLoaded;
    }

    private void OnNavButtonClick(object sender, RoutedEventArgs args)
    {
        if (args.Source is Button b && b.DataContext is HomeNavPageViewModel pg)
        {
            pg.Navigate();
            args.Handled = true;
        }
    }

    private void ItemsControlLoaded(object sender, RoutedEventArgs e)
    {
        ItemsControl.Loaded -= ItemsControlLoaded;

        var panel = ItemsControl.ItemsPanelRoot;

        _animations = GetAnimations();
        for (int i = 0; i < panel.Children.Count; i++)
        {
            var item = panel.Children[i];
            var vis = ElementComposition.GetElementVisual(item);
            vis.ImplicitAnimations = _animations;
        }
    }

    private ImplicitAnimationCollection GetAnimations()
    {
        var compositor = ElementComposition.GetElementVisual(this).Compositor;

        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(250);

        // Weirdness, wrap panel doesn't work right if we only have the offset animation
        // Copied from ControlCatalog page, but changed the rotation to 0 (filed issue for this)
        var rotationAnimation = compositor.CreateScalarKeyFrameAnimation();
        rotationAnimation.Target = "RotationAngle";
        rotationAnimation.InsertKeyFrame(.5f, 0f);
        rotationAnimation.InsertKeyFrame(1f, 0f);
        rotationAnimation.Duration = TimeSpan.FromMilliseconds(400);

        var animationGroup = compositor.CreateAnimationGroup();
        animationGroup.Add(offsetAnimation);
        animationGroup.Add(rotationAnimation);

        _animations = compositor.CreateImplicitAnimationCollection();
        _animations["Offset"] = animationGroup;

        return _animations;

        
        //var offsetAnim = compositor.CreateVector3KeyFrameAnimation();
        //offsetAnim.Target = "Offset";
        //offsetAnim.InsertExpressionKeyFrame(1f, "this.FinalValue");
        //offsetAnim.Duration = TimeSpan.FromMilliseconds(250);

        //var iac = compositor.CreateImplicitAnimationCollection();
        //iac["Offset"] = offsetAnim;
        //return iac;
    }

    private ImplicitAnimationCollection _animations;
}
