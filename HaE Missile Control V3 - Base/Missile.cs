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
        public class Missile
        {
            public IMyProgrammableBlock missileCore;
            public bool launched;
            public long Id;
            public MissileStatus status = MissileStatus.Idle;

            public bool Alive => !missileCore.IsClosed();
            public Vector3D CurrentPos => missileCore.GetPosition();
            public bool raycastActive = false;

            public Missile(IMyProgrammableBlock missileCore)
            {
                this.missileCore = missileCore;
                Id = missileCore.EntityId;
            }

            public void LaunchMissileTurretGuided()
            {
                if (missileCore.TryRun("LaunchTurretGuided"))
                {
                    launched = true;
                    status = MissileStatus.Launched | MissileStatus.TurretGuided;
                }
            }

            public void RetargetRayCast(Vector3D pos)
            {
                if (missileCore.TryRun("RetargetRayCast|" + pos))
                {
                    launched = true;
                    raycastActive = true;

                    status = MissileStatus.LidarGuided | MissileStatus.Launched;
                }
            }

            [Flags]
            public enum MissileStatus
            {
                Idle,
                Launched,
                TurretGuided,
                LidarGuided
            }
        }
    }
}
