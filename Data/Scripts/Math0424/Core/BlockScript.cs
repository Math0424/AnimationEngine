using AnimationEngine.LanguageV1;
using AnimationEngine.LogicV1;
using AnimationEngine.Util;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace AnimationEngine.Core
{
    /*
     * Houses the subparts and main scripts
     */
    internal class BlockScript
    {

        public IMyCubeBlock Block { get; private set; }
        private ScriptConstants constants;

        public BlockScript(ScriptConstants constants)
        {
            this.constants = constants;

            foreach (var x in constants.ObjectDefs)
            {
                RegisterObject(x);
            }
        }

        public T GetComponent<T>() where T : BlockComponent
        {
            foreach(var x in components)
            {
                if (x.GetType() == typeof(T))
                {
                    return (T)x;
                }
            }
            return null;
        }

        //on initaliazation
        private Dictionary<string, Actionable> objects = new Dictionary<string, Actionable>();
        private List<BlockComponent> components = new List<BlockComponent>();
        private bool Built;
        private MyCubeBlockDefinition Definition;

        //modified during gameplay
        private List<Delayed> delay = new List<Delayed>();

        public void Init(IMyCubeBlock block)
        {
            Block = block;
            block.OnClose += Close;

            foreach (var x in constants.ScriptActions)
            {
                RegisterAction(x);
            }

            components.ForEach(x => x.Initalize(Block));
            objects.Add("block", new BlockCore(this));

            foreach (var x in objects.Values)
            {
                if (x is Initializable)
                {
                    ((Initializable)x).Initalize(Block);
                }
            }

            Call("blockaction", "create");
            Definition = ((MyCubeBlockDefinition)block.SlimBlock.BlockDefinition);
            Built = block.SlimBlock.BuildLevelRatio < Definition.CriticalIntegrityRatio;
        }

        private void Close(IMyEntity ent)
        {
            AnimationEngine.RemoveScript(this);
        }

        private void Call(string action, string function)
        {
            foreach (var x in constants.ScriptActions)
            {
                if (x.Name.Value.Equals(action))
                {
                    Execute($"{x.ID}_{function}");
                    break;
                }
            }
        }

        public void Execute(string call)
        {
            if (constants.Calls.ContainsKey(call.ToLower()))
            {
                Caller[] callz = constants.Calls[call.ToLower()];
                foreach (var x in callz)
                {
                    if (x.FuncCall)
                    {
                        Execute(x.Object);
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

        public void Tick(int tick)
        {

            for (int i = 0; i < delay.Count; i++)
            {
                Delayed delayed = delay[i];
                if (delayed.Delay <= 0)
                {
                    objects[delayed.Object].Call(delayed.Name, delayed.Args);
                    delayed.Executed = true;
                }
                delayed.Delay -= tick;
                delay[i] = delayed;
            }
            delay.RemoveAll(x => x.Executed);

            try
            {
                var check = Block.SlimBlock.BuildLevelRatio < Definition.CriticalIntegrityRatio;
                if (check != Built)
                {
                    Built = check;
                    if (Built)
                    {
                        Call("blockaction", "built");
                    }
                }

                foreach (var x in components)
                    x.Tick(tick);
                foreach (var z in objects.Values)
                    z.Tick(tick);
            }
            catch (Exception ex)
            {
                Utils.LogToFile("Error with blockscript " + Definition.Id.SubtypeId);
                Utils.LogToFile(ex.StackTrace);
                Utils.LogToFile(ex);
            }
        }


        private void RegisterAction(ScriptAction act)
        {
            string subpart;
            switch (act.Name.Value.ToString())
            {
                case "buttonaction":
                    subpart = act.Paramaters[0].Value.ToString().ToLower();
                    var btnComp = ((SubpartCore)objects[subpart]).GetComponent<ButtonComp>();

                    foreach (var x in act.Funcs)
                    {
                        switch (x.Name.Value.ToString())
                        {
                            case "pressedon": btnComp.ButtonOn += () => Execute($"{act.ID}_{x.Name.Value}"); break;
                            case "pressedoff": btnComp.ButtonOff += () => Execute($"{act.ID}_{x.Name.Value}"); break;
                            case "pressed": btnComp.OnInteract += () => Execute($"{act.ID}_{x.Name.Value}"); break;
                        }
                    }
                    break;
                case "productionaction":
                    var proComp = new ProductionTickComp(-1);
                    components.Add(proComp);

                    foreach (var x in act.Funcs)
                    {
                        switch (x.Name.Value.ToString())
                        {
                            case "startproducing": proComp.StartedProducing += () => Execute($"{act.ID}_{x.Name.Value}"); break;
                            case "stopproducing": proComp.StoppedProducing += () => Execute($"{act.ID}_{x.Name.Value}"); break;
                            case "producingloop":
                                proComp.LoopTime = (int)x.Paramaters[0].Value;
                                proComp.Ticked += () => Execute($"{act.ID}_{x.Name.Value}");
                                break;
                        }
                    }
                    break;
                case "distanceaction":
                    var distComp = new DistanceComp((float)act.Paramaters[0].Value);
                    components.Add(distComp);

                    foreach (var x in act.Funcs)
                    {
                        switch (x.Name.Value.ToString())
                        {
                            case "arrive": distComp.InRange += () => Execute($"{act.ID}_{x.Name.Value}"); break;
                            case "leave": distComp.OutOfRange += () => Execute($"{act.ID}_{x.Name.Value}"); break;
                        }
                    }
                    break;
                case "blockaction":
                    var workComp = new WorkingTickComp(-1);
                    components.Add(workComp);

                    foreach (var x in act.Funcs)
                    {
                        switch (x.Name.Value.ToString())
                        {
                            case "working": workComp.OnIsWorking += () => Execute($"{act.ID}_{x.Name.Value}"); break;
                            case "notworking": workComp.OnNotWorking += () => Execute($"{act.ID}_{x.Name.Value}"); break;
                            case "workingloop":
                                workComp.LoopTime = (int)x.Paramaters[0].Value;
                                workComp.Ticked += () => Execute($"{act.ID}_{x.Name.Value}");
                                break;
                        }
                    }
                    break;
                case "dooraction":
                    ((IMyDoor)Block).DoorStateChanged += (b) => {
                        if (b) { Execute($"{act.ID}_open"); } else { Execute($"{act.ID}_close"); }
                    };
                    break;
                case "cockpitaction":
                    CockpitComp cockComp = new CockpitComp();
                    components.Add(cockComp);

                    foreach (var x in act.Funcs)
                    {
                        switch (x.Name.Value.ToString())
                        {
                            case "enter": cockComp.EnteredSeat += () => Execute($"{act.ID}_{x.Name.Value}"); break;
                            case "exit": cockComp.ExitedSeat += () => Execute($"{act.ID}_{x.Name.Value}"); break;
                        }
                    }
                    break;
                case "landinggearaction":
                    ((IMyLandingGear)Block).LockModeChanged += (e, f) => {
                        switch (f)
                        {
                            case SpaceEngineers.Game.ModAPI.Ingame.LandingGearMode.Locked:
                                Execute($"{act.ID}_lock");
                                break;
                            case SpaceEngineers.Game.ModAPI.Ingame.LandingGearMode.Unlocked:
                                Execute($"{act.ID}_unlock");
                                break;
                            case SpaceEngineers.Game.ModAPI.Ingame.LandingGearMode.ReadyToLock:
                                Execute($"{act.ID}_readylock");
                                break;
                        }
                    };
                    break;
            }
        }

        private void RegisterObject(ScriptConstants.ObjectDef def)
        {
            SubpartCore subpart;
            switch (def.Type)
            {
                case TokenType.SUBPART:
                    objects.Add(def.Name.ToLower(), new SubpartCore(def.Name, null));
                    break;
                case TokenType.BUTTON:
                    subpart = new SubpartCore(def.Name, null);
                    subpart.AddComponent(new ButtonComp(def.Values[0].ToString()));
                    objects.Add(def.Name.ToLower(), subpart);
                    break;
                case TokenType.EMISSIVE:
                    objects.Add(def.Name.ToLower(), new Emissive(def.Values[0].ToString(), null));
                    break;
                case TokenType.EMITTER:
                    objects.Add(def.Name.ToLower(), new Emitter(def.Values[0].ToString(), null));
                    break;
                case TokenType.LIGHT:
                    objects.Add(def.Name.ToLower(), new Light(def.Values[0].ToString(), (float)def.Values[1]));
                    break;
            }
        }

    }
}
