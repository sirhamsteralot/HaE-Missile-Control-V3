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
        public class ControlModule
        {
            public List<IMyGyro> gyros = new List<IMyGyro>();
            public List<IMyThrust> thrusters = new List<IMyThrust>();
            public IMyShipController control;

            public ControlModule(GridTerminalSystemUtils gts, IMyShipController control)
            {
                this.control = control;

                gts.GetBlocksOfTypeOnGrid(thrusters);
                gts.GetBlocksOfTypeOnGrid(gyros);
            }

            public void AimMissile(Vector3D targetDir)
            {
                GyroUtils.PointInDirection(gyros, control, targetDir, 1, true);
            }

            public void ApplyThrust(Vector3D dir)
            {
                ThrustUtils.SetThrustBasedDot(thrusters, dir, 2.5);
            }

            public void LaunchForward()
            {
                ThrustUtils.SetThrustBasedDot(thrusters, control.WorldMatrix.Forward);
            }
        }
    }
}
