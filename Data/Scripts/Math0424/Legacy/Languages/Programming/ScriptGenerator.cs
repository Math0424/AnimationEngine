﻿using AnimationEngine.Core;
using AnimationEngine.LanguageV1;
using AnimationEngine.Utility;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using static VRage.Game.MyObjectBuilder_Checkpoint;

namespace AnimationEngine.Language
{
    internal class ScriptGenerator
    {
        public ModItem Mod;
        public ScriptError Error { get; protected set; }
        public string[] RawScript { get; protected set; }
        public List<Token> Tokens = new List<Token>();
        public Dictionary<string, string> headers = new Dictionary<string, string>();

#if !DEBUG
        public ScriptGenerator(ModItem mod, string path)
        {
            if (MyAPIGateway.Utilities.FileExistsInModLocation(path, mod))
            {
                Error = new ScriptError();
                
                RawScript = MyAPIGateway.Utilities.ReadFileInModLocation(path, mod).ReadToEnd().Split('\n');
#else
        public ScriptGenerator(ModItem mod, string path) { }

        public ScriptGenerator(ModItem mod, string path, out ScriptRunner rout)
        { 
            if (File.Exists(path))
            {
                Error = new ScriptError();
                RawScript = File.ReadAllLines(path);
#endif
                try
                {
                    long start = DateTime.Now.Ticks;
                    //Log($"Reading script {Path.GetFileName(path)} for {mod.Name}");
                    this.Mod = mod;
                    //Log($"|  Lexer");
                    Lexer.TokenizeScript(this);
                    //Log($"|  |  Generated {Tokens.Count} tokens");
                    //Log($"|  Reading headers");
                    ParseHeaders();
                    //Log($"|  |  Found {headers.Count} headers");

                    if (!headers.ContainsKey("version"))
                        throw new Exception("Cannot find script @version header");
                    int versionId;
                    if (!int.TryParse(headers["version"], out versionId))
                        throw new Exception($"version number '{headers["version"]}' cannot be read");

                    if (!headers.ContainsKey("blockid"))
                        throw new Exception("Cannot find block id");


                    int weaponId = -1;
                    if (headers.ContainsKey("weaponcore"))
                    {
                        if (!int.TryParse(headers["weaponcore"], out weaponId))
                        {
                            throw new Exception($"Unknown WeaponCore weapon ID of '{headers["weaponcore"]}'");
                        }
                    }
                    bool toolcore = headers.ContainsKey("toolcore");

                    ScriptRunner runner = null;
                    List<Subpart> subparts = null;
                    switch (versionId)
                    {
                        case 1: new ScriptV1Generator(this, out runner, out subparts); break;
                        case 2:
                            if (weaponId != -1)
                            {
                                new ScriptV2Generator(this, out runner, out subparts, "weaponcore");
                                runner = new WeaponcoreScriptRunner(weaponId, runner);
                                //Log($"|  Loading Weaponcore Script Runner");
                            } 
                            else if(toolcore)
                            {
                                new ScriptV2Generator(this, out runner, out subparts);
                                runner = new ToolcoreScriptRunner(weaponId, runner);
                                //Log($"|  Loading Toolcore Script Runner");
                            }
                            else
                                new ScriptV2Generator(this, out runner, out subparts);
                            break;
                        default: throw new Exception($"Unsupported script version number {versionId}");
                    }

#if DEBUG
                    rout = runner;
#endif

                    //Log($"|  Registering block");
                    AnimationEngine.AddToRegisteredScripts(headers["blockid"], subparts.ToArray(), runner);
                    //Log($"|  |  Registered script to '{headers["blockid"]}'");

                    //Log($"Compiled script ({(DateTime.Now.Ticks - start) / TimeSpan.TicksPerMillisecond}ms)");
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

        private void ParseHeaders()
        {
            int next = FindNext(0, TokenType.AT);
            List<Token> arr;
            while (next != -1)
            {
                arr = Grab(next);
                if (arr.Count == 4)
                    headers[arr[1].Value.ToString().ToLower()] = arr[2].Value.ToString();
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

        public ScriptError DetailedErrorLog(string reason, Token token)
        {
            return Error.AppendError($"{reason} : line {token.Line}", RawScript[token.Line].Trim(), token.Col - (token.Value.ToString().Length / 2));
        }

        private void Log(object msg)
        {
            Utils.LogToFile(msg);
        }
    }
}
