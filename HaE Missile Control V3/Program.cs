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
    partial class Program : MyGridProgram
    {
        EntityTracking_Module targetTracker;
        ControlModule controlModule;
        ProNav proNav;
        CommsHandler commsHandler;

        IMyShipController control;
        IMyTimerBlock trigger;

        CurrentStatus status;


        public Program()
        {
            commsHandler = new CommsHandler(this);

            GridTerminalSystemUtils GTS = new GridTerminalSystemUtils(Me, GridTerminalSystem);
            control = GTS.GetBlockWithNameOnGrid("Control") as IMyShipController;
            trigger = GTS.GetBlockWithNameOnGrid("Trigger") as IMyTimerBlock;

            targetTracker = new EntityTracking_Module(GTS, control, null);
            targetTracker.onEntityDetected += OnTargetFound;

            controlModule = new ControlModule(GTS, control);
            proNav = new ProNav(control, 30);

            status = CurrentStatus.Idle;

            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Antenna) != 0 ||
                (updateSource & UpdateType.Terminal) != 0 ||
                (updateSource & UpdateType.Trigger) != 0 ||
                (updateSource & UpdateType.Script) != 0)
                commsHandler.HandleMain(argument);

            targetTracker.Poll();
        }

        public void OnTargetFound(HaE_Entity target)
        {
            if (status == CurrentStatus.Idle || status == CurrentStatus.Launching)
            {
                return;
            } else if (status == CurrentStatus.TurretGuided)
            {
                if (target.trackingType != HaE_Entity.TrackingType.Turret)
                    return;
            } else if (status == CurrentStatus.LidarGuided)
            {
                if (target.trackingType != HaE_Entity.TrackingType.Lidar)
                    return;
            }

            if ((target.entityInfo.Relationship & MyRelationsBetweenPlayerAndBlock.Enemies) == 0 &&
                (target.entityInfo.Relationship & MyRelationsBetweenPlayerAndBlock.Neutral) == 0)
                return;

            Vector3D reqDir = proNav.Navigate(target);
            double reqMag = reqDir.Normalize();

            controlModule.AimMissile(reqDir);

            controlModule.ApplyThrust(reqDir);
            trigger?.Trigger();
        }
    }
}