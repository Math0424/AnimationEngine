using AnimationEngine.Utility;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using static VRage.Game.MyObjectBuilder_Checkpoint;

namespace AnimationEngine.Language
{
    internal abstract class ScriptGenerator
    {
        public ScriptError Error { get; protected set; }
        public string[] RawScript { get; protected set; }
        public List<Token> Tokens = new List<Token>();
        private Dictionary<string, string> headers = new Dictionary<string, string>();

        public ScriptGenerator(ModItem mod, string path)
        {
            if (MyAPIGateway.Utilities.FileExistsInModLocation(path, mod))
            {
                Error = new ScriptError();
                RawScript = MyAPIGateway.Utilities.ReadFileInModLocation(path, mod).ReadToEnd().Split('\n');

                try
                {
                    long start = DateTime.Now.Ticks;
                    Log($"Reading script {Path.GetFileName(path)} for {mod.Name}");
                    Log($"|  Lexer");
                    Lexer.TokenizeScript(this);
                    Log($"|    Read {Tokens.Count} tokens");
                    Log($"|  Reading headers");
                    ParseHeaders();
                    Log($"|    Found {headers.Count} headers");
                    
                    if (!headers.ContainsKey("version"))
                        throw new Exception("Cannot find script @version header");
                    int versionId;
                    if (!int.TryParse(headers["version"], out versionId))
                        throw new Exception($"version number '{headers["version"]}' cannot be read");

                    switch (versionId)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        default:
                            throw new Exception($"Unsupported script version number {versionId}");
                    }

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
        }

        private void ParseHeaders()
        {
            int next = FindNext(0, TokenType.AT);
            List<Token> arr;
            while (next != -1)
            {
                arr = Grab(next);
                if (arr.Count == 4)
                    headers[arr[1].Value.ToString().ToLower()] = arr[2].Value.ToString().ToLower();
                next = FindNext(next + 1, TokenType.AT);
            }
        }

        private List<Token> Grab(int start)
        {
            List<Token> arr = new List<Token>();
            for (int i = start; i < Tokens.Count; i++)
            {
                arr.Add(Tokens[i]);
                if (Tokens[i].Type == TokenType.ENDL)
                    break;
            }
            return arr;
        }

        private int FindNext(int start, TokenType type)
        {
            for (int i = start; i < Tokens.Count; i++)
                if (Tokens[i].Type == type)
                    return i;
            return -1;
        }

        private void Log(object msg)
        {
            Utils.LogToFile(msg);
        }
    }
}
