using ICities;

namespace NoPillars
{
    public class NoPillarsMod : IUserMod
    {
        public string Name => "No Pillars";
        public string Description => "Toggle Pillars, collision and zoning";

        public void OnSettingsUI(UIHelperBase helper)
        {
            Options.Util.AddOptionsGroup(helper, "No Pillars Options");
        }
    }
}
