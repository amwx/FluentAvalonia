using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Diagnostics;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Metadata;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media;
using FluentAvaloniaSamples.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IconElement = FluentAvalonia.UI.Controls.IconElement;
using PathIcon = FluentAvalonia.UI.Controls.PathIcon;

namespace FluentAvaloniaSamples
{
    public class ControlExample : HeaderedContentControl
    {
		public ControlExample()
		{
			properties = new AvaloniaList<DocsItemViewModel>();
		}

		public static readonly StyledProperty<string> TargetTypeProperty =
			AvaloniaProperty.Register<ControlExample, string>("TargetType");

		public static readonly StyledProperty<bool> ShowDocsProperty =
			AvaloniaProperty.Register<ControlExample, bool>("ShowDocs", true);

		public static readonly StyledProperty<IControl> DynamicXamlTargetProperty =
			AvaloniaProperty.Register<ControlExample, IControl>("DynamicXamlTarget");

		public static readonly StyledProperty<string> DynamicXamlPropertiesProperty =
			AvaloniaProperty.Register<ControlExample, string>("DynamicXamlProperties");

		public static readonly StyledProperty<string> XamlSourceProperty =
            AvaloniaProperty.Register<ControlExample, string>("XamlSource");

        public static readonly StyledProperty<string> CSharpSourceProperty =
            AvaloniaProperty.Register<ControlExample, string>("CSharpSource");

        public static readonly StyledProperty<string> UsageNotesProperty =
            AvaloniaProperty.Register<ControlExample, string>("UsageNotes");

        public static readonly StyledProperty<IControl> OptionsProperty =
            AvaloniaProperty.Register<ControlExample, IControl>("Options");
				
		public string TargetType
		{
			get => GetValue(TargetTypeProperty);
			set => SetValue(TargetTypeProperty, value);
		}

		public bool ShowDocs
		{
			get => GetValue(ShowDocsProperty);
			set => SetValue(ShowDocsProperty, value);
		}

		public IControl DynamicXamlTarget
		{
			get => GetValue(DynamicXamlTargetProperty);
			set => SetValue(DynamicXamlTargetProperty, value);
		}

		public string DynamicXamlProperties
		{
			get => GetValue(DynamicXamlPropertiesProperty);
			set => SetValue(DynamicXamlPropertiesProperty, value);
		}

		public string XamlSource
        {
            get => GetValue(XamlSourceProperty);
            set => SetValue(XamlSourceProperty, value);
        }

        public string CSharpSource
        {
            get => GetValue(CSharpSourceProperty);
            set => SetValue(CSharpSourceProperty, value);
        }

        public string UsageNotes
        {
            get => GetValue(UsageNotesProperty);
            set => SetValue(UsageNotesProperty, value);
        }

