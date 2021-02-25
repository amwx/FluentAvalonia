using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
    public enum DialogResult
    {
        OK,
        Cancel
    }

    public class ColorDialog : TemplatedControl
    {
        public static readonly StyledProperty<Color> ColorProperty =
            AvaloniaProperty.Register<ColorDialog, Color>("Color", defaultValue: Colors.Black);

        public static readonly DirectProperty<ColorDialog, bool> UseLightDismissProperty =
            AvaloniaProperty.RegisterDirect<ColorDialog, bool>("UseLightDismiss",
                x => x.UseLightDismiss, (x, v) => x.UseLightDismiss = v);

        public Color Color
        {
            get => GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public bool UseLightDismiss
        {
            get => _useLightDismiss;
            set => SetAndRaise(UseLightDismissProperty, ref _useLightDismiss, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _okButton = e.NameScope.Find<Button>("OKButton");
            _okButton.Click += OnConfirm;
            _cancelButton = e.NameScope.Find<Button>("CancelButton");
            _cancelButton.Click += OnCancel;

            _picker = e.NameScope.Find<ColorPicker>("ColorPicker");
            //Binding doesn't work between Color2 & Color, so we have to manually manage this
            //They are implicitly convertible tho, but binding doesn't like that
            //one Plus of this is we can't change the color of the dialog accidentally if we 
            //don't confirm the selection
            _picker.Color = Color;
        }

        private void OnCancel(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            
            this.IsVisible = false;
            var wnd = this.GetLogicalParent<Window>();
            if (wnd != null)
                wnd.Close();

            _tcs.SetResult(DialogResult.Cancel);
        }

        private void OnConfirm(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {            
            Color = _picker.Color;
            this.IsVisible = false;
            var wnd = this.GetLogicalParent<Window>();
            if (wnd != null)
                wnd.Close();

            _tcs.SetResult(DialogResult.OK);
        }

        public async Task<DialogResult> ShowDialog(Control showAt = null)
        {
            _tcs = new TaskCompletionSource<DialogResult>();
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime c)
            {
                Window cur = null;
                for (int i = 0; i < c.Windows.Count; i++)
                {
                    if (c.Windows[i].IsActive)
                    {
                        cur = c.Windows[i];
                        break;
                    }    
                }

                if (cur == null)
                {
                    cur = c.MainWindow;
                }

                Window w = new Window();
                w.Content = this;
                w.SizeToContent = SizeToContent.WidthAndHeight;
                w.Topmost = true;
                w.Background = null; //Don't waste by drawing background
                w.SystemDecorations = SystemDecorations.None;
                w.ShowInTaskbar = false;
                w.TransparencyLevelHint = WindowTransparencyLevel.Transparent;

                if (cur != null)
                {
                    cur.Closed += (s, e) =>
                    {
                        w.Close();
                    };
                    cur.PlatformImpl.WindowStateChanged += (x) =>
                    {
                        if (x == WindowState.Minimized)
                        {
                            w.IsVisible = false;
                        }
                        else
                        {
                            w.IsVisible = true;
                        }
                    };
                }

                if (showAt == null)
                {
                    w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    w.WindowStartupLocation = WindowStartupLocation.Manual;
                    w.Position = showAt.PointToScreen(new Point(0, 0));
                }

                w.ShowInTaskbar = false;

                w.Closed += DialogClosed;
                this.IsVisible = true;

                if (cur != null)
                {
                    w.Show(cur);
                }
                else
                {
                    w.Show();
                }

                if (_picker != null)
                {
                    _picker.Color = Color;
                }

                await _tcs.Task;
            }

            return _tcs.Task.Result;
        }

        private void DialogClosed(object sender, System.EventArgs e)
        {
            //_tcs.SetResult(Color); ;
        }

        private bool _useLightDismiss;
        private TaskCompletionSource<DialogResult> _tcs;
        private Button _okButton;
        private Button _cancelButton;
        private ColorPicker _picker;
    }
}
