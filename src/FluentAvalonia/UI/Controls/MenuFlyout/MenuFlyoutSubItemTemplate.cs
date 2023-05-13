using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Represents a <see cref="IDataTemplate"/> that is used to generate items in a 
/// <see cref="FAMenuFlyout"/>
/// </summary>
public class MenuFlyoutSubItemTemplate : ITreeDataTemplate
{
    /// <summary>
    /// The type of data being bound
    /// </summary>
    public Type DataType { get; set; }

    /// <summary>
    /// The binding for the <see cref="MenuFlyoutSubItem.Text"/> property
    /// </summary>
    [AssignBinding]
    public BindingBase HeaderText { get; set; }

    /// <summary>
    /// The binding for the <see cref="MenuFlyoutSubItem.Icon"/> property
    /// </summary>
    [AssignBinding]
    public BindingBase Icon { get; set; }

    /// <summary>
    /// The binding to resolve the <see cref="MenuFlyoutSubItem.Items"/>
    /// </summary>
    [AssignBinding]
    public BindingBase SubItems { get; set; }

    // This template doesn't actually build anything, it just gives the MenuFlyoutItemContainerGenerator
    // what it needs to assemble a MenuFlyoutSubItem
    public Control Build(object param) => null;

    public InstancedBinding ItemsSelector(object item)
    {
        return null;
    }
       
    public bool Match(object data)
    {
        return DataType?.IsInstanceOfType(data) ?? true;
    }
}
