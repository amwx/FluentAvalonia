using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace FluentAvalonia.Core;

internal interface IFAWindowProvider
{
    Window CreateTaskDialogHost(TaskDialog dialog);
}
