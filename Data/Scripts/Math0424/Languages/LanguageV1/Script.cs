using AnimationEngine.Core;
using AnimationEngine.LogicV1;
using AnimationEngine.Util;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using VRage.Utils;
using static VRage.Game.MyObjectBuilder_Checkpoint;

namespace AnimationEngine.LanguageV1
{
    internal class Script
    {

        public ScriptError Error { get; private set; }
        public string[] RawScript { get; private set; }

        //stage 1
        public List<Token> Tokens = new List<Token>();

        //stage 2
        public Dictionary<string, Token> headers = new Dictionary<string, Token>();
        public Dictionary<string, Entity> objects = new Dictionary<string, Entity>();
        public Dictionary<string, Function> functions = new Dictionary<string, Function>();
        
        public List<ScriptAction> actions = new List<ScriptAction>();

        public Script(ModItem mod, string path)
        {
            if (MyAPIGateway.Utilities.FileExistsInModLocation(path, mod))
            {
                Error = new ScriptError();
                RawScript = MyAPIGateway.Utilities.ReadFileInModLocation(path, mod).ReadToEnd().Split('\n');
                try
                {
                    long start = DateTime.Now.Ticks;
                    Log($"Compiling script {Path.GetFileName(path)} for {mod.Name}");
                    Log($"|  Lexer 1/4");
                    Lexer.TokenizeScript(this);
                    Log($"|    loaded {Tokens.Count} tokens");
                    Log($"|  Parser 2/4");
                    Parser parser = new Parser(this);
                    Log($"|    created {headers.Count} headers");
                    Log($"|    created {objects.Count} objects");
                    Log($"|    created {functions.Count} functions");
                    Log($"|    created {actions.Count} actions");
                    Log($"|  Logic 3/4");
                    string blockid;
                    ScriptConstants script = ScriptConstantsAssembler.Assemble(this, out blockid);
                    Log($"|    assembled logic for block '{blockid}'");
                    Log($"|  Registering 4/4");
                    AnimationEngine.AddToRegistered(blockid.ToLower(), script);
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
                throw new Exception($"Script file not found! ({path} {mod.Name})");
            }
        }

        public ScriptError DetailedLog(string reason, Token token)
        {
            return Error.AppendError($"{reason} : line {token.Line}", RawScript[token.Line], token.Col - (token.Value.ToString().Length / 2));
        }

        private void Log(object msg)
        {
            Utils.LogToFile(msg);
        }

    }
}
