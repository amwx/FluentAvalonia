// NOTE: This class is experimental and will not receive support.
// Use at your own risk

using System.Diagnostics;
using System.Numerics;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;
using FluentAvalonia.Core.Attributes;

namespace FluentAvalonia.UI.Controls.Experimental;

public class ConnectedAnimation
{
    internal ConnectedAnimation(Visual source, ConnectedAnimationService service)
    {
        Configuration = new GravityConnectedAnimationConfiguration();
        IsScaleAnimationEnabled = true;

        _owningService = service;
        _sourceElement = source;
        var compVis = ElementComposition.GetElementVisual(source);
        if (compVis == null)
            throw new InvalidOperationException("No CompositionVisual found for source element");

        var bnds = source.GetTransformedBounds();
        var tBounds = bnds.Value.Bounds.TransformToAABB(bnds.Value.Transform);

        _initialSize = new Vector2(
            (float)tBounds.Width,
            (float)tBounds.Height);
        _initialOffset = new Vector3((float)tBounds.X, (float)tBounds.Y, 0);

        _initialOpacity = compVis.Opacity;
        CreationTime = DateTime.Now;
    }

    public ConnectedAnimationConfiguration Configuration { get; set; }

    public bool IsScaleAnimationEnabled { get; set; }

    internal DateTime CreationTime { get; }

    [NotImplemented]
    public void Cancel() { }

    public bool TryStart(Visual destination)
    {
        return TryStart(destination, null);
    }

    public bool TryStart(Visual destination, IList<Visual> coordinatedVisuals)
    {
        if (_sourceElement == destination)
            return false;

        // WinUI expires Connected Animations after 300ms - we'll do the same
        var now = DateTime.Now;
        var delta = now - CreationTime;
        if (delta.TotalMilliseconds > 300)
            return false;

        ConfigureConnectedAnimation(destination, coordinatedVisuals);

        return true;
    }

    private void ConfigureConnectedAnimation(Visual destination, IList<Visual> coordinatedVisuals = null)
    {
        var destVis = ElementComposition.GetElementVisual(destination);
        if (destVis == null)
            throw new Exception("No composition visual found for destination element");

        // TODO: Gravity should respect the default easing and timings with 
        // the gravity step having a special easing to simulate the gravity effect
        var easing = Configuration is GravityConnectedAnimationConfiguration ?
            new QuarticEaseInOut() : _owningService.DefaultEasingFunction;

        var duration = Configuration is GravityConnectedAnimationConfiguration ?
            TimeSpan.FromMilliseconds(750) : _owningService.DefaultDuration;

        var comp = destVis.Compositor;

        var bnds = destination.GetTransformedBounds();
        var tBounds = bnds.Value.Bounds.TransformToAABB(bnds.Value.Transform);

        var finalSize = new Vector2(
            (float)tBounds.Width,
            (float)tBounds.Height);
        var finalOffset = new Vector3((float)tBounds.X, (float)tBounds.Y, 0);

        var delta = _initialOffset - finalOffset;

        var group = comp.CreateAnimationGroup();

        if (_initialOpacity != destVis.Opacity)
        {
            var finalOpacity = destVis.Opacity;
            var opacAnim = comp.CreateScalarKeyFrameAnimation();
            opacAnim.Target = "Opacity";
            opacAnim.Duration = duration;
            opacAnim.SetScalarParameter("StartValue", _initialOpacity);
            opacAnim.SetScalarParameter("FinalValue", finalOpacity);
            opacAnim.InsertExpressionKeyFrame(0, "StartValue");
            opacAnim.InsertExpressionKeyFrame(1, "FinalValue", easing);
            group.Add(opacAnim);
        }

        var offsetAnim = comp.CreateVector3KeyFrameAnimation();
        offsetAnim.Target = "Offset";
        offsetAnim.Duration = duration;

        offsetAnim.SetVector3Parameter("StartValue", destVis.Offset + delta);
        offsetAnim.SetVector3Parameter("FinalValue", destVis.Offset);
        offsetAnim.InsertExpressionKeyFrame(0, "StartValue");

        if (Configuration is GravityConnectedAnimationConfiguration)
        {
            // Scale Gravity paramter here by distance so that gravity isn't overly exaggerated 
            // if the targets are close together
            // TODO: If start is above end, have the gravity fall below at the end of the animation
            offsetAnim.SetScalarParameter("Gravity", MathF.Abs(finalOffset.Y - _initialOffset.Y) * 0.1f);
            offsetAnim.InsertExpressionKeyFrame(0.1f, "Vector3(StartValue.X, StartValue.Y + Gravity, 0)");
        }

        offsetAnim.InsertExpressionKeyFrame(1, "FinalValue", easing);
        group.Add(offsetAnim);

        if (IsScaleAnimationEnabled)
        {
            var sx = _initialSize.X / finalSize.X;
            var sy = _initialSize.Y / finalSize.Y;

            var scaleAnim = comp.CreateVector3KeyFrameAnimation();
            scaleAnim.Target = "Scale";
            scaleAnim.Duration = duration;

            scaleAnim.SetVector3Parameter("StartValue", new Vector3(sx, sy, 1));
            scaleAnim.SetVector3Parameter("FinalValue", new Vector3(1, 1, 1));
            scaleAnim.InsertExpressionKeyFrame(0, "StartValue");
            scaleAnim.InsertExpressionKeyFrame(1, "FinalValue", easing);
            group.Add(scaleAnim);
        }

        if (coordinatedVisuals != null)
        {
            for (int i = 0; i < coordinatedVisuals.Count; i++)
            {
                CreateCoordinatedAnimation(coordinatedVisuals[i], duration, easing);
            }
        }

        destVis.StartAnimationGroup(group);
    }

