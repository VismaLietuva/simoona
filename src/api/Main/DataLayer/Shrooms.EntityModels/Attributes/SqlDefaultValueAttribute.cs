using System;

namespace Shrooms.EntityModels.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SqlDefaultValueAttribute : Attribute
    {
        public string DefaultValue { get; set; }
    }
}