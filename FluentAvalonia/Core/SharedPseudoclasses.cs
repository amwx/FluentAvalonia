using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.Core;

// These are pseudoclass names that are used in multiple controls. Share them so we don't have
// a bunch of duplicate strings everywhere
internal class SharedPseudoclasses
{
    public const string s_pcOpen = ":open";
    public const string s_pcCompact = ":compact";
    public const string s_pcIcon = ":icon";
    public const string s_pcLabel = ":label";
    public const string s_pcFlyout = ":flyout";
    public const string s_pcHotkey = ":hotkey";
    public const string s_pcOverflow = ":overflow";
}
