using Avalonia.Media;
using System.Linq;
using System.Runtime.InteropServices;

namespace FluentAvalonia.UI.Controls
{
    public enum Symbol
    {
        Accept,
        Accounts,
        Add,
        AddFriend,
        AlignCenter,
        AlignLeft,
        AlignRight,
        AllApps,
        Attach,
        Back,
        BackToWindow,
        BlockContact,
        Bold,
        BulletedList,
        Calculator,
        Calendar,
        CalendarDay,
        CalendarReply,
        CalendarWeek,
        Camera,
        Cancel,
        Caption,
        CellPhone,
        Characters,
        Checkmark,
        ChevronDown,
        ChevronLeft,
        ChevronRight,
        ChevronUp,
        ClearSelection,
        ContactInfo,
        Copy,
        Crop,
        Cut,
        Delete,
        Directions,
        DisableUpdates,
        DisconnectDrive,
        Dislike,
        DockBottom,
        DockLeft,
        DockRight,
        Document,
        Down,
        Download,
        Emoji,
        Emoji2,
        FavoriteStarFill,
        FavoriteStart,
        Filter,
        Font,
        FontColor,
        FontDecrease,
        FontIncrease,
        FontSize,
        Forward,
        FourBars,
        FullScreen,
        Globe,
        Go,
        GoToStart,
        HangUp,
        Help,
        HideBcc,
        Highlight,
        Import,
        ImportAll,
        Important,
        IndeterminateCheck,
        Italic,
        Library,
        Like,
        LikeDislike,
        Link,
        Mail,
        MailReply,
        MailReplyAll,
        MapDrive,
        MapPin,
        Microphone,
        More,
        Mute,
        Navigation,
        NewFolder,
        NewWindow,
        Next,
        OneBar,
        OpenLocal,
        OpenWith,
        Orientation,
        Page,
        Paste,
        Pause,
        People,
        Phone,
        Picture,
        Pin,
        Play,
        PostUpdate,
        Preview,
        Previous,
        Priority,
        ProtectedDocument,
        Read,
        Redo,
        Refresh,
        Remote,
        Remove,
        Rename,
        Repair,
        RepeatAll,
        RepeatOne,
        Reshare,
        Rotate,
        Save,
        SaveLocal,
        Scan,
        Search,
        SelectAll,
        Send,
        SetLockScreen,
        Settings,
        Shop,
        ShowBcc,
        Shuffle,
        Slideshow,
        Sort,
        Stop,
        Switch,
        SwitchApps,
        Sync,
        SyncFolder,
        Tag,
        ThreeBars,
        TwoBars,
        Underline,
        Undo,
        Unfavorite,
        UnsyncFolder,
        Up,
        Upload,
        Video,
        VideoChat,
        View,
        Webcam,
        ZeroBars,
        Zoom,
        ZoomOut,

    }

