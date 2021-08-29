using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using FluentAvalonia.Interop;
using FluentAvalonia.UI.Media;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FluentAvalonia.Styling
{
    public class FluentAvaloniaTheme : IStyle, IResourceProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentTheme"/> class.
        /// </summary>
        /// <param name="baseUri">The base URL for the XAML context.</param>
        public FluentAvaloniaTheme(Uri baseUri)
        {
            _baseUri = baseUri;
			Register();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentTheme"/> class.
        /// </summary>
        /// <param name="serviceProvider">The XAML service provider.</param>
        public FluentAvaloniaTheme(IServiceProvider serviceProvider)
        {
            _baseUri = ((IUriContext)serviceProvider.GetService(typeof(IUriContext))).BaseUri;
			Register();
		}

        /// <summary>
        /// Gets or sets the mode of the fluent theme (light, dark).
        /// </summary>
        public string RequestedTheme
        {
            get => _mode;
            set
            {
                if (_mode != value)
                {
                    _mode = value;
										
					RequestedThemeChanged?.Invoke(this, value);
					
                    if (_loaded != null)
					{
						var thm = value.Equals("Light", StringComparison.OrdinalIgnoreCase) ? "Light" : "Dark";
						_isLoading = true;
						var old = _loaded[2];
						//Only switch the third item in loaded (light/dark theme)
						//Everything else remains the same
						(old as IResourceProvider)?.RemoveOwner(Owner);
						_loaded[2] = null;
						var newStyles = (IStyle)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/StylesV{ControlsVersion}/{thm}Resources.axaml", UriKind.Absolute), _baseUri);
						_isLoading = false;

						_loaded[2] = newStyles;
						(newStyles as IResourceProvider)?.AddOwner(Owner);
						
					}

					//Apply the update (this is at app level & will propagate through...)
					Owner?.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Empty);
                }
            }
        }

		public int ControlsVersion
		{
			get => _controlsVersion;
			set
			{
				if (value != _controlsVersion)
				{
					_controlsVersion = value;
					Init(true);
					Owner?.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Sets whether ContentControlThemeFontFamily should be changed to SegoeUI on windows systems
		/// This value is only respected on startup
		/// </summary>
		public bool UseSegoeUIOnWindows { get; set; } = true;

		/// <summary>
		/// Gets or sets whether the SystemAccentColor resources should be updated to use the current
		/// user's preferred colors. Windows 10 only.
		/// This value is only respected on startup
		/// </summary>
		public bool GetUserAccentColor { get; set; } = true;

		/// <summary>
		/// Gets or sets whether the default theme (Light or Dark) should default to the User's
		/// current theme. Windows 10 only.
		/// This value is only respected on startup
		/// </summary>
		public bool DefaultToUserTheme { get; set; } = true;

		public Color? CustomAccentColor
		{
			get => _customAccentColor;
			set
			{
				if (_customAccentColor != value)
				{
					Logger.TryGet(LogEventLevel.Information, "CustomAccentColor")?
						.Log("CustomAccentColor", "NOTE: FluentAvaloniaTheme auto generates the 3 light/dark variants. HOWEVER, no checking is done to ensure the accent color or variants meet accessibility standards. That is left for you to decide. Be sure to check your accent colors!");
					_customAccentColor = value;

					if (_loaded != null && _loaded.Length > 1)
					{
						GenerateCustomAccentColor(value);
						Owner?.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Empty);
					}
				}
			}
		}

        public IResourceHost Owner
		{
			get => _owner;
			set
			{
				if (_owner != value)
				{
					_owner = value;
					OwnerChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

        bool IResourceNode.HasResources
		{
			get
			{
				for (int i = 0; i < _loaded.Length; i++)
				{
					if (_loaded[i] is IResourceProvider rp && rp.HasResources)
						return true;
				}

				return false;
			}
		}

        public IReadOnlyList<IStyle> Children
		{
			get
			{
				if (_loaded == null)
					Init();

				return _loaded;
			}
		}

		public event EventHandler OwnerChanged;
        

		public event EventHandler<string> RequestedThemeChanged;

        public SelectorMatchResult TryAttach(IStyleable target, IStyleHost host)
		{
			_cache ??= new Dictionary<Type, List<IStyle>>();

			if (_cache.TryGetValue(target.StyleKey, out var cached))
			{
				if (cached is object)
				{
					foreach (var style in cached)
					{
						style.TryAttach(target, host);
					}

					return SelectorMatchResult.AlwaysThisType;
				}
				else
				{
					return SelectorMatchResult.NeverThisType;
				}
			}
			else
			{
				List<IStyle> matches = null;

				foreach (var child in Children)
				{
					if (child.TryAttach(target, host) != SelectorMatchResult.NeverThisType)
					{
						matches ??= new List<IStyle>();
						matches.Add(child);
					}
				}

				_cache.Add(target.StyleKey, matches);

				return matches is null ?
					SelectorMatchResult.NeverThisType :
					SelectorMatchResult.AlwaysThisType;
			}
		}

        public bool TryGetResource(object key, out object value)
        {
            if (!_isLoading)
			{
				for (int i = Children.Count - 1; i >= 0; i--)
				{
					if (Children[i] is IResourceProvider rp && rp.TryGetResource(key, out value))
						return true;
				}
			}

            value = null;
            return false;
        }

        void IResourceProvider.AddOwner(IResourceHost owner)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");

			if (Owner != null)
				throw new InvalidOperationException("An Owner already exists");

			Owner = owner;

			for (int i = 0; i < Children.Count; i++)
			{
				if (Children[i] is IResourceProvider rp)
				{
					rp.AddOwner(owner);
				}
			}
		}

        void IResourceProvider.RemoveOwner(IResourceHost owner)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");

			if (Owner == owner)
			{
				Owner = null;

				for (int i = 0; i < Children.Count; i++)
				{
					if (Children[i] is IResourceProvider rp)
					{
						rp.RemoveOwner(owner);
					}
				}
			}
		}

		public void InvalidateThemingFromSystemThemeChange()
		{
			InitIfNecessary();
		}

		/// <summary>
		/// Attempts to force the Native Win32 titlebar to adjust to the current theme (light or dark)
		/// If a custom theme is in place, set theme arg to 'Light' or 'Dark'. Otherwise, defaults to light
		/// </summary>
		/// <param name="w"></param>
		/// <param name="theme"></param>
		public void ForceNativeTitleBarToTheme(Window w, string theme = null)
		{
			Contract.Requires<ArgumentNullException>(w != null);

			Win32Interop.OSVERSIONINFOEX osInfo = new Win32Interop.OSVERSIONINFOEX { OSVersionInfoSize = Marshal.SizeOf(typeof(Win32Interop.OSVERSIONINFOEX)) };
			Win32Interop.RtlGetVersion(ref osInfo);

			if (string.IsNullOrEmpty(theme))
			{
				theme = IsValidRequestedTheme(RequestedTheme) ? RequestedTheme : "Light";
			}
			else
			{
				theme = IsValidRequestedTheme(theme) ? theme : IsValidRequestedTheme(RequestedTheme) ? RequestedTheme : "Light";
			}

			Win32Interop.ApplyTheme(w.PlatformImpl.Handle.Handle, theme.Equals("Dark", StringComparison.OrdinalIgnoreCase), osInfo);
		}

		private bool IsValidRequestedTheme(string thm)
		{
			if ("Light".Equals(thm, StringComparison.OrdinalIgnoreCase) ||
				"Dark".Equals(thm, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return false;
		}

		private void Init(bool clear = false)
		{
			_isLoading = true;

			if (clear && _loaded != null) //This is a complete refresh, e.g., changing Version #
			{
				for (int i = 0; i < _loaded.Length; i++)
				{
					(_loaded[i] as IResourceProvider)?.RemoveOwner(Owner);
				}
				_loaded = null;
			}

			//Make sure we don't get an invalid version number
			var version = ControlsVersion > 2 || ControlsVersion < 1 ? 2 : ControlsVersion;
			string thm = GetTheme();

			_loaded = new[]
			{
				(IStyle)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/StylesV{version}/AccentColors.axaml", UriKind.Absolute), _baseUri),
				(IStyle)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/StylesV{version}/BaseResources.axaml", UriKind.Absolute), _baseUri),
				(IStyle)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/StylesV{version}/{thm}Resources.axaml", UriKind.Absolute), _baseUri),
				(IStyle)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/StylesV{version}/Controls.axaml", UriKind.Absolute), _baseUri),
			};

			InitIfNecessary();
			_isLoading = false;
		}

		private void Register()
		{
			//For easy access, save to AvaloniaLocator
			AvaloniaLocator.CurrentMutable.Bind<FluentAvaloniaTheme>().ToConstant(this);
		}

		private void InitIfNecessary()
		{
			if (!UseSegoeUIOnWindows && !GetUserAccentColor && !DefaultToUserTheme && CustomAccentColor == null)
				return;

			if (CustomAccentColor != null)
			{
				GenerateCustomAccentColor(_customAccentColor);
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{				
				if (UseSegoeUIOnWindows)
				{
					//This is defined in the BaseResources.axaml file
					(_loaded[1] as Styles).Resources["ContentControlThemeFontFamily"] = new FontFamily("Segoe UI");
				}

				if (CustomAccentColor == null && GetUserAccentColor)
				{
					//These are defined in the AccentColors.axaml file
					(_loaded[0] as Styles).Resources["SystemAccentColor"] = Win32Interop.GetThemeColorRef("ImmersiveSystemAccent");
					(_loaded[0] as Styles).Resources["SystemAccentColorLight1"] = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentLight1");
					(_loaded[0] as Styles).Resources["SystemAccentColorLight2"] = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentLight2");
					(_loaded[0] as Styles).Resources["SystemAccentColorLight3"] = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentLight3");
					(_loaded[0] as Styles).Resources["SystemAccentColorDark1"] = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentDark1");
					(_loaded[0] as Styles).Resources["SystemAccentColorDark2"] = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentDark2");
					(_loaded[0] as Styles).Resources["SystemAccentColorDark3"] = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentDark3");
				}
			}		
		}

		private string GetTheme()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && DefaultToUserTheme)
			{
				Win32Interop.OSVERSIONINFOEX osInfo = new Win32Interop.OSVERSIONINFOEX { OSVersionInfoSize = Marshal.SizeOf(typeof(Win32Interop.OSVERSIONINFOEX)) };
				Win32Interop.RtlGetVersion(ref osInfo);

				bool useDark = Win32Interop.GetSystemTheme(osInfo);
				_mode = useDark ? "Dark" : "Light";
				return _mode;
			}

			if (_mode == "Light" || _mode == "Dark")
				return _mode;

			_mode = "Light"; // Default to Mode
			return _mode;
		}

		private void GenerateCustomAccentColor(Color? c)
		{
			if (!c.HasValue)
			{
				if (GetUserAccentColor && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					(_loaded[0] as Styles).Resources["SystemAccentColor"] = Win32Interop.GetThemeColorRef("ImmersiveSystemAccent");
					(_loaded[0] as Styles).Resources["SystemAccentColorLight1"] = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentLight1");
					(_loaded[0] as Styles).Resources["SystemAccentColorLight2"] = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentLight2");
					(_loaded[0] as Styles).Resources["SystemAccentColorLight3"] = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentLight3");
					(_loaded[0] as Styles).Resources["SystemAccentColorDark1"] = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentDark1");
					(_loaded[0] as Styles).Resources["SystemAccentColorDark2"] = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentDark2");
					(_loaded[0] as Styles).Resources["SystemAccentColorDark3"] = Win32Interop.GetThemeColorRef("ImmersiveSystemAccentDark3");
				}
				else
				{
					_loaded[0] = (IStyle)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/StylesV{_controlsVersion}/AccentColors.axaml", UriKind.Absolute), _baseUri);
				}

				return;
			}
			else
			{
				(_loaded[0] as Styles).Resources["SystemAccentColor"] = c.Value;

				Color2 col = c.Value;

				(_loaded[0] as Styles).Resources["SystemAccentColorLight1"] = (Color)col.LightenPercent(0.15f);
				(_loaded[0] as Styles).Resources["SystemAccentColorLight2"] = (Color)col.LightenPercent(0.3f);
				(_loaded[0] as Styles).Resources["SystemAccentColorLight3"] = (Color)col.LightenPercent(0.45f);
				(_loaded[0] as Styles).Resources["SystemAccentColorDark1"] = (Color)col.LightenPercent(-0.15f);
				(_loaded[0] as Styles).Resources["SystemAccentColorDark2"] = (Color)col.LightenPercent(-0.30f);
				(_loaded[0] as Styles).Resources["SystemAccentColorDark3"] = (Color)col.LightenPercent(-0.45f);
			}	
		}

		private Dictionary<Type, List<IStyle>> _cache;
		private IResourceHost _owner;
        private readonly Uri _baseUri;
        private IStyle[] _loaded;
        private bool _isLoading;
        private string _mode = string.Empty;
		private int _controlsVersion = 2;
		private Color? _customAccentColor;
    }
}
