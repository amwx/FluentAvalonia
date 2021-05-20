using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace FluentAvalonia.UI.Controls
{
    public class SymbolIcon : IconElement
    {
        static SymbolIcon()
        {
            FontSizeProperty.OverrideDefaultValue<SymbolIcon>(18d);
            TextBlock.ForegroundProperty.Changed.AddClassHandler<SymbolIcon>((x, _) => x.GenerateText());
        }

        public static readonly DirectProperty<SymbolIcon, Symbol> SymbolProperty =
            AvaloniaProperty.RegisterDirect<SymbolIcon, Symbol>("Symbol",
                x => x.Symbol, (x,v) => x.Symbol = v);

        public static readonly StyledProperty<double> FontSizeProperty =
            TextBlock.FontSizeProperty.AddOwner<SymbolIcon>();

		public static readonly DirectProperty<SymbolIcon, bool> UseFilledProperty =
			AvaloniaProperty.RegisterDirect<SymbolIcon, bool>("UseFilled",
				x => x.UseFilled, (x, v) => x.UseFilled = v);

        public Symbol Symbol
        {
            get => _symbol;
            set
            {
                if (SetAndRaise(SymbolProperty, ref _symbol, value))
                {
                    GenerateText();
                    InvalidateMeasure();
                }
            }
        }

        public double FontSize
        {
            get => GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

		public bool UseFilled
		{
			get => _useFilled;
			set
			{
				if (SetAndRaise(UseFilledProperty, ref _useFilled, value))
				{
					GenerateText();
					InvalidateMeasure();
				}
			}
		}

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_textLayout == null)
                GenerateText();

            return _textLayout.Size;
        }

        public override void Render(DrawingContext context)
        {
            if (_textLayout == null)
                GenerateText();

            var dstRect = new Rect(Bounds.Size);
            using (context.PushClip(dstRect))
            using (context.PushPreTransform(Matrix.CreateTranslation(dstRect.Center.X - _textLayout.Size.Width / 2,
                dstRect.Center.Y - _textLayout.Size.Height / 2)))
            {
                _textLayout.Draw(context);
            }
        }

        private void GenerateText()
        {
            var code = (int)_symbol;

			// Some *genius* at MS decided that the filled & regular fonts may not have the same code points
			// for the same icon *SIGH*, Symbol Enum is based on the regular font (unfilled), so we need to
			// correct some icons where necessary (some match, some don't)
			if (_useFilled)
			{
				int tmp = VerifySymbolCodePoint();
				if (tmp != int.MinValue)
					code = tmp;
			}
				
            var glyph = char.ConvertFromUtf32(code).ToString();

            //Hardcode straight to font file
            Typeface tf = new Typeface(_useFilled ?
				new FontFamily("avares://FluentAvalonia/Fonts#FluentSystemIcons-Filled") : 
				new FontFamily("avares://FluentAvalonia/Fonts#FluentSystemIcons-Regular"));

            _textLayout = new TextLayout(glyph, tf,
               FontSize, Foreground, TextAlignment.Left);
        }

		private int VerifySymbolCodePoint()
		{
			switch (_symbol)
			{
				case Symbol.Earth:
					return 0xF3DA;

				case Symbol.Edit:
					return 0xF9F9;

				case Symbol.Emoji:
					return 0xF3E0;

				case Symbol.Filter:
					return 0xF407;

				case Symbol.Folder:
					return 0xF41F;

				case Symbol.FolderLink:
					return 0xF42C;

				case Symbol.OpenFolder:
					return 0xF433;

				case Symbol.ZipFolder:
					return 0xF43A;

				case Symbol.FontDecrease:
					return 0xF43C;

				case Symbol.FontIncrease:
					return 0xF43E;

				case Symbol.Games:
					return 0xF455;

				case Symbol.Globe:
					return 0xFDC7;

				case Symbol.Highlight:
					return 0xF481;

				case Symbol.Home:
					return 0xFA38;

				case Symbol.Icons:
					return 0xF48D;

				case Symbol.Image:
					return 0xF492;

				case Symbol.ImageAltText:
					return 0xF495;

				case Symbol.ImageEdit:
					return 0xF49B;

				case Symbol.ImageCopy:
					return 0xF498;

				case Symbol.Important:
					return 0xF4A7;

				case Symbol.Keyboard:
					return 0xF4C3;

				case Symbol.Library:
					return 0xF4DE;

				case Symbol.Link:
					return 0xF4F1;

				case Symbol.List:
					return 0xF4F9;

				case Symbol.Mail:
					return 0xF513;

				case Symbol.MailReadAll:
					return 0xF518;

				case Symbol.MailUnreadAll:
					return 0xF519;

				case Symbol.MailRead:
					return 0xF52E;

				case Symbol.MailUnread:
					return 0xF533;

				case Symbol.Map:
					return 0xF538;

				case Symbol.MapDrive:
					return 0xF53B;

				case Symbol.MoreVertical:
					return 0xF563;

				case Symbol.Navigation:
					return 0xF56B;

				case Symbol.New:
					return 0xF56E;

				case Symbol.Next:
					return 0xF574;

				case Symbol.Page:
					return 0xF597;

				case Symbol.People:
					return 0xFCF5;

				case Symbol.Phone:
					return 0xF5EB;

				case Symbol.MapPin:
					return 0xFE94;

				case Symbol.Play:
					return 0xF611;

				case Symbol.PreviewLink:
					return 0xF630;

				case Symbol.Previous:
					return 0xF633;

				case Symbol.Print:
					return 0xF636;

				case Symbol.Help:
					return 0xF645;

				case Symbol.RadioButton:
					return 0xF64F;

				case Symbol.Restore:
					return 0xF67A;

				case Symbol.Ruler:
					return 0xF687;

				case Symbol.Save:
					return 0xF68A;

				case Symbol.SaveAsCopy:
					return 0xF68D;

				case Symbol.Scan:
					return 0xF695;

				case Symbol.Send:
					return 0xF6A4;

				case Symbol.Settings:
					return 0xF6B4;

				case Symbol.Share:
					return 0xF6B9;

				case Symbol.ShareAndroid:
						return 0xF6BB;

				case Symbol.ShareIOS:
					return 0xF6C1;

				case Symbol.Star:
					return 0xF71A;

				case Symbol.StarAdd:
					return 0xF71D;

				case Symbol.StarEmphasis:
					return 0xFD10;

				case Symbol.StarOff:
					return 0xF72A;

				case Symbol.StarProhibited:
					return 0xF732;

				case Symbol.Stop:
					return 0xF743;

				case Symbol.Tag:
					return 0xF795;

				case Symbol.Target:
					return 0xFD14;

				case Symbol.TargetEdit:
					return 0xF79D;

				case Symbol.AlignCenter:
					return 0xF7B2;

				case Symbol.AlignDistributed:
					return 0xF7B4;

				case Symbol.AlignJustified:
					return 0xF7B6;

				case Symbol.AlignLeft:
					return 0xF7B8;

				case Symbol.AlignRight:
					return 0xF7BA;

				case Symbol.Bold:
					return 0xF7BD;

				case Symbol.ClearFormatting:
					return 0xF7D5;

				case Symbol.FontColor:
					return 0xF7D8;

				case Symbol.Font:
					return 0xF7FD;

				case Symbol.FontSize:
					return 0xF7FF;

				case Symbol.Italic:
					return 0xF80D;

				case Symbol.Underline:
					return 0xF824;

				case Symbol.Dislike:
					return 0xF837;

				case Symbol.Like:
					return 0xF839;

				case Symbol.Video:
					return 0xF866;

				case Symbol.WeatherBlowingSnow:
					return 0xF885;

				case Symbol.WeatherCloudy:
					return 0xF888;

				case Symbol.WeatherDustStorm:
					return 0xF88B;

				case Symbol.WeatherFog:
					return 0xF88E;

				case Symbol.WeatherHailDay:
					return 0xF891;

				case Symbol.WeatherHailNight:
					return 0xF894;

				case Symbol.WeatherMoon:
					return 0xF897;

				case Symbol.WeatherPartlyCloudyDay:
					return 0xF89A;

				case Symbol.WeatherPartlyCloudyNight:
					return 0xF89D;

				case Symbol.WeatherRain:
					return 0xF8A0;

				case Symbol.WeatherRainShowersDay:
					return 0xF8A3;

				case Symbol.WeatherRainShowersNight:
					return 0xF8A6;

				case Symbol.WeatherRainSnow:
					return 0xF8A9;

				case Symbol.WeatherSnow:
					return 0xF8AC;

				case Symbol.WeatherSnowShowerDay:
					return 0xF8AF;

				case Symbol.WeatherSnowShowerNight:
					return 0xF8B2;

				case Symbol.WeatherSnowflake:
					return 0xF8B5;

				case Symbol.WeatherSqualls:
					return 0xF8B8;

				case Symbol.WeatherSunny:
					return 0xF8BB;

				case Symbol.WeatherThunderstorm:
					return 0xF8BE;

				case Symbol.Wifi1:
					return 0xF8C5;

				case Symbol.Wifi2:
					return 0xF8C7;

				case Symbol.Wifi3:
					return 0xF8C9;

				case Symbol.Wifi4:
					return 0xFCB;

				case Symbol.WifiProtected:
					return 0xF8CC;

				case Symbol.WifiWarning:
					return 0xFB71;

				case Symbol.NewWindow:
					return 0xFB24;

				case Symbol.XboxConsole:
					return 0xF8DB;

				case Symbol.ZoomIn:
					return 0xF8DD;

				case Symbol.ZoomOut:
					return 0xF8DF;

				case Symbol.ClosedCaption:
					return 0xF98B;

				case Symbol.Comment:
					return 0xF991;

				case Symbol.Open:
					return 0xFA68;

				case Symbol.Pause:
					return 0xF5AD;

				case Symbol.Speaker0:
					return 0xFAAB;

				case Symbol.Speaker1:
					return 0xFAAF;

				case Symbol.Speaker2:
					return 0xFC70;

				case Symbol.SpeakerOff:
					return 0xFAB6;

				case Symbol.WeatherDrizzle:
					return 0xFB11;

				case Symbol.WeatherHaze:
					return 0xFB14;

				case Symbol.WeatherSunnyHigh:
					return 0xFB1E;

				case Symbol.WeatherSunnyLow:
					return 0xFB21;

				case Symbol.Undo:
					return 0xFBD4;

				case Symbol.CloudSync:
					return 0xFB86;

				case Symbol.FullScreenMaximize:
					return 0xFC1F;

				case Symbol.FullScreenMinimize:
					return 0xFC20;

				case Symbol.MoreHorizontal:
					return 0xFC39;

				case Symbol.ShareScreen:
					return 0xFC63;

				case Symbol.SpeakerMute:
					return 0xFC75;

				case Symbol.SpeakerBluetooth:
					return 0xFAB1;

				case Symbol.Orientation:
					return 0xFCF4;

				case Symbol.Calendar:
					return 0xFDB9;

				case Symbol.BulletList:
					return 0xFD8A;

				case Symbol.DockLeft:
					return 0xFC03;

				case Symbol.DockRight:
					return 0xFC08;
										
				default:
					return int.MinValue;
			}
		}

        private Symbol _symbol;
        private TextLayout _textLayout;
		private bool _useFilled;
    }
}
