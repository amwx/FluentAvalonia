// NOTE: This class is experimental and will not receive support.
// Use at your own risk

using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls.Experimental;

public class ConnectedAnimationService
{
    internal ConnectedAnimationService(TopLevel topLevel)
    {
        _topLevel = new WeakReference<TopLevel>(topLevel);
        _animations = new Dictionary<string, ConnectedAnimation>();
        DefaultDuration = TimeSpan.FromMilliseconds(300);
        DefaultEasingFunction = new SplineEasing(0.8, 0, 0.2, 1);
    }

    internal static readonly AttachedProperty<ConnectedAnimationService> ConnectedAnimationServiceProperty =
        AvaloniaProperty.RegisterAttached<ConnectedAnimationService, TopLevel, ConnectedAnimationService>(
            nameof(ConnectedAnimationService));

    public TimeSpan DefaultDuration { get; set; } = TimeSpan.FromMilliseconds(3000);

    public Easing DefaultEasingFunction { get; set; } = new SplineEasing(0.8,0,0.2,1);

    public static ConnectedAnimationService GetForView(TopLevel topLevel)
    {
        var service = topLevel.GetValue(ConnectedAnimationServiceProperty);

        if (service == null)
        {
            service = new ConnectedAnimationService(topLevel);
            topLevel.SetValue(ConnectedAnimationServiceProperty, service);
        }

        return service;
    }

    public ConnectedAnimation GetAnimation(string key)
    {
        if (_animations.TryGetValue(key, out var animation))
        {
            _animations.Remove(key);
            return animation;
        }

        return null;
    }

    public ConnectedAnimation PrepareToAnimate(string key, Visual source)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Invalid key specified");

        var animation = new ConnectedAnimation(source, this);

        // If the specified key already exists, just replace the ConnectedAnimation as its
        // likely the animation never got called for some reason and is now invalid. 
        if (_animations.ContainsKey(key))
        {
            _animations[key] = animation;
        }
        else
        {
            _animations.Add(key, animation);
        }

        return animation;
    }

    private WeakReference<TopLevel> _topLevel;
    private Dictionary<string, ConnectedAnimation> _animations;
}
