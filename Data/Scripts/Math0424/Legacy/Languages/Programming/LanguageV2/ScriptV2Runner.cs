using AnimationEngine.Core;
using AnimationEngine.Language.Libs;
using AnimationEngine.Utility;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using static VRage.Game.MyObjectBuilder_Checkpoint;

/// <summary>
/// This handles the code, its pretty much a emulator for a basic computer
/// </summary>
namespace AnimationEngine.Language
{
    internal enum ProgramFunc
    {
        bXor,

        LdI, // push immediate [0] to stack 
        AddI, // stack[0] = stack[1] + Immediate
        SubI, // stack[0] = stack[1] - Immediate

        Add, // stack[0] = stack[1] + stack[2]
        Sub, // above
        Mul, // above
        Div, // above
        Mod, // above

        B, // non function branch
        Cmp, // set flags about top element
        BZ,  // branch zero
        BNZ, // branch non zero
        BNE,  // branch negative
        BNEZ, //branch negative or zero

        Pop, // pop without jump
        PopJ, // remove object at top of register with jump

        LdrI, // pushes immediate [0] to top of register
        Cpy, // copy [0] to [1]

        LdG, // load a global
        StG, // save a global

        RDly, // reset delay
        Mth, // call method from immediate [0]

        Jmp, //jump to method at [0]

        End, // terminate program (or jump to previous _callStack)
    }

    internal class ScriptV2Runner : ScriptRunner
    {
        CoreScript core;
        ModItem modItem;

        #region Script
        private RAStack<SVariable> _stack;
        private Stack<int> _callStack;
        private bool nFlag, zFlag, nzFlag;

        private readonly Line[] _program;
        private readonly SVariable[] _immediates;
        private readonly Dictionary<string, int> _methodLookup;

        private SVariable[] _globals;
        private List<ScriptLib> _libraries;
        #endregion
        
        private int _delay;
        private List<Delay> _delays;
        private List<Entity> _ents;
        private List<ScriptAction> _actions;
        private List<ScriptAction> _terminals;
        int _stackStart;

        public void Execute(string function, params SVariable[] args)
        {
            if (function != null && _methodLookup.ContainsKey(function))
            {
                _stackStart = _stack.Count + 1;
                foreach (var x in args)
                    _stack.Push(x);
                try
                {
                    Execute(_methodLookup[function]);
                }
                catch (Exception ex)
                {
                    Utils.LogToFile(ex);
                }
            } 
        }

        public void Stop()
        {
            _delays?.Clear();
        }

        public ScriptRunner Clone()
        {
            return new ScriptV2Runner(this);
        }

        public void AddLibrary(ScriptLib library)
        {
            _libraries.Add(library);
        }

        public void InitBuilt(CoreScript script)
        {
            core = script;

            if ((core.Flags & CoreScript.BlockFlags.ActionsInited) == 0) {
                core.Flags |= CoreScript.BlockFlags.ActionsInited;

                foreach (var x in _ents)
                    InitEnt(x);
                foreach (var x in _actions)
                    InitAction(x);
            }

            foreach (var x in _libraries)
            {
                IMyEntity ent = script.Entity;
                if (x is Parentable && ((Parentable)x).GetParent() != null)
                    ent = script.Subparts[((Parentable)x).GetParent()].Subpart;
                if (x is Initializable)
                    ((Initializable)x).Init(ent);
            }
        }

