//#define VERBOSE_LOG

using AnimationEngine.Language;
using System;
using System.Collections.Generic;
using AnimationEngine.Language.Libs;
using AnimationEngine.Utility;
using VRageMath;
using AnimationEngine.Core;

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

        private List<SVariable> _globals;
        private List<ScriptLib> _libraries;
        #endregion

        List<Entity> _ents;

        public void Execute(string function, params SVariable[] args)
        {
            if (_methodLookup.ContainsKey(function))
            {
                foreach(var x in args)
                    _stack.Push(x);
                Execute(_methodLookup[function]);
            }
        }

        public ScriptRunner Clone()
        {
            return new ScriptV2Runner(this);
        }

        public void Init(CoreScript script)
        {
            core = script;
        }

        public void Tick(int time)
        {
            
        }

        public void Close()
        {

        }

        public ScriptV2Runner(List<Entity> ents, List<SVariable> globals, Line[] program, SVariable[] immediates, Dictionary<string, int> methods)
        {
            _globals = globals;
            _program = program;
            _immediates = immediates;
            _methodLookup = methods;
            _ents = ents;
        }
        
        public ScriptV2Runner(ScriptV2Runner copy)
        {
            _stack = new RAStack<SVariable>();
            _callStack = new Stack<int>();
            _globals = new List<SVariable>(copy._globals);

            _libraries = new List<ScriptLib>();
            _libraries.Add(new ScriptMath());
            _libraries.Add(new ScriptAPI());

            _program = copy._program;
            _immediates = copy._immediates;
            _methodLookup = copy._methodLookup;
        }

        private void Execute(int line)
        {
            int start = _stack.Count;
            Line curr;

            int count = 0;
            while (count++ < 100000)
            {
                curr = _program[line++];


#if VERBOSE_LOG

                Console.Write($"Line {line:D3}: ({_stack.Count:D3}) {curr.Arg,-4} > ");
                if (curr.Arr != null)
                {
                    foreach (var x in curr.Arr)
                    {
                        Console.Write($"{x:D3} ");
                    }
                }
                Console.Write("\n");
#endif


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
                        zFlag = x.AsInt() == 0;
                        nzFlag = x.AsInt() != 0;
                        nFlag = x.AsInt() < 0;
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
                        _context = curr.Arr[0];
                        break;
                    case ProgramFunc.Mth:
                        SVariable[] arr = new SVariable[curr.Arr[1]];
                        _stack.CopyTo(_stack.Count - curr.Arr[1], arr, 0, curr.Arr[1]);
                        SVariable s = _libraries[_context].Execute(_immediates[curr.Arr[0]].ToString(), arr);
                        if (s != null)
                        {
                            _stack.Push(s);
                        }
                        else
                        {
                            _stack.Push(new SVariableInt(0));
                        }
                        break;
                    
                    case ProgramFunc.Jmp:
                        _callStack.Push(line);
                        line = curr.Arr[0]; 
                        break;
                    
                    case ProgramFunc.End:
                        if (_callStack.Count == 0)
                        {
                            if (_stack.Count - start != 0)
                            {
                                throw new Exception($"Critical error, stack leaking ({_stack.Count - start})! Please send your script(s) to Math#0424 for analysis");
                            }
                            return;
                        }
                        line = _callStack.Pop();
                        break;
                }
                if (_stack.Count > 1000)
                {
                    throw new Exception("Script too complex, large stack detected (this is not a bug)");
                }
            }
            throw new Exception("Script too complex, large loop detected (this is not a bug)");
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


}
