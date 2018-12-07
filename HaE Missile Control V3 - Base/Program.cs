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
        public string targetingCameraName { get { return (string)nameSerializer.GetValue("targetingCameraName"); } }
        public string controllername { get { return (string)nameSerializer.GetValue("controllername"); } }
        public double targetingCastLength { get { return (double)nameSerializer.GetValue("targetingCastLength"); } }

        public string missileTag { get { return (string)nameSerializer.GetValue("missileTag"); } }
        public string siloDoorTag { get { return (string)nameSerializer.GetValue("siloDoorTag"); } }
        public string missileStatusLCDTag { get { return (string)nameSerializer.GetValue("missileStatusLCDTag"); } }
        public string IgnoreTag { get { return (string)nameSerializer.GetValue("IgnoreTag"); } }


        INISerializer nameSerializer;
        CommsHandler commsHandler;
        MissileSilos silos;
        Scheduler launchScheduler;
        StatusWriter statusWriter;
        MissileManager missiles;
        EntityTracking_Module entityTrackingModule;

        Action<HaE_Entity> waitForNextLock = null;


        public Program()
        {
            #region serializer
            nameSerializer = new INISerializer("HaE MissileBase");
            nameSerializer.AddValue("missileTag", x => x, "[HaE Missile]");
            nameSerializer.AddValue("siloDoorTag", x => x, "[HaE SiloDoor]");
            nameSerializer.AddValue("missileStatusLCDTag", x => x, "[HaE MissileStatus]");
            nameSerializer.AddValue("targetingCameraName", x => x, "TargetingCamera");
            nameSerializer.AddValue("controllername", x => x, "controller");
            nameSerializer.AddValue("IgnoreTag", x => x, "[IgnoreTracker]");
            nameSerializer.AddValue("targetingCastLength", x => double.Parse(x), 3000);

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
            missiles = new MissileManager(GTS, this, missileTag);

            var antennas = new List<IMyRadioAntenna>();
            GTS.GetBlocksOfTypeOnGrid(antennas);

            var camera = GridTerminalSystem.GetBlockWithName(targetingCameraName) as IMyCameraBlock;
            var controller = GridTerminalSystem.GetBlockWithName(controllername) as IMyShipController;
            #endregion

            #region initModules
            if (antennas.Count > 0)
                commsHandler = new CommsHandler(this, antennas.First());
            else
                commsHandler = new CommsHandler(this, null);

            var commands = new Commands(this, commsHandler);
            commands.RegisterCommands();

            silos = new MissileSilos(GTS, this, siloDoorTag);
            launchScheduler = new Scheduler();

            if (camera != null && controller != null)
            {
                

                entityTrackingModule = new EntityTracking_Module(GTS, controller, camera, IgnoreTag);

                ITracking cameraTracker = null;
                foreach (ITracking tracker in entityTrackingModule.ObjectTrackers)
                {
                    var camT = tracker as LidarTracking;
                    if (camT != null)
                        cameraTracker = camT;
                }
                entityTrackingModule.ObjectTrackers.Clear();
                entityTrackingModule.ObjectTrackers.Add(cameraTracker);

                entityTrackingModule.onEntityDetected += OnEntityDetected;
            } else
            {
                Echo($"camera: {camera != null}\ncontroller: {controller != null}");
            }
            

            var lcds = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType(lcds, x=> x.CustomName.Contains(missileStatusLCDTag));
            statusWriter = new StatusWriter(this, lcds);
            #endregion

            Runtime.UpdateFrequency = UpdateFrequency.Update1 | UpdateFrequency.Update100;
        }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Update100) != 0)
            {
                missiles.FetchMissiles();
                statusWriter.UpdateStatus();
                entityTrackingModule.TimeoutEntities(TimeSpan.FromSeconds(5));
            }

            entityTrackingModule?.Poll();

            statusWriter.Main();
            launchScheduler.Main();
            commsHandler.HandleMain(argument, (updateSource & UpdateType.Antenna) != 0);
        }

        #region events
        public void OnEntityDetected(HaE_Entity entity)
        {
            statusWriter.LogEvent($"LIDAR: Entity detected! | {entity.entityInfo.Name}");

            if (waitForNextLock != null)
            {
                waitForNextLock.Invoke(entity);
                waitForNextLock = null;
            }
        }
        #endregion
    }
}