﻿using AnimationEngine.Core;
using AnimationEngine.Language;
using AnimationEngine.LogicV1;
using AnimationEngine.Utility;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using static VRage.Game.MyObjectBuilder_Checkpoint;

namespace AnimationEngine.LanguageV1
{
    internal class ScriptV1Runner : ScriptRunner
    {
        ModItem mod;
        private CoreScript core;
        private List<Delayed> delay;
        private Dictionary<string, ScriptLib> libraries;

        private List<ObjectDef> objectDefs;
        private List<V1ScriptAction> scriptActions;
        private Dictionary<string, Caller[]> callingArray;

        public ModItem GetMod()
        {
            return mod;
        }

        public ScriptV1Runner(ModItem modName, List<ObjectDef> objectDefs, List<V1ScriptAction> scriptActions, Dictionary<string, Caller[]> callingArray)
        {
            this.mod = modName;
            this.objectDefs = objectDefs;
            this.scriptActions = scriptActions;
            this.callingArray = callingArray;
        }

        public ScriptRunner Clone()
        {
            return new ScriptV1Runner(mod, objectDefs, scriptActions, callingArray);
        }

        public void Stop()
        {
            delay?.Clear();
        }

        public void InitBuilt(CoreScript script)
        {
            libraries = new Dictionary<string, ScriptLib>();
            delay = new List<Delayed>();
            core = script;

            foreach (var obj in objectDefs)
                InitObject(obj);
            foreach (var obj in scriptActions)
                InitAction(obj);

            libraries.Add("block", new BlockCore(script));

            
            foreach (var x in libraries.Values)
                if (x is Initializable)
                    try
                    {
                        ((Initializable)x).Init(script.Entity);
                    }
                    catch (Exception ex)
                    {
                        Utils.LogToFile(ex);
                    }

            Call("blockaction", "create");
        }

        public void Tick(int time)
        {
            for (int i = 0; i < delay.Count; i++)
            {
                Delayed delayed = delay[i];
                if (delayed.Delay <= 0)
                {
                    if (libraries.ContainsKey(delayed.Object))
                        libraries[delayed.Object].Execute(delayed.Name, delayed.Args);
                    delayed.Executed = true;
                }
                delayed.Delay -= time;
                delay[i] = delayed;
            }
            delay.RemoveAll(x => x.Executed);

            foreach (var x in libraries.Values)
                if (!(x is SubpartCore))
                    x.Tick(time);

            if ((core.Flags & CoreScript.BlockFlags.Built) != 0)
            {
                core.Flags |= CoreScript.BlockFlags.Built;
                Call("blockaction", "built");
            }
        }

        private void CallFunction(string name)
        {
            //Utils.LogToFile($"{Definition?.DisplayNameText ?? "null"} is callign {name} contains = {callingArray.ContainsKey(name)}");
            if (callingArray.ContainsKey(name))
            {
                Caller[] callz = callingArray[name];
                foreach (var x in callz)
                {
                    if (x.FuncCall)
                    {
                        CallFunction(x.Object);
                    }
                    else
                    {
                        foreach (var z in x.Args)
                        {
                            delay.Add(new Delayed()
                            {
                                Object = x.Object,
                                Args = z.Value,
                                Name = z.Name,
                                Delay = z.Delay,
                                Executed = false,
                            });
                        }
                    }
                }
            }
        }

        public void Call(string action, string function)
        {
            foreach (var x in scriptActions)
            {
                if (x.Name.Value.Equals(action))
                {
                    CallFunction($"{x.ID}_{function}");
                    break;
                }
            }
        }

