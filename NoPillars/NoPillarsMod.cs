using ICities;
using NoPillars.Options;

namespace NoPillars
{
    public class NoPillarsMod : IUserMod
    {
        private static bool _optionsLoaded;

        public string Name
        {
            get
            {
                if (!_optionsLoaded)
                {
                    OptionsLoader.LoadOptions();
                    _optionsLoaded = true;
                }
                return "No Pillars";
            }
        }

        public string Description => "Toggle Pillars, collision and zoning";

        public void OnSettingsUI(UIHelperBase helper)
        {
            Options.Util.AddOptionsGroup(helper, "No Pillars Options");
        }
    }
}
