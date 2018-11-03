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
        public string controllerName { get { return (string)nameSerializer.GetValue("controllerName"); } }
        public string mergeBlockName { get { return (string)nameSerializer.GetValue("mergeBlockName"); } }
        public string triggerName { get { return (string)nameSerializer.GetValue("triggerName"); } }

        EntityTracking_Module targetTracker;
        ControlModule controlModule;
        ProNav proNav;
        CommsHandler commsHandler;
        INISerializer nameSerializer;

        IMyShipController control;
        IMyTimerBlock trigger;
        IMyShipMergeBlock mergeBlock;

        CurrentMode mode;

        Scheduler missionScheduler;


        public Program()
        {
            #region serializer
            nameSerializer = new INISerializer("HaE Missile");
            nameSerializer.AddValue("controllerName", x => x, "Control");
            nameSerializer.AddValue("mergeBlockName", x => x, "MergeBlock");
            nameSerializer.AddValue("triggerName", x => x, "Trigger");

            if (Me.CustomData == "")
            {
                string temp = Me.CustomData;
                nameSerializer.FirstSerialization(ref temp);
                Me.CustomData = temp;
            }
            else
            {
                nameSerializer.DeSerialize(Me.CustomData);
            }
            #endregion

            #region fetchblocks
            GridTerminalSystemUtils GTS = new GridTerminalSystemUtils(Me, GridTerminalSystem);
            control = GTS.GetBlockWithNameOnGrid(controllerName) as IMyShipController;
            trigger = GTS.GetBlockWithNameOnGrid(triggerName) as IMyTimerBlock;
            mergeBlock = GTS.GetBlockWithNameOnGrid(mergeBlockName) as IMyShipMergeBlock;

            var antennas = new List<IMyRadioAntenna>();
            GTS.GetBlocksOfTypeOnGrid(antennas);
            #endregion

            #region initModules
            if (antennas.Count > 0)
                commsHandler = new CommsHandler(this, antennas.First());
            else
                commsHandler = new CommsHandler(this, null);

            var commands = new Commands(this, commsHandler);
            commands.RegisterCommands();

            targetTracker = new EntityTracking_Module(GTS, control, null);
            targetTracker.onEntityDetected += OnTargetFound;

            controlModule = new ControlModule(GTS, control);
            proNav = new ProNav(control, 30);

            missionScheduler = new Scheduler();
            #endregion

            mode = CurrentMode.Idle;

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
                commsHandler.HandleMain(argument, (updateSource & UpdateType.Antenna) != 0);

            missionScheduler.Main();

            if (mode == CurrentMode.Idle || mode == CurrentMode.Launching)
                return;

            
            targetTracker.Poll();
        }

        public void OnTargetFound(HaE_Entity target)
        {
            if (mode == CurrentMode.Idle || mode == CurrentMode.Launching)
            {
                return;
            } else if (mode == CurrentMode.TurretGuided)
            {
                if (target.trackingType != HaE_Entity.TrackingType.Turret)
                    return;
            } else if (mode == CurrentMode.LidarGuided)
            {
                if (target.trackingType != HaE_Entity.TrackingType.Lidar)
                    return;
            }

            if ((target.entityInfo.Relationship != MyRelationsBetweenPlayerAndBlock.Enemies) &&
                (target.entityInfo.Relationship != MyRelationsBetweenPlayerAndBlock.Neutral) &&
                (target.entityInfo.Relationship != MyRelationsBetweenPlayerAndBlock.NoOwnership))
                return;

            Vector3D reqDir = proNav.Navigate(target);
            double reqMag = reqDir.Normalize();

            controlModule.AimMissile(reqDir);

            controlModule.ApplyThrust(reqDir);
            trigger?.Trigger();
        }
    }
}