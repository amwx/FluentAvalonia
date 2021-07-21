using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Input;
using Avalonia.Markup.Parsers;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Metadata;
using System;

namespace FluentAvalonia.UI.Controls
{
	public class MenuFlyoutSubItemTemplate : ITreeDataTemplate
	{
		public Type DataType { get; set; }

		[AssignBinding]
		public BindingBase HeaderText { get; set; }
		
		[AssignBinding]
		public BindingBase Icon { get; set; }

		[AssignBinding]
		public BindingBase SubItems { get; set; }

		// This template doesn't actually build anything, it just gives the MenuFlyoutItemContainerGenerator
		// what it needs to assemble a MenuFlyoutSubItem
		public IControl Build(object param) => null;

		public InstancedBinding ItemsSelector(object item)
		{
			if (SubItems != null)
			{
				var obs = SubItems switch
				{
					Binding reflection => ExpressionObserverBuilder.Build(item, reflection.Path),
					CompiledBindingExtension compiled => new ExpressionObserver(item, compiled.Path.BuildExpression(false)),
					_ => throw new InvalidOperationException("Only Binding and CompiledBinding is supported!")
				};

				return InstancedBinding.OneWay(obs, BindingPriority.Style);
			}

			return null;
		}

		public InstancedBinding TextSelector(object item)
		{
			if (HeaderText != null)
			{
				var obs = HeaderText switch
				{
					Binding reflection => ExpressionObserverBuilder.Build(item, reflection.Path),
					CompiledBindingExtension compiled => new ExpressionObserver(item, compiled.Path.BuildExpression(false)),
					_ => throw new InvalidOperationException("Only Binding and CompiledBinding is supported!")
				};

				return InstancedBinding.OneWay(obs, BindingPriority.Style);
			}

			return null;
		}

		public InstancedBinding IconSelector(object item)
		{
			if (Icon != null)
			{
				var obs = Icon switch
				{
					Binding reflection => ExpressionObserverBuilder.Build(item, reflection.Path),
					CompiledBindingExtension compiled => new ExpressionObserver(item, compiled.Path.BuildExpression(false)),
					_ => throw new InvalidOperationException("Only Binding and CompiledBinding is supported!")
				};

				return InstancedBinding.OneWay(obs, BindingPriority.Style);
			}

			return null;
		}

		public bool Match(object data)
		{
			return DataType?.IsInstanceOfType(data) ?? true;
		}
	}
}
