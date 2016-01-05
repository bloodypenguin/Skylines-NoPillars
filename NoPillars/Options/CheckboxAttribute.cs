using System;
using System.Reflection;

namespace NoPillars.Options
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CheckboxAttribute : Attribute {
        public CheckboxAttribute(string description, string actionClass, string actionMethod)
        {
            Description = description;
            method = global::NoPillars.Util.FindType(actionClass)
                .GetMethod(actionMethod, BindingFlags.Public | BindingFlags.Static);
        }

        public CheckboxAttribute(string description)
        {
            Description = description;
            method = null;
        }

        public string Description { get; }
        public MethodInfo method { get; }
    }
}