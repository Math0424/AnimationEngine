using AnimationEngine.Core;
using AnimationEngine.Language;
using AnimationEngine.Utility;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace AnimationEngine
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    internal class AnimationEngine : MySessionComponentBase
    {

        private static readonly string MainPath = "data/animation/";
        private static readonly string MainInfo = "main.info";
        private static readonly string MainScript = "main.bsl";

        private static List<CoreScript> loaded = new List<CoreScript>();
        private static List<string> failed = new List<string>();

        private static Dictionary<string, MyTuple<Subpart[], ScriptRunner>> registeredScripts = new Dictionary<string, MyTuple<Subpart[], ScriptRunner>>();

        public static void AddToRegisteredScripts(string id, Subpart[] subparts, ScriptRunner constant)
        {
            if (registeredScripts.ContainsKey(id))
            {
                Utils.LogToFile($"Warning, Multiple scripts registered for {id}, overriding previous script.");
            }
            registeredScripts[id] = new MyTuple<Subpart[], ScriptRunner>(subparts, constant);
        }

        protected override void UnloadData()
        {
            Utils.CloseLog();
        }

        int currentTick = 0;
        public override void UpdateAfterSimulation()
        {
            currentTick++;
            //TODO: create a better data scructure
            foreach (var x in loaded)
            {
                if (x == null || x.Entity == null || !x.Entity.InScene)
                    continue;

                if (MyAPIGateway.Utilities?.IsDedicated ?? false)
                {
                    if (currentTick % 3 == 0)
                    {
                        TickObject(x, 3);
                    }
                    continue;
                }

                if (MyAPIGateway.Session?.Camera != null && MyAPIGateway.Session.Camera.Position != null)
                {
                    double dist = Vector3.DistanceSquared(MyAPIGateway.Session.Camera.Position, x.Entity.GetPosition());
                    if (dist > 1000000) // 1km
                    {
                        if (currentTick % 30 == 0)
                            TickObject(x, 30);
                    }
                    if (dist > 100000) // 316 meters
                    {
                        if (currentTick % 10 == 0)
                            TickObject(x, 10);
                    }
                    else if (dist > 10000) // 100 meters
                    {
                        if (currentTick % 4 == 0)
                            TickObject(x, 4);
                    }
                    else if (dist > 1000) //31 meters
                    {
                        if (currentTick % 2 == 0)
                            TickObject(x, 2);
                    }
                    else // otherwise 60 fps
                    {
                        TickObject(x, 1);
                    }
                }
            }
        }

        private void TickObject(CoreScript script, int time)
        {
            try
            {
                script?.Tick(time);
            }
            catch (Exception ex)
            {
                Utils.LogToFile($"Error while ticking {script.Entity.Name}");
                Utils.LogToFile(ex.TargetSite);
                Utils.LogToFile(ex.StackTrace);
                Utils.LogToFile(ex.Message);
            }

        }

        public override void LoadData()
        {
            Utils.LogToFile($"Starting Animation Engine...");
            Utils.LogToFile($"Reading {MyAPIGateway.Session.Mods.Count} mods");
            int registered = 0;
            foreach (var mod in MyAPIGateway.Session.Mods)
            {
                try
                {
                    if (MyAPIGateway.Utilities.FileExistsInModLocation(MainPath + MainScript, mod))
                    {
                        new ScriptGenerator(mod, MainPath + MainScript);
                        registered++;
                    }
                    else if (MyAPIGateway.Utilities.FileExistsInModLocation(MainPath + MainInfo, mod))
                    {
                        Utils.LogToFile($"Reading animation file for {mod.Name}");
                        foreach (var s in MyAPIGateway.Utilities.ReadFileInModLocation(MainPath + MainInfo, mod).ReadToEnd().Split('\n'))
                        {
                            if (s.ToLower().StartsWith("animation "))
                            {
                                new ScriptGenerator(mod, MainPath + s.ToLower().Substring(10).Trim() + ".bsl");
                                registered++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!(ex is ScriptError))
                    {
                        Utils.LogToFile("System critical script error!");
                    }
                    Utils.LogToFile(ex.ToString());
                    failed.Add(mod.Name);
                }
            }
            Utils.LogToFile($"Loaded {registered} scripts");

            if (failed.Count != 0)
            {
                string mods = "";
                foreach (var x in failed)
                {
                    mods += x + ", ";
                }
                mods = mods.Substring(0, mods.Length - 2);
                MyAPIGateway.Utilities.ShowMessage("AnimationEngine", $"These mods have errors\n{mods}\n check logs for more info");
            }

            MyEntities.OnEntityCreate += OnEntityAdded;


        }

        public void OnEntityAdded(IMyEntity ent)
        {
            if (ent is IMyCubeGrid)
            {
                ((IMyCubeGrid)ent).OnBlockAdded -= OnBlockAdded;
                ((IMyCubeGrid)ent).OnBlockAdded += OnBlockAdded;
            }
        }

        public void OnBlockAdded(IMySlimBlock block)
        {
            string id = block.BlockDefinition.Id.SubtypeId.String;
            if (block.FatBlock != null && registeredScripts.ContainsKey(id))
            {
                if (block.FatBlock is IMyCubeBlock)
                {
                    var x = registeredScripts[id];
                    CoreScript script = new CoreScript(x.Item1);
                    script.AddComponent(x.Item2.Clone());
                    script.Init(block.FatBlock);
                }
                else
                {
                    Utils.LogToFile($"Cannot attach script to {block.FatBlock.EntityId} (Cannot attach to armor blocks)");
                }
            }
        }

        public static void RemoveScript(CoreScript script)
        {
            loaded.Remove(script);
        }

        public static void AddScript(CoreScript script)
        {
            loaded.Add(script);
        }

    }
}
