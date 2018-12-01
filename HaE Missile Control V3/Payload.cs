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
        public class Payload
        {
            List<IMyWarhead> warheads = new List<IMyWarhead>();
            List<MyDetectedEntityInfo> detected = new List<MyDetectedEntityInfo>();
            IMySensorBlock sensor;

            double backupDetonateEngageDistSq;

            public Payload(GridTerminalSystemUtils GTS, double backupDetonateEngageDist)
            {
                this.backupDetonateEngageDistSq = backupDetonateEngageDist * backupDetonateEngageDist;

                GTS.GetBlocksOfTypeOnGrid(warheads);

                var templist = new List<IMySensorBlock>();
                GTS.GetBlocksOfTypeOnGrid(templist);

                if (templist.Count > 0)
                {
                    sensor = templist.First();
                }
            }

            public void Detonate()
            {
                foreach (var warhead in warheads)
                {
                    warhead.IsArmed = true;
                    warhead.Detonate();
                }
            }

            public void SetCountDown(float seconds)
            {
                foreach (var warhead in warheads)
                {
                    warhead.DetonationTime = seconds;
                    warhead.StartCountdown();
                }
            }

            public void Main()
            {
                if (sensor == null)
                    return;

                detected.Clear();
                sensor.DetectedEntities(detected);

                foreach (var entity in detected)
                {
                    if ((entity.Relationship & (MyRelationsBetweenPlayerAndBlock.Enemies | MyRelationsBetweenPlayerAndBlock.Neutral | MyRelationsBetweenPlayerAndBlock.NoOwnership)) != 0)
                        Detonate();
                }
            }

            public void UpdateDist(double distanceSq)
            {
                if (distanceSq <= backupDetonateEngageDistSq)
                {
                    SetCountDown(2f);
                }
            }
        }
    }
}
