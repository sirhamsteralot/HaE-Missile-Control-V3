﻿using Sandbox.Game.EntityComponents;
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

            private List<Missile> siloMissiles = new List<Missile>();
            private List<Missile> launchedMissiles = new List<Missile>();

            private GridTerminalSystemUtils GTS;
            private string missileTag;

            public MissileManager(GridTerminalSystemUtils GTS, string missileTag)
            {
                this.GTS = GTS;
                this.missileTag = missileTag;
            }

            public Missile GetLaunchedMissile(Vector3D closestTo)
            {
                if (launchedMissiles.Count < 1)
                    return null;

                launchedMissiles.Sort((x1, x2) => Vector3D.DistanceSquared(x1.CurrentPos, closestTo).CompareTo(Vector3D.DistanceSquared(x2.CurrentPos, closestTo)));
                return launchedMissiles.First();
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

                int countToRemove = 0;
                for(int i = 0; i < launchedMissiles.Count; i++)
                {
                    if (!launchedMissiles[i].Alive)
                    {
                        launchedMissiles.Move(i, launchedMissiles.Count - 1);
                        countToRemove++;
                        i--;
                    }
                }

                launchedMissiles.RemoveRange(launchedMissiles.Count - countToRemove, countToRemove);
            }

            #region comparer

            #endregion
        }
    }
}
