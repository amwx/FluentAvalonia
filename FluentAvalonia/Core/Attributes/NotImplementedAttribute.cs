using System;

namespace FluentAvalonia.Core.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class NotImplementedAttribute : Attribute
    {
        public NotImplementedAttribute() { }


    }
}
