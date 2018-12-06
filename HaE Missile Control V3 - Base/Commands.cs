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
                handler.AddCommand("SendLidarPos", SendLidarPos);
                handler.AddCommand("AquireLidarLock", AquireLidarLock);
                handler.AddCommand("SendLidarData", SendLidarData);
            }

            #region commands
            private void LaunchTurretGuided(List<string> args, long source)
            {
                P.launchScheduler.AddTask(P.silos.Open());
                P.launchScheduler.AddTask(LaunchTurretGuidedSM());
            }

            private IEnumerator<bool> LaunchTurretGuidedSM()
            {
                if (P.missiles.Count > 0)
                {
                    P.missiles.GetMissile(true).LaunchMissileTurretGuided();
                }
                yield return false;
            }

            private void AquireLidarLock(List<string> args, long source)
            {
                if (P.entityTrackingModule != null)
                {
                    P.entityTrackingModule.PaintTarget(P.targetingCastLength);
                    P.Echo("Locked!");
                }
                    
            }

            private void SendLidarPos(List<string> args, long source)
            {
                Vector3D pos;
                if (Vector3D.TryParse(args[1], out pos))
                {
                    var missile = P.missiles.GetLaunchedMissile(pos, Missile.MissileStatus.Launched);
                    missile?.RetargetRayCast(pos);
                }
            }

            private void SendLidarData(List<string> args, long source)
            {
                P.waitForNextLock += WaitForNextLock;
            }
            private void WaitForNextLock(HaE_Entity entity)
            {
                var missile = P.missiles.GetLaunchedMissile(entity.entityInfo.Position, Missile.MissileStatus.Launched);
                missile?.RetargetRayCast(entity.entityInfo.Position);
            }
            #endregion
        }
    }
}
