using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework.Plugins;
using ICities;
using UnityEngine;

namespace NoPillars
{
    public static class Util
    {
        public static Type FindType(string className)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types.Where(type => type.Name == className))
                    {
                        return type;
                    }
                }
                catch
                {
                    // ignored
                }
            }
            return null;
        }

        public static bool IsModActive(string modName)
        {
            var plugins = PluginManager.instance.GetPluginsInfo();
            return (from plugin in plugins.Where(p => p.isEnabled)
                    select plugin.GetInstances<IUserMod>() into instances
                    where instances.Any()
                    select instances[0].Name into name
                    where name == modName
                    select name).Any();
        }


        public static HashSet<NetInfo> GetAllPrefabs()
        {
            var result = new HashSet<NetInfo>();
            foreach (var prefab in Resources.FindObjectsOfTypeAll<NetInfo>())
            {
                result.Add(prefab);
            }
            return result;
        }
    }
}