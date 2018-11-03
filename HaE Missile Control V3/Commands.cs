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
                P.missionScheduler.AddTask(LaunchGuidedTask());
            }
            private IEnumerator<bool> LaunchGuidedTask()
            {
                P.mode = CurrentMode.Launching;
                P.controlModule.LaunchForward();

                for (int i = 0; i < 60; i++)        //Delay enabling of the turretguidance till a second after launch
                    yield return true;

                P.mode = CurrentMode.TurretGuided;
            }
            #endregion
        }
    }
}
