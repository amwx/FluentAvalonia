using System;

namespace FluentAvalonia.Core.Attributes;

/// <summary>
/// Marks that a property or class has not been implemented in the FluentAvalonia version
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
public class NotImplementedAttribute : Attribute
{
    public NotImplementedAttribute() { }
}
