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
        public class MissileSilos
        {
            List<IMyDoor> siloDoors = new List<IMyDoor>();
            Program P;

            public MissileSilos(GridTerminalSystemUtils GTS , Program P, string SiloDoorTag)
            {
                GTS.GridTerminalSystem.GetBlocksOfType(siloDoors, x => x.CustomName.Contains(SiloDoorTag));
                this.P = P;
            }

            public DoorStatus GetSiloStatus()
            {
                if (siloDoors.Count > 0)
                    return siloDoors.First().Status;

                return DoorStatus.Open;
            }

            public IEnumerator<bool> Open()
            {
                if (siloDoors.Count < 1)
                    yield return false;

                P.Echo("Opening!");

                int amountToRemove = 0;
                for (int i = 0; i < siloDoors.Count; i++)
                {
                    if (siloDoors[i].IsClosed())
                    {
                        siloDoors.Move(i, siloDoors.Count - 1);
                        amountToRemove++;
                        continue;
                    }

                    siloDoors[i].OpenDoor();
                }
                siloDoors.RemoveRange(siloDoors.Count - amountToRemove, amountToRemove);


                foreach(var door in siloDoors)
                {
                    while (door.Status != DoorStatus.Open)
                    {
                        P.Echo("Opening...");
                        yield return true;
                    }
                }

                P.Echo("Opened!");
            }

            public void Close()
            {
                foreach (var door in siloDoors)
                {
                    door.CloseDoor();
                }
            }
        }
    }
}
