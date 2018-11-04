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

            public MissileSilos(GridTerminalSystemUtils GTS , string SiloDoorTag)
            {
                GTS.GridTerminalSystem.GetBlocksOfType(siloDoors, x => x.CustomName.Contains(SiloDoorTag));
            }

            public IEnumerator<bool> Open()
            {
                if (siloDoors.Count < 1)
                    yield return false;

                foreach (var door in siloDoors)
                {
                    door.OpenDoor();
                }

                foreach(var door in siloDoors)
                {
                    if (door.Status == DoorStatus.Open)
                        continue;

                    while (door.Status == DoorStatus.Opening)
                        yield return true;
                }
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
