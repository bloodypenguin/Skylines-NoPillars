using System;
using System.Linq;
using ICities;

namespace NoPillars.Options
{
    public static class Util
    {
        public static string GetPropertyDescription<T>(this T value, string propertyName)
        {
            var fi = value.GetType().GetProperty(propertyName);
            var attributes =
         (CheckboxAttribute[])fi.GetCustomAttributes(typeof(CheckboxAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;

            return propertyName;
        }

        public static Action<bool> GetPropertyAction<T>(this T value, string propertyName)
        {
            var fi = value.GetType().GetProperty(propertyName);
            var attributes =
         (CheckboxAttribute[])fi.GetCustomAttributes(typeof(CheckboxAttribute), false);

            if (attributes == null || attributes.Length != 1 || attributes[0].method == null)
                return b => { };

            var method = attributes[0].method;
            return b =>
            {
                method.Invoke(null, new object[] { b });
            };


        }

        public static void AddOptionsGroup(UIHelperBase helper, string groupName)
        {
            var group = helper.AddGroup(groupName);
            var properties = typeof(NoPillars.Options.Options).GetProperties();
            foreach (var name in from property in properties select property.Name)
            {
                var description = OptionsHolder.Options.GetPropertyDescription(name);
                group.AddCheckbox(description, name, OptionsHolder.Options.GetPropertyAction(name));

            }
        }
    }
}