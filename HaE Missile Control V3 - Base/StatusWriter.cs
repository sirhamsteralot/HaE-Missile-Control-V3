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
    partial class Program
    {
        public class StatusWriter
        {
            private List<IMyTextPanel> lcds;

            private bool update;
            private Program P;
            private StringBuilder buffer;
            private StringBuilder writeBuffer;
            private HashSet<string> logEvents;

            Scheduler internalScheduler;

            public StatusWriter(Program p, List<IMyTextPanel> lcds)
            {
                this.lcds = lcds;
                buffer = new StringBuilder();
                writeBuffer = new StringBuilder();
                logEvents = new HashSet<string>();
                internalScheduler = new Scheduler();
                P = p;
            }

            public void Main()
            {
                internalScheduler.Main();

                if (update)
                    UpdateLCD();
            }

            public void LogEvent(string message)
            {
                logEvents.Add(message);
            }

            
            public void UpdateStatus()
            {
                buffer.Clear();

                buffer.AppendLine("HaE MissileControl V3");
                buffer.AppendLine($"MissileCount: {P.missiles.Count}");
                buffer.AppendLine($"Silostatus: {P.silos.GetSiloStatus()}");
                buffer.AppendLine($"Launched: {P.missiles.LaunchedCount}");
                buffer.Append('=', 40).AppendLine();

                foreach(var logEvent in logEvents)
                {
                    buffer.AppendLine(logEvent);
                }
                logEvents.Clear();

                if (!buffer.Equals(writeBuffer))
                {
                    update = true;
                    writeBuffer.Clear();
                    writeBuffer.Append(buffer);
                }
            }

            private void UpdateLCD()
            {
                if (internalScheduler.QueueCount < 1)
                    internalScheduler.AddTask(InternalUpdate());
            }

            private IEnumerator<bool> InternalUpdate()
            {
                foreach (var lcd in lcds)
                {
                    lcd.WritePublicText(writeBuffer.ToString());
                    yield return true;
                }
                update = false;
            }
        }
    }
}
