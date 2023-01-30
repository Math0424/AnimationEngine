using AnimationEngine.CoreScript;
using System;
using System.Collections.Generic;
using System.IO;
using AnimationEngine.LanguageV2.Nodes;
using Sandbox.ModAPI;
using static VRage.Game.MyObjectBuilder_Checkpoint;
using AnimationEngine.Utility;

namespace AnimationEngine.Language
{
    internal class ScriptV2Generator : ScriptGenerator
    {

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
        public List<SVariable> globals = new List<SVariable>();

        public Dictionary<int, SVariable> globalVars = new Dictionary<int, SVariable>();
        public Dictionary<string, int> methodLookup = new Dictionary<string, int>();

        public int AddImmediate(SVariable var)
        {
            for(int i = 0; i < _immediates.Count; i++) {
                if (var.Equals(_immediates[i]))
                {
                    return i;
                }
            }
            _immediates.Add(var);
            return _immediates.Count - 1;
        }

        public ScriptV2Generator(ModItem mod, string path) : base(mod, path)
        {
            if (MyAPIGateway.Utilities.FileExistsInModLocation(path, mod))
            {
                Error = new ScriptError();
                RawScript = MyAPIGateway.Utilities.ReadFileInModLocation(path, mod).ReadToEnd().Split('\n');
                try
                {
                    long start = DateTime.Now.Ticks;
                    Log($"Compiling script {Path.GetFileName(path)} for {{mod.Name}}");
                    Log($"|  Lexer 1/5");
                    Lexer.TokenizeScript(this);
                    Log($"|    loaded {Tokens.Count} tokens");
                    Log($"|  Parser 2/5");
                    //TODO parse headers here and choose compiler

                    Context.EnterNewContext(Tokens.ToArray());
                    Context.SetScript(this);

                    objects.Add(new Entity("math"));
                    objects.Add(new Entity("api"));
                    //objects.Add(new Entity("block"));
                    //objects.Add(new Entity("wc"));
                    ScriptNode root = new ScriptNode();

                    Log($"|    generated {GetNodeCount(root)} nodes");
                    Log($"|    |    created {globalCount} globals");
                    Log($"|    |    created {objects.Count} objects");
                    Log($"|    |    created {functions.Count} functions");
                    Log($"|    |    created {actions.Count} actions");
                    Log($"|  Compilation 3/5");
                    root.Compile();
                    Log($"|    finalized {program.Count} lines of bytecode");

                    //Log("\n--Immediates--\n");
                    //int i = 0;
                    //foreach (var x in _immediates)
                    //    Log($"{i++:D3} {x}");
                    //
                    //Log("\n--Script--\n");
                    //i = 0;
                    //foreach (var x in program) {
                    //    string v = $"{i++:D4} {x.Arg} : ";
                    //    if (x.Arr != null)
                    //    {
                    //        foreach (var y in x.Arr)
                    //        {
                    //            v += $"{y:D4} ";
                    //        }
                    //    } 
                    //    else
                    //    {
                    //        v += "NULL";
                    //    }
                    //    Log(v);
                    //}
                    //Log("\n");

                    Log($"|  Logic 4/5");
                    string blockid = "Dummy";
                    //ScriptConstants script = ScriptConstantsAssembler.Assemble(this, out blockid);
                    //AnimationEngine.AddToRegistered(blockid.ToLower(), script);
                    Log($"|    assembled logic for block '{blockid}'");
                    Log($"|  Registering 5/5");
                    //TODO: add to all blocks of this type
                    Log($"|    added '{blockid}' to the script register");
                    Log($"Compiled script ({(DateTime.Now.Ticks - start) / TimeSpan.TicksPerMillisecond}ms)");
                }
                catch (Exception ex)
                {
                    if (!(ex is ScriptError))
                    {
                        Error.AppendError(ex);
                    }
                    throw Error;
                }
            }
            else
            {
                throw new Exception($"Script file not found! ({path} {{mod.Name}})");
            }
        }

        private int GetNodeCount(CompilationNode node)
        {
            int i = 1;
            foreach(var x in node.children)
            {
                i += GetNodeCount(x);
            }
            return i;
        }

        public ScriptError DetailedErrorLog(string reason, Token token)
        {
            return Error.AppendError($"{reason} : line {token.Line}", RawScript[token.Line].Trim(), token.Col - (token.Value.ToString().Length / 2));
        }

        public void Log(object msg)
        {
            Utils.LogToFile(msg);
        }

    }
}