        private void InitEnt(Entity ent)
        {
            switch (ent.Type.Value.ToString().ToLower())
            {
                case "api":
                    _libraries.Add(new ScriptAPI(this)); break;
                case "math":
                    _libraries.Add(new ScriptMath()); break;
                case "block":
                    _libraries.Add(new BlockCore(core)); break;
                case "grid":
                    _libraries.Add(new GridCore(core)); break;
                case "xmlscript":
                    var x = new XMLScriptCore(core, core.Mod, ent.Args[0].Value.ToString());
                    _libraries.Add(x);
                    core.FlattenSubparts(x.GetSubpartNames());
                    break;
                case "button":
                    var btnSubpart = core.Subparts[ent.Name.Value.ToString().ToLower()];
                    if (!btnSubpart.HasComponent<ButtonComp>())
                    {
                        var btnComp = new ButtonComp(ent.Args[1].Value.ToString());
                        btnSubpart.AddComponent(btnComp);
                        btnComp.Init(btnSubpart);
                    }
                    _libraries.Add(btnSubpart);
                    break;
                case "subpart":
                    _libraries.Add(core.Subparts[ent.Name.Value.ToString().ToLower()]);
                    break;
                case "emissive":
                    _libraries.Add(new Emissive(ent.Args[0].Value.ToString(), ent.Parent.Value?.ToString().ToLower()));
                    break;
                case "emitter":
                    _libraries.Add(new Emitter(ent.Args[0].Value.ToString(), ent.Parent.Value?.ToString().ToLower()));
                    break;
                case "light":
                    _libraries.Add(new Light(ent.Args[0].Value.ToString(), (float)ent.Args[1].Value, ent.Parent.Value?.ToString().ToLower()));
                    break;
            }
        }

