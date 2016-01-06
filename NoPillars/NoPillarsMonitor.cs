using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColossalFramework;
using UnityEngine;

namespace NoPillars
{
    public class NoPillarsMonitor : MonoBehaviour
    {
        private const string GameObjectName = "NoPillarsMonitor";

        private NetInfo currentPrefab;
        private FieldInfo finePrefab;
        private Type fineType;
        private List<NetInfoExtensions.Metadata> savedMetadata;
        private int currentPillars = -1;
        private int currentCollide = -1;
        private bool currentAlwaysVisible;

        public static void Initialize()
        {
            var gameObject = new GameObject(GameObjectName);
            var monitor = gameObject.AddComponent<NoPillarsMonitor>();
            if (!Util.IsModActive("Fine Road Heights"))
            {
                return;
            }
            monitor.fineType = FineNetToolType();
            if (monitor.fineType != null)
            {
                monitor.finePrefab = monitor.fineType.GetField("m_prefab",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            }
        }

        public static void Dispose()
        {
            var gameObject = GameObject.Find(GameObjectName);
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }


        public void Awake()
        {
            currentAlwaysVisible = Options.OptionsHolder.Options.alwaysVisible;
        }

        public void Update()
        {
            var prefab = GetToolPrefab();
            var alwaysVisible = Options.OptionsHolder.Options.alwaysVisible;
            if (prefab == null && !alwaysVisible)
            {
                NoPillarsUI.Hide();
                if (currentPrefab == null && currentAlwaysVisible == alwaysVisible)
                {
                    return;
                }
                Reset();
                currentPrefab = null;
                if (Options.OptionsHolder.Options.resetOnHide)
                {
                    NoPillarsUI.Reset();
                }
                currentAlwaysVisible = alwaysVisible;
            }
            else {
                currentAlwaysVisible = alwaysVisible;
                NoPillarsUI.Show();
                var collide = NoPillarsUI.Collide;
                var pillars = NoPillarsUI.Pillars;
                if (currentCollide == collide && currentPillars == pillars)
                {
                    return;
                }
                Reset();
                currentPrefab = prefab;
                currentPillars = pillars;
                currentCollide = collide;
                savedMetadata = new List<NetInfoExtensions.Metadata>();

                foreach (var prefabToModify in Util.GetAllPrefabs())
                {
                    savedMetadata.Add(prefabToModify.GetMetadata());
                    prefabToModify.SetMetadata(collide, pillars);
                }
            }
        }

        private void Reset()
        {
            if (savedMetadata != null)
            {
                foreach (var metadata in savedMetadata)
                {
                    metadata.info.SetMetadata(metadata);
                }
            }
            savedMetadata = null;
            currentPrefab = null;
            currentPillars = -1;
            currentCollide = -1;
        }

        private NetInfo GetToolPrefab() { 
            var tool = ToolsModifierControl.GetCurrentTool<ToolBase>();
            if (tool == null)
            {
                return null;
            }
            var netTool = tool as NetTool;
            if (netTool != null)
            {
                return netTool.m_prefab;
            }
            var buildingTool = tool as BuildingTool;
            if (buildingTool != null)
            {
                BuildingInfo building;
                if (buildingTool.m_relocate == 0)
                {
                    building = buildingTool.m_prefab;
                }
                else
                {
                    building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingTool.m_relocate].Info;
                }
                var paths = building?.m_paths;
                if (paths == null || paths.Length < 1)
                {
                    return null;
                }
                return (from path in paths where path.m_netInfo != null select path.m_netInfo).FirstOrDefault();
            }
            if (tool.GetType() == fineType)
            {
                return (NetInfo)finePrefab.GetValue(tool);
            }
            return null;
        }

        private static Type FineNetToolType()
        {
            return Util.FindType("NetToolFine");
        }

    }
}