using System.Collections.Generic;
using System.Linq;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace NoPillars
{
    public class NoPillarsMod : IUserMod
    {
        public string Name { get { return "No Pillars"; } }
        public string Description { get { return "Toggle pillars, collision and zoning"; } }
    }

    public class NoPillarsLoadingExtension : LoadingExtensionBase
    {
        public static UIButton b_pillars;
        public static UIButton b_collide;
        //public static UIButton b_trafficLights;

        public class SaveInfo
        {
            public NetInfo prefab;

            public bool collide;
            public bool zoning;
            //public bool trafficLights;
            public BuildingInfo bpi;
            public BuildingInfo bmi;
        }
        public static FastList<SaveInfo> saveList;
        public static TrainTrackBridgeAI track;

        public static int pillars;
        public static int collide;
        //public static int trafficLights;

        public override void OnLevelLoaded(LoadMode mode)
        {
            pillars = 0;
            collide = 0;
            //trafficLights = 0;
            saveList = null;

            var uiView = UIView.GetAView();

            b_pillars = makeButton(uiView, "Pillars");
            b_pillars.tooltip = "Change pillars mode";
            b_pillars.transformPosition = new Vector3(-1.45f, -0.82f);
            b_pillars.eventClick += togglePillars;

            b_collide = makeButton(uiView, "Collide");
            b_collide.tooltip = "Change collision mode";
            b_collide.transformPosition = new Vector3(-1.45f, -0.88f);
            b_collide.eventClick += toggleColliding;

//            b_trafficLights = makeButton(uiView, "Traffic Lights");
//            b_trafficLights.tooltip = "Change traffic lights mode";
//            b_trafficLights.transformPosition = new Vector3(-1.25f, -0.82f);
//            b_trafficLights.eventClick += toggleTrafficLights;
        }

        public override void OnLevelUnloading()
        {
            revert();
            pillars = 0;
            collide = 0;
            //trafficLights = 0;
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
            b.isTooltipLocalized = false;
            return b;
        }

        private static HashSet<NetInfo> GetPrefabs()
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

            var prefabs = GetPrefabs();
            if (track == null)
            {
                foreach (var prefab in prefabs.Where(prefab => prefab.m_netAI is TrainTrackBridgeAI && prefab.name == "Train Track Elevated"))
                {
                    track = (TrainTrackBridgeAI)prefab.m_netAI;
                } 
            }
            var modifiedAi = new List<NetAI>();
            foreach (var prefab in prefabs)
            {
                saveList.Add(GetSave(prefab));
                prefab.m_canCollide = (collide != 1) && prefab.m_canCollide;
                var mNetAi = prefab.m_netAI;
                if (prefab.m_netAI == null || modifiedAi.Contains(mNetAi))
                {
                    continue;
                }
                modifiedAi.Add(mNetAi);

                var ta = mNetAi as TrainTrackBridgeAI;
                if (ta != null)
                {
                    if (pillars == 1)
                    {
                        ta.m_bridgePillarInfo = null;
                        ta.m_middlePillarInfo = null;
                    }
                    else if (pillars == 2 && track != null)
                    {
                        ta.m_bridgePillarInfo = track.m_bridgePillarInfo;
                        ta.m_middlePillarInfo = track.m_middlePillarInfo;
                    }
                }
                var ra = mNetAi as RoadBridgeAI;
                if (ra != null)
                {
                    if (pillars == 1)
                    {
                        ra.m_bridgePillarInfo = null;
                        ra.m_middlePillarInfo = null;
                    }
                    else if (pillars == 2 && track != null)
                    {
                        ra.m_bridgePillarInfo = track.m_bridgePillarInfo;
                        ra.m_middlePillarInfo = track.m_middlePillarInfo;
                    }
                }
                var pa = mNetAi as PedestrianBridgeAI;
                if (pa != null)
                {
                    if (pillars != 0)
                    {
                        pa.m_bridgePillarInfo = null;
                    }
                }
                var r2 = mNetAi as RoadAI;
                if (r2 != null)
                {
                    switch (collide)
                    {
                        case 0:
                            break;
                        case 1:
                        case 2:
                            r2.m_enableZoning = false;
                            break;
                        case 3:
                            r2.m_enableZoning = true;
                            break;
                    }
//                    switch (trafficLights)
//                    {
//                        case 0:
//                            break;
//                        case 1:
//                            r2.m_trafficLights = false;
//                            break;
//                        case 2:
//                            r2.m_trafficLights = true;
//                            break;    
//                    }

                }

            }
        }

        private static SaveInfo GetSave(NetInfo prefab)
        {
            var mNetAi = prefab.m_netAI;
            var si = new SaveInfo();
            si.prefab = prefab;
            si.collide = prefab.m_canCollide;
            var ta = mNetAi as TrainTrackBridgeAI;
            if (ta != null)
            {
                si.bpi = ta.m_bridgePillarInfo;
                si.bmi = ta.m_middlePillarInfo;
            }
            var ra = mNetAi as RoadBridgeAI;
            if (ra != null)
            {
                si.bpi = ra.m_bridgePillarInfo;
                si.bmi = ra.m_middlePillarInfo;
            }
            var pa = mNetAi as PedestrianBridgeAI;
            if (pa != null)
            {
                si.bpi = pa.m_bridgePillarInfo;
            }
            var r2 = mNetAi as RoadAI;
            if (r2 != null)
            {
                si.zoning = r2.m_enableZoning;
//                    si.trafficLights = r2.m_trafficLights;
            }
            return si;
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
//                    r2.m_trafficLights = si.trafficLights;
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
            collide = (collide + 1) % 4;
            modify();
            switch (collide)
            {
                case 0:
                    b_collide.text = "Collide";
                    break;
                case 1:
                    b_collide.text = "Overlap";
                    break;
                case 2:
                    b_collide.text = "No Zoning";
                    break;
                case 3:
                    b_collide.text = "Force Zoning";
                    break;

            }
        }

//        private void toggleTrafficLights(UIComponent component, UIMouseEventParameter eventParam)
//        {
//            trafficLights = (trafficLights + 1) % 3;
//            modify();
//            switch (trafficLights)
//            {
//                case 0:
//                    b_trafficLights.text = "Traffic Lights";
//                    break;
//                case 1:
//                    b_trafficLights.text = "No Traffic Lights";
//                    break;
//                case 2:
//                    b_trafficLights.text = "Force Traffic Lights";
//                    break;
//
//            }
//        }
    }
}
