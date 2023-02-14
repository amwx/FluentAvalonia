using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Services;

public class DialogService
{
    public static readonly DialogService Instance = new DialogService();

    

    private TopLevel GetTopLevel()
    {
        if (_root == null)
        {
            var app = Application.Current.ApplicationLifetime;
            if (app is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _root = desktop.MainWindow;
            }
            else if (app is ISingleViewApplicationLifetime single)
            {
                _root = TopLevel.GetTopLevel(single.MainView);
            }
        }

        return _root;
    }

    private TopLevel _root;
}
