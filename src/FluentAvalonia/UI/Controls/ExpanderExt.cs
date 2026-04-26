using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Styling;
using FluentAvalonia.Core;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Special helper class to enable WinUI like animations on the Expander control
/// </summary>
public sealed class ExpanderExt : AvaloniaObject
{
    static ExpanderExt()
    {
        ExpanderAnimationTypeProperty.Changed.Subscribe(
            new SimpleObserver<AvaloniaPropertyChangedEventArgs>(HandleExpanderAnimationTypeChanged));
    }

    /// <summary>
    /// Defines the ExpanderAnimationType attached property
    /// </summary>
    public static readonly AttachedProperty<string> ExpanderAnimationTypeProperty =
        AvaloniaProperty.RegisterAttached<ExpanderExt, Expander, string>("ExpanderAnimationType");

    private static readonly AttachedProperty<ExpanderInfo> ExpanderAnimationInfoProperty =
        AvaloniaProperty.RegisterAttached<ExpanderExt, Expander, ExpanderInfo>("ExpanderAnimationInfo");

    /// <summary>
    /// Gets the current value of the <see cref="ExpanderAnimationTypeProperty"/>
    /// </summary>
    public static string GetExpanderAnimationType(Expander exp) =>
        exp.GetValue(ExpanderAnimationTypeProperty);

    /// <summary>
    /// Sets the current value of the <see cref="ExpanderAnimationTypeProperty"/>
    /// </summary>
    public static void SetExpanderAnimationType(Expander exp, string value) =>
        exp.SetValue(ExpanderAnimationTypeProperty, value);

    private static void HandleExpanderAnimationTypeChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var val = args.GetNewValue<string>();
        var expander = args.Sender as Expander;

        if (expander == null)
            return;

