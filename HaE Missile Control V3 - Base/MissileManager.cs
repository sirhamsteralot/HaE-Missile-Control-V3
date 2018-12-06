using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class MissileManager
        {
            public int Count => siloMissiles.Count;
            public int LaunchedCount => launchedMissiles.Count;

            private List<Missile> siloMissiles = new List<Missile>();
            private List<Missile> launchedMissiles = new List<Missile>();

            private GridTerminalSystemUtils GTS;
            private string missileTag;

            private Program P;

            public MissileManager(GridTerminalSystemUtils GTS, Program P, string missileTag)
            {
                this.GTS = GTS;
                this.missileTag = missileTag;
                this.P = P;
            }

            public Missile GetLaunchedMissile(Vector3D closestTo)
            {
                if (launchedMissiles.Count < 1)
                    return null;

                launchedMissiles.Sort((x1, x2) => Vector3D.DistanceSquared(x1.CurrentPos, closestTo).CompareTo(Vector3D.DistanceSquared(x2.CurrentPos, closestTo)));

                Missile missile = null;

                try
                {
                    missile = launchedMissiles.First(x => x.launched && x.Alive && !x.raycastActive);
                } catch (Exception e)
                {
                    P.Echo(e.Message + "\n" +e.StackTrace);
                }
                
                return missile;
            }

            public Missile GetMissile(bool launching)
            {
                if (siloMissiles.Count < 1)
                    FetchMissiles();

                if (siloMissiles.Count < 1)
                    return null;

                var missile = siloMissiles.First();

                if (launching)
                {
                    siloMissiles.RemoveAt(0);
                    launchedMissiles.Add(missile);
                }
                
                return missile;
            }

            List<IMyProgrammableBlock> tempPbs = new List<IMyProgrammableBlock>();
            public void FetchMissiles()
            {
                tempPbs.Clear();
                siloMissiles.Clear();
                GTS.GridTerminalSystem.GetBlocksOfType(tempPbs, x => x.CustomName.Contains(missileTag));

                foreach (var pb in tempPbs)
                {
                    siloMissiles.Add(new Missile(pb));
                }

                for(int i = 0; i < launchedMissiles.Count; i++)
                {
                    if (!launchedMissiles[i].Alive)
                    {
                        launchedMissiles.RemoveAtFast(i);
                        i--;
                        P.statusWriter.LogEvent("Missile Closed!");
                    }
                }
            }

            #region comparer

            #endregion
        }
    }
}
