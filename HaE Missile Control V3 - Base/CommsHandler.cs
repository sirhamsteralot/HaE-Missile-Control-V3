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
        public class CommsHandler
        {
            public Program parent;
            public ACPWrapper acpWrapper;
            public long myId => parent.Me.EntityId;

            Dictionary<string, Action<List<string>, long>> commands;

            public CommsHandler(Program parent, IMyRadioAntenna antenna)
            {
                this.parent = parent;
                if (antenna != null)
                    acpWrapper = new ACPWrapper(parent, antenna);

                commands = new Dictionary<string, Action<List<string>, long>>();
            }

            public void AddCommand(string name, Action<List<string>, long> action)
            {
                commands[name] = action;
            }

            public void RemoveCommand (string name)
            {
                commands.Remove(name);
            }

            public void HandleMain(string argument, bool antenna)
            {
                string[] split;
                long messageSource = 0;

                if (antenna && acpWrapper != null)
                {
                    split = acpWrapper.Main(argument, out messageSource);
                } else
                {
                    split = argument.Split('|');
                }

                if (split.Count() < 1)
                    return;

                Action<List<string>, long> action;
                if (commands.TryGetValue(split[0], out action))
                {
                    split.RemoveAtFast(0);
                    action?.Invoke(split.ToList(), messageSource);
                }
            }
        }
    }
}
