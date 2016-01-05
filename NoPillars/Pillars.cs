using System.Collections.Generic;
using System.IO;
using System.Linq;
using ColossalFramework.Packaging;

namespace NoPillars
{
    public static class Pillars
    {
        public static BuildingInfo[] pillars { get; private set; }
        public static bool networkSkinsEnabled { get; private set; }

        public static void Initialize()
        {
            networkSkinsEnabled = Util.IsModActive("Network Skins");
            pillars = GetUniquePillars();
        }

        private static BuildingInfo[] GetUniquePillars()
        {
            var pillarsTemp = new HashSet<BuildingInfo>();
            foreach (var buildingInfo in Util.GetAllPrefabs().SelectMany(prefab => prefab.GetPillars()))
            {
                pillarsTemp.Add(buildingInfo);
            }
            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
            {
                var prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);
                if (prefab == null || prefab.m_buildingAI.GetType() != typeof(BuildingAI))
                {
                    continue;
                }
                var asset = PackageManager.FindAssetByName(prefab.name);
                var crpPath = asset?.package?.packagePath;
                var directoryName = Path.GetDirectoryName(crpPath);
                if (directoryName == null)
                {
                    continue;
                }
                var pillarConfigPath = Path.Combine(directoryName, "Pillar.xml");
                if (!File.Exists(pillarConfigPath))
                {
                    continue;
                }
                pillarsTemp.Add(prefab);
            }
            return pillarsTemp.ToArray();
        }
    }
}