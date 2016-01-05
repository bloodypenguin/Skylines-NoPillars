using System;
using System.Collections.Generic;

namespace NoPillars
{
    public static class NetInfoExtensions
    {

        public struct Metadata
        {
            public NetInfo info;
            public bool collide;
            public bool zoning;
            public BuildingInfo bpi;
            public BuildingInfo bmi;
        }

        public static Metadata GetMetadata(this NetInfo prefab)
        {
            var mNetAi = prefab.m_netAI;
            var si = new Metadata
            {
                collide = prefab.m_canCollide,
                info = prefab
            };
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
            }
            return si;
        }

        public static void SetMetadata(this NetInfo prefab, Metadata metadata)
        {
            if (metadata.info != prefab)
            {
                throw new Exception("NoPillars: wrong metadata!");
            }
            prefab.m_canCollide = metadata.collide;
            var r2 = prefab.m_netAI as RoadAI;
            if (r2 != null)
            {
                r2.m_enableZoning = metadata.zoning;
            }
            if (Pillars.networkSkinsEnabled)
            {
                return;
            }
            var ta = prefab.m_netAI as TrainTrackBridgeAI;
            if (ta != null)
            {
                ta.m_bridgePillarInfo = metadata.bpi;
                ta.m_middlePillarInfo = metadata.bmi;
            }
            var ra = prefab.m_netAI as RoadBridgeAI;
            if (ra != null)
            {
                ra.m_bridgePillarInfo = metadata.bpi;
                ra.m_middlePillarInfo = metadata.bmi;
            }
            var pa = prefab.m_netAI as PedestrianBridgeAI;
            if (pa != null)
            {
                pa.m_bridgePillarInfo = metadata.bpi;
            }

        }

        public static void SetMetadata(this NetInfo prefab, int collide, int pillars)
        {
            prefab.m_canCollide = (collide != 1) && prefab.m_canCollide;
            var mNetAi = prefab.m_netAI;
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
            }
            if (Pillars.networkSkinsEnabled)
            {
                return;
            }
            var ta = mNetAi as TrainTrackBridgeAI;
            if (ta != null)
            {
                if (pillars == 1)
                {
                    ta.m_bridgePillarInfo = null;
                    ta.m_middlePillarInfo = null;
                }
                else if (pillars > 1)
                {
                    ta.m_bridgePillarInfo = Pillars.pillars[pillars - 2];
                    ta.m_middlePillarInfo = Pillars.pillars[pillars - 2];
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
                else if (pillars > 1)
                {
                    ra.m_bridgePillarInfo = Pillars.pillars[pillars - 2];
                    ra.m_middlePillarInfo = Pillars.pillars[pillars - 2];
                }
            }
            var pa = mNetAi as PedestrianBridgeAI;
            if (pa != null)
            {
                if (pillars == 1)
                {
                    pa.m_bridgePillarInfo = null;
                }
                else if (pillars > 1)
                {
                    pa.m_bridgePillarInfo = Pillars.pillars[pillars - 2];
                }
            }

        }

        public static HashSet<BuildingInfo> GetPillars(this NetInfo prefab)
        {
            HashSet<BuildingInfo> pillars = new HashSet<BuildingInfo>();
            var roadAi = prefab.m_netAI as RoadBridgeAI;
            if (roadAi != null)
            {
                pillars.Add(roadAi.m_bridgePillarInfo);
                pillars.Add(roadAi.m_middlePillarInfo);
            }
            var trainTrackAi = prefab.m_netAI as TrainTrackBridgeAI;
            if (trainTrackAi != null)
            {
                pillars.Add(trainTrackAi.m_bridgePillarInfo);
                pillars.Add(trainTrackAi.m_middlePillarInfo);
            }
            var pedestrianPathAi = prefab.m_netAI as PedestrianBridgeAI;
            if (pedestrianPathAi != null)
            {
                pillars.Add(pedestrianPathAi.m_bridgePillarInfo);
            }
            pillars.RemoveWhere(bi => bi == null);
            return pillars;
        }
    }
}