using AnimationEngine.Core;
using AnimationEngine.Language;
using AnimationEngine.LogicV1;
using AnimationEngine.Utility;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using VRage.Utils;

namespace AnimationEngine.LanguageV1
{
    internal class ScriptV1Generator
    {
        public Dictionary<string, Token> headers = new Dictionary<string, Token>();
        public Dictionary<string, Entity> objects = new Dictionary<string, Entity>();
        public Dictionary<string, V1Function> functions = new Dictionary<string, V1Function>();
        public List<V1ScriptAction> actions = new List<V1ScriptAction>();
        private List<V1ScriptAction> actionsUpdated = new List<V1ScriptAction>();

        private List<Subpart> subparts = new List<Subpart>();
        private List<ObjectDef> objectDefs = new List<ObjectDef>();
        private Dictionary<string, Caller[]> calls = new Dictionary<string, Caller[]>();

        public ScriptGenerator Script { get; private set; }

        public ScriptV1Generator(ScriptGenerator generator, out ScriptRunner runner, out List<Subpart> defs)
        {
            Script = generator;

            Log($"|  Running Generator V1");
            new Parser(this);
            Log($"|  |  created {headers.Count} headers");
            Log($"|  |  created {objects.Count} objects");
            Log($"|  |  created {functions.Count} functions");
            Log($"|  |  created {actions.Count} actions");

            foreach (var obj in objects.Values)
                AssembleObject(obj);

            foreach (var obj in functions.Values)
            {
                string name = obj.Name.Value.ToString().ToLower();
                calls.Add(name, new Caller[obj.Body.Length]);
                for (int i = 0; i < obj.Body.Length; i++)
                {
                    AssembleCall(ref calls[name][i], obj.Body[i], name);
                }
            }

            int actId = 0;
            foreach (var obj in actions)
            {
                V1ScriptAction act = obj;
                act.ID = actId++;

                V1Lexicon.IsValidAction(act, this);

                foreach (var x in act.Funcs)
                {
                    string name = $"{act.ID}_{x.Name.Value}";
                    calls.Add(name, new Caller[x.Body.Length]);
                    for (int i = 0; i < x.Body.Length; i++)
                    {
                        AssembleCall(ref calls[name][i], x.Body[i], name);
                    }
                }
                actionsUpdated.Add(act);
            }

            runner = new ScriptV1Runner(objectDefs, actionsUpdated, calls);
            defs = subparts;
        }

        private void AssembleCall(ref Caller call, V1Call exp, string name)
        {
            switch (exp.Type)
            {
                case TokenType.FUNCCALL:
                    if (exp.Title.Equals(name))
                    {
                        throw Script.DetailedErrorLog("Cannot call function within function", exp.Title);
                    }

                    bool contains = false;
                    foreach (var x in functions)
                    {
                        if (x.Key.Equals(exp.Title.Value.ToString().ToLower()))
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains)
                    {
                        throw Script.DetailedErrorLog("Unknown call to function", exp.Title);
                    }

                    call.FuncCall = true;
                    call.Object = exp.Title.Value.ToString();
                    return;
                case TokenType.OBJECTCALL:
                    call.FuncCall = false;
                    call.Object = exp.Title.Value.ToString();
                    call.Args = AssembleExpression(exp);
                    return;
            }
            throw Script.Error.AppendError("Generic impossible error message");
        }

        private Argument[] AssembleExpression(V1Call exp)
        {
            List<Argument> stack = new List<Argument>();

            foreach (var x in exp.Expressions)
            {
                MethodDef? def;
                if (!V1Lexicon.IsValidMethodName(x, out def))
                {
                    throw Script.DetailedErrorLog("Unknown method call", x.Title);
                }
                if (!V1Lexicon.IsValidMethod(x))
                {
                    string error1 = "";
                    foreach (var z in def.Value.Args)
                    {
                        error1 += z.ToString() + " ";
                    }
                    string error2 = "";
                    foreach (var z in x.Args)
                    {
                        error2 += z.Type.ToString() + " ";
                    }
                    throw Script.DetailedErrorLog($"Method paramater missmatch expected '{error1.Trim()}' got '{error2.Trim()}'", x.Title);
                }
                if (x.Title.Value.ToString().Equals("delay"))
                {
                    int delay = (int)x.Args[0].Value;
                    for (int i = 0; i < stack.Count; i++)
                    {
                        Argument arg = stack[i];
                        arg.Delay += delay;
                        stack[i] = arg;
                    }
                }
                else
                {
                    SVariable[] arr = new SVariable[x.Args.Length];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = SVarUtil.Convert(x.Args[i]);
                    }
                    stack.Add(new Argument()
                    {
                        Delay = 0,
                        Name = x.Title.Value.ToString().ToLower(),
                        Value = arr
                    });
                }
            }

            return stack.ToArray();
        }

        private void AssembleObject(Entity ent)
        {
            if (ent.Type.Value == null)
                return;
            switch (ent.Type.Value.ToString().ToLower())
            {
                case "subpart":
                    objectDefs.Add(new ObjectDef(ent.Type.Value.ToString().ToLower(), ent.Name.Value.ToString(), null));
                    subparts.Add(new Subpart(ent.Name.Value.ToString(), null));
                    break;
                case "button":
                    if (ent.Args == null || ent.Args.Length != 1 || ent.Args[0].Type != TokenType.STR)
                    {
                        throw Script.DetailedErrorLog("Invalid button declaration", ent.Name);
                    }
                    objectDefs.Add(new ObjectDef(ent.Type.Value.ToString().ToLower(), ent.Name.Value.ToString(), null, ent.Args[0].Value));
                    subparts.Add(new Subpart(ent.Name.Value.ToString(), null));
                    break;
                case "emissive":
                    if (ent.Args == null || ent.Args.Length != 1 || ent.Args[0].Type != TokenType.STR)
                    {
                        throw Script.DetailedErrorLog("Invalid emissive declaration", ent.Name);
                    }
                    objectDefs.Add(new ObjectDef(ent.Type.Value.ToString().ToLower(), ent.Name.Value.ToString(), null, ent.Args[0].Value));
                    break;
                case "emitter":
                    if (ent.Args == null || ent.Args.Length != 1 || ent.Args[0].Type != TokenType.STR)
                    {
                        throw Script.DetailedErrorLog("Invalid emitter declaration", ent.Name);
                    }
                    objectDefs.Add(new ObjectDef(ent.Type.Value.ToString().ToLower(), ent.Name.Value.ToString(), null, ent.Args[0].Value));
                    break;
                case "light":
                    if (ent.Args == null || ent.Args.Length != 3 || ent.Args[0].Type != TokenType.STR || ent.Args[2].Type != TokenType.FLOAT)
                    {
                        throw Script.DetailedErrorLog($"Invalid light declaration", ent.Name);
                    }
                    objectDefs.Add(new ObjectDef(ent.Type.Value.ToString().ToLower(), ent.Name.Value.ToString(), null, ent.Args[0].Value.ToString(), ent.Args[2].Value));
                    break;
            }
        }

        private void Log(object msg)
        {
            Utils.LogToFile(msg);
        }

        public ScriptError DetailedLog(string obj, Token t)
        {
            return Script.DetailedErrorLog(obj, t);
        }
    }
}