        private void InitAction(ScriptAction action)
        {
            switch (action.TokenName)
            {
                case "button":
                    var part = core.Subparts[action.Paramaters[0].Value.ToString().ToLower()];
                    if (part.HasComponent<ButtonComp>())
                        foreach (var x in action.Funcs)
                            switch (x.TokenName)
                            {
                                case "pressed":
                                    part.GetFirstComponent<ButtonComp>().Pressed += (e) => Execute($"act_{action.ID}_pressed", e);
                                    break;
                                case "hovering":
                                    part.GetFirstComponent<ButtonComp>().Hovering += (e) => Execute($"act_{action.ID}_hovering", e);
                                    break;
                            }
                    break;
                case "block":
                    if (!core.HasComponent<BlockStateComp>())
                        core.AddComponent(new BlockStateComp());
                    foreach (var x in action.Funcs)
                        switch (x.TokenName)
                        {
                            case "create":
                                core.GetFirstComponent<BlockStateComp>().Create += () => Execute($"act_{action.ID}_create");
                                break;
                            case "built":
                                core.GetFirstComponent<BlockStateComp>().Built += () => Execute($"act_{action.ID}_built");
                                break;
                            case "damaged":
                                core.GetFirstComponent<BlockStateComp>().Damaged += () => Execute($"act_{action.ID}_damaged");
                                break;
                            case "working":
                                core.GetFirstComponent<BlockStateComp>().Working += () => Execute($"act_{action.ID}_working");
                                break;
                            case "notworking":
                                core.GetFirstComponent<BlockStateComp>().NotWorking += () => Execute($"act_{action.ID}_notworking");
                                break;
                        }
                    break;
                case "inventory":
                    if (!core.HasComponent<InventoryFillComp>())
                        core.AddComponent(new InventoryFillComp());
                    foreach (var x in action.Funcs)
                        switch (x.TokenName)
                        {
                            case "changed":
                                core.GetFirstComponent<InventoryFillComp>().Changed += (e) => Execute($"act_{action.ID}_changed", new SVariableFloat(e));
                                break;
                        }
                    break;
                case "power":
                    if (!core.HasComponent<PowerTickComp>())
                        core.AddComponent(new PowerTickComp());
                    foreach (var x in action.Funcs)
                        switch (x.TokenName)
                        {
                            case "consumed":
                                core.GetFirstComponent<PowerTickComp>().Consumed += (e) => Execute($"act_{action.ID}_consumed", new SVariableFloat(e));
                                break;
                            case "produced":
                                core.GetFirstComponent<PowerTickComp>().Produced += (e) => Execute($"act_{action.ID}_produced", new SVariableFloat(e));
                                break;
                        }
                    break;
                case "production":
                    if (!core.HasComponent<ProductionTickComp>())
                        core.AddComponent(new ProductionTickComp(-1));
                    foreach (var x in action.Funcs)
                        switch (x.TokenName)
                        {
                            case "startproducing":
                                core.GetFirstComponent<ProductionTickComp>().StartedProducing += () => Execute($"act_{action.ID}_startproducing");
                                break;
                            case "stopproducing":
                                core.GetFirstComponent<ProductionTickComp>().StoppedProducing += () => Execute($"act_{action.ID}_stopproducing");
                                break;
                        }
                    break;
                case "shiptool":
                    if (!core.HasComponent<ShipToolComp>())
                        core.AddComponent(new ShipToolComp());
                    foreach (var x in action.Funcs)
                        switch (x.TokenName)
                        {
                            case "activated":
                                core.GetFirstComponent<ShipToolComp>().ToolActivated += (e) => Execute($"act_{action.ID}_activated", e);
                                break;
                        }
                    break;
                case "distance":
                    if (!core.HasComponent<DistanceComp>())
                        core.AddComponent(new DistanceComp((float)action.Paramaters[0].Value));
                    foreach (var x in action.Funcs)
                        switch (x.TokenName)
                        {
                            case "changed":
                                core.GetFirstComponent<DistanceComp>().Changed += (e) => Execute($"act_{action.ID}_changed", e);
                                break;
                            case "arrive":
                                core.GetFirstComponent<DistanceComp>().InRange += () => Execute($"act_{action.ID}_arrive");
                                break;
                            case "leave":
                                core.GetFirstComponent<DistanceComp>().OutOfRange += () => Execute($"act_{action.ID}_leave");
                                break;
                        }
                    break;
                case "door":
                    ((IMyDoor)core.Entity).DoorStateChanged += (b) =>
                    {
                        if (b) { Execute($"act_{action.ID}_open"); } else { Execute($"act_{action.ID}_close"); }
                    };
                    break;
                case "cockpit":
                    if (!core.HasComponent<CockpitComp>())
                        core.AddComponent(new CockpitComp());
                    foreach (var x in action.Funcs)
                        switch (x.TokenName)
                        {
                            case "enter": core.GetFirstComponent<CockpitComp>().EnteredSeat += () => Execute($"act_{action.ID}_enter"); break;
                            case "exit": core.GetFirstComponent<CockpitComp>().ExitedSeat += () => Execute($"act_{action.ID}_exit"); break;
                        }
                    break;
                case "landinggear":
                    ((IMyLandingGear)core.Entity).LockModeChanged += (e, f) =>
                    {
                        switch (e.LockMode)
                        {
                            case SpaceEngineers.Game.ModAPI.Ingame.LandingGearMode.Locked:
                                Execute($"act_{action.ID}_lock");
                                break;
                            case SpaceEngineers.Game.ModAPI.Ingame.LandingGearMode.Unlocked:
                                Execute($"act_{action.ID}_unlock");
                                break;
                            case SpaceEngineers.Game.ModAPI.Ingame.LandingGearMode.ReadyToLock:
                                Execute($"act_{action.ID}_readylock");
                                break;
                        }
                    };
                    break;
            }
        }

        public void Tick(int time)
        {
            for (int i = 0; i < _delays.Count; i++)
            {
                Delay d = _delays[i];
                d.DelayTime -= time;
                if (d.DelayTime <= 0)
                    _libraries[d.Context].Execute(d.Method, d.Args);
                _delays[i] = d;
            }
            _delays.RemoveAll((e) => e.DelayTime <= 0);

            foreach (var x in _libraries)
                if (!(x is SubpartCore))
                    x.Tick(time);
        }

        public void Close()
        {
            foreach(var x in _libraries)
                x.Close();
        }

        public ModItem GetMod()
        {
            return modItem;
        }

