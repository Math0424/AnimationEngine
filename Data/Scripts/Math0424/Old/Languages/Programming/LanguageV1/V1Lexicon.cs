using AnimationEngine.Language;
using AnimationEngine.LanguageV1;
using System.Collections.Generic;

namespace AnimationEngine.LogicV1
{
    internal struct Caller
    {
        public string Object;
        public bool FuncCall;
        public Argument[] Args;
    }

    internal struct Argument
    {
        public string Name;
        public SVariable[] Value;
        public int Delay;
    }

    internal struct Delayed
    {
        public string Object;
        public string Name;
        public SVariable[] Args;
        public int Delay;
        public bool Executed;
    }

    internal static class V1Lexicon
    {
        public static bool IsValidMethodName(V1Expression exp, out MethodDef? def)
        {
            def = null;
            foreach (var x in DefinedMethods)
            {
                if (x.Name.Equals(exp.Title.Value.ToString().ToLower()))
                {
                    def = x;
                    return true;
                }
            }
            return false;
        }
        public static bool IsValidMethod(V1Expression exp)
        {
            foreach (var x in DefinedMethods)
            {
                if (x.Name.Equals(exp.Title.Value.ToString().ToLower()))
                {
                    if (exp.Args.Length == x.Args.Length)
                    {
                        for (int i = 0; i < x.Args.Length; i++)
                        {
                            if (exp.Args[i].Type != x.Args[i])
                            {
                                goto next;
                            }
                        }
                        return true;
                    }
                }
            next:;
            }
            return false;
        }

        private static readonly MethodDef[] DefinedMethods = new MethodDef[]
        {
            new MethodDef("delay", TokenType.INT),
            new MethodDef("log", TokenType.STR),

            new MethodDef("poweron"),
            new MethodDef("poweroff"),
            new MethodDef("togglepower"),

            new MethodDef("opendoor"),
            new MethodDef("closedoor"),
            new MethodDef("toggledoor"),

            new MethodDef("translatepilot"),
            new MethodDef("rotatepilot"),

            new MethodDef("lockon"),
            new MethodDef("lockoff"),
            new MethodDef("togglelock"),

            new MethodDef("pilottranslate", TokenType.MVECTOR, TokenType.INT, TokenType.LERP),
            new MethodDef("pilotrotate", TokenType.MVECTOR, TokenType.FLOAT, TokenType.INT, TokenType.LERP),
            new MethodDef("pilotrotatearound", TokenType.MVECTOR, TokenType.MVECTOR, TokenType.FLOAT, TokenType.INT, TokenType.LERP),
            new MethodDef("pilotspin", TokenType.MVECTOR, TokenType.FLOAT, TokenType.INT),
            new MethodDef("pilotvibrate", TokenType.FLOAT, TokenType.INT),

            new MethodDef("translate", TokenType.MVECTOR, TokenType.INT, TokenType.LERP),
            new MethodDef("rotate", TokenType.MVECTOR, TokenType.FLOAT, TokenType.INT, TokenType.LERP),
            new MethodDef("rotatearound", TokenType.MVECTOR, TokenType.MVECTOR, TokenType.FLOAT, TokenType.INT, TokenType.LERP),
            new MethodDef("spin", TokenType.MVECTOR, TokenType.FLOAT, TokenType.INT),
            new MethodDef("vibrate", TokenType.FLOAT, TokenType.INT),

            new MethodDef("scale", TokenType.MVECTOR, TokenType.INT, TokenType.LERP),
            new MethodDef("setvisible", TokenType.BOOL),
            new MethodDef("reset"),
            new MethodDef("resetpos"),
            new MethodDef("setresetpos"),

            new MethodDef("enabled", TokenType.BOOL),
            new MethodDef("interactable", TokenType.BOOL),

            new MethodDef("setcolor", TokenType.INT, TokenType.INT, TokenType.INT, TokenType.FLOAT),
            new MethodDef("setcolor", TokenType.INT, TokenType.INT, TokenType.INT, TokenType.FLOAT, TokenType.BOOL),
            new MethodDef("setsubpartcolor", TokenType.STR, TokenType.INT, TokenType.INT, TokenType.INT, TokenType.FLOAT),

            new MethodDef("setcolor", TokenType.INT, TokenType.INT, TokenType.INT),

            new MethodDef("lighton"),
            new MethodDef("lightoff"),
            new MethodDef("togglelight"),

            new MethodDef("playparticle", TokenType.STR, TokenType.FLOAT, TokenType.FLOAT, TokenType.MVECTOR, TokenType.INT, TokenType.INT, TokenType.INT),
            new MethodDef("playparticle", TokenType.STR, TokenType.FLOAT, TokenType.FLOAT, TokenType.MVECTOR),
            new MethodDef("playparticle", TokenType.STR, TokenType.FLOAT, TokenType.FLOAT),
            new MethodDef("stopparticle"),

            new MethodDef("playsound", TokenType.STR),
            new MethodDef("stopsound"),
        };

        public static void IsValidAction(V1ScriptAction act, ScriptV1Generator script)
        {
            foreach (var defAct in DefinedActions)
            {
                if (defAct.Name.Equals(act.Name.Value.ToString().ToLower()))
                {
                    if (defAct.Args.Length != act.Paramaters.Length)
                    {
                        throw script.DetailedLog($"Paramater missmatch, expected {defAct.Args.Length} args", act.Name);
                    }

                    for (int i = 0; i < defAct.Args.Length; i++)
                    {
                        if (defAct.Args[i] != act.Paramaters[i].Type)
                        {
                            throw script.DetailedLog($"Invalid paramater, expected {defAct.Args[i]} : found {act.Paramaters[i].Type}", act.Paramaters[i]);
                        }
                    }

                    foreach (var z in act.Funcs)
                    {
                        bool contains = false;
                        foreach (var y in defAct.Methods)
                        {
                            if (y.Name.Equals(z.Name.Value.ToString()))
                            {
                                if (y.Args.Length != z.Paramaters.Length)
                                {
                                    throw script.DetailedLog($"Paramater missmatch, expected {y.Args.Length} args", z.Name);
                                }

                                for (int i = 0; i < y.Args.Length; i++)
                                {
                                    if (y.Args[i] != z.Paramaters[i].Type)
                                    {
                                        throw script.DetailedLog($"Invalid paramater, expected {y.Args[i]} : found {z.Paramaters[i].Type}", z.Name);
                                    }
                                }
                                contains = true;
                                break;
                            }
                        }
                        if (!contains)
                        {
                            throw script.DetailedLog($"Unknown action value in {z.Name.Value}", z.Name);
                        }
                    }
                    return;
                }
            }
            throw script.DetailedLog("Unknown action", act.Name);
        }

        private static readonly List<ActionDef> DefinedActions = new List<ActionDef>()
        {
            { new ActionDef("buttonaction", new TokenType[] { TokenType.KEWRD },
                new MethodDef("pressedon"),
                new MethodDef("pressedoff"),
                new MethodDef("pressed"))
            },

            { new ActionDef("productionaction", new TokenType[0],
                new MethodDef("startproducing"),
                new MethodDef("producingloop", TokenType.INT),
                new MethodDef("stopproducing"))
            },

            { new ActionDef("dooraction", new TokenType[0],
                new MethodDef("open"),
                new MethodDef("close"))
            },

            { new ActionDef("cockpitaction", new TokenType[0],
                new MethodDef("enter"),
                new MethodDef("exit"))
            },

            { new ActionDef("landinggearaction", new TokenType[0],
                new MethodDef("lock"),
                new MethodDef("unlock"),
                new MethodDef("readylock"))
            },

            { new ActionDef("distanceaction", new TokenType[] { TokenType.FLOAT },
                new MethodDef("arrive"),
                new MethodDef("leave"))
            },

            { new ActionDef("blockaction", new TokenType[0],
                new MethodDef("create"),
                new MethodDef("built"),
                new MethodDef("working"),
                new MethodDef("workingloop", TokenType.INT),
                new MethodDef("notworking"))
            },
        };
    }

    internal struct MethodDef
    {
        public string Name;
        public TokenType[] Args;
        public MethodDef(string Name, params TokenType[] Args)
        {
            this.Name = Name;
            this.Args = Args;
        }
    }

    internal struct ActionDef
    {
        public string Name;
        public TokenType[] Args;
        public MethodDef[] Methods;
        public ActionDef(string Name, TokenType[] Args, params MethodDef[] Methods)
        {
            this.Name = Name;
            this.Methods = Methods;
            this.Args = Args;
        }
    }

}
