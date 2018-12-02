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
                handler.AddCommand("LaunchGPSGuided", LaunchGPSGuided);
                handler.AddCommand("RetargetRayCast", RetargetRayCast);
            }

            #region commands
            private void LaunchTurretGuided(List<string> args, long source)
            {
                P.missionScheduler.AddTask(LaunchTask());
                P.missionScheduler.AddTask(SwitchToMode(CurrentMode.TurretGuided));
            }

            private void LaunchGPSGuided(List<string> args, long source)
            {
                if (args.Count < 2)
                    return;
                Vector3D targetloc;
                if (!Vector3D.TryParse(args[1], out targetloc))
                    return;    

                P.missionScheduler.AddTask(LaunchTask());

                P.missionScheduler.AddTask(TargetLocation(targetloc));

                P.missionScheduler.AddTask(SwitchToMode(CurrentMode.GPSGuided));
            }

            private void RetargetRayCast(List<string> args, long source)
            {
                if (args.Count < 2)
                    return;

                Vector3D targetloc;
                if (!Vector3D.TryParse(args[1], out targetloc))
                    return;

                
                if (!P.targetTracker.PaintTarget(targetloc).entityInfo.IsEmpty())
                    P.missionScheduler.AddTask(SwitchToMode(CurrentMode.LidarGuided));
            }
            #endregion

            #region helperTasks
            private IEnumerator<bool> LaunchTask()
            {
                P.mode = CurrentMode.Launching;
                if (P.mergeBlock != null)
                    P.mergeBlock.Enabled = false;
                P.controlModule.LaunchForward();

                for (int i = 0; i < 90; i++)        //Delay enabling of the turretguidance till 1.5 seconds after launch
                    yield return true;
            }
            private IEnumerator<bool> TargetLocation(Vector3D targetLocation)
            {
                Vector3D direction = targetLocation - P.control.GetPosition();
                double distance = direction.Normalize();
                P.controlModule.LaunchForward();
                P.controlModule.AimMissile(direction);
                yield return true;
            }

            private IEnumerator<bool> SwitchToMode(CurrentMode mode)
            {
                P.mode = mode;
                yield return true;
            }
            #endregion
        }
    }
}