        public IControl Options
        {
            get => GetValue(OptionsProperty);
            set => SetValue(OptionsProperty, value);
        }

		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			base.OnApplyTemplate(e);
			var ic = e.NameScope.Find<ItemsRepeater>("DocsItemsControl");
			if (ic != null)
			{
				ic.Items = properties;
			}
		}

		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == TargetTypeProperty)
			{
				var str = change.NewValue.GetValueOrDefault<string>();
				if (!string.IsNullOrEmpty(str))
				{
					var type = Type.GetType($"FluentAvalonia.UI.Controls.{str}, FluentAvalonia") ??
						Type.GetType($"FluentAvalonia.UI.Controls.Primitives.{str}, FluentAvalonia");
					if (type != null)
					{
						properties.Clear();

						properties.Add(new ClassItemViewModel(type));

						var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
						properties.AddRange(							
							props
							.Where(x => x.DeclaringType.Assembly == type.Assembly)
							.Select(x => new PropertyItemViewModel(x, type)));
						
						var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
						properties.AddRange(
							methods
							.Where(x => x.DeclaringType.Assembly == type.Assembly && !x.IsSpecialName && x.GetBaseDefinition() == x)
							.Select(x => new MethodItemViewModel(x, type)));

						var events = type.GetEvents(BindingFlags.Public | BindingFlags.Instance);
						properties.AddRange(
							events
							.Where(x => x.DeclaringType.Assembly == type.Assembly)
							.Select(x => new EventItemViewModel(x)));
					}
				}
			}
			else if (change.Property == DynamicXamlTargetProperty)
			{
				if (change.OldValue.GetValueOrDefault() is IControl cOld)
				{
					cOld.PropertyChanged -= OnTargetAvPropertyChanged;
				}

				if (change.NewValue.GetValueOrDefault() is IControl cNew)
				{
					availProps = AvaloniaPropertyRegistry.Instance.GetRegistered(cNew)
					   .Union(AvaloniaPropertyRegistry.Instance.GetRegisteredAttached(cNew.GetType())).ToArray();
					cNew.PropertyChanged += OnTargetAvPropertyChanged;
				}
			}
			else if (change.Property == DynamicXamlPropertiesProperty)
			{
				if (dynamicProps == null)
					dynamicProps = new Dictionary<string, string>();
				else
					dynamicProps.Clear();

				if (change.NewValue.GetValueOrDefault() is string s)
				{
					var chrRd = new CharacterReader(s.AsSpan());
					while (!chrRd.End)
					{
						var token = chrRd.TakeUntil(';');

						var index = token.IndexOf(',');
						if (index != -1)
						{
							var name = token.Slice(0, index);
							var staticValue = token.Slice(index+1);
							dynamicProps.Add(name.ToString(), staticValue.ToString());
						}
						else
						{
							dynamicProps.Add(token.ToString(), string.Empty);
						}
						chrRd.Skip(1);
					}					
				}

				UpdateXamlSource();
			}
		}

		private void OnTargetAvPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
		{
			if (dynamicProps == null || dynamicProps.Count == 0)
				return;

			if (e.Sender is AvaloniaObject ao)
			{
				UpdateXamlSource();				
			}
		}

		private void UpdateXamlSource()
		{
			if (DynamicXamlTarget == null || dynamicProps == null || dynamicProps.Count == 0)
				return;

			var owningType = DynamicXamlTarget.GetType().Name;
			string complexValues = string.Empty;
			string text = $"<ui:{owningType} ";
			for (int i = 0; i < availProps.Length; i++)
			{
				var name = availProps[i].Name;

				if (dynamicProps.ContainsKey(name))
				{
					var val = DynamicXamlTarget.GetValue(availProps[i]);

					if (IsComplexValue(val))
					{
						complexValues += FormatComplexValue(owningType, name, val);
					}
					else
					{
						if (dynamicProps[name] == string.Empty)
						{
							text += $"{name}=\"{FormatValue(val)}\" ";
						}
						else
						{
							text += $"{name}=\"{dynamicProps[name]}\" ";
						}
					}					
				}
			}

			if (string.IsNullOrEmpty(complexValues))
			{
				XamlSource = text + " />";
			}
			else
			{
				text += ">" + "\n" + complexValues + $"\n</ui:{owningType}>";
				XamlSource = text;
			}
			
		}

		private string FormatValue(object item)
		{
			if (item == null)
				return string.Empty;

			var type = item.GetType();
			if (type == typeof(Color) || type == typeof(Color2))
			{
				return ((Color2)item).ToHexString(true);
			}
			else if (item is ISolidColorBrush scb)
			{
				return ((Color2)scb.Color).ToHexString(true);
			}
			else if (type == typeof(Thickness))
			{
				var t = (Thickness)item;
				return t.IsUniform ? t.Left.ToString() : $"{t.Left} {t.Top} {t.Right} {t.Bottom}";
			}
			return item.ToString(); 
		}

		private string FormatComplexValue(string ownerType, string propName, object item)
		{
			if (item is IconElement)
			{
				if (item is SymbolIcon si)
				{
					var txt = $"    <ui:{ownerType}.{propName}>\n";
					txt += $"        <ui:SymbolIcon Symbol=\"{si.Symbol}\" />\n";
					txt += $"    </ui:{ownerType}.{propName}>";

					return txt;
				}
				else if (item is BitmapIcon bi)
				{
					var txt = $"    <ui:{ownerType}.{propName}>\n";
					txt += $"        <ui:BitmapIcon UriSource=\"{bi.UriSource}\" />\n";
					txt += $"    </ui:{ownerType}.{propName}>";

					return txt;
				}
				else if (item is FontIcon fi)
				{
					var txt = $"    <ui:{ownerType}.{propName}>\n";
					txt += $"        <ui:FontIcon Glpyh=\"{fi.Glyph}\" />\n";
					txt += $"    </ui:{ownerType}.{propName}>";

					return txt;
				}
				else if (item is PathIcon pi)
				{
					// No way to get path data from StreamGeometry so we can't do anything
					// about PathIcon
				}
			}

			return string.Empty;
		}

		private bool IsComplexValue(object item)
		{
			if (item is IconElement)
				return true;

			return false;
		}

		private AvaloniaList<DocsItemViewModel> properties;

		private AvaloniaProperty[] availProps;
		private Dictionary<string, string> dynamicProps;
	}

	ref struct CharacterReader
	{
		private ReadOnlySpan<char> _s;

		public CharacterReader(ReadOnlySpan<char> s)
			: this()
		{
			_s = s;
		}

		public bool End => _s.IsEmpty;
		public char Peek => _s[0];
		public int Position { get; private set; }
		public char Take()
		{
			Position++;
			char taken = _s[0];
			_s = _s.Slice(1);
			return taken;
		}

		public void SkipWhitespace()
		{
			var trimmed = _s.TrimStart();
			Position += _s.Length - trimmed.Length;
			_s = trimmed;
		}

		public bool TakeIf(char c)
		{
			if (Peek == c)
			{
				Take();
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool TakeIf(Func<char, bool> condition)
		{
			if (condition(Peek))
			{
				Take();
				return true;
			}
			return false;
		}

		public ReadOnlySpan<char> TakeUntil(char c)
		{
			int len;
			for (len = 0; len < _s.Length && _s[len] != c; len++)
			{
			}
			var span = _s.Slice(0, len);
			_s = _s.Slice(len);
			Position += len;
			return span;
		}

		public ReadOnlySpan<char> TakeWhile(Func<char, bool> condition)
		{
			int len;
			for (len = 0; len < _s.Length && condition(_s[len]); len++)
			{
			}
			var span = _s.Slice(0, len);
			_s = _s.Slice(len);
			Position += len;
			return span;
		}

		public ReadOnlySpan<char> TryPeek(int count)
		{
			if (_s.Length < count)
				return ReadOnlySpan<char>.Empty;
			return _s.Slice(0, count);
		}

		public ReadOnlySpan<char> PeekWhitespace()
		{
			var trimmed = _s.TrimStart();
			return _s.Slice(0, _s.Length - trimmed.Length);
		}

		public void Skip(int count)
		{
			if (_s.Length < count)
				return;
			_s = _s.Slice(count);
		}
	}


	public class DocsItemViewModel : ViewModelBase
	{
		public DocsItemViewModel() { }

		public string Name
		{
			get => _name;
			set => this.RaiseAndSetIfChanged(ref _name, value);
		}

		public string Type
		{
			get => _type;
			set => this.RaiseAndSetIfChanged(ref _type, value);
		}

		public string DeclareType
		{
			get => _declareType;
			set => this.RaiseAndSetIfChanged(ref _declareType, value);
		}

		protected string ResolveType(Type t)
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

		private string _declareType;
		private string _name;
		private string _type;
	}

	public class ClassItemViewModel : DocsItemViewModel
	{
		public ClassItemViewModel(Type t)
		{
			InheritChain = new List<string>();

			Type cur = t;
			while (cur != null)
			{
				InheritChain.Add(cur.Name);
				InheritChain.Add(" > ");
				cur = cur.BaseType;
			}
			InheritChain.RemoveAt(InheritChain.Count - 1);
			InheritChain.Reverse();
		}

		public List<string> InheritChain { get; } 
	}
	
	public class PropertyItemViewModel : DocsItemViewModel
	{
		public PropertyItemViewModel(PropertyInfo pi, Type controlType)
		{
			Name = pi.Name;
			CanRead = pi.CanRead;
			CanWrite = pi.CanWrite;
			Type = ResolveType(pi.PropertyType);

			if (controlType != pi.DeclaringType) //Inherited property
			{
				DeclareType = ResolveType(pi.DeclaringType);
			}
		}

		public bool CanRead
		{
			get => _canRead;
			set => this.RaiseAndSetIfChanged(ref _canRead, value);
		}

		public bool CanWrite
		{
			get => _canWrite;
			set => this.RaiseAndSetIfChanged(ref _canWrite, value);
		}


		private bool _canRead;
		private bool _canWrite;
	}	

	public class MethodItemViewModel : DocsItemViewModel
	{
		public MethodItemViewModel(MethodInfo mi, Type controlType)
		{
			Name = mi.Name;
			Type = ResolveType(mi.ReturnType);
			IsAbstract = mi.IsAbstract;
			
			Params = new List<ParamInfoViewModel>(mi.GetParameters().Select(x => new ParamInfoViewModel(x)));
			
			if (controlType != mi.DeclaringType) //Inherited method
			{
				DeclareType = ResolveType(mi.DeclaringType);
			}
		}

		public bool IsAbstract
		{
			get => _isAbstract;
			set => RaiseAndSetIfChanged(ref _isAbstract, value);
		}

		public List<ParamInfoViewModel> Params { get; }

		private bool _isAbstract;
	}

	public class EventItemViewModel : DocsItemViewModel
	{
		public EventItemViewModel(EventInfo ei)
		{
			Name = ei.Name;
			Type = ResolveType(ei.EventHandlerType);
		}
	}

	public class ParamInfoViewModel : DocsItemViewModel
	{
		public ParamInfoViewModel(ParameterInfo pi)
		{
			Type = ResolveType(pi.ParameterType);
			Name = pi.Name;
			IsOptional = pi.IsOptional;
			DefaultValue = pi.DefaultValue;
		}

		public bool IsOptional { get; set; }
		public object DefaultValue { get; set; }
	}

	public class DocTypeTemplateSelector : IElementFactory
	{
		public static readonly AttachedProperty<string> RecyclePoolKeyProperty =
			AvaloniaProperty.RegisterAttached<DocTypeTemplateSelector, IControl, string>("RecyclePoolKey");

		public static void SetRecyclePoolKey(IControl c, string s)
		{
			c.SetValue(RecyclePoolKeyProperty, s);
		}
		public static string GetRecyclePoolKey(IControl c) => c.GetValue(RecyclePoolKeyProperty);


		[Content]
		public List<IDataTemplate> Templates { get; set; } = new List<IDataTemplate>();

		public IControl Build(object param) => GetElement(new ElementFactoryGetArgs { Data = param });

		public IControl GetElement(ElementFactoryGetArgs args)
		{
			for (int i = 0; i < Templates.Count; i++)
			{
				if (Templates[i].Match(args.Data))
				{
					var ctrl = pool.TryGetElement(args.Data.GetType().FullName, args.Parent);

					if (ctrl == null)
						ctrl = Templates[i].Build(args.Data);

					SetRecyclePoolKey(ctrl, args.Data.GetType().FullName);

					return ctrl;
				}
			}

			return null;
		}

		public bool Match(object data)
		{
			for (int i = 0; i < Templates.Count; i++)
			{
				if (Templates[i].Match(data))
					return true;
			}

			return false;
		}

		public void RecycleElement(ElementFactoryRecycleArgs args)
		{
			if (args.Element != null)
			{
				var key = GetRecyclePoolKey(args.Element);
				pool.PutElement(args.Element, key, args.Parent);
			}
		}

		private static RecyclePool pool = new RecyclePool();
	}
}
