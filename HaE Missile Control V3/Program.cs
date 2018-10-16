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

        IMyShipController control;
        IMyTimerBlock trigger;


        public Program()
        {
            GridTerminalSystemUtils GTS = new GridTerminalSystemUtils(Me, GridTerminalSystem);
            control = GridTerminalSystem.GetBlockWithName("Control") as IMyShipController;
            trigger = GridTerminalSystem.GetBlockWithName("Trigger") as IMyTimerBlock;

            targetTracker = new EntityTracking_Module(GTS, control, null);
            targetTracker.onEntityDetected += OnTargetFound;

            controlModule = new ControlModule(GTS, control);
            proNav = new ProNav(control, 30);

            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {
            targetTracker.Poll();
        }

        public void OnTargetFound(HaE_Entity target)
        {
            if (target.trackingType != HaE_Entity.TrackingType.Turret)
                return;

            Vector3D reqDir = proNav.Navigate(target);
            double reqMag = reqDir.Normalize();

            Echo($"correction: {reqMag}");

            controlModule.AimMissile(reqDir);

            controlModule.ApplyThrust(1f);
            trigger?.Trigger();
        }
    }
}