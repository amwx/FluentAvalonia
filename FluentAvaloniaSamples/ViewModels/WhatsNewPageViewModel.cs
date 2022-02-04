using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Platform;

namespace FluentAvaloniaSamples.ViewModels
{
    public class WhatsNewPageViewModel : ViewModelBase
    {
        public WhatsNewPageViewModel()
        {
            Versions = new List<FAVersionInfo>();

            using (var stream = AvaloniaLocator.Current.GetService<IAssetLoader>().Open(new Uri("avares://FluentAvaloniaSamples/Assets/ChangeLog.txt")))
            using (var sr = new StreamReader(stream))
            {
                FAVersionInfo currentVersion = null;
                bool hasVersion = false;
                while (sr.Peek() != -1)
                {

                    var line = sr.ReadLine();
                    if (line.StartsWith('#')) // Comment
                        continue;

                    if (string.IsNullOrEmpty(line)) // Empty line
                        continue;

                    if (line.StartsWith('['))
                    {
                        if (!hasVersion)
                        {
                            currentVersion = new FAVersionInfo() { Version = line.Substring(1, line.Length - 2) };
                            hasVersion = true;
                        }
                        else
                        {
                            // new version found

                            Versions.Insert(0, currentVersion);
                            currentVersion = new FAVersionInfo() { Version = line.Substring(1, line.Length - 2) };
                        }

                        continue;
                    }

                    currentVersion.ChangeItems.Add(ReadLine(line));
                }

                Versions.Insert(0, currentVersion);
            }


            CurrentVersion = Versions[0];
        }

        public List<FAVersionInfo> Versions { get; }

        public FAVersionInfo CurrentVersion
        {
            get => _version;
            set => RaiseAndSetIfChanged(ref _version, value);
        }

        private FAChangeItemBase ReadLine(string line)
        {
            if (line.StartsWith('*'))
            {
                return new FAChangeItemHeader { Header = line.Substring(1) };
            }
            else if (line.StartsWith("--"))
            {
                return new FAChangeSubItem { Text = line.Substring(2) };
            }
            else
            {
                return new FAChangeItemDescription { Text = line.Substring(1) };
            }
        }

        private FAVersionInfo _version;
    }

    public class FAVersionInfo
    {
        public string Version { get; set; }

        public List<FAChangeItemBase> ChangeItems { get; } = new List<FAChangeItemBase>();
    }

    public class FAChangeItemBase
    {

    }

    public class FAChangeItemHeader : FAChangeItemBase
    {
        public string Header { get; set; }
    }

    public class FAChangeItemDescription : FAChangeItemBase
    {
        public string Text { get; set; }
    }

    public class FAChangeSubItem : FAChangeItemBase
    {
        public string Text { get; set; }
    }
}