        if (val != null && val.Equals(s_Fluentv2, StringComparison.OrdinalIgnoreCase))
        {
            expander.SetValue(ExpanderAnimationInfoProperty, new ExpanderInfo(expander));
        }
        else
        {
            var info = expander.GetValue(ExpanderAnimationInfoProperty);
            info?.Dispose();
            expander.ClearValue(ExpanderAnimationInfoProperty);
        }
    }
        

    private static readonly string s_Fluentv2 = "FluentV2";

    private class ExpanderInfo : IDisposable
    {
        public ExpanderInfo(Expander expander)
        {
            _expander = expander;

            expander.TemplateApplied += HandleExpanderTemplateApplied;
            _expandedChangedNotice = expander.GetPropertyChangedObservable(Expander.IsExpandedProperty)
                .Subscribe(new SimpleObserver<AvaloniaPropertyChangedEventArgs>(HandleIsExpandedChanged));
        }

        private void HandleExpanderTemplateApplied(object sender, TemplateAppliedEventArgs e)
        {
            if (_expanderContent != null)
            {
                _expanderContent.SizeChanged -= HandleContentSizeChanged;
            }

            var expanderContentClip = e.NameScope.Get<Border>("ExpanderContentClip");
            var visual = ElementComposition.GetElementVisual(expanderContentClip);
            // WinUI uses an actual CompositionClip here (CreateInsetClip())
            // but we don't have that so just clip to bounds for now
            visual.ClipToBounds = true;

            var expanderContent = e.NameScope.Get<Border>("ExpanderContent");
            SetExpanderContent(expanderContent);

            UpdateExpandState(false);
        }

        private void SetExpanderContent(Border expanderContent)
        {
            _expanderContent = expanderContent;
            expanderContent.SizeChanged += HandleContentSizeChanged;
        }

        private void HandleContentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _contentSize = e.NewSize;
        }

        private void HandleIsExpandedChanged(AvaloniaPropertyChangedEventArgs args)
        {
            UpdateExpandState(true);
        }

        private void UpdateExpandState(bool useTransitions)
        {
            var expanded = _expander.IsExpanded;

            if (useTransitions && _expanderContent == null)
                useTransitions = false;

            var pc = _expander.Classes as IPseudoClasses;
            pc.Set(":noAnimation", !useTransitions);
            pc.Set(":expanded", expanded);

            if (useTransitions)
            {
                var direction = _expander.ExpandDirection;

                if (expanded)
                {
                    switch (direction)
                    {
                        case ExpandDirection.Down:
                        case ExpandDirection.Up:
                            RunExpandDownUpAnimation(direction == ExpandDirection.Down);
                            break;

                        case ExpandDirection.Left:
                        case ExpandDirection.Right:
                            RunExpandLeftRightAnimation(direction == ExpandDirection.Right);
                            break;
                    }                    
                }
                else
                {
                    switch (direction)
                    {
                        case ExpandDirection.Down:
                        case ExpandDirection.Up:
                            RunCollapseDownUpAnimation(direction == ExpandDirection.Down);
                            break;

                        case ExpandDirection.Left:
                        case ExpandDirection.Right:
                            RunCollapseLeftRightAnimation(direction == ExpandDirection.Right);
                            break;
                    }                    
                }
            }
        }

        private async void RunExpandDownUpAnimation(bool down)
        {
            _expanderContent.SetCurrentValue(Visual.IsVisibleProperty, true);

            if (_expander.Parent is SettingsExpander se && se.Presenter != null)
            {
                // SettingsExpander does not use Virtualization, so it's safe here to use
                // Infinity to measure
                se.Presenter.Measure(Size.Infinity);
                _contentSize = se.Presenter.DesiredSize;
            }
            else
            {
                _expanderContent.Measure(Size.Infinity);
                _contentSize = _expanderContent.DesiredSize;
            }
                
            var startY = down ? -_contentSize.Height : _contentSize.Height;
            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(333),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.Zero,
                        Setters =
                        {
                            new Setter(Visual.IsVisibleProperty, true),
                            new Setter(TranslateTransform.YProperty, startY)
                        }
                    },
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.FromMilliseconds(333),
                        Setters =
                        {
                            new Setter(TranslateTransform.YProperty, 0d)
                        },
                        KeySpline = new KeySpline(0,0,0,1)
                    }
                }
            };

            await ani.RunAsync(_expanderContent);
        }

        private async void RunCollapseDownUpAnimation(bool down)
        {
            var endY = down ? -_contentSize.Height : _contentSize.Height;
            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(167),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.Zero,
                        Setters =
                        {
                            new Setter(TranslateTransform.YProperty, 0d)
                        }
                    },
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.FromMilliseconds(167),
                        Setters =
                        {
                            new Setter(TranslateTransform.YProperty, endY),
                            new Setter(Visual.IsVisibleProperty, false)
                        },
                        KeySpline = new KeySpline(1,1,0,1)
                    }
                }
            };

            await ani.RunAsync(_expanderContent);

            _expanderContent.SetValue(Visual.IsVisibleProperty, false);
        }

        private async void RunExpandLeftRightAnimation(bool right)
        {
            _expanderContent.SetCurrentValue(Visual.IsVisibleProperty, true);
            _expanderContent.Measure(Size.Infinity);
            _contentSize = _expanderContent.DesiredSize;
            var startX = right ? -_contentSize.Width : _contentSize.Width;
            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(333),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.Zero,
                        Setters =
                        {
                            new Setter(Visual.IsVisibleProperty, true),
                            new Setter(TranslateTransform.XProperty, startX)
                        }
                    },
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.FromMilliseconds(333),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, 0d)
                        },
                        KeySpline = new KeySpline(0,0,0,1)
                    }
                }
            };

            await ani.RunAsync(_expanderContent);
        }

        private async void RunCollapseLeftRightAnimation(bool right)
        {
            var endX = right ? -_contentSize.Width : _contentSize.Width;
            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(167),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.Zero,
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, 0d)
                        }
                    },
                    new KeyFrame
                    {
                        KeyTime = TimeSpan.FromMilliseconds(167),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, endX),
                            new Setter(Visual.IsVisibleProperty, false)
                        },
                        KeySpline = new KeySpline(1,1,0,1)
                    }
                }
            };

            await ani.RunAsync(_expanderContent);

            _expanderContent.SetValue(Visual.IsVisibleProperty, false);
        }

        public void Dispose()
        {
            _expandedChangedNotice?.Dispose();
            _expander.TemplateApplied -= HandleExpanderTemplateApplied;

            if (_expanderContent != null)
            {
                _expanderContent.SizeChanged -= HandleContentSizeChanged;
            }
        }

        private readonly Expander _expander;
        private Border _expanderContent;
        private Size _contentSize;
        private IDisposable _expandedChangedNotice;
    }
}
