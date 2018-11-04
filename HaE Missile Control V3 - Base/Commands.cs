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
        public class Commands
        {
            private Program P;
            private CommsHandler handler;

            public Commands(Program P, CommsHandler handler)
            {
                this.P = P;
                this.handler = handler;
            }

            public void RegisterCommands()
            {
                handler.AddCommand("LaunchTurretGuided", LaunchTurretGuided);
            }

            #region commands
            private void LaunchTurretGuided(List<string> args, long source)
            {
                if (P.missiles.Count < 1)
                    P.FetchMissiles();
                P.launchScheduler.AddTask(P.silos.Open());
                P.launchScheduler.AddTask(LaunchTurretGuidedSM());
            }

            private IEnumerator<bool> LaunchTurretGuidedSM()
            {
                P.Echo($"missilecount: {P.missiles.Count}");

                if (P.missiles.Count > 0)
                {
                    P.missiles.First(x => !x.launched).LaunchMissileTurretGuided();
                    P.missiles.RemoveAt(0);
                }
                yield return false;
            }
            #endregion
        }
    }
}
