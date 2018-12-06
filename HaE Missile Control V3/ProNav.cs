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
        public class ProNav
        {
            public double NavGain;

            public ControlModule controller;

            public ProNav(ControlModule controller, double navGain = 3)
            {
                NavGain = navGain;
                this.controller = controller;
            }

            /// <summary>
            /// PRONAV
            /// </summary>
            /// <param name="target"></param>
            /// <returns>Heading we need to aim at</returns>
            public Vector3D Navigate(HaE_Entity target)
            {
                Vector3D targetpos = target.entityInfo.Position;
                if (target.entityInfo.HitPosition.HasValue)
                    targetpos = target.entityInfo.HitPosition.Value;

                Vector3D myVel = controller.control.GetShipVelocities().LinearVelocity;

                Vector3D rangeVec = targetpos - controller.control.GetPosition();
                Vector3D closingVel = target.entityInfo.Velocity - myVel;

                Vector3D accel = CalculateAccel(rangeVec, closingVel);
                accel += -controller.control.GetNaturalGravity();                              //Gravity term

                double maxForwardAccel = ThrustUtils.GetForwardThrust(controller.thrusters, controller.control);
                maxForwardAccel /= controller.control.CalculateShipMass().PhysicalMass;

                double forwardAccel = maxForwardAccel;
                double accelMag = accel.Normalize();

                forwardAccel -= accelMag;
                forwardAccel = MathHelperD.Clamp(forwardAccel, 0, maxForwardAccel);

                accel *= accelMag;
                accel += Vector3D.Normalize(rangeVec) * forwardAccel;
                return accel;
            }

            private Vector3D CalculateAccel(Vector3D rangeVec, Vector3D closingVelocity)
            {
                // Calculate rotation vec
                Vector3D RxV = Vector3D.Cross(rangeVec, closingVelocity);
                Vector3D RdR = rangeVec * rangeVec;
                Vector3D rotVec = RxV / RdR;

                Vector3D Los = Vector3D.Normalize(rangeVec);

                // Pronav term
                Vector3D accelerationNormal = (NavGain * closingVelocity).Cross(rotVec);
                return accelerationNormal;
            }
        }
    }
}