        private void InitAction(V1ScriptAction act)
        {
            switch (act.Name.Value.ToString())
            {
                case "buttonaction":
                    var subpart = act.Paramaters[0].Value.ToString();
                    ButtonComp btnComp = core.Subparts[subpart].GetFirstComponent<ButtonComp>();
                    btnComp.Init(core.Subparts[subpart]);

                    foreach (var x in act.Funcs)
                    {
                        switch (x.Name.Value.ToString())
                        {
                            case "pressedon": btnComp.Pressed += (b) => { if (b.AsBool()) { CallFunction($"{act.ID}_{x.Name.Value}"); } }; break; 
                            case "pressedoff": btnComp.Pressed += (b) => { if (!b.AsBool()) { CallFunction($"{act.ID}_{x.Name.Value}"); } }; break;
                            case "pressed": btnComp.Pressed += (b) => CallFunction($"{act.ID}_{x.Name.Value}"); break;
                        }
                    }
                    break;
                case "productionaction":
                    var proComp = new ProductionTickComp(-1);
                    core.AddComponent(proComp);

                    foreach (var x in act.Funcs)
                    {
                        switch (x.Name.Value.ToString())
                        {
                            case "startproducing": proComp.StartedProducing += () => CallFunction($"{act.ID}_{x.Name.Value}"); break;
                            case "stopproducing": proComp.StoppedProducing += () => CallFunction($"{act.ID}_{x.Name.Value}"); break;
                            case "producingloop":
                                proComp.LoopTime = (int)x.Paramaters[0].Value;
                                proComp.Ticked += () => CallFunction($"{act.ID}_{x.Name.Value}");
                                break;
                        }
                    }
                    break;
                case "distanceaction":
                    var distComp = new DistanceComp((float)act.Paramaters[0].Value);
                    core.AddComponent(distComp);

                    foreach (var x in act.Funcs)
                    {
                        switch (x.Name.Value.ToString())
                        {
                            case "arrive": distComp.InRange += () => CallFunction($"{act.ID}_{x.Name.Value}"); break;
                            case "leave": distComp.OutOfRange += () => CallFunction($"{act.ID}_{x.Name.Value}"); break;
                        }
                    }
                    break;
                case "blockaction":
                    var workComp = new WorkingTickComp(-1);
                    core.AddComponent(workComp);

                    foreach (var x in act.Funcs)
                    {
                        switch (x.Name.Value.ToString())
                        {
                            case "working": workComp.OnIsWorking += () => CallFunction($"{act.ID}_{x.Name.Value}"); break;
                            case "notworking": workComp.OnNotWorking += () => CallFunction($"{act.ID}_{x.Name.Value}"); break;
                            case "workingloop":
                                workComp.LoopTime = (int)x.Paramaters[0].Value;
                                workComp.Ticked += () => CallFunction($"{act.ID}_{x.Name.Value}");
                                break;
                        }
                    }
                    break;
                case "dooraction":
                    ((IMyDoor)core.Entity).DoorStateChanged += (b) =>
                    {
                        if (b) { CallFunction($"{act.ID}_open"); } else { CallFunction($"{act.ID}_close"); }
                    };
                    break;
                case "cockpitaction":
                    CockpitComp cockComp = new CockpitComp();
                    core.AddComponent(cockComp);

                    foreach (var x in act.Funcs)
                    {
                        switch (x.Name.Value.ToString())
                        {
                            case "enter": cockComp.EnteredSeat += () => CallFunction($"{act.ID}_{x.Name.Value}"); break;
                            case "exit": cockComp.ExitedSeat += () => CallFunction($"{act.ID}_{x.Name.Value}"); break;
                        }
                    }
                    break;
                case "landinggearaction":
                    ((IMyLandingGear)core.Entity).LockModeChanged += (e, f) =>
                    {
                        switch (f)
                        {
                            case SpaceEngineers.Game.ModAPI.Ingame.LandingGearMode.Locked:
                                CallFunction($"{act.ID}_lock");
                                break;
                            case SpaceEngineers.Game.ModAPI.Ingame.LandingGearMode.Unlocked:
                                CallFunction($"{act.ID}_unlock");
                                break;
                            case SpaceEngineers.Game.ModAPI.Ingame.LandingGearMode.ReadyToLock:
                                CallFunction($"{act.ID}_readylock");
                                break;
                        }
                    };
                    break;
            }
        }

        private void InitObject(ObjectDef def)
        {
            switch (def.Type)
            {
                case "subpart":
                    libraries[def.Name.ToLower()] = core.Subparts[def.Name];
                    break;
                case "button":
                    core.Subparts[def.Name].AddComponent(new ButtonComp(def.Values[0].ToString()));
                    libraries[def.Name.ToLower()] = core.Subparts[def.Name];
                    break;
                case "emissive":
                    libraries[def.Name.ToLower()] = new Emissive(def.Values[0].ToString(), null);
                    break;
                case "emitter":
                    libraries[def.Name.ToLower()] = new Emitter(def.Values[0].ToString(), null);
                    break;
                case "light":
                    libraries[def.Name.ToLower()] = new Light(def.Values[0].ToString(), (float)def.Values[1], null, true, 1, 2);
                    break;
            }
        }

        public void Execute(string function, params SVariable[] args)
        {
            if (libraries.ContainsKey(function))
                CallFunction(function);
        }

        public void Close()
        {
            foreach(var x in libraries)
                x.Value.Close();
        }

    }
}
