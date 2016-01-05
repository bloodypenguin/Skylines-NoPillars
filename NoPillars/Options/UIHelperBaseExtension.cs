using System;
using ICities;

namespace NoPillars.Options
{
    public static class UIHelperBaseExtension
    {
        public static void AddCheckbox(this UIHelperBase group, string text, string propertyName, Action<bool>  action)
        {
            var property = typeof (NoPillars.Options.Options).GetProperty(propertyName);
            group.AddCheckbox(text, (bool)property.GetValue(OptionsHolder.Options, null),
                b =>
                {
                    property.SetValue(OptionsHolder.Options, b, null);
                    OptionsLoader.SaveOptions();
                    action.Invoke(b);
                });
        } 
    }
}