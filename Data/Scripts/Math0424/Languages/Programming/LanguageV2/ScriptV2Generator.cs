using AnimationEngine.Core;
using AnimationEngine.LanguageV2.Nodes;
using AnimationEngine.Utility;
using System.Collections.Generic;

namespace AnimationEngine.Language
{
    internal class ScriptV2Generator
    {
        ScriptGenerator generator;
        public ScriptError Error => generator.Error;
        public List<Token> Tokens => generator.Tokens;
        public string[] RawScript => generator.RawScript;

        // parser
        public Dictionary<string, Token> headers = new Dictionary<string, Token>();
        public Dictionary<string, Function> functions = new Dictionary<string, Function>();
        public List<Entity> objects = new List<Entity>();

        public List<ScriptAction> actions = new List<ScriptAction>();
        public List<ScriptAction> terminals = new List<ScriptAction>();

        public int globalCount = 0;

        // compiler
        public List<Line> program = new List<Line>();
        public List<SVariable> _immediates = new List<SVariable>();
        public SVariable[] globals;

        public Dictionary<string, int> methodLookup = new Dictionary<string, int>();

        public int AddImmediate(SVariable var)
        {
            for (int i = 0; i < _immediates.Count; i++)
            {
                if (var.Equals(_immediates[i]))
                {
                    return i;
                }
            }
            _immediates.Add(var);
            return _immediates.Count - 1;
        }

        public ScriptV2Generator(ScriptGenerator generator, out ScriptRunner runner, out List<Subpart> defs)
        {
            this.generator = generator;

            Context.EnterNewContext(Tokens.ToArray());
            Context.SetScript(this);

            objects.Add(new Entity("math"));
            objects.Add(new Entity("api"));
            objects.Add(new Entity("block"));
            ScriptNode root = new ScriptNode();

            Log($"|  Running Generator V2");
            Log($"|  |  generated {GetNodeCount(root)} nodes");
            Log($"|  |  |  created {globalCount} globals");
            Log($"|  |  |  created {objects.Count} objects");
            Log($"|  |  |  created {functions.Count} functions");
            Log($"|  |  |  created {actions.Count} actions");
            Log($"|  Compilation");
            globals = new SVariable[globalCount];
            root.Compile();
            Log($"|  |  finalized {program.Count} lines of bytecode");


            /*int i;
            //Log("--Tokens--");
            //i = 0;
            //foreach (var x in Tokens)
            //    Log($"{i++:D3} {x.Type}");

            Log("--Globals--");
            i = 0;
            foreach (var x in globals)
                Log($"{i++:D3} {x}");

            Log("--Immediates--");
            i = 0;
            foreach (var x in _immediates)
                Log($"{i++:D3} {x}");
            
            Log("--Script--");
            i = 0;
            foreach (var x in program) {
                string v = $"{i++:D4} {x.Arg} : ";
                if (x.Arr != null)
                {
                    foreach (var y in x.Arr)
                    {
                        v += $"{y:D4} ";
                    }
                } 
                else
                {
                    v += "NULL";
                }
                Log(v);
            }*/

            List<Subpart> subparts = new List<Subpart>();
            foreach (var x in objects)
            {
                if (x.Type.Value.ToString().ToLower() == "subpart")
                    subparts.Add(new Subpart(x.Args[0].Value.ToString(), x.Parent.Value == null ? null : x.Parent.Value.ToString()));
            }

            runner = new ScriptV2Runner(objects, actions, globals, program.ToArray(), _immediates.ToArray(), methodLookup);
            defs = subparts;
        }

        private int GetNodeCount(CompilationNode node)
        {
            int i = 1;
            foreach (var x in node.children)
            {
                i += GetNodeCount(x);
            }
            return i;
        }

        public ScriptError DetailedErrorLog(string reason, Token token)
        {
            return Error.AppendError($"{reason} : line {token.Line}", RawScript[token.Line], token.Col - (token.Value.ToString().Length / 2));
        }

        public void Log(object msg)
        {
            Utils.LogToFile(msg);
        }

    }
}
