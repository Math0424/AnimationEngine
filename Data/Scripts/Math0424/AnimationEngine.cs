using AnimationEngine.Core;
using AnimationEngine.Language;
using AnimationEngine.LanguageXML;
using AnimationEngine.Utility;
using CoreSystems.Api;
using Math0424.Networking;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace AnimationEngine
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    internal class AnimationEngine : MySessionComponentBase
    {

        private static readonly string MainPath = "data/animation/";
        private static readonly string MainInfo = "main.info";
        private static readonly string MainScript = "main.bsl";

        private static Dictionary<string, MyTuple<Subpart[], ScriptRunner>> registeredScripts = new Dictionary<string, MyTuple<Subpart[], ScriptRunner>>();

        private static Dictionary<long, List<CoreScript>> loaded = new Dictionary<long, List<CoreScript>>();
        private static List<long> HasScript = new List<long>();
        
        private static List<string> failed = new List<string>();
        private static List<MyTuple<CoreScript, IMyEntity>> delayed = new List<MyTuple<CoreScript, IMyEntity>>();

        public static WcApi WCApi = new WcApi();
        public static Action WCReady;

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
            WCApi.Unload();
            Utils.CloseLog();
        }

        private void PrintSubparts(MyEntity ent, int index)
        {
            foreach (var x in ent.Subparts.Keys)
            {
                Utils.LogToFile($"{string.Concat(Enumerable.Repeat("|  ", index))}{x}");
                PrintSubparts(ent.Subparts[x], index + 1);
            }
        }

        private bool HasSubpart(MyEntity ent, string name)
        {
            if (ent == null || ent.Subparts.Count == 0)
                return false;

            foreach(var x in ent.Subparts)
            {
                if (x.Key.Equals(name))
                    return true;
                if (HasSubpart(x.Value, name))
                    return true;
            }

            return false;
        }

        public override void BeforeStart()
        {
            EasyNetworker.Init(45876);
            EasyNetworker.RegisterPacket(typeof(ButtonComp.ButtonPacket), ButtonComp.ButtonIn);

            WCApi.Load(WCReady);

            MyEntity testEnt = new MyEntity();
            foreach (var x in registeredScripts.Keys)
            {
                bool contains = false;
                foreach(var y in MyDefinitionManager.Static.GetAllDefinitions())
                {
                    if (y.Id.SubtypeId.ToString().Equals(x))
                    {
                        contains = true;

                        if (y is MyCubeBlockDefinition)
                        {
                            testEnt.Init(new StringBuilder("ModelTester"), ((MyCubeBlockDefinition)y).Model, null, 1);
                            foreach (var z in registeredScripts[x].Item1)
                            {
                                if (!HasSubpart(testEnt, z.Name))
                                {
                                    Utils.LogToFile($"Error at {x}: Cannot find subpart '{z.Name}' - '{z.Parent ?? "main"}'");
                                    if (!failed.Contains(x))
                                    {
                                        failed.Add(x);
                                    }
                                }
                            }
                            if (failed.Contains(x))
                            {
                                Utils.LogToFile($"Valid subpart ID's of {x}");
                                PrintSubparts(testEnt, 1);
                            }
                        }
                    }
                }
                if (!contains)
                {
                    Utils.LogToFile($"Error: Cannot find block with name of '{x}'");
                    failed.Add(x);
                } 
            }
            testEnt.Close();

            if (failed.Count != 0)
            {
                string failedEnts = "";
                foreach (var x in failed)
                {
                    failedEnts += x + ", ";
                }
                failedEnts = failedEnts.Substring(0, failedEnts.Length - 2);
                MyAPIGateway.Utilities.ShowMessage("AnimationEngine", $"These blocks have errors\n{failedEnts}\n check logs for more info");
            }
        }

        int currentTick = 0;
        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Utilities == null || MyAPIGateway.Session == null)
                return;

            while(delayed.Count > 0)
            {
                var x = delayed[0];
                try
                {
                    if (WCApi.IsReady && x.Item1.HasComponent<WeaponcoreScriptRunner>())
                        x.Item1.GetFirstComponent<WeaponcoreScriptRunner>().ListenToEvents((MyEntity)x.Item2);
                    x.Item1.Init(x.Item2);
                }
                catch (Exception ex)
                {
                    Utils.LogToFile($"Entity INIT: Error while attaching to {x.Item2.DisplayName}");
                    Utils.LogToFile(ex.Message + ex.StackTrace);
                    Utils.LogToFile(ex.InnerException);
                }
                delayed.RemoveAt(0);
            }
            currentTick++;

            //TODO: create a better data scructure
            foreach (var x in loaded)
            {
                MyEntity ent = MyEntities.GetEntityById(x.Key);
                if (ent == null || !ent.InScene)
                    continue;


                if (MyAPIGateway.Utilities.IsDedicated)
                {
                    if (currentTick % 3 == 0)
                    {
                        foreach(var y in x.Value)
                            TickObject(y, 3);
                    }
                    continue;
                }

                if (MyAPIGateway.Session.Camera != null && MyAPIGateway.Session.Camera.Position != null)
                {
                    double dist = ent.GetDistanceBetweenCameraAndBoundingSphere();
                    if (dist > 5000)
                    {
                        if (currentTick % 45 == 0) 
                            foreach (var y in x.Value)
                                TickObject(y, 45);
                    }
                    else if (dist > 2500)
                    {
                        if (currentTick % 20 == 0)
                            foreach (var y in x.Value)
                                TickObject(y, 20);
                    }
                    else if (dist > 1000)
                    {
                        if (currentTick % 5 == 0)
                            foreach (var y in x.Value)
                                TickObject(y, 5);
                    }
                    else if (dist > 200)
                    {
                        if (currentTick % 2 == 0)
                            foreach (var y in x.Value)
                                TickObject(y, 2);
                    }
                    else
                    {
                        foreach (var y in x.Value)
                            TickObject(y, 1);
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
                Utils.LogToFile($"Error while ticking a block from mod '{script.GetModName()}'");
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
                            else if (s.ToLower().StartsWith("xmlanimation "))
                            {
                                new XMLScriptGenerator(mod, MainPath + s.ToLower().Substring(13).Trim() + ".xml");
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

            MyEntities.OnEntityAdd += OnEntityAdded;
        }

        public void OnEntityAdded(IMyEntity ent)
        {
            if (ent is IMyCubeGrid)
            {
                foreach (var x in ((MyCubeGrid)ent).GetFatBlocks())
                    OnBlockAdded(x.SlimBlock);

                ((IMyCubeGrid)ent).OnBlockAdded -= OnBlockAdded;
                ((IMyCubeGrid)ent).OnBlockAdded += OnBlockAdded;

                ((IMyCubeGrid)ent).OnGridSplit += OnGridSplit;
                ((IMyCubeGrid)ent).OnGridMerge += OnGridMeger;
            }

        }

        //transfer fatblocks from one to another
        //this is really slow, figure a faster way
        private void OnGridMeger(IMyCubeGrid newGrid, IMyCubeGrid originalGrid)
        {
            if (loaded.ContainsKey(originalGrid.EntityId))
            {
                long newId = newGrid.EntityId;
                long originalId = originalGrid.EntityId;
                MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                {
                    if (!loaded.ContainsKey(originalId))
                        return;
                    if (!loaded.ContainsKey(newId))
                        loaded[newId] = new List<CoreScript>();
                    loaded[newId].AddList(loaded[originalId]);
                    loaded.Remove(originalId);
                });
            }
        }

        //transfer fatblocks from one to another
        //this is really slow, figure a faster way
        private void OnGridSplit(IMyCubeGrid originalGrid, IMyCubeGrid newGrid)
        {
            if (newGrid == null || originalGrid == null)
                return;
            
            try
            {
                if (loaded.ContainsKey(originalGrid.EntityId))
                {
                    List<CoreScript> toRemove = new List<CoreScript>();
                    foreach (var x in loaded[originalGrid.EntityId])
                    {
                        foreach (var y in ((MyCubeGrid)newGrid).GetFatBlocks())
                        {
                            if (x.Entity.EntityId == y.EntityId)
                                toRemove.Add(x);
                        }
                    }

                    long newId = newGrid.EntityId;
                    long originalId = originalGrid.EntityId;
                    MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                    {
                        loaded[originalId].RemoveAll((e) => toRemove.Contains(e));
                        if (!loaded.ContainsKey(newId))
                            loaded[newId] = new List<CoreScript>();
                        loaded[newId].AddList(toRemove);
                    });
                }
            }
            catch (Exception ex)
            {
                Utils.LogToFile("Crash while splitting grid");
                Utils.LogToFile(ex.StackTrace);
                Utils.LogToFile(ex.Message);
                Utils.LogToFile(ex.Data);
                Utils.LogToFile(ex.TargetSite);
                Utils.LogToFile(ex.InnerException);
                Utils.LogToFile(ex.Source);
            }
            
        }

        private void OnBlockAdded(IMySlimBlock block)
        {
            string id = block.BlockDefinition.Id.SubtypeId.String;
            if (block.FatBlock != null && registeredScripts.ContainsKey(id) && !HasScript.Contains(block.FatBlock.EntityId))
            {
                var x = registeredScripts[id];
                CoreScript script = new CoreScript(x.Item2.GetModName(), x.Item1);
                script.AddComponent(x.Item2.Clone());
                delayed.Add(new MyTuple<CoreScript, IMyEntity>(script, block.FatBlock));
            }
        }

        public static void RemoveScript(CoreScript script)
        {
            if (loaded.ContainsKey(script.ParentId))
                loaded[script.ParentId].Remove(script);
            HasScript.Remove(script.EntityId);
        }

        public static void AddScript(CoreScript script)
        {
            if (HasScript.Contains(script.Entity.EntityId))
                return;

            long pid = script.Entity.Parent.EntityId;
            if (!loaded.ContainsKey(pid))
                loaded[pid] = new List<CoreScript>();
            loaded[pid].Add(script);

            HasScript.Add(script.Entity.EntityId);
        }


    }
}
