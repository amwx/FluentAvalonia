using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

namespace FluentAvaloniaSamples.Pages
{
    public class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            _headerRightContent = this.FindControl<Control>("HeaderRightContent");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == BoundsProperty)
            {
                if (_headerRightContent != null)
                    HandleAdaptiveWidth(change.GetNewValue<Rect>().Width);
            }
        }

        private async void HandleAdaptiveWidth(double width)
        {
            if (width < _adaptiveTriggerWidth && !_isInSmallMode)
            {
                _isInSmallMode = true;
                Grid.SetColumn(_headerRightContent, 0);
                Grid.SetRow(_headerRightContent, 1);
                _headerRightContent.Opacity = 0;

                RunConnectedAnimation(300, -75);                
            }
            else if (width > _adaptiveTriggerWidth && _isInSmallMode)
            {
                _isInSmallMode = false;
                Grid.SetColumn(_headerRightContent, 1);
                Grid.SetRow(_headerRightContent, 0);
                _headerRightContent.Opacity = 0;

                RunConnectedAnimation(-175, 45);
            }
        }

        private async void RunConnectedAnimation(double startX, double startY)
        {
            if (_cts != null)
			{
                _cts?.Cancel();
                _cts?.Dispose();
                _headerRightContent.Opacity = 1;
                _cts = null;
			}

            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(167),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(OpacityProperty, 0d),
                            new Setter(TranslateTransform.XProperty, startX),
                            new Setter(TranslateTransform.YProperty, startY)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters =
                        {
                            new Setter(OpacityProperty, 1d),
                            new Setter(TranslateTransform.XProperty, 0d),
                            new Setter(TranslateTransform.YProperty, 0d)
                        },
                        KeySpline = new KeySpline(0,0,0,1)
                    }
                }
            };

            _cts = new CancellationTokenSource();
            await ani.RunAsync(_headerRightContent, null, cancellationToken: _cts.Token);
            _cts.Dispose();
            _cts = null;
            _headerRightContent.Opacity = 1;
        }

        private CancellationTokenSource _cts;
        private bool _isInSmallMode;
        private Control _headerRightContent;
        private readonly double _adaptiveTriggerWidth = 665;
    }
}