        public ScriptV2Runner(ModItem moditem, List<Entity> ents, List<ScriptAction> actions, List<ScriptAction> terminals, SVariable[] globals, Line[] program, SVariable[] immediates, Dictionary<string, int> methods)
        {
            this.modItem = moditem;
            _globals = globals;
            _program = program;
            _immediates = immediates;
            _methodLookup = methods;
            _ents = ents;
            _actions = actions;
            _terminals = terminals;
        }

        public ScriptV2Runner(ScriptV2Runner copy)
        {
            modItem = copy.modItem;
            _libraries = new List<ScriptLib>();
            _stack = new RAStack<SVariable>();
            _callStack = new Stack<int>();
            SVariable[] cpy = new SVariable[copy._globals.Length];
            Array.Copy(copy._globals, cpy, cpy.Length);
            _globals = cpy;
            _delays = new List<Delay>();

            _program = copy._program;
            _immediates = copy._immediates;
            _methodLookup = copy._methodLookup;
            _ents = copy._ents;
            _actions = copy._actions;
            _terminals = copy._terminals;

#if DEBUG
            _libraries.Add(new ScriptAPI(this));
            _libraries.Add(new ScriptMath());

            Utils.LogToFile($"--Libraries--");
            for (int i = 0; i < _libraries.Count; i++)
            {
                Utils.LogToFile($"{i}: {_libraries[i]}");
            }
#endif
        }