    private void CreateCoordinatedAnimation(Visual element, TimeSpan duration, Easing easing)
    {
        var destVis = ElementComposition.GetElementVisual(element);
        // For coordinated visuals, if the CompositionVisual isn't available,
        // just skip it instead of failing
        if (destVis == null)
            return;

        var comp = destVis.Compositor;

        var bnds = element.GetTransformedBounds();
        var tBounds = bnds.Value.Bounds.TransformToAABB(bnds.Value.Transform);

        var finalOffset = new Vector3((float)tBounds.X, (float)tBounds.Y, 0);
        var delta = _initialOffset - finalOffset;

        var group = comp.CreateAnimationGroup();

        // Coordinated visuals always have an opacity and we hold that for most of the original
        // animation to give the focus on the primary object and these "settle in" later
        var finalOpacity = destVis.Opacity;
        var opacAnim = comp.CreateScalarKeyFrameAnimation();
        opacAnim.Target = "Opacity";
        opacAnim.Duration = duration;
        opacAnim.DelayBehavior = Avalonia.Rendering.Composition.Animations.AnimationDelayBehavior.SetInitialValueBeforeDelay;
        //opacAnim.De
        opacAnim.SetScalarParameter("StartValue", 0);
        opacAnim.SetScalarParameter("FinalValue", finalOpacity);
        opacAnim.InsertExpressionKeyFrame(0, "StartValue");
        opacAnim.InsertExpressionKeyFrame(0.5f, "StartValue");
        opacAnim.InsertExpressionKeyFrame(1, "FinalValue", easing);
        group.Add(opacAnim);

        var offsetAnim = comp.CreateVector3KeyFrameAnimation();
        offsetAnim.Target = "Offset";
        offsetAnim.Duration = duration;
        offsetAnim.SetVector3Parameter("StartValue", destVis.Offset + delta);
        offsetAnim.SetVector3Parameter("FinalValue", destVis.Offset);
        offsetAnim.InsertExpressionKeyFrame(0, "StartValue");
        offsetAnim.InsertExpressionKeyFrame(1, "FinalValue", easing);
        group.Add(offsetAnim);

        // Never run the scale on the coordinated animation - just opacity and offset

        destVis.StartAnimationGroup(group);
    }

    private ConnectedAnimationService _owningService;
    private Visual _sourceElement;
    private float _initialOpacity;
    private Vector3 _initialOffset;
    private Vector2 _initialSize;
}
