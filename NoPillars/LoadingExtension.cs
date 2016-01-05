using ICities;

namespace NoPillars
{
    public class LoadingExtension : LoadingExtensionBase
    {

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            Pillars.Initialize();
            NetInfoExtensions.Initialize();
            NoPillarsUI.Initialize();
            NoPillarsMonitor.Initialize();
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            NoPillarsUI.Dispose();
            NetInfoExtensions.Reset();
            NoPillarsMonitor.Dispose();
        }
    }
}