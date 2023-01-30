using AnimationEngine.Core;
using AnimationEngine.Language;
using AnimationEngine.LanguageV1;
using AnimationEngine.Utility;
using System.Collections.Generic;

namespace AnimationEngine.LogicV1
{
    internal static class ScriptConstantsAssembler
    {
        public static ScriptConstants Assemble(ScriptV1Generator script, out string blockId)
        {
            if (!script.headers.ContainsKey("version") || script.headers["version"].Type != TokenType.INT || (int)script.headers["version"].Value != 1)
            {
                throw script.Error.AppendError("Invalid version header");
            }
            if (!script.headers.ContainsKey("blockid"))
            {
                throw script.Error.AppendError("Missing blockid header");
            }
            blockId = (string)script.headers["blockid"].Value;
            ScriptConstants constants = new ScriptConstants();

            foreach(var obj in script.objects.Values) {
                AssembleObject(obj, constants, script);
            }

            foreach (var obj in script.functions.Values)
            {
                string name = obj.Name.Value.ToString().ToLower();
                constants.Calls.Add(name, new Caller[obj.Body.Length]);
                for (int i = 0; i < obj.Body.Length; i++)
                {
                    AssembleCall(ref constants.Calls[name][i], obj.Body[i], name, script);
                }
            }

            int actId = 0;
            foreach(var obj in script.actions)
            {
                V1ScriptAction act = obj;
                act.ID = actId++;

                Lexicon.IsValidAction(act, ref script);

                foreach (var x in act.Funcs)
                {
                    string name = $"{act.ID}_{x.Name.Value}";
                    constants.Calls.Add(name, new Caller[x.Body.Length]);
                    for (int i = 0; i < x.Body.Length; i++)
                    {
                        AssembleCall(ref constants.Calls[name][i], x.Body[i], name, script);
                    }
                }
                constants.ScriptActions.Add(act);
            }

            return constants;
        }


        private static void AssembleCall(ref Caller call, V1Call exp, string name, ScriptV1Generator script)
        {
            switch(exp.Type)
            {
                case TokenType.FUNCCALL:
                    if (exp.Title.Equals(name))
                    {
                        throw script.DetailedLog("Cannot call function within function", exp.Title);
                    }

                    bool contains = false;
                    foreach(var x in script.functions)
                    {
                        if (x.Key.Equals(exp.Title.Value.ToString().ToLower()))
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains)
                    {
                        throw script.DetailedLog("Unknown call to function", exp.Title);
                    }

                    call.FuncCall = true;
                    call.Object = exp.Title.Value.ToString();
                    return;
                case TokenType.OBJECT:
                    call.FuncCall = false;
                    call.Object = exp.Title.Value.ToString();
                    call.Args = AssembleExpression(exp, script);
                    return;
            }
            throw script.Error.AppendError("Generic impossible error message");
        }

        private static Argument[] AssembleExpression(V1Call exp, ScriptV1Generator script)
        {
            List<Argument> stack = new List<Argument>();

            foreach(var x in exp.Expressions)
            {
                MethodDef? def;
                if (!Lexicon.IsValidMethodName(x, out def))
                {
                    throw script.DetailedLog("Unknown method call", x.Title);
                }
                if (!Lexicon.IsValidMethod(x))
                {
                    string error1 = "";
                    foreach(var z in def.Value.Args)
                    {
                        error1 += z.ToString() + " ";
                    }
                    string error2 = "";
                    foreach (var z in x.Args)
                    {
                        error2 += z.Type.ToString() + " ";
                    }
                    throw script.DetailedLog($"Method paramater missmatch expected '{error1.Trim()}' got '{error2.Trim()}'", x.Title);
                }
                if (x.Title.Value.ToString().Equals("delay"))
                {
                    int delay = (int)x.Args[0].Value;
                    for(int i = 0; i < stack.Count; i++)
                    {
                        Argument arg = stack[i];
                        arg.Delay += delay;
                        stack[i] = arg;
                    }
                }
                else
                {
                    object[] arr = new object[x.Args.Length];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = x.Args[i].Value;
                    }
                    stack.Add(new Argument() { 
                        Delay = 0,
                        Name = x.Title.Value.ToString().ToLower(),
                        Value = arr
                    });
                }
            }

            return stack.ToArray();
        }

        private static void AssembleObject(Entity ent, ScriptConstants constants, ScriptV1Generator script)
        {
            switch(ent.Type.Value.ToString().ToLower())
            {
                case "subpart":
                    constants.ObjectDefs.Add(new ScriptConstants.ObjectDef(ent.Type, ent.Name.Value.ToString()));
                    break;
                case "button":
                    if (ent.Args == null || ent.Args.Length != 1 || ent.Args[0].Type != TokenType.STR)
                    {
                        throw script.DetailedLog("Invalid button declaration", ent.Name);
                    }
                    constants.ObjectDefs.Add(new ScriptConstants.ObjectDef(ent.Type, ent.Name.Value.ToString(), ent.Args[0].Value));
                    break;
                case "emissive":
                    if (ent.Args == null || ent.Args.Length != 1 || ent.Args[0].Type != TokenType.STR)
                    {
                        throw script.DetailedLog("Invalid emissive declaration", ent.Name);
                    }
                    constants.ObjectDefs.Add(new ScriptConstants.ObjectDef(ent.Type, ent.Name.Value.ToString(), ent.Args[0].Value));
                    break;
                case "emitter":
                    if (ent.Args == null || ent.Args.Length != 1 || ent.Args[0].Type != TokenType.STR)
                    {
                        throw script.DetailedLog("Invalid emitter declaration", ent.Name);
                    }
                    constants.ObjectDefs.Add(new ScriptConstants.ObjectDef(ent.Type, ent.Name.Value.ToString(), ent.Args[0].Value));
                    break;
                case "light":
                    if (ent.Args == null || ent.Args.Length != 3 || ent.Args[0].Type != TokenType.STR || ent.Args[2].Type != TokenType.FLOAT)
                    {
                        throw script.DetailedLog($"Invalid light declaration", ent.Name);
                    }
                    constants.ObjectDefs.Add(new ScriptConstants.ObjectDef(ent.Type, ent.Name.Value.ToString(), ent.Args[0].Value.ToString(), ent.Args[2].Value));
                    break;
            }
        }

    }
}
