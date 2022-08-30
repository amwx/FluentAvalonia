using System.Collections.Generic;
using Avalonia.Media;
using FluentAvaloniaSamples.Utilities;

namespace FluentAvaloniaSamples.ViewModels;

public class ColorPickerPageViewModel : ViewModelBase
{
    public ColorPickerPageViewModel()
    {
        PaletteColors = CreateStandardColors();

        SetColorCommand = new FACommand(Execute);
    }

    public List<Color> PaletteColors { get; }

    public FACommand SetColorCommand { get; }

    public Color CurrentColor
    {
        get => _color;
        set => RaiseAndSetIfChanged(ref _color, value);
    }

    public void Execute(object @param)
    {
        CurrentColor = Color.Parse(param.ToString());
    }

    private List<Color> CreateStandardColors()
    {
        var cols = new List<Color>(70)
        {
            Colors.DarkRed,
            Colors.Red,
            Colors.Orange,
            Colors.Yellow,
            Colors.Lime,
            Colors.DodgerBlue,
            Colors.Blue,
            Colors.DarkBlue,
            Colors.Purple,
            Colors.Magenta
        };

        // Lighten 80%,60%,40%
        float amnt = 0.8f;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                cols.Add(AdjustColor(cols[j], amnt));
            }
            amnt -= 0.2f;
        }

        //Darken 25%,50%
        amnt = -0.25f;
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                cols.Add(AdjustColor(cols[j], amnt));
            }
            amnt -= 0.25f;
        }

        //Finally add the monochrome colors

        cols.Add(Colors.Black);
        cols.Add(Color.FromRgb(28, 28, 28));
        cols.Add(Color.FromRgb(57, 57, 57));
        cols.Add(Color.FromRgb(85, 85, 85));
        cols.Add(Color.FromRgb(113, 113, 113));
        cols.Add(Color.FromRgb(142, 142, 142));
        cols.Add(Color.FromRgb(170, 170, 170));
        cols.Add(Color.FromRgb(198, 198, 198));
        cols.Add(Color.FromRgb(227, 227, 227));
        cols.Add(Colors.White);

        return cols;
    }

    private Color AdjustColor(Color c, float factor)
    {
        float r = c.R;
        float g = c.G;
        float b = c.B;

        if (factor < 0)
        {
            factor += 1;
            r *= factor;
            g *= factor;
            b *= factor;
        }
        else
        {
            r = (255 - r) * factor + r;
            g = (255 - g) * factor + g;
            b = (255 - b) * factor + b;
        }

        return Color.FromArgb(c.A, (byte)r, (byte)g, (byte)b);
    }

    private Color _color = Colors.DarkSlateBlue;
}
