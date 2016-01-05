using System;
using System.Collections.Generic;
using System.Reflection;
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
        private int currentMode = -1;

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

        public void Update()
        {
            var prefab = GetToolPrefab();
            if (prefab == null)
            {
                NoPillarsUI.Hide();
                if (currentPrefab == null)
                {
                    return;
                }
                Reset();
                NoPillarsUI.Reset();
            }
            else {
                NoPillarsUI.Show();
                var collide = NoPillarsUI.Collide;
                var pillars = NoPillarsUI.Pillars;
                var mode = NoPillarsUI.Mode;
                if (currentCollide == collide && currentPillars == pillars && currentMode == mode && (mode == (int)ModificationMode.AllPrefabs || currentPrefab == prefab))                {
                    return;
                }
                if (currentPrefab != null)
                {
                    Reset();
                }
                currentPrefab = prefab;
                currentPillars = pillars;
                currentCollide = collide;
                currentMode = mode;
                savedMetadata = new List<NetInfoExtensions.Metadata>();

                HashSet<NetInfo> prefabsToModify;
                switch (mode)
                {
                    case (int)ModificationMode.AllVersions:
                        prefabsToModify = prefab.GetAllVersions();
                        break;
                    case (int)ModificationMode.AllPrefabs:
                        prefabsToModify = Util.GetAllPrefabs();
                        break;
                    default:
                        prefabsToModify = new HashSet<NetInfo> { prefab };
                        break;
                }
                foreach (var prefabToModify in prefabsToModify)
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
            currentMode = -1;
        }

        private NetInfo GetToolPrefab()
        {
            if (finePrefab == null)
            {
                var netTool = ToolsModifierControl.GetCurrentTool<NetTool>();
                return netTool?.m_prefab;
            }
            var tool = ToolsModifierControl.GetCurrentTool<ToolBase>();
            if (tool != null && tool.GetType() == fineType)
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