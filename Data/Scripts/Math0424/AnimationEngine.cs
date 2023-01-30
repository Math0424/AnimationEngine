using AnimationEngine.Core;
using AnimationEngine.LanguageV1;
using AnimationEngine.Utility;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace AnimationEngine
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    internal class AnimationEngine : MySessionComponentBase
    {

        private static readonly string MainPath = "data/animation/";
        private static readonly string MainInfo = "main.info";
        private static readonly string MainScript = "main.bsl";

        private static Dictionary<string, ScriptConstants> registered = new Dictionary<string, ScriptConstants>();
        private static List<BlockScript> loaded = new List<BlockScript>();

        private static List<string> failed = new List<string>();

        public static void AddToRegistered(string id, ScriptConstants constant)
        {
            if (registered.ContainsKey(id))
            {
                Utility.LogToFile($"Warning, Multiple scripts registered for block {id}, overriding previous script.");
            }
            registered[id] = constant;
        }

        protected override void UnloadData()
        {
            Utility.CloseLog();
        }

        int currentTick = 0;
        public override void UpdateAfterSimulation()
        {
            try
            {
                currentTick++;
                //TODO: create a better data scructure
                foreach (var x in loaded)
                {
                    if (MyAPIGateway.Utilities.IsDedicated)
                    {
                        if (currentTick % 3 == 0)
                        {
                            x?.Tick(3);
                        }
                        continue;
                    }

                    if (MyAPIGateway.Session.Camera != null)
                    {
                        double dist = Vector3.DistanceSquared(MyAPIGateway.Session.Camera.Position, x.Block.GetPosition());
                        if (dist > 1000000) // 1km
                        {
                            if (currentTick % 30 == 0)
                                x?.Tick(30);
                        }
                        if (dist > 100000) // 316 meters
                        {
                            if (currentTick % 10 == 0)
                                x?.Tick(10);
                        } 
                        else if (dist > 10000) // 100 meters
                        {
                            if (currentTick % 4 == 0)
                                x?.Tick(4);
                        } 
                        else if(dist > 1000) //31 meters
                        {
                            if (currentTick % 2 == 0)
                                x?.Tick(2);
                        }
                        else // otherwise 60 fps
                        {
                            x?.Tick(1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.LogToFile(ex.TargetSite);
                Utility.LogToFile(ex.StackTrace);
                Utility.LogToFile(ex.Message);
            }
        }

        public override void LoadData()
        {
            Utility.LogToFile($"Starting Animation Engine...");
            Utility.LogToFile($"Reading {MyAPIGateway.Session.Mods.Count} mods");
            int registered = 0;
            foreach (var mod in MyAPIGateway.Session.Mods)
            {
                try
                {
                    if (MyAPIGateway.Utilities.FileExistsInModLocation(MainPath + MainScript, mod))
                    {
                        new ScriptV1Generator(mod, MainPath + MainScript);
                        registered++;
                    } 
                    else if(MyAPIGateway.Utilities.FileExistsInModLocation(MainPath + MainInfo, mod))
                    {
                        Utility.LogToFile($"Reading animation file for {mod.Name}");
                        foreach (var s in MyAPIGateway.Utilities.ReadFileInModLocation(MainPath + MainInfo, mod).ReadToEnd().Split('\n'))
                        {
                            if (s.ToLower().StartsWith("animation "))
                            {
                                new ScriptV1Generator(mod, MainPath + s.ToLower().Substring(10).Trim() + ".bsl");
                                registered++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!(ex is ScriptError))
                    {
                        Utility.LogToFile("System critical script error!");
                    }
                    Utility.LogToFile(ex.ToString());
                    failed.Add(mod.Name);
                }
            }
            Utility.LogToFile($"Loaded {registered} scripts");

            if (failed.Count != 0)
            {
                MyAPIGateway.Utilities.ShowMessage("AnimationEngine", "One or more scripts failed to compile, please check your SE logs for more information (paste the link in clipboard into explorer)");
                MyClipboardHelper.SetClipboard(Utility.GetLogPath());
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
            string id = block.BlockDefinition.Id.SubtypeId.String.ToLower();
            if (block.FatBlock != null && registered.ContainsKey(id))
            {
                if (block.FatBlock is IMyCubeBlock)
                {
                    BlockScript script = new BlockScript(registered[id]);
                    script.Init(block.FatBlock);
                    loaded.Add(script);
                    Utility.LogToFile($"Attached script to {id} ({block.FatBlock.EntityId})");
                }
                else
                {
                    Utility.LogToFile($"Cannot attach script to {id} (Cannot attach to armor blocks)");
                }
            }
        }

        public static void RemoveScript(BlockScript script)
        {
            loaded.Remove(script);
        }

    }
}