        private void Execute(int line)
        {
            Line curr;

            int count = 0;
            while (count++ < 100000)
            {
                curr = _program[line++];

                //string output = $"Line {(line - 1):D3}: ({_stack.Count:D3}) {curr.Arg,-4} > ";
                //if (curr.Arr != null)
                //{
                //    foreach (var x in curr.Arr)
                //    {
                //        output += $"{x:D3} ";
                //    }
                //}
                //Utils.LogToFile(output);


                switch (curr.Arg)
                {
                    case ProgramFunc.bXor:
                        _stack.Set(curr.Arr[0], new SVariableInt(_stack.Peek(curr.Arr[1]).AsInt() ^ _stack.Peek(curr.Arr[2]).AsInt()));
                        break;
                    case ProgramFunc.Add:
                        _stack.Set(curr.Arr[0], _stack.Peek(curr.Arr[1]).Add(_stack.Peek(curr.Arr[2])));
                        break;
                    case ProgramFunc.Sub:
                        _stack.Set(curr.Arr[0], _stack.Peek(curr.Arr[1]).Sub(_stack.Peek(curr.Arr[2])));
                        break;
                    case ProgramFunc.Mul:
                        _stack.Set(curr.Arr[0], _stack.Peek(curr.Arr[1]).Mul(_stack.Peek(curr.Arr[2])));
                        break;
                    case ProgramFunc.Div:
                        _stack.Set(curr.Arr[0], _stack.Peek(curr.Arr[1]).Div(_stack.Peek(curr.Arr[2])));
                        break;
                    case ProgramFunc.Mod:
                        _stack.Set(curr.Arr[0], _stack.Peek(curr.Arr[1]).Mod(_stack.Peek(curr.Arr[2])));
                        break;
                    case ProgramFunc.B:
                        line = curr.Arr[0];
                        break;
                    case ProgramFunc.Cmp:
                        var x = _stack.Peek(curr.Arr[0]);
                        zFlag = x.AsFloat() == 0;
                        nzFlag = x.AsFloat() != 0;
                        nFlag = x.AsFloat() < 0;
                        break;
                    case ProgramFunc.BNZ:
                        if (nzFlag)
                        {
                            line = curr.Arr[0];
                        }
                        break;
                    case ProgramFunc.BZ:
                        if (zFlag)
                        {
                            line = curr.Arr[0];
                        }
                        break;
                    case ProgramFunc.BNE:
                        if (nFlag)
                        {
                            line = curr.Arr[0];
                        }
                        break;
                    case ProgramFunc.BNEZ:
                        if (nFlag || zFlag)
                        {
                            line = curr.Arr[0];
                        }
                        break;

                    case ProgramFunc.LdI:
                        _stack.Push(new SVariableInt(curr.Arr[0]));
                        break;
                    case ProgramFunc.SubI:
                        _stack.Set(curr.Arr[0], _stack.Peek(curr.Arr[1]).Sub(new SVariableInt(curr.Arr[2])));
                        break;
                    case ProgramFunc.AddI:
                        _stack.Set(curr.Arr[0], _stack.Peek(curr.Arr[1]).Add(new SVariableInt(curr.Arr[2])));
                        break;

                    case ProgramFunc.LdrI:
                        _stack.Push(_immediates[curr.Arr[0]]);
                        break;
                    case ProgramFunc.LdG:
                        _stack.Push(_globals[curr.Arr[0]]);
                        break;
                    case ProgramFunc.StG:
                        _globals[curr.Arr[0]] = _stack.Peek(0);
                        break;

                    case ProgramFunc.PopJ:
                        _stack.Pop(curr.Arr[0]);
                        line += curr.Arr[0] - 1;
                        break;
                    case ProgramFunc.Pop:
                        _stack.Pop(curr.Arr[0]);
                        break;

                    case ProgramFunc.Cpy:
                        _stack.Set(curr.Arr[1], _stack.Peek(curr.Arr[0]));
                        break;

                    case ProgramFunc.RDly:
                        _delay = 0;
                        break;
                    case ProgramFunc.Mth:
                        SVariable[] arr = new SVariable[curr.Arr[1]];
                        _stack.CopyTo(_stack.Count - curr.Arr[1], arr, 0, curr.Arr[1]);
                        _stack.RemoveRange(_stack.Count - curr.Arr[1], curr.Arr[1]);
                        if (_immediates[curr.Arr[0]].ToString().ToLower() == "delay")
                        {
                            _delay += arr[0].AsInt();
                            _stack.Push(new SVariableInt(0));
                        }
                        else if(_delay != 0)
                        {
                            _delays.Add(new Delay(_delay, curr.Arr[2], _immediates[curr.Arr[0]].ToString(), arr));
                            _stack.Push(new SVariableInt(0));
                        }
                        else
                        {
                            SVariable s = _libraries[curr.Arr[2]].Execute(_immediates[curr.Arr[0]].ToString(), arr);
                            _stack.Push(s ?? new SVariableInt(0));
                        }
                        break;
                    case ProgramFunc.Jmp:
                        _callStack.Push(line);
                        line = curr.Arr[0];
                        break;

                    case ProgramFunc.End:
                        if (_callStack.Count == 0)
                        {
                            //if an action is called with more than 1 input this will fasely trigger 
                            //TODO: if adding more then 1 action fix this method
                            if (_stack.Count - _stackStart != 0)
                            {
                                throw new Exception($"Critical error, stack leaking ({_stack.Count - _stackStart})! Please send your script(s) to @realmath on discord");
                            }
                            _stack.Pop();
                            return;
                        }
                        line = _callStack.Pop();
                        break;
                }
                if (_stack.Count > 1000)
                {
                    throw new Exception("Script too complex, large stack (this is not a bug)");
                }
            }
            throw new Exception("Script too complex, large loop (this is not a bug)");
        }

    }

    internal struct Line
    {
        public Line(ProgramFunc arg)
        {
            Arg = arg;
            Arr = null;
        }
        public Line(ProgramFunc arg, params int[] arr)
        {
            Arg = arg;
            Arr = arr;
        }
        public ProgramFunc Arg;
        public int[] Arr;
    }

    internal struct Delay
    {
        public Delay(int delay, int context, string method, SVariable[] args)
        {
            this.DelayTime = delay;
            this.Context = context;
            this.Method = method;
            this.Args = args;
        }
        public int Context;
        public string Method;
        public SVariable[] Args;
        public int DelayTime;
    }


}
