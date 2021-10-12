using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Logging;
using Avalonia.Styling;
using System;
using System.Diagnostics;

namespace FluentAvalonia.UI.Controls
{
	public class HyperlinkButton : Button, IStyleable
	{
		Type IStyleable.StyleKey => typeof(HyperlinkButton);

		public static readonly DirectProperty<HyperlinkButton, Uri> NavigateUriProperty =
			AvaloniaProperty.RegisterDirect<HyperlinkButton, Uri>(nameof(NavigateUri), x => x.NavigateUri, (x, v) => x.NavigateUri = v);

		public Uri NavigateUri
		{
			get => _navigateUri;
			set => SetAndRaise(NavigateUriProperty, ref _navigateUri, value);
		}

		protected override void OnClick()
		{
			base.OnClick();

			if (NavigateUri != null)
			{
				try
				{
					// NOTE: Will not open avares files or anything embedded in the assembly
					Process.Start(new ProcessStartInfo(NavigateUri.ToString()) { UseShellExecute = true, Verb = "open" });
				}
				catch
				{
					Logger.TryGet(LogEventLevel.Error, $"Unable to open Uri {NavigateUri}");
				}
			}
		}

		protected override bool RegisterContentPresenter(IContentPresenter presenter)
		{
			if (presenter.Name == "ContentPresenter")
			{
				return true;
			}

			return false;
		}

		private Uri _navigateUri;
	}
}
