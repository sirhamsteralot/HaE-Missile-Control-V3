﻿using Sandbox.Game.EntityComponents;
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
        public string missileTag { get { return (string)nameSerializer.GetValue("missileTag"); } }
        public string siloDoorTag { get { return (string)nameSerializer.GetValue("siloDoorTag"); } }
        public string missileStatusLCDTag { get { return (string)nameSerializer.GetValue("missileStatusLCDTag"); } }

        INISerializer nameSerializer;
        CommsHandler commsHandler;
        MissileSilos silos;
        Scheduler launchScheduler;
        StatusWriter statusWriter;
        MissileManager missiles;

        public Program()
        {
            #region serializer
            nameSerializer = new INISerializer("HaE MissileBase");
            nameSerializer.AddValue("missileTag", x => x, "[HaE Missile]");
            nameSerializer.AddValue("siloDoorTag", x => x, "[HaE SiloDoor]");
            nameSerializer.AddValue("missileStatusLCDTag", x => x, "[HaE MissileStatus]");

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
            }

            statusWriter.Main();
            launchScheduler.Main();
            commsHandler.HandleMain(argument, (updateSource & UpdateType.Antenna) != 0);
        }
    }
}