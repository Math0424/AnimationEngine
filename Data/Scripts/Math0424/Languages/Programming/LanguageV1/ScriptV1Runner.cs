using AnimationEngine.Core;
using AnimationEngine.Language;
using AnimationEngine.LogicV1;
using AnimationEngine.Utility;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace AnimationEngine.LanguageV1
{
    internal class ScriptV1Runner : ScriptRunner
    {
        private CoreScript core;
        private List<Delayed> delay;
        private Dictionary<string, ScriptLib> libraries;

        private bool Built;
        private MyCubeBlockDefinition Definition;

        public List<ObjectDef> objectDefs;
        private List<V1ScriptAction> scriptActions;
        private Dictionary<string, Caller[]> callingArray;

        public ScriptV1Runner(List<ObjectDef> objectDefs, List<V1ScriptAction> scriptActions, Dictionary<string, Caller[]> callingArray)
        {
            this.objectDefs = objectDefs;
            this.scriptActions = scriptActions;
            this.callingArray = callingArray;
        }

        public ScriptRunner Clone()
        {
            return new ScriptV1Runner(objectDefs, scriptActions, callingArray);
        }

        public void Init(CoreScript script)
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
                    ((Initializable)x).Init(script.Entity);

            Call("blockaction", "create");
            Definition = ((MyCubeBlockDefinition)((IMyCubeBlock)script.Entity).SlimBlock.BlockDefinition);
            Built = ((IMyCubeBlock)core.Entity).SlimBlock.BuildLevelRatio > Definition.CriticalIntegrityRatio;
        }

        public void Tick(int time)
        {
            for (int i = 0; i < delay.Count; i++)
            {
                Delayed delayed = delay[i];
                if (delayed.Delay <= 0)
                {
                    if (libraries.ContainsKey(delayed.Object))
                        libraries[delayed.Object.ToLower()].Execute(delayed.Name, delayed.Args);
                    delayed.Executed = true;
                }
                delayed.Delay -= time;
                delay[i] = delayed;
            }
            delay.RemoveAll(x => x.Executed);

            foreach (var x in libraries.Values)
                if (!(x is SubpartCore))
                    x.Tick(time);

            var check = ((IMyCubeBlock)core.Entity).SlimBlock.BuildLevelRatio > Definition.CriticalIntegrityRatio;
            if (check != Built)
            {
                Built = check;
                if (Built)
                    Call("blockaction", "built");
            }
        }

        private void CallFunction(string name)
        {
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
                    Execute($"{x.ID}_{function}");
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
                            case "pressedon": btnComp.ButtonOn += () => CallFunction($"{act.ID}_{x.Name.Value}"); break;
                            case "pressedoff": btnComp.ButtonOff += () => CallFunction($"{act.ID}_{x.Name.Value}"); break;
                            case "pressed": btnComp.OnInteract += () => CallFunction($"{act.ID}_{x.Name.Value}"); break;
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
                    libraries[def.Name.ToLower()] = new Emissive(def.Values[0].ToString());
                    break;
                case "emitter":
                    libraries[def.Name.ToLower()] = new Emitter(def.Values[0].ToString());
                    break;
                case "light":
                    libraries[def.Name.ToLower()] = new Light(def.Values[0].ToString(), (float)def.Values[1]);
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

        }
    }
}
