using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.OpenGL;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Indentation.CSharp;
using AvaloniaEdit.TextMate;
using FluentAvalonia.Core.Attributes;
using FluentAvalonia.UI.Controls;

namespace FAControlsGallery.Controls;

public class ControlDefinitionOverlay : TemplatedControl
{
    public ControlDefinitionOverlay()
    {
        IsVisible = false;
    }

    public static readonly StyledProperty<Type> TargetTypeProperty =
        AvaloniaProperty.Register<ControlDefinitionOverlay, Type>(nameof(TargetType));

    public static readonly StyledProperty<string> InheritanceProperty =
        AvaloniaProperty.Register<ControlDefinitionOverlay, string>(nameof(Inheritance));

    public static readonly StyledProperty<string> PseudoclassesListProperty =
        AvaloniaProperty.Register<ControlDefinitionOverlay, string>(nameof(PseudoclassesList));

    public Type TargetType
    {
        get => GetValue(TargetTypeProperty);
        set => SetValue(TargetTypeProperty, value);
    }

    public string Inheritance
    {
        get => GetValue(InheritanceProperty);
        set => SetValue(InheritanceProperty, value);
    }

    public string PseudoclassesList
    {
        get => GetValue(PseudoclassesListProperty);
        set => SetValue(PseudoclassesListProperty, value);
    }


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_closeButton != null)
            _closeButton.Click -= OnCloseClick;

        base.OnApplyTemplate(e);

        _pseudoclassesDisplay = e.NameScope.Find<ItemsControl>("PseudoclassesDisplay");
        _templatePartsDisplay = e.NameScope.Find<ItemsControl>("TemplatePartsDisplay");

        _textEditor = e.NameScope.Find<TextEditor>("TextEditor");

        if (_textMateInstall == null)
        {
            _textMateInstall = _textEditor.InstallTextMate(SampleCodePresenter.GetTextMateRegistryOptions(), false);
            _textMateInstall.SetGrammar(SampleCodePresenter._cSharpLangId);

            if (ActualThemeVariant == ThemeVariant.Dark)
            {
                _textMateInstall.SetTheme(SampleCodePresenter.GetDarkTheme());
            }
            else
            {
                _textMateInstall.SetTheme(_textMateInstall.RegistryOptions.GetDefaultTheme());
            }
        }

        _textEditor.TextArea.IndentationStrategy = new CSharpIndentationStrategy();
        //_textEditor.SyntaxHighlighting = CSharpHighlightingSource.CSharpDarkMode;

        _closeButton = e.NameScope.Find<Button>("CloseButton");
        _closeButton.Click += OnCloseClick;

        Show();
    }

    public async void Show()
    {
        Inheritance = null;
        PseudoclassesList = null;

        Opacity = 0;
        IsVisible = true;
        var ani = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(250),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 0d),
                        new Setter(ScaleTransform.ScaleXProperty, 1.05d),
                        new Setter(ScaleTransform.ScaleYProperty, 1.05d)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 1d),
                        new Setter(ScaleTransform.ScaleXProperty, 1d),
                        new Setter(ScaleTransform.ScaleYProperty, 1d)
                    },
                    KeySpline = new KeySpline(0,0,0,1)
                }
            },
            FillMode = FillMode.Forward
        };

        if (_textMateInstall != null)
        {
            try
            {
                if (ActualThemeVariant == ThemeVariant.Dark)
                {
                    _textMateInstall.SetTheme(SampleCodePresenter.GetDarkTheme());
                    
                }
                else
                {
                    _textMateInstall.SetTheme(_textMateInstall.RegistryOptions.GetDefaultTheme());
                }
            }
            catch { }
        }        

        await ani.RunAsync(this);

        await BuildControlDefinition(TargetType);

        if (this.TryFindResource("TextControlSelectionHighlightColor", out var value))
        {
            if (value is ISolidColorBrush sb)
            {
                var b = new ImmutableSolidColorBrush(sb.Color, 0.5);
                _textEditor.TextArea.SelectionBrush = b;
            }
        }
    }

    public async void Hide()
    {
        var ani = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(167),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 1d),
                        new Setter(ScaleTransform.ScaleXProperty, 1d),
                        new Setter(ScaleTransform.ScaleYProperty, 1d)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(OpacityProperty, 0d),
                        new Setter(ScaleTransform.ScaleXProperty, 1.05d),
                        new Setter(ScaleTransform.ScaleYProperty, 1.05d)
                    },
                    KeySpline = new KeySpline(0,0,0,1)
                }
            },
            FillMode = FillMode.Forward
        };

        await ani.RunAsync(this);

        IsVisible = false;
        _textEditor.Document.Text = string.Empty;
        PseudoClasses.Set(":hasInheritance", false);
        PseudoClasses.Set(":hasPC", false);
        PseudoClasses.Set(":hasTP", false);
    }

    public async Task<bool> BuildControlDefinition(Type t)
    {
        var targetType = TargetType;

        try
        {
            var inheritance = await GetInheritance(t);

            //var ps = GetPseudoclasses(t);
            var pc = await GetPseudoclasses(t, targetType);
            var tp = await GetTemplateParts(t);
            var src = await GeneratePseudoSource(t);

            Dispatcher.UIThread.Post(() =>
            {
                Inheritance = inheritance;
                _pseudoclassesDisplay.ItemsSource = pc;
                _templatePartsDisplay.ItemsSource = tp;
                _textEditor.Document = new TextDocument(new StringTextSource(src));

                _textEditor.TextArea.IndentationStrategy.IndentLines(_textEditor.Document, 0, _textEditor.Document.LineCount);

                PseudoClasses.Set(":hasInheritance", t.BaseType != null);
                PseudoClasses.Set(":hasPC", pc.Count > 0);
                PseudoClasses.Set(":hasTP", tp.Count > 0);

            }, DispatcherPriority.Background);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private Task<string> GetInheritance(Type t)
    {
        return Task.Run(() =>
        {
            var sb = new StringBuilder();

            sb.Append(t.Name);

            var cur = t.BaseType;

            while (cur != null)
            {
                sb.Insert(0, " > ");
                sb.Insert(0, cur.Name);
                cur = cur.BaseType;
            }

            return sb.ToString();
        });        
    }

    private Task<List<PseudoclassesList>> GetPseudoclasses(Type t, Type targetType)
    {
        return Task.Run(() =>
        {
            var l = new List<PseudoclassesList>();
            while (t != null)
            {
                var pcl = new PseudoclassesList(t);
                if (t == targetType)
                {
                    pcl.DisplayType = false;
                }

                if (pcl.Pseudoclasses.Count > 0)
                    l.Add(pcl);

                t = t.BaseType;
            }

            return l;
        });
    }

    private Task<List<TemplatePartItem>> GetTemplateParts(Type t)
    {
        return Task.Run(() =>
        {
            var l = new List<TemplatePartItem>();
            var tpAttr = t.GetCustomAttributes(typeof(TemplatePartAttribute), false);
            foreach (TemplatePartAttribute attr in tpAttr)
            {
                l.Add(new TemplatePartItem(attr.Name, attr.Type));
            }

            return l;
        });
    }

    private Task<string> GeneratePseudoSource(Type t)
    {
        return Task.Run(() =>
        {
            var sb = new StringBuilder();

            if (t.IsInterface)
            {
                BuildInterfacePseudoSource(t, sb);
            }
            else
            {
                BuildClassPseudoSource(t, sb);
            }

            return sb.ToString();
        });
    }

    private void BuildClassPseudoSource(Type t, StringBuilder sb)
    {
        sb.Append($"public class ");
        sb.Append(t.Name);
        sb.Append(" : ");
        sb.AppendLine(t.BaseType.Name);

        sb.AppendLine("{");

        // Ignore constructors, all controls have to have a public ctor 
        
        // AvaloniaProperties
        var avProps = AvaloniaPropertyRegistry.Instance.GetRegistered(t)
                .Where(x => x.OwnerType == t);
                
        foreach (var p in avProps)
        {
            if (p.IsDirect)
            {
                sb.Append("public static readonly DirectProperty<");
                sb.Append(p.OwnerType.Name);
                sb.Append(", ");
                sb.Append(ResolveType(p.PropertyType));
                sb.Append("> ");
                sb.Append(p.Name);
                sb.AppendLine("Property;");
            }
            else
            {
                sb.Append("public static readonly StyledProperty<");
                sb.Append(ResolveType(p.PropertyType));
                sb.Append("> ");
                sb.Append(p.Name);
                sb.AppendLine("Property;");
            }
        }
        

        sb.AppendLine();

        // CLR properties
        var clr = t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.DeclaringType == t);

        // Keep track of enums used, we'll define them too at the end of the file
        List<Type> enums = new List<Type>();

        foreach (var p in clr)
        {
            if (p.GetCustomAttribute<NotImplementedAttribute>(true) != null)
            {
                sb.Append("[Not Implemented] ");
            }

            sb.Append("public ");
            sb.Append(ResolveType(p.PropertyType));
            sb.Append(' ');
            sb.Append(p.Name);

            // I don't know of many set only properties so we're safe with this
            sb.Append(" { get; ");

            if (p.CanWrite)
            {
                sb.Append("set; }\n");
            }
            else
            {
                sb.Append("}\n");
            }

            if (p.PropertyType.IsEnum)
            {
                enums.Add(p.PropertyType);
            }
        }

        sb.AppendLine();

        // Public events
        var events = t.GetEvents(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.DeclaringType == t);

        foreach (var ev in events)
        {
            if (ev.GetCustomAttribute<NotImplementedAttribute>(true) != null)
            {
                sb.Append("[Not Implemented] ");
            }

            sb.Append("public event ");

            sb.Append(ResolveType(ev.EventHandlerType));
            sb.Append(' ');
            sb.Append(ev.Name);
            sb.Append(';');
            sb.Append('\n');
        }

        sb.AppendLine();

        // Public methods

        var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.DeclaringType == t && !x.IsSpecialName && x.GetBaseDefinition() == x);

        foreach (var m in methods)
        {
            if (m.GetCustomAttribute<NotImplementedAttribute>(true) != null)
            {
                sb.Append("[Not Implemented] ");
            }

            sb.Append("public ");

            if (m.IsVirtual)
            {
                sb.Append("virtual ");
            }

            sb.Append(ResolveType(m.ReturnType));
            sb.Append(' ');

            sb.Append(m.Name);
            sb.Append('(');

            var args = m.GetParameters();
            foreach (var arg in args)
            {
                sb.Append(ResolveType(arg.ParameterType));
                sb.Append(' ');
                sb.Append(arg.Name);
                sb.Append(", ");
            }

            if (args.Length > 0)
            {
                sb.Remove(sb.Length - 2, 2);
            }
            sb.Append(')');
            sb.Append(';');
            sb.Append('\n');
        }


        sb.AppendLine("}\n"); // End class definition

        // Now any enums used in the above class, let's define here

        foreach (var en in enums)
        {
            // Skip the Symbol enum because it's too big and can be looked up on the SymbolIcon page
            if (en == typeof(Symbol))
                continue;

            if (en.GetCustomAttribute<FlagsAttribute>() != null)
            {
                sb.Append("[Flags]\n");
            }

            sb.Append("public enum ");
            sb.Append(en.Name);
            sb.Append("\n{\n");

            var names = Enum.GetNames(en);
            for (int i = 0; i < names.Length; i++)
            {
                var fi = en.GetField(names[i]);

                if (fi.GetCustomAttribute<ObsoleteAttribute>() != null)
                {
                    sb.Append("[Obsolete] ");
                }

                sb.Append(names[i]);
                sb.Append(' ');
                sb.Append('=');
                sb.Append(' ');
                sb.Append(fi.GetRawConstantValue().ToString());
                sb.Append(',');
                sb.Append('\n');
            }

            sb.AppendLine("}\n");
        }
    }

    private void BuildInterfacePseudoSource(Type t, StringBuilder sb)
    {
        sb.Append("public interface ");
        sb.AppendLine(t.Name);
        sb.AppendLine("{");

        var clr = t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.DeclaringType == t);

        // Keep track of enums used, we'll define them too at the end of the file
        List<Type> enums = new List<Type>();

        foreach (var p in clr)
        {
            if (p.GetCustomAttribute<NotImplementedAttribute>(true) != null)
            {
                sb.Append("[Not Implemented] ");
            }

            sb.Append(ResolveType(p.PropertyType));
            sb.Append(' ');
            sb.Append(p.Name);

            // I don't know of many set only properties so we're safe with this
            sb.Append(" { get; ");

            if (p.CanWrite)
            {
                sb.Append("set; }\n");
            }
            else
            {
                sb.Append("}\n");
            }

            if (p.PropertyType.IsEnum)
            {
                enums.Add(p.PropertyType);
            }
        }


        // Events
        var events = t.GetEvents(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.DeclaringType == t);

        foreach (var ev in events)
        {
            if (ev.GetCustomAttribute<NotImplementedAttribute>(true) != null)
            {
                sb.Append("[Not Implemented] ");
            }

            sb.Append("event ");

            sb.Append(ResolveType(ev.EventHandlerType));
            sb.Append(' ');
            sb.Append(ev.Name);
            sb.Append(';');
            sb.Append('\n');
        }

        sb.AppendLine();

        // Public methods
        var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.DeclaringType == t && !x.IsSpecialName && x.GetBaseDefinition() == x);

        foreach (var m in methods)
        {
            if (m.GetCustomAttribute<NotImplementedAttribute>(true) != null)
            {
                sb.Append("[Not Implemented] ");
            }

            sb.Append(ResolveType(m.ReturnType));
            sb.Append(' ');

            sb.Append(m.Name);
            sb.Append('(');

            var args = m.GetParameters();
            foreach (var arg in args)
            {
                sb.Append(ResolveType(arg.ParameterType));
                sb.Append(' ');
                sb.Append(arg.Name);
                sb.Append(", ");

                if (arg.ParameterType.IsEnum)
                {
                    enums.Add(arg.ParameterType);
                }
            }

            if (args.Length > 0)
            {
                sb.Remove(sb.Length - 2, 2);
            }
            sb.AppendLine(");");
        }

        sb.AppendLine("}"); // End interface
        sb.AppendLine();

        // Now any enums used in the above class, let's define here

        foreach (var en in enums)
        {
            // Skip the Symbol enum because it's too big and can be looked up on the SymbolIcon page
            if (en == typeof(Symbol))
                continue;

            if (en.GetCustomAttribute<FlagsAttribute>() != null)
            {
                sb.Append("[Flags]\n");
            }

            sb.Append("public enum ");
            sb.Append(en.Name);
            sb.Append("\n{\n");

            var names = Enum.GetNames(en);
            for (int i = 0; i < names.Length; i++)
            {
                var fi = en.GetField(names[i]);

                if (fi.GetCustomAttribute<ObsoleteAttribute>() != null)
                {
                    sb.Append("[Obsolete] ");
                }

                sb.Append(names[i]);
                sb.Append(' ');
                sb.Append('=');
                sb.Append(' ');
                sb.Append(fi.GetRawConstantValue().ToString());
                sb.Append(',');
                sb.Append('\n');
            }

            sb.AppendLine("}\n");
        }
    }

    private string ResolveType(Type t)
    {
        if (t.IsGenericType)
        {
            var genType = t.GetGenericTypeDefinition().Name;
            genType = genType.Substring(0, genType.IndexOf('`'));
            var args = t.GetGenericArguments().Select(x => ResolveType(x)).ToArray();

            if (genType.Equals("Nullable", StringComparison.OrdinalIgnoreCase))
            {
                return $"{args[0]}?";
            }
            else
            {
                return $"{genType}<{string.Join(',', args)}>";
            }
        }
        else
        {
            if (t == typeof(object))
                return "object";
            else if (t == typeof(string))
                return "string";
            else if (t == typeof(bool))
                return "bool";
            else if (t == typeof(char))
                return "char";
            else if (t == typeof(int))
                return "int";
            else if (t == typeof(float))
                return "float";
            else if (t == typeof(double))
                return "double";
            else if (t == typeof(long))
                return "long";
            else if (t == typeof(ulong))
                return "ulong";
            else if (t == typeof(uint))
                return "uint";
            else if (t == typeof(byte))
                return "byte";
            else if (t == typeof(short))
                return "short";
            else if (t == typeof(decimal))
                return "decimal";
            else if (t == typeof(void))
                return "void";

            return t.Name;
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private Button _closeButton;
    private ItemsControl _pseudoclassesDisplay;
    private ItemsControl _templatePartsDisplay;
    private TextEditor _textEditor;
    private TextMate.Installation _textMateInstall;
}

public class PseudoclassesList
{
    public PseudoclassesList(Type type)
    {
        DeclaredOnType = type;
        Pseudoclasses = new List<string>();
        var pcAttr = type.GetCustomAttributes(typeof(PseudoClassesAttribute), false);
        foreach (PseudoClassesAttribute attr in pcAttr)
        {
            foreach (var pc in attr.PseudoClasses)
            {
                Pseudoclasses.Add(pc);
            }
        }
    }

    public Type DeclaredOnType { get; }

    public List<string> Pseudoclasses { get; }

    public bool DisplayType { get; set; } = true;
}

public class TemplatePartItem
{
    public TemplatePartItem(string name, Type type)
    {
        ControlType = type;
        PartName = name;
    }

    public Type ControlType { get; }

    public string PartName { get; }
}
