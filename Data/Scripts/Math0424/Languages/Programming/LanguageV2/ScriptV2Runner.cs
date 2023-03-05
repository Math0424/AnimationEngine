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

/// <summary>
/// This handles the code, its pretty much a emulator for a basic computer
/// </summary>
namespace AnimationEngine.Language
{
    internal enum ProgramFunc
    {
        bXor,

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

        LdI, // pushes immediate [0] to top of register
        Cpy, // copy [0] to [1]

        LdG, // load a global
        StG, // save a global

        Cxt, // set context to object [0]
        Mth, // call method from immediate [0]

        Jmp, //jump to method at [0]

        End, // terminate program (or jump to previous _callStack)
    }

    internal class ScriptV2Runner : ScriptRunner
    {
        CoreScript core;

        #region Script
        private RAStack<SVariable> _stack;
        private Stack<int> _callStack;
        private bool nFlag, zFlag, nzFlag;
        private int _context;

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
        int _stackStart;
        
        private bool Built;
        private MyCubeBlockDefinition Definition;
        private string buildId;

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
            else
            {
                Utils.LogToFile($"Error with {Definition.Id.SubtypeName}: Cannot find method {function}");
            }
        }

        public ScriptRunner Clone()
        {
            return new ScriptV2Runner(this);
        }

        public void Init(CoreScript script)
        {
            core = script;

            foreach (var x in _ents)
                InitEnt(x);
            foreach (var x in _actions)
                InitAction(x);

            foreach (var x in _libraries)
            {
                IMyEntity ent = script.Entity;
                if (x is Parentable && ((Parentable)x).GetParent() != null)
                {
                    //Utils.LogToFile($"Getting subpart {((Parentable)x).GetParent()}");
                    //foreach(var z in script.Subparts)
                    //{
                    //    Utils.LogToFile($"{z.Key} : {z.Value}");
                    //}
                    ent = script.Subparts[((Parentable)x).GetParent()].Subpart;
                } 
                if (x is Initializable)
                    ((Initializable)x).Init(ent);
            }

            Definition = ((MyCubeBlockDefinition)((IMyCubeBlock)script.Entity).SlimBlock.BlockDefinition);
            Built = ((IMyCubeBlock)script.Entity).SlimBlock.BuildLevelRatio > Definition.CriticalIntegrityRatio;
        }

        private void InitEnt(Entity ent)
        {
            switch (ent.Type.Value.ToString().ToLower())
            {
                case "math":
                    _libraries.Add(new ScriptMath()); break;
                case "api":
                    _libraries.Add(new ScriptAPI(this)); break;
                case "block":
                    _libraries.Add(new BlockCore(core)); break;
                case "button":
                case "subpart":
                    _libraries.Add(core.Subparts[ent.Name.Value.ToString().ToLower()]);
                    break;
                case "emissive":
                    _libraries.Add(new Emissive(ent.Args[0].Value.ToString()));
                    break;
                case "emitter":
                    _libraries.Add(new Emitter(ent.Args[0].Value.ToString(), ent.Parent.Value?.ToString()));
                    break;
                case "light":
                    _libraries.Add(new Light(ent.Args[0].Value.ToString(), (float)ent.Args[1].Value, ent.Parent.Value?.ToString()));
                    break;
            }
        }

        private void InitAction(ScriptAction action)
        {
            switch (action.TokenName)
            {
                case "button":
                    var part = core.Subparts[action.Paramaters[0].Value.ToString()];
                    var dummy = action.Paramaters[1].Value.ToString();

                    if (!part.HasComponent<ButtonComp>())
                        part.AddComponent(new ButtonComp(dummy));

                    part.GetFirstComponent<ButtonComp>().Init(part);
                    part.GetFirstComponent<ButtonComp>().Pressed += (e) => Execute($"act_{action.ID}_pressed", e);
                    break;
                case "block":
                    foreach (var x in action.Funcs)
                        switch (x.TokenName)
                        {
                            case "create": Execute($"act_{action.ID}_create"); break;
                            case "built":
                                buildId = $"act_{action.ID}_built";
                                break;
                            case "working":
                                if (!core.HasComponent<WorkingTickComp>())
                                    core.AddComponent(new WorkingTickComp(-1));
                                core.GetFirstComponent<WorkingTickComp>().OnIsWorking += () => Execute($"act_{action.ID}_working");
                                break;
                            case "notworking":
                                if (!core.HasComponent<WorkingTickComp>())
                                    core.AddComponent(new WorkingTickComp(-1));
                                core.GetFirstComponent<WorkingTickComp>().OnNotWorking += () => Execute($"act_{action.ID}_notworking");
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
                                core.GetFirstComponent<ProductionTickComp>().StartedProducing += () => Execute($"act_{action.ID}_stopproducing");
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
                        switch (f)
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

        private void InitTerminal(ScriptAction terminal)
        {
            //TODO: this
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

            var check = ((IMyCubeBlock)core.Entity).SlimBlock.BuildLevelRatio > Definition.CriticalIntegrityRatio;
            if (check != Built)
            {
                Built = check;
                if (Built)
                    Execute(buildId);
            }
        }

        public void Close()
        {

        }

        public ScriptV2Runner(List<Entity> ents, List<ScriptAction> actions, SVariable[] globals, Line[] program, SVariable[] immediates, Dictionary<string, int> methods)
        {
            _globals = globals;
            _program = program;
            _immediates = immediates;
            _methodLookup = methods;
            _ents = ents;
            _actions = actions;
        }

        public ScriptV2Runner(ScriptV2Runner copy)
        {
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

                    case ProgramFunc.Cxt:
                        _delay = 0;
                        _context = curr.Arr[0];
                        break;
                    case ProgramFunc.Mth:
                        SVariable[] arr = new SVariable[curr.Arr[1]];
                        _stack.CopyTo(_stack.Count - curr.Arr[1], arr, 0, curr.Arr[1]);
                        _stack.RemoveRange(_stack.Count - curr.Arr[1], curr.Arr[1]);
                        if (_immediates[curr.Arr[0]].ToString() == "delay")
                        {
                            _delay += arr[0].AsInt();
                            _stack.Push(new SVariableInt(0));
                        }
                        else if(_delay != 0)
                        {
                            _delays.Add(new Delay(_delay, _context, _immediates[curr.Arr[0]].ToString(), arr));
                            _stack.Push(new SVariableInt(0));
                        }
                        else
                        {
                            SVariable s = _libraries[_context].Execute(_immediates[curr.Arr[0]].ToString(), arr);
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
                                throw new Exception($"Critical error, stack leaking ({_stack.Count - _stackStart})! Please send your script(s) to Math#0424");
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
