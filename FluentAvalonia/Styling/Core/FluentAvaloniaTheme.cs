using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
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
		/// Initializes a new instance of the <see cref="FluentAvaloniaTheme"/> class.
		/// </summary>
		/// <param name="baseUri">The base URL for the XAML context.</param>
		public FluentAvaloniaTheme(Uri baseUri)
		{
			_baseUri = baseUri;
			Register();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FluentAvaloniaTheme"/> class.
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
						var old = _loaded[2];
						//Only switch the third item in loaded (light/dark theme)
						//Everything else remains the same
						(old as IResourceProvider)?.RemoveOwner(Owner);
						_loaded[2] = null;
						var newStyles = (IStyle)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/StylesV{ControlsVersion}/{thm}Resources.axaml", UriKind.Absolute), _baseUri);
						
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
			get => 2;
			set
			{
				throw new NotSupportedException("Fluent v1 Styles have been deprecated and removed from FluentAvalonia. This property is now obsolete");
				//if (value != _controlsVersion)
				//{
				//	_controlsVersion = value;
				//	Init(true);
				//	Owner?.NotifyHostedResourcesChanged(ResourcesChangedEventArgs.Empty);
				//	if (value == 1)
				//	{
				//		Debug.WriteLine("NOTE: Fluent v1 Styles are now considered deprecated, and support will be removed from FluentAvalonia in a future version");						
				//	}
				//}
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
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                if (Children[i] is IResourceProvider rp && rp.TryGetResource(key, out value))
                    return true;
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
				theme = IsValidRequestedTheme(RequestedTheme) ? RequestedTheme : LightModeString;
			}
			else
			{
				theme = IsValidRequestedTheme(theme) ? theme : IsValidRequestedTheme(RequestedTheme) ? RequestedTheme : LightModeString;
			}

			Win32Interop.ApplyTheme(w.PlatformImpl.Handle.Handle, theme.Equals(DarkModeString, StringComparison.OrdinalIgnoreCase), osInfo);
		}

		private bool IsValidRequestedTheme(string thm)
		{
			if (LightModeString.Equals(thm, StringComparison.OrdinalIgnoreCase) ||
				DarkModeString.Equals(thm, StringComparison.OrdinalIgnoreCase) ||
                HighContrastModeString.Equals(thm, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return false;
		}

		private void Init(bool clear = false)
		{
			if (clear && _loaded != null) //This is a complete refresh, e.g., changing Version #
			{
				for (int i = 0; i < _loaded.Length; i++)
				{
					(_loaded[i] as IResourceProvider)?.RemoveOwner(Owner);
				}
				_loaded = null;
			}

			//Make sure we don't get an invalid version number
			string thm = GetTheme();

            // Must populate this incrementally, otherwise some resources won't evaluate if they're in other files
            _loaded = new IStyle[4];
            _loaded[0] = (IStyle)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/StylesV2/AccentColors.axaml", UriKind.Absolute), _baseUri);
            _loaded[1] = (IStyle)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/StylesV2/BaseResources.axaml", UriKind.Absolute), _baseUri);
            _loaded[2] = (IStyle)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/StylesV2/{thm}Resources.axaml", UriKind.Absolute), _baseUri);
            _loaded[3] = (IStyle)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/StylesV2/Controls.axaml", UriKind.Absolute), _baseUri);

			// TODO: Figure out how to load HighContrast theme colors from system
			// This only loads one version of the HC theme & doesn't respect the variants
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// Populate HighContrast from System Colors
				if (string.Equals(thm, HighContrastModeString))
				{
					bool GetSystemColor(SystemColors color, out Color c)
					{
						try
						{
							var intCol = Win32Interop.GetSysColor(color);
							var r = (byte)((intCol >> 16) & 0xFF);
							var g = (byte)((intCol >> 8) & 0xFF);
							var b = (byte)(intCol & 0xFF);

							c = Color.FromRgb(r, g, b);

							return true;
						}
						catch
						{
							c = Colors.Transparent;
							return false;
						}
					}

					if (GetSystemColor(SystemColors.COLOR_WINDOWTEXT, out Color windowT))
						(_loaded[0] as Styles).Resources["SystemColorWindowTextColor"] = windowT;

					if (GetSystemColor(SystemColors.COLOR_GRAYTEXT, out Color grey))
						(_loaded[0] as Styles).Resources["SystemColorGrayTextColor"] = grey;

					if (GetSystemColor(SystemColors.COLOR_BTNFACE, out Color btn))
						(_loaded[0] as Styles).Resources["SystemColorButtonFaceColor"] = btn;

					if (GetSystemColor(SystemColors.COLOR_WINDOW, out Color window))
						(_loaded[0] as Styles).Resources["SystemColorWindowColor"] = window;

					if (GetSystemColor(SystemColors.COLOR_BTNTEXT, out Color btnT))
						(_loaded[0] as Styles).Resources["SystemColorButtonTextColor"] = btnT;

					if (GetSystemColor(SystemColors.COLOR_HIGHLIGHT, out Color highlight))
						(_loaded[0] as Styles).Resources["SystemColorHighlightColor"] = highlight;

					if (GetSystemColor(SystemColors.COLOR_HIGHLIGHTTEXT, out Color highlightT))
						(_loaded[0] as Styles).Resources["SystemColorHighlightTextColor"] = highlightT;

					if (GetSystemColor(SystemColors.COLOR_HOTLIGHT, out Color hotlight))
						(_loaded[0] as Styles).Resources["SystemColorHotlightColor"] = hotlight;
				}
			}

			InitIfNecessary();
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
					Win32Interop.OSVERSIONINFOEX osInfo = new Win32Interop.OSVERSIONINFOEX { OSVersionInfoSize = Marshal.SizeOf(typeof(Win32Interop.OSVERSIONINFOEX)) };
					Win32Interop.RtlGetVersion(ref osInfo);

					if (osInfo.BuildNumber >= 22000) // Windows 11
					{
						//This is defined in the BaseResources.axaml file
						(_loaded[1] as Styles).Resources["ContentControlThemeFontFamily"] = new FontFamily("Segoe UI Variable Text");
					}
					else // Windows 10
					{
						//This is defined in the BaseResources.axaml file
						(_loaded[1] as Styles).Resources["ContentControlThemeFontFamily"] = new FontFamily("Segoe UI");
					}                    
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

                try
                {
                    var hc = new Win32Interop.HIGHCONTRAST
                    {
                        cbSize = (uint)Marshal.SizeOf<Win32Interop.HIGHCONTRAST>()
                    };

                    bool ok = Win32Interop.SystemParametersInfo(0x0042 /*SPI_GETHIGHCONTRAST*/, 0, ref hc, 0);

                    if (ok && (hc.dwFlags & HCF.HCF_HIGHCONTRASTON) == HCF.HCF_HIGHCONTRASTON)
                    {
                        _mode = HighContrastModeString;
                        return _mode;
                    }
                }
                catch { }
				

                bool useDark = Win32Interop.GetSystemTheme(osInfo);
				_mode = useDark ? DarkModeString : LightModeString;
				return _mode;
			}

			if (_mode == LightModeString || _mode == DarkModeString || _mode == HighContrastModeString)
				return _mode;

			_mode = LightModeString; // Default to Mode
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
					_loaded[0] = (IStyle)AvaloniaXamlLoader.Load(new Uri($"avares://FluentAvalonia/Styling/StylesV2/AccentColors.axaml", UriKind.Absolute), _baseUri);
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
		private string _mode = string.Empty;
		private Color? _customAccentColor;

        public readonly string LightModeString = "Light";
        public readonly string DarkModeString = "Dark";
        public readonly string HighContrastModeString = "HighContrast";
	}
}
