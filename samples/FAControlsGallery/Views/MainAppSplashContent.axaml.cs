using System.Numerics;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Threading;

namespace FAControlsGallery.Views;
public partial class MainAppSplashContent : UserControl
{
    public MainAppSplashContent()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (!Design.IsDesignMode)
        {
            Dispatcher.UIThread.Post(() =>
            {
                RunAnimations();
            }, DispatcherPriority.Background);
        }        
    }

    private void RunAnimations()
    {
        var comp = ElementComposition.GetElementVisual(this).Compositor;
        var grad1 = ElementComposition.GetElementVisual(GRAD_1);
        grad1.Opacity = 0;
        var grad2 = ElementComposition.GetElementVisual(GRAD_2);
        grad2.Opacity = 0;
        var grad3 = ElementComposition.GetElementVisual(GRAD_3);
        grad3.Opacity = 0;
        var grad4 = ElementComposition.GetElementVisual(GRAD_4);
        grad4.Opacity = 0;

        // GRADIENT BAR ANIMATION
        grad1.CenterPoint = new Vector3((float) (grad1.Size.X / 2f), (float) (grad1.Size.Y / 2f), (float) grad1.CenterPoint.Z);
        grad1.StartAnimationGroup(GetGradientBarAnimation(comp, 1));

        grad2.CenterPoint = new Vector3((float) grad2.Size.X / 2f, (float) grad2.Size.Y / 2f, (float) grad2.CenterPoint.Z);
        grad2.StartAnimationGroup(GetGradientBarAnimation(comp, 2));

        grad3.CenterPoint = new Vector3((float) grad3.Size.X / 2f, (float) grad3.Size.Y / 2f, (float) grad3.CenterPoint.Z);
        grad3.StartAnimationGroup(GetGradientBarAnimation(comp, 3));

        grad4.CenterPoint = new Vector3((float) grad4.Size.X / 2f, (float) grad4.Size.Y / 2f, (float) grad4.CenterPoint.Z);
        grad4.StartAnimationGroup(GetGradientBarAnimation(comp, 4));

        // BLOCKS
        var topLeft = ElementComposition.GetElementVisual(APPS_TOPLEFT);
        topLeft.Opacity = 0;
        var bottomLeft = ElementComposition.GetElementVisual(APPS_BOTTOMLEFT);
        bottomLeft.Opacity = 0;
        var bottomRight = ElementComposition.GetElementVisual(APPS_BOTTOMRIGHT);
        bottomRight.Opacity = 0;

        var topRight = ElementComposition.GetElementVisual(APPS_TOPRIGHT);
        topRight.Opacity = 0;

        topLeft.StartAnimationGroup(GetBlockAnimation(comp, false));
        bottomLeft.StartAnimationGroup(GetBlockAnimation(comp, true));
        bottomRight.StartAnimationGroup(GetBlockAnimation(comp, true));
        topRight.StartAnimationGroup(GetBlockAnimation2(comp));

        // FA
        var fa = ElementComposition.GetElementVisual(FA);
        fa.Opacity = 0;
        fa.CenterPoint = new Vector3((float) fa.Size.X / 2f,(float)  (fa.Size.Y / 2f - (fa.Size.Y * 0.1f)), (float) fa.CenterPoint.Z);
        fa.StartAnimationGroup(GetFAAnimation(comp));
    }

    private CompositionAnimationGroup GetGradientBarAnimation(Compositor comp, int step)
    {
        var ani = comp.CreateVector3KeyFrameAnimation();
        ani.Duration = TimeSpan.FromMilliseconds(600);

        ani.InsertKeyFrame(0f, Vector3.Zero);

        switch (step)
        {
            case 2:
                ani.InsertKeyFrame(0.15f, Vector3.Zero);
                break;

            case 3:
                ani.InsertKeyFrame(0.3f, Vector3.Zero);
                break;

            case 4:
                ani.InsertKeyFrame(0.45f, Vector3.Zero);
                break;
        }

        ani.InsertKeyFrame(1f, Vector3.One, new SplineEasing(0, 0, 0, 1));
        ani.Target = "Scale";

        var opacAni = comp.CreateScalarKeyFrameAnimation();
        opacAni.Target = "Opacity";
        opacAni.Duration = ani.Duration;

        opacAni.InsertKeyFrame(0f, 0);
        switch (step)
        {
            case 2:
                opacAni.InsertKeyFrame(0.15f, 0f);
                break;

            case 3:
                opacAni.InsertKeyFrame(0.3f, 0f);
                break;

            case 4:
                opacAni.InsertKeyFrame(0.45f, 0f);
                break;
        }

        opacAni.InsertKeyFrame(1f, 1f,new SplineEasing(0, 0, 0, 1));


        var group = comp.CreateAnimationGroup();
        group.Add(ani);
        group.Add(opacAni);

        return group;
    }

    private CompositionAnimationGroup GetBlockAnimation(Compositor comp, bool bottom)
    {
        var ani = comp.CreateVector3KeyFrameAnimation();
        ani.Duration = TimeSpan.FromMilliseconds(600);
        ani.Target = "Offset";
        var hgt = (float)Bounds.Height;
        ani.InsertKeyFrame(0f, new Vector3(0, -hgt, 0));

        if (!bottom)
        {
            ani.InsertKeyFrame(0.15f, new Vector3(0, -hgt, 0));
        }

        ani.InsertExpressionKeyFrame(1f, "this.FinalValue", new SplineEasing(0, 0, 0, 1));

        var opacAni = comp.CreateScalarKeyFrameAnimation();
        opacAni.Target = "Opacity";
        opacAni.Duration = ani.Duration;

        opacAni.InsertKeyFrame(0f, 0);
        opacAni.InsertKeyFrame(1f, 1f, new SplineEasing(0, 0, 0, 1));

        var group = comp.CreateAnimationGroup();
        group.Add(ani);
        group.Add(opacAni);

        return group;
    }

    private CompositionAnimationGroup GetBlockAnimation2(Compositor comp)
    {
        var ani = comp.CreateVector3KeyFrameAnimation();
        ani.Duration = TimeSpan.FromMilliseconds(300);
        ani.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
        ani.DelayTime = TimeSpan.FromMilliseconds(300);
        ani.Target = "Offset";
        var hgt = (float)Bounds.Height;
        ani.InsertKeyFrame(0f, new Vector3(0, -hgt, 0));
        //ani.SetVector3Parameter("BounceOffset", new Vector3(0, 25, 0));
        //ani.InsertExpressionKeyFrame(0.98f, "this.FinalValue + BounceOffset");
        ani.InsertExpressionKeyFrame(1f, "this.FinalValue", new ElasticEaseOut());

        var opacAni = comp.CreateScalarKeyFrameAnimation();
        opacAni.Target = "Opacity";
        opacAni.Duration = ani.Duration;
        opacAni.DelayBehavior = ani.DelayBehavior;
        opacAni.DelayTime = ani.DelayTime;

        opacAni.InsertKeyFrame(0f, 0);
        opacAni.InsertKeyFrame(1f, 1f, new SplineEasing(0, 0, 0, 1));

        var group = comp.CreateAnimationGroup();
        group.Add(ani);
        group.Add(opacAni);

        return group;
    }

    private CompositionAnimationGroup GetFAAnimation(Compositor comp)
    {
        var ani = comp.CreateVector3KeyFrameAnimation();
        ani.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
        ani.DelayTime = TimeSpan.FromMilliseconds(400);
        ani.Target = "Offset";
        ani.Duration = TimeSpan.FromMilliseconds(450);
        var hgt = (float)(Bounds.Height * 0.25f);
        ani.InsertKeyFrame(0f, new Vector3(0, -hgt, 0));
        ani.InsertExpressionKeyFrame(1f, "this.FinalValue", new SplineEasing(0.175,0.885,0.32,1.275));

        var sAni = comp.CreateVector3KeyFrameAnimation();
        sAni.Duration = ani.Duration;
        sAni.Target = "Scale";
        sAni.Duration = ani.Duration;
        sAni.DelayBehavior = ani.DelayBehavior;
        sAni.DelayTime = ani.DelayTime;
        sAni.InsertKeyFrame(0f, Vector3.Zero);
        sAni.InsertKeyFrame(1f, Vector3.One, new SplineEasing(0.175, 0.885, 0.32, 1.275));

        var opacAni = comp.CreateScalarKeyFrameAnimation();
        opacAni.Target = "Opacity";
        opacAni.Duration = ani.Duration;
        opacAni.DelayBehavior = ani.DelayBehavior;
        opacAni.DelayTime = ani.DelayTime;

        opacAni.InsertKeyFrame(0f, 0);
        opacAni.InsertKeyFrame(1f, 1f, new SplineEasing(0, 0, 0, 1));

        var group = comp.CreateAnimationGroup();
        group.Add(ani);
        group.Add(opacAni);
        group.Add(sAni);

        return group;
    }
}