    internal static class SymbolHelper
    {
        /// <summary>
        /// Helper function to get the appropriate glyph character code for use with SymbolThemeFontFamily
        /// Since Segoe MDL2 Assets isn't always available we fall back on winjs-symbols, included in
        /// FluentAvalonia. Not all symbols match, or match perfectly, but it's still a reasonable solution
        /// </summary>
        /// <param name="symbol">The symbol of the glyph we want</param>
        /// <param name="useSegoe">Pass true here if you already know Segoe MDL2 is available, pass false if not or you want to autodetect</param>
        /// <param name="autoDetect">if useSegoe param is false, and autoDetect is true, we check here if we're on Windows and if the Avalonia FontManager has Segoe MDL2 Assets available</param>
        /// <returns></returns>
        public static string GetCharacterForSymbol(Symbol symbol, bool useSegoe, bool autoDetect = false)
        {
            bool useSegoeAssets = useSegoe;

            if (!useSegoe && autoDetect)
            {
                if (IsWindowsAndHasSegoe())
                    useSegoeAssets = true;
            }

            //Maps the Symbol Enum to the glyph in SegoeMDL2Assets or winjs-symbols
            switch (symbol)
            {
                case Symbol.Accept:
                    return useSegoeAssets ? "\uE8FB" : "\uE10B";
                case Symbol.Accounts:
                    return useSegoeAssets ? "\uE910" : "\uE168";
                case Symbol.Add:
                    return useSegoeAssets ? "\uE710" : "\uE109";
                case Symbol.AddFriend:
                    return useSegoeAssets ? "\uE8FA" : "\uE1E2";
                case Symbol.AlignCenter:
                    return useSegoeAssets ? "\uE8E3" : "\uE1A1";
                case Symbol.AlignLeft:
                    return useSegoeAssets ? "\uE8E4" : "\uE1A2";
                case Symbol.AlignRight:
                    return useSegoeAssets ? "\uE8E2" : "\uE1A0";
                case Symbol.AllApps:
                    return useSegoeAssets ? "\uE71D" : "\uE179";
                case Symbol.Attach:
                    return useSegoeAssets ? "\uE723" : "\uE16C";
                case Symbol.Back:
                    return useSegoeAssets ? "\uE72B" : "\uE112";
                case Symbol.BackToWindow:
                    return useSegoeAssets ? "\uE73F" : "\uE1D8";
                case Symbol.BlockContact:
                    return useSegoeAssets ? "\uE8F8" : "\uE1E0";
                case Symbol.Bold:
                    return useSegoeAssets ? "\uE8DD" : "\uE19B";
                case Symbol.BulletedList:
                    return useSegoeAssets ? "\uE8FD" : "\uE14C";
                case Symbol.Calculator:
                    return useSegoeAssets ? "\uE8EF" : "\uE1D0";
                case Symbol.Calendar:
                    return useSegoeAssets ? "\uE787" : "\uE163";
                case Symbol.CalendarDay:
                    return useSegoeAssets ? "\uE8BF" : "\uE161";
                case Symbol.CalendarReply:
                    return useSegoeAssets ? "\uE8F5" : "\uE1DB";
                case Symbol.CalendarWeek:
                    return useSegoeAssets ? "\uE8C0" : "\uE162";
                case Symbol.Camera:
                    return useSegoeAssets ? "\uE722" : "\uE114";
                case Symbol.Cancel:
                    return useSegoeAssets ? "\uE711" : "\uE10A";
                case Symbol.Caption:
                    return useSegoeAssets ? "\uE8BA" : "\uE15A";
                case Symbol.CellPhone:
                    return useSegoeAssets ? "\uE8EA" : "\uE1C9";
                case Symbol.Characters:
                    return useSegoeAssets ? "\uE8C1" : "\uE164";
                case Symbol.Checkmark:
                    return useSegoeAssets ? "\uE73E" : "\uE081";
                case Symbol.ChevronDown:
                    return useSegoeAssets ? "\uE70D" : "\uE018";
                case Symbol.ChevronLeft:
                    return useSegoeAssets ? "\uE76B" : "\uE26C";
                case Symbol.ChevronRight:
                    return useSegoeAssets ? "\uE76C" : "\uE26B";
                case Symbol.ChevronUp:
                    return useSegoeAssets ? "\uE70E" : "\uE019";
                case Symbol.ClearSelection:
                    return useSegoeAssets ? "\uE8E6" : "\uE1C5";
                case Symbol.ContactInfo:
                    return useSegoeAssets ? "\uE779" : "\uE136";
                case Symbol.Copy:
                    return useSegoeAssets ? "\uE8C8" : "\uE16F";
                case Symbol.Crop:
                    return useSegoeAssets ? "\uE7A8" : "\uE123";
                case Symbol.Cut:
                    return useSegoeAssets ? "\uE8C6" : "\uE16B";
                case Symbol.Delete:
                    return useSegoeAssets ? "\uE74D" : "\uE107";
                case Symbol.Directions:
                    return useSegoeAssets ? "\uE8F0" : "\uE1D1";
                case Symbol.DisableUpdates:
                    return useSegoeAssets ? "\uE8D8" : "\uE194";
                case Symbol.DisconnectDrive:
                    return useSegoeAssets ? "\uE8CD" : "\uE17A";
                case Symbol.Dislike:
                    return useSegoeAssets ? "\uE8E0" : "\uE19E";
                case Symbol.DockBottom:
                    return useSegoeAssets ? "\uE90E" : "\uE147";
                case Symbol.DockLeft:
                    return useSegoeAssets ? "\uE90C" : "\uE145";
                case Symbol.DockRight:
                    return useSegoeAssets ? "\uE90D" : "\uE146";
                case Symbol.Document:
                    return useSegoeAssets ? "\uE8A5" : "\uE130";
                case Symbol.Down:
                    return useSegoeAssets ? "\uE74B" : "\uE0E5";
                case Symbol.Download:
                    return useSegoeAssets ? "\uE896" : "\uE118";
                case Symbol.Emoji:
                    return useSegoeAssets ? "\uE899" : "\uE11D";
                case Symbol.Emoji2:
                    return useSegoeAssets ? "\uE76E" : "\uE170";
                case Symbol.FavoriteStarFill:
                    return useSegoeAssets ? "\uE735" : "\uE1CF";
                case Symbol.FavoriteStart:
                    return useSegoeAssets ? "\uE734" : "\uE1CF";
                case Symbol.Filter:
                    return useSegoeAssets ? "\uE71C" : "\uE16E";
                case Symbol.Font:
                    return useSegoeAssets ? "\uE8D2" : "\uE185";
                case Symbol.FontColor:
                    return useSegoeAssets ? "\uE8D3" : "\uE186";
                case Symbol.FontDecrease:
                    return useSegoeAssets ? "\uE8E7" : "\uE1C6";
                case Symbol.FontIncrease:
                    return useSegoeAssets ? "\uE8E8" : "\uE1C7";
                case Symbol.FontSize:
                    return useSegoeAssets ? "\uE8E9" : "\uE1C8";
                case Symbol.Forward:
                    return useSegoeAssets ? "\uE72A" : "\uE111";
                case Symbol.FourBars:
                    return useSegoeAssets ? "\uE908" : "\uE1E9";
                case Symbol.FullScreen:
                    return useSegoeAssets ? "\uE740" : "\uE1D9";
                case Symbol.Globe:
                    return useSegoeAssets ? "\uE774" : "\uE128";
                case Symbol.Go:
                    return useSegoeAssets ? "\uE8AD" : "\uE143";
                case Symbol.GoToStart:
                    return useSegoeAssets ? "\uE8FC" : "\uE1EF";
                case Symbol.HangUp:
                    return useSegoeAssets ? "\uE778" : "\uE137";
                case Symbol.Help:
                    return useSegoeAssets ? "\uE897" : "\uE11B";
                case Symbol.HideBcc:
                    return useSegoeAssets ? "\uE8C5" : "\uE16A";
                case Symbol.Highlight:
                    return useSegoeAssets ? "\uE7E6" : "\uE193";
                case Symbol.Import:
                    return useSegoeAssets ? "\uE8B5" : "\uE150";
                case Symbol.ImportAll:
                    return useSegoeAssets ? "\uE8B6" : "\uE151";
                case Symbol.Important:
                    return useSegoeAssets ? "\uE8C9" : "\uE171";
                case Symbol.Italic:
                    return useSegoeAssets ? "\uE8DB" : "\uE199";
                case Symbol.Library:
                    return useSegoeAssets ? "\uE8F1" : "\uE1D3";
                case Symbol.Like:
                    return useSegoeAssets ? "\uE8E1" : "\uE19F";
                case Symbol.LikeDislike:
                    return useSegoeAssets ? "\uE8DF" : "\uE19D";
                case Symbol.Link:
                    return useSegoeAssets ? "\uE71B" : "\uE167";
                case Symbol.Mail:
                    return useSegoeAssets ? "\uE715" : "\uE119";
                case Symbol.MailReply:
                    return useSegoeAssets ? "\uE8CA" : "\uE172";
                case Symbol.MailReplyAll:
                    return useSegoeAssets ? "\uE8C2" : "\uE165";
                case Symbol.MapDrive:
                    return useSegoeAssets ? "\uE8CE" : "\uE17B";
                case Symbol.MapPin:
                    return useSegoeAssets ? "\uE707" : "\uE139";
                case Symbol.Microphone:
                    return useSegoeAssets ? "\uE720" : "\uE1D6";
                case Symbol.More:
                    return useSegoeAssets ? "\uE712" : "\uE10C";
                case Symbol.Mute:
                    return useSegoeAssets ? "\uE74F" : "\uE198";
                case Symbol.Navigation:
                    return useSegoeAssets ? "\uE700" : "\uE700";
                case Symbol.NewFolder:
                    return useSegoeAssets ? "\uE8F4" : "\uE1DA";
                case Symbol.NewWindow:
                    return useSegoeAssets ? "\uE78B" : "\uE17C";
                case Symbol.Next:
                    return useSegoeAssets ? "\uE893" : "\uE101";
                case Symbol.OneBar:
                    return useSegoeAssets ? "\uE905" : "\uE1E6";
                case Symbol.OpenLocal:
                    return useSegoeAssets ? "\uE8DA" : "\uE197";
                case Symbol.OpenWith:
                    return useSegoeAssets ? "\uE7AC" : "\uE17D";
                case Symbol.Orientation:
                    return useSegoeAssets ? "\uE8B4" : "\uE14F";
                case Symbol.Page:
                    return useSegoeAssets ? "\uE7C3" : "\uE132";
                case Symbol.Paste:
                    return useSegoeAssets ? "\uE77F" : "\uE16D";
                case Symbol.Pause:
                    return useSegoeAssets ? "\uE769" : "\uE103";
                case Symbol.People:
                    return useSegoeAssets ? "\uE716" : "\uE125";
                case Symbol.Phone:
                    return useSegoeAssets ? "\uE717" : "\uE13A";
                case Symbol.Picture:
                    return useSegoeAssets ? "\uE8B9" : "\uE158";
                case Symbol.Pin:
                    return useSegoeAssets ? "\uE718" : "\uE141";
                case Symbol.Play:
                    return useSegoeAssets ? "\uE768" : "\uE102";
                case Symbol.PostUpdate:
                    return useSegoeAssets ? "\uE8F3" : "\uE1D7";
                case Symbol.Preview:
                    return useSegoeAssets ? "\uE8FF" : "\uE295";
                case Symbol.Previous:
                    return useSegoeAssets ? "\uE892" : "\uE100";
                case Symbol.Priority:
                    return useSegoeAssets ? "\uE8D0" : "\uE182";
                case Symbol.ProtectedDocument:
                    return useSegoeAssets ? "\uE8A6" : "\uE131";
                case Symbol.Read:
                    return useSegoeAssets ? "\uE8C3" : "\uE165";
                case Symbol.Redo:
                    return useSegoeAssets ? "\uE7A6" : "\uE10D";
                case Symbol.Refresh:
                    return useSegoeAssets ? "\uE72C" : "\uE149";
                case Symbol.Remote:
                    return useSegoeAssets ? "\uE8AF" : "\uE14B";
                case Symbol.Remove:
                    return useSegoeAssets ? "\uE738" : "\uE108";
                case Symbol.Rename:
                    return useSegoeAssets ? "\uE8AC" : "\uE13E";
                case Symbol.Repair:
                    return useSegoeAssets ? "\uE90F" : "\uE15E";
                case Symbol.RepeatAll:
                    return useSegoeAssets ? "\uE8EE" : "\uE1CD";
                case Symbol.RepeatOne:
                    return useSegoeAssets ? "\uE8ED" : "\uE1CC";
                case Symbol.Reshare:
                    return useSegoeAssets ? "\uE8EB" : "\uE1CA";
                case Symbol.Rotate:
                    return useSegoeAssets ? "\uE7AD" : "\uE14A";
                case Symbol.Save:
                    return useSegoeAssets ? "\uE74E" : "\uE105";
                case Symbol.SaveLocal:
                    return useSegoeAssets ? "\uE78C" : "\uE159";
                case Symbol.Scan:
                    return useSegoeAssets ? "\uE8FE" : "\uE294";
                case Symbol.Search:
                    return useSegoeAssets ? "\uE721" : "\uE094";
                case Symbol.SelectAll:
                    return useSegoeAssets ? "\uE8B3" : "\uE14E";
                case Symbol.Send:
                    return useSegoeAssets ? "\uE725" : "\uE122";
                case Symbol.SetLockScreen:
                    return useSegoeAssets ? "\uE7B5" : "\uE18C";
                case Symbol.Settings:
                    return useSegoeAssets ? "\uE713" : "\uE115";
                case Symbol.Shop:
                    return useSegoeAssets ? "\uE719" : "\uE14D";
                case Symbol.ShowBcc:
                    return useSegoeAssets ? "\uE8C4" : "\uE169";
                case Symbol.Shuffle:
                    return useSegoeAssets ? "\uE8B1" : "\uE14B";
                case Symbol.Slideshow:
                    return useSegoeAssets ? "\uE786" : "\uE173";
                case Symbol.Sort:
                    return useSegoeAssets ? "\uE8CB" : "\uE174";
                case Symbol.Stop:
                    return useSegoeAssets ? "\uE71A" : "\uE15B";
                case Symbol.Switch:
                    return useSegoeAssets ? "\uE8AB" : "\uE13C";
                case Symbol.SwitchApps:
                    return useSegoeAssets ? "\uE8F9" : "\uE1E1";
                case Symbol.Sync:
                    return useSegoeAssets ? "\uE895" : "\uE117";
                case Symbol.SyncFolder:
                    return useSegoeAssets ? "\uE8F7" : "\uE1DF";
                case Symbol.Tag:
                    return useSegoeAssets ? "\uE8EC" : "\uE1CB";
                case Symbol.ThreeBars:
                    return useSegoeAssets ? "\uE907" : "\uE1E8";
                case Symbol.TwoBars:
                    return useSegoeAssets ? "\uE906" : "\uE1E7";
                case Symbol.Underline:
                    return useSegoeAssets ? "\uE8DC" : "\uE19A";
                case Symbol.Undo:
                    return useSegoeAssets ? "\uE7A7" : "\uE10E";
                case Symbol.Unfavorite:
                    return useSegoeAssets ? "\uE8D9" : "\uE195";
                case Symbol.UnsyncFolder:
                    return useSegoeAssets ? "\uE8F6" : "\uE1DD";
                case Symbol.Up:
                    return useSegoeAssets ? "\uE74A" : "\uE0E4";
                case Symbol.Upload:
                    return useSegoeAssets ? "\uE898" : "\uE11C";
                case Symbol.Video:
                    return useSegoeAssets ? "\uE714" : "\uE116";
                case Symbol.VideoChat:
                    return useSegoeAssets ? "\uE8AA" : "\uE13B";
                case Symbol.View:
                    return useSegoeAssets ? "\uE890" : "\uE18B";
                case Symbol.Webcam:
                    return useSegoeAssets ? "\uE8B8" : "\uE156";
                case Symbol.ZeroBars:
                    return useSegoeAssets ? "\uE904" : "\uE1E5";
                case Symbol.Zoom:
                    return useSegoeAssets ? "\uE71E" : "\uE12E";
                case Symbol.ZoomOut:
                    return useSegoeAssets ? "\uE71F" : "\uE1A4";
                case Symbol.IndeterminateCheck:
                    return useSegoeAssets ? "\uE73C" : "\uE15B"; //This actually uses the Stop symbol for winjs-symbols, but its the best we can do
            }

            return null;
        }

        private static bool IsWindowsAndHasSegoe()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var names = FontManager.Current.GetInstalledFontFamilyNames(true).FirstOrDefault(x => x == "Segoe MDL2 Assets");
                if (names != null && names.Count() > 0)
                    return true;
            }
            return false;
        }
    }

}
