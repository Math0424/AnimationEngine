//#define VERBOSE_LOG

using AnimationEngine.Language;
using System;
using System.Collections.Generic;
using AnimationEngine.CoreScript;
using AnimationEngine.CoreScript.Libs;
using AnimationEngine.Utils;
using VRageMath;

/// <summary>
/// This handles the code, its pretty much a emulator for a basic computer
/// </summary>
namespace AnimationEngine.CoreScript
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

    internal class MyCoreScript
    {
        private RAStack<SVariable> _stack;
        private Stack<int> _callStack;
        private bool nFlag, zFlag, nzFlag;
        private int _context;

        private readonly Line[] _program;
        private readonly SVariable[] _immediates;
        private readonly Dictionary<string, int> _methodLookup;

        private List<SVariable> _globals;
        private List<ScriptLib> _libraries;
        //private BlockScript script;
        //public void Initalize(BlockScript script)
        //{
        //    this.script = script;
        //}

        public MyCoreScript(ScriptCreator creator)
        {
            _libraries = new List<ScriptLib>();
            _libraries.Add(new ScriptMath());
            _libraries.Add(new ScriptAPI());

            _stack = new RAStack<SVariable>();
            _callStack = new Stack<int>();
            _globals = new List<SVariable>(creator.globals);
            
            _program = creator.program.ToArray();
            _immediates = creator._immediates.ToArray();
            _methodLookup = new Dictionary<string, int>(creator.methodLookup);
        }

        public MyCoreScript(MyCoreScript copy)
        {
            _stack = new RAStack<SVariable>();
            _callStack = new Stack<int>();
            _globals = new List<SVariable>(copy._globals);

            _program = copy._program;
            _immediates = copy._immediates;
            _methodLookup = copy._methodLookup;
        }
        
        public void Push(SVariable var)
        {
            _stack.Push(var);
        }

        public SVariable Pop(int i)
        {
            return _stack.Pop(i);
        }

        public bool Execute(string name)
        {
            if (!_methodLookup.ContainsKey(name))
            {
                return false;
            }
            Execute(_methodLookup[name]);
            return true;
        }

        public void Execute(int line)
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

        public void CallMethod(string name)
        {
            if (_methodLookup.ContainsKey(name))
            {
                Execute(_methodLookup[name]);
            }
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
