using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;

namespace FluentAvalonia.UI.Media.Animation
{
    //NOTE: These are custom implementations of these classes as the source is still closed...
    public abstract class NavigationTransitionInfo : AvaloniaObject
    {
        public abstract void RunAnimation(Animatable ctrl);
    }

    public enum SlideNavigationTransitionEffect
    {
        FromLeft,
        FromRight,
        FromTop,
        FromBottom
    }

    public class SlideNavigationTransitionInfo : NavigationTransitionInfo
    {
        public SlideNavigationTransitionEffect Effect { get; set; } = SlideNavigationTransitionEffect.FromRight;

        public double FromHorizontalOffset { get; set; } = 56;
        public double FromVerticalOffset { get; set; } = 56;

        public async override void RunAnimation(Animatable ctrl)
        {
            double length = 0;
            bool isVertical = false;
            switch (Effect)
            {
                case SlideNavigationTransitionEffect.FromLeft:
                    length = -FromHorizontalOffset;
                    break;
                case SlideNavigationTransitionEffect.FromRight:
                    length = FromHorizontalOffset;
                    break;
                case SlideNavigationTransitionEffect.FromTop:
                    length = -FromVerticalOffset;
                    isVertical = true;
                    break;
                case SlideNavigationTransitionEffect.FromBottom:
                    length = FromVerticalOffset;
                    isVertical = true;
                    break;
            }

            var animation = new Avalonia.Animation.Animation
            {
                Easing = new SplineEasing(0.1, 0.9, 0.2, 1.0),
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(isVertical ? TranslateTransform.YProperty : TranslateTransform.XProperty, length),
							new Setter(Visual.OpacityProperty, 0d)
                        },
                        Cue = new Cue(0d)
                    },
					new KeyFrame
					{
						Setters=
						{
							new Setter(Visual.OpacityProperty, 1d)
						},
						Cue = new Cue(0.05d)
					},
                    new KeyFrame
                    {
                        Setters =
                        {
							new Setter(Visual.OpacityProperty, 1d),
							new Setter(isVertical ? TranslateTransform.YProperty : TranslateTransform.XProperty, 0.0)
                        },
                        Cue = new Cue(1d)
                    }
                },
                Duration = TimeSpan.FromSeconds(0.167),
				FillMode = FillMode.Forward
			};

            await animation.RunAsync(ctrl);

			(ctrl as IVisual).Opacity = 1;
		}
    }

    public class EntranceNavigationTransitionInfo : NavigationTransitionInfo
    {
        public double FromHorizontalOffset { get; set; } = 0;
        public double FromVerticalOffset { get; set; } = 28;

        //SlideUp and FadeIn
        public async override void RunAnimation(Animatable ctrl)
        {
            var animation = new Avalonia.Animation.Animation
            {
                Easing = new SplineEasing(0.1,0.9,0.2,1.0),
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(Visual.OpacityProperty, 0.0),
                            new Setter(TranslateTransform.XProperty,FromHorizontalOffset),
                            new Setter(TranslateTransform.YProperty, FromVerticalOffset)
                        },
                        Cue = new Cue(0d)
                    },
					new KeyFrame
                    {
                        Setters =
                        {
							new Setter(Visual.OpacityProperty, 1d),
							new Setter(TranslateTransform.XProperty,0.0),
                            new Setter(TranslateTransform.YProperty, 0.0)
                        },
                        Cue = new Cue(1d)
                    }
                },
                Duration = TimeSpan.FromSeconds(0.67), 
				FillMode = FillMode.Forward
            };

            await animation.RunAsync(ctrl);

			(ctrl as IVisual).Opacity = 1;
        }
    }

    public class DrillInNavigationTransitionInfo : NavigationTransitionInfo
    {
        public bool IsReversed { get; set; } = false; //Zoom out if true

        //Zoom & Fade
        public async override void RunAnimation(Animatable ctrl)
        {
            var animation = new Avalonia.Animation.Animation
            {
                Easing = new SplineEasing(0.1, 0.9, 0.2, 1.0),
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(Visual.OpacityProperty, 0.0),
                            new Setter(ScaleTransform.ScaleXProperty, IsReversed ? 1.5 : 0.0),
                            new Setter(ScaleTransform.ScaleYProperty, IsReversed ? 1.5 : 0.0)
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(Visual.OpacityProperty, 1.0),
                            new Setter(ScaleTransform.ScaleXProperty, IsReversed ? 1.0 : 1.0),
                            new Setter(ScaleTransform.ScaleYProperty, IsReversed ? 1.0 : 1.0)
                        },
                        Cue = new Cue(1d)
                    }
                },
                Duration = TimeSpan.FromSeconds(0.67),
				FillMode = FillMode.Forward
			};

            await animation.RunAsync(ctrl);

			(ctrl as IVisual).Opacity = 1;
		}
    }

    public class SuppressNavigationTransitionInfo : NavigationTransitionInfo
    {
        public override void RunAnimation(Animatable ctrl)
        {
			//Do nothing
			(ctrl as IVisual).Opacity = 1;
		}
    }

}
