﻿using System.Collections.Generic;
using System.Linq;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace NoPillars
{
    public class NoPillarsMod : IUserMod
    {
        public string Name { get { return "No Pillars"; } }
        public string Description { get { return "Toggle pillars and collision, and use railroad tracks in the asset editor"; } }
    }

    public class NoPillarsLoadingExtension : LoadingExtensionBase
    {
        public static UIButton b_pillars;
        public static UIButton b_collide;
        public static UIButton b_tstrack;
        public static UIButton b_mstrack;

        public class SaveInfo
        {
            public NetInfo prefab;

            public bool collide;
            public bool zoning;
            public BuildingInfo bpi;
            public BuildingInfo bmi;
        }
        public static FastList<SaveInfo> saveList;
        public static SaveInfo track;

        public static int pillars;
        public static bool collide = true;

        public override void OnLevelLoaded(LoadMode mode)
        {
            pillars = 0;
            collide = true;
            saveList = null;

            var uiView = UIView.GetAView();

            var aeo = 0f;
            if (mode == LoadMode.LoadAsset || mode == LoadMode.NewAsset)
            {
                aeo = 0.2f;

                b_tstrack = makeButton(uiView, "Train S.Track");
                b_tstrack.tooltip = "Build train station track";
                b_tstrack.transformPosition = new Vector3(-1.25f + aeo, -0.82f);
                b_tstrack.eventClick += (component, eventParam) =>
                {
                    switchToRoadByName("Train Station Track");
                };

                b_mstrack = makeButton(uiView, "Metro S.Track");
                b_tstrack.tooltip = "Build metro station track";
                b_mstrack.transformPosition = new Vector3(-1.25f + aeo, -0.88f);
                b_mstrack.eventClick += (component, eventParam) =>
                {
                    switchToRoadByName("Metro Station Track");
                };
            }

            b_pillars = makeButton(uiView, "Pillars");
            b_pillars.tooltip = "Change pillars mode";
            b_pillars.transformPosition = new Vector3(-1.45f + aeo, -0.82f);
            b_pillars.eventClick += togglePillars;

            b_collide = makeButton(uiView, "Collide");
            b_collide.tooltip = "Change collision mode";
            b_collide.transformPosition = new Vector3(-1.45f + aeo, -0.88f);
            b_collide.eventClick += toggleColliding;
        }

        public override void OnLevelUnloading()
        {
            revert();
            pillars = 0;
            collide = true;
        }

        private UIButton makeButton(UIView uiView, string t)
        {
            UIButton b = (UIButton)uiView.AddUIComponent(typeof(UIButton));
            b.text = t;
            b.width = 100;
            b.height = 24;
            b.normalBgSprite = "ButtonMenu";
            b.disabledBgSprite = "ButtonMenuDisabled";
            b.hoveredBgSprite = "ButtonMenuHovered";
            b.focusedBgSprite = "ButtonMenuFocused";
            b.pressedBgSprite = "ButtonMenuPressed";
            b.textColor = new Color32(255, 255, 255, 255);
            b.disabledTextColor = new Color32(7, 7, 7, 255);
            b.hoveredTextColor = new Color32(7, 132, 255, 255);
            b.focusedTextColor = new Color32(255, 255, 255, 255);
            b.pressedTextColor = new Color32(30, 30, 44, 255);
            b.playAudioEvents = true;
            return b;
        }

        private void switchToRoadByName(string name)
        {
            var nt = ToolsModifierControl.SetTool<NetTool>();
            if (nt == null)
            {
                return;
            }
            foreach (var prefab in getPrefabs().Where(prefab => prefab.name == name))
            {
                nt.m_prefab = prefab;
                return;
            }
        }

        private static IEnumerable<NetInfo> getPrefabs()
        {
            var result = new HashSet<NetInfo>();
            foreach (var collection in Object.FindObjectsOfType<NetCollection>())
            {
                AddPrefabsToResult(ref collection.m_prefabs, ref result);
            }

            AddPrefabsFromGameObject("German Roads Prefabs", ref result);
            AddPrefabsFromGameObject("CSL-Traffic Custom Prefabs", ref result);
            AddPrefabsFromGameObject("Some Roads Prefabs", ref result);

            return result;
        }

        private static void AddPrefabsFromGameObject(string gameObjectName, ref HashSet<NetInfo> result)
        {
            var gameObject = GameObject.Find(gameObjectName);
            if (gameObject == null)
            {
                return;
            }
            var prefabs = gameObject.GetComponentsInChildren<NetInfo>(true);
            AddPrefabsToResult(ref prefabs, ref result);
        }

        private static void AddPrefabsToResult(ref NetInfo[] prefabs, ref HashSet<NetInfo> result)
        {
            if (prefabs == null)
            {
                return;
            }
            foreach (var prefab in prefabs)
            {
                if (prefab == null || result.Contains(prefab))
                {
                    continue;
                }
                result.Add(prefab);
            }

        }

        private void modify()
        {
            if (saveList != null)
            {
                revert();
            }
            saveList = new FastList<SaveInfo>();

            foreach (var prefab in getPrefabs())
            {
                var si = new SaveInfo();

                if (prefab.name == "Train Track Elevated")
                {
                    track = si;
                }

                si.prefab = prefab;
                si.collide = prefab.m_canCollide;
                prefab.m_canCollide = collide && prefab.m_canCollide;

                var mNetAi = prefab.m_netAI;
                var ta = mNetAi as TrainTrackBridgeAI;
                if (ta != null)
                {
                    si.bpi = ta.m_bridgePillarInfo;
                    si.bmi = ta.m_middlePillarInfo;
                    if (pillars == 1)
                    {
                        ta.m_bridgePillarInfo = null;
                        ta.m_middlePillarInfo = null;
                    }
                    else if (pillars == 2 && track != null)
                    {
                        ta.m_bridgePillarInfo = track.bpi;
                        ta.m_middlePillarInfo = track.bmi;
                    }
                }
                var ra = mNetAi as RoadBridgeAI;
                if (ra != null)
                {
                    si.bpi = ra.m_bridgePillarInfo;
                    si.bmi = ra.m_middlePillarInfo;
                    if (pillars == 1)
                    {
                        ra.m_bridgePillarInfo = null;
                        ra.m_middlePillarInfo = null;
                    }
                    else if (pillars == 2 && track != null)
                    {
                        ra.m_bridgePillarInfo = track.bpi;
                        ra.m_middlePillarInfo = track.bmi;
                    }
                }
                var pa = mNetAi as PedestrianBridgeAI;
                if (pa != null)
                {
                    si.bpi = pa.m_bridgePillarInfo;
                    if (pillars != 0)
                    {
                        pa.m_bridgePillarInfo = null;
                    }
                }
                var r2 = mNetAi as RoadAI;
                if (r2 != null)
                {
                    si.zoning = r2.m_enableZoning;
                    if (!collide) r2.m_enableZoning = false;
                }
                saveList.Add(si);
            }
        }

        private void revert()
        {
            if (saveList == null)
            {
                return;
            }
            foreach (var si in saveList)
            {
                si.prefab.m_canCollide = si.collide;
                var ta = si.prefab.m_netAI as TrainTrackBridgeAI;
                if (ta != null)
                {
                    ta.m_bridgePillarInfo = si.bpi;
                    ta.m_middlePillarInfo = si.bmi;
                }
                var ra = si.prefab.m_netAI as RoadBridgeAI;
                if (ra != null)
                {
                    ra.m_bridgePillarInfo = si.bpi;
                    ra.m_middlePillarInfo = si.bmi;
                }
                var pa = si.prefab.m_netAI as PedestrianBridgeAI;
                if (pa != null)
                {
                    pa.m_bridgePillarInfo = si.bpi;
                }
                var r2 = si.prefab.m_netAI as RoadAI;
                if (r2 != null)
                {
                    r2.m_enableZoning = si.zoning;
                }

            }
            saveList = null;
        }

        private void togglePillars(UIComponent component, UIMouseEventParameter eventParam)
        {
            pillars = (pillars + 1) % 3;
            modify();
            switch (pillars)
            {
                case 0:
                    b_pillars.text = "Pillars";
                    break;
                case 1:
                    b_pillars.text = "Floating";
                    break;
                case 2:
                    b_pillars.text = "Pillars (Side)";
                    break;
            }
        }

        private void toggleColliding(UIComponent component, UIMouseEventParameter eventParam)
        {
            collide = !collide;
            modify();
            b_collide.text = collide ? "Collide" : "Overlap";
        }
    }
}
