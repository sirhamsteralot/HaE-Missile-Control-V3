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
        public class StatusWriter
        {
            private List<IMyTextPanel> lcds;

            private bool update;
            private Program P;
            private StringBuilder buffer;
            private StringBuilder writeBuffer;

            public StatusWriter(Program p, List<IMyTextPanel> lcds)
            {
                this.lcds = lcds;
                buffer = new StringBuilder();
                writeBuffer = new StringBuilder();
                P = p;
            }

            public void Main()
            {
                if (update)
                    UpdateLCD();
            }

            
            public void UpdateStatus()
            {
                buffer.Clear();

                buffer.AppendLine("HaE MissileControl V3");
                buffer.AppendLine($"MissileCount: {P.missiles.Count}");
                buffer.AppendLine($"Silostatus: {P.silos.GetSiloStatus()}");

                if (buffer != writeBuffer)
                {
                    update = true;
                    writeBuffer = buffer;
                }
            }

            private void UpdateLCD()
            {
                foreach(var lcd in lcds)
                {
                    lcd.WritePublicText(writeBuffer.ToString());
                }
            }
        }
    }
}
