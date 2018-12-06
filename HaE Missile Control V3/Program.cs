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
        public double backupDetonationEngageDist { get { return (double)nameSerializer.GetValue("backupDetonationEngageDist"); } }

        EntityTracking_Module targetTracker;
        ControlModule controlModule;
        ProNav proNav;
        CommsHandler commsHandler;
        INISerializer nameSerializer;
        Payload payload;

        IMyShipController control;
        IMyShipMergeBlock mergeBlock;

        CurrentMode mode;

        Scheduler missionScheduler;
        bool initialized = false;


        public Program()
        {
            #region serializer
            nameSerializer = new INISerializer("HaE Missile");
            nameSerializer.AddValue("controllerName", x => x, "Control");
            nameSerializer.AddValue("mergeBlockName", x => x, "MergeBlock");
            nameSerializer.AddValue("backupDetonationEngageDist", x => double.Parse(x), 100);

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
        }

        public void Init()
        {
            #region fetchblocks
            GridTerminalSystemUtils GTS = new GridTerminalSystemUtils(Me, GridTerminalSystem);
            control = GTS.GetBlockWithNameOnGrid(controllerName) as IMyShipController;
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

            EntityTracking_Module.refExpSettings refExp = EntityTracking_Module.refExpSettings.Sensor | EntityTracking_Module.refExpSettings.Turret;
            targetTracker = new EntityTracking_Module(GTS, control, null, refExp);
            targetTracker.onEntityDetected += OnTargetFound;

            controlModule = new ControlModule(GTS, control);
            proNav = new ProNav(controlModule, 30);

            missionScheduler = new Scheduler();

            payload = new Payload(GTS, backupDetonationEngageDist);
            #endregion

            mode = CurrentMode.Idle;

            initialized = true;
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
            {
                if (!initialized)
                    Init();

                commsHandler.HandleMain(argument, (updateSource & UpdateType.Antenna) != 0);
            }
                

            missionScheduler.Main();

            if (mode == CurrentMode.Idle || mode == CurrentMode.Launching)
                return;

            
            targetTracker.Poll();
            payload.Main();
        }

        public void OnTargetFound(HaE_Entity target)
        {
            Vector3D targetPos = target.entityInfo.Position;

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

                if (target.entityInfo.HitPosition.HasValue)
                    targetPos = target.entityInfo.HitPosition.Value;
            }

            if ((target.entityInfo.Relationship != MyRelationsBetweenPlayerAndBlock.Enemies) &&
                (target.entityInfo.Relationship != MyRelationsBetweenPlayerAndBlock.Neutral) &&
                (target.entityInfo.Relationship != MyRelationsBetweenPlayerAndBlock.NoOwnership))
                return;

            double distSq = Vector3D.DistanceSquared(Me.GetPosition(), targetPos);
            payload.UpdateDist(distSq);

            Vector3D reqDir = proNav.Navigate(target);
            double reqMag = reqDir.Normalize();

            controlModule.AimMissile(reqDir);

            controlModule.ApplyThrust(reqDir);
        }
    }
}