using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnimationEngine.Language
{
    internal static class LanguageDictionary
    {
#if RELEASE
        //I am lazy and dont like writing out the documentation. have it do it for me
        public static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Objects\n");
            foreach (var x in _objects)
            {
                string o = "";
                foreach (var y in x.Names)
                    o += y + ", ";
                if (o.Length > 0)
                    o = o.Substring(0, o.Length - 2);
                sb.AppendLine($"#### **{x.Name}({o})**");
            }
            sb.AppendLine();

            sb.AppendLine("# Actions\n");
            foreach (var x in _specialActions)
            {
                string o = "";
                foreach(var y in x.Args)
                    o += y + ", ";
                if (o.Length > 0)
                    o = o.Substring(0, o.Length - 2);
                sb.AppendLine($"#### **{x.Name}({o})**");
                foreach(var y in x.Children)
                {
                    o = "";
                    foreach (var z in y.Args)
                        o += z + ", ";
                    if (o.Length > 0)
                        o = o.Substring(0, o.Length - 2);
                    sb.AppendLine($"* {y.Name}({o})");
                }
                sb.AppendLine();
            }
            sb.AppendLine("# Terminals\n");
            foreach (var x in _terminalActions)
            {
                string o = "";
                foreach (var y in x.Args)
                    o += y + ", ";
                if (o.Length > 0)
                    o = o.Substring(0, o.Length - 2);
                sb.AppendLine($"#### **{x.Name}({o})**");
                foreach (var y in x.Children)
                {
                    o = "";
                    foreach (var z in y.Args)
                        o += z + ", ";
                    if (o.Length > 0)
                        o = o.Substring(0, o.Length - 2);
                    sb.AppendLine($"* {y.Name}({o})");
                }
                sb.AppendLine();
            }
            sb.AppendLine("# Libraries\n");
            foreach (var x in _libDictionary)
            {
                sb.AppendLine($"#### **{x.Name}**");
                foreach (var y in x.Methods)
                {
                    string o = "";
                    foreach (var z in y.Tokens)
                        o += "**" + z + "**, ";
                    if (o.Length > 0)
                        o = o.Substring(0, o.Length - 2);
                    sb.AppendLine($"* .{y.Name}({o})");
                }
                sb.AppendLine();
            }

            Console.WriteLine(sb.ToString());
        }
#endif

        private static List<SpecialObject> _objects = new List<SpecialObject>()
        {
            new SpecialObject("xmlscript", new TokenType[] {TokenType.STR}, "ScriptPath"),
            new SpecialObject("subpart", new TokenType[] {TokenType.STR}, "SubpartName"),
            new SpecialObject("button", new TokenType[] {TokenType.STR, TokenType.STR}, "SubpartName", "DummyName"),
            new SpecialObject("emissive", new TokenType[] {TokenType.STR}, "EmissiveMaterialID"),
            new SpecialObject("emitter", new TokenType[] {TokenType.STR}, "DummyName"),
            new SpecialObject("light", new TokenType[] {TokenType.STR, TokenType.FLOAT}, "DummyName", "Radius"),
        };
        
        private static List<SpecialAction> _specialActions = new List<SpecialAction>()
        {
            new SpecialAction("toolcore", new TokenType[0],
                new SpecialAction("functional", new TokenType[0]),
                new SpecialAction("powered", new TokenType[0]),
                new SpecialAction("enabled", new TokenType[0]),
                new SpecialAction("activated", new TokenType[0]),
                new SpecialAction("leftclick", new TokenType[0]),
                new SpecialAction("rightclick", new TokenType[0]),
                new SpecialAction("click", new TokenType[0]),
                new SpecialAction("firing", new TokenType[0]),
                new SpecialAction("hit", new TokenType[0]),
                new SpecialAction("rayhit", new TokenType[0])
            ),

            new SpecialAction("weaponcore", new TokenType[0],
                new SpecialAction("reloading", new TokenType[0]),
                new SpecialAction("firing", new TokenType[0]),
                new SpecialAction("tracking", new TokenType[0]),
                new SpecialAction("overheated", new TokenType[0]),
                new SpecialAction("turnon", new TokenType[0]),
                new SpecialAction("turnoff", new TokenType[0]),
                new SpecialAction("burstreload", new TokenType[0]),
                new SpecialAction("nomagstoload", new TokenType[0]),
                new SpecialAction("prefire", new TokenType[0]),
                new SpecialAction("emptyongameload", new TokenType[0]),
                new SpecialAction("stopfiring", new TokenType[0]),
                new SpecialAction("stoptracking", new TokenType[0]),
                new SpecialAction("lockdelay", new TokenType[0]),
                new SpecialAction("init", new TokenType[0]),
                new SpecialAction("homing", new TokenType[0]),
                new SpecialAction("targetaligned", new TokenType[0]),
                new SpecialAction("whileon", new TokenType[0]),
                new SpecialAction("targetranged100", new TokenType[0]),
                new SpecialAction("targetranged75", new TokenType[0]),
                new SpecialAction("targetranged50", new TokenType[0]),
                new SpecialAction("targetranged25", new TokenType[0])
            ),

            new SpecialAction("button", new TokenType[] { TokenType.KEWRD },
                new SpecialAction("pressed", new TokenType[] { TokenType.KEWRD }),
                new SpecialAction("hovering", new TokenType[] { TokenType.KEWRD })
            ),

            new SpecialAction("block", new TokenType[0],
                new SpecialAction("create", new TokenType[0]),
                new SpecialAction("built", new TokenType[0]),
                new SpecialAction("damaged", new TokenType[0]),
                new SpecialAction("working", new TokenType[0]),
                new SpecialAction("notworking", new TokenType[0])
            ),

            new SpecialAction("inventory", new TokenType[0],
                new SpecialAction("changed", new TokenType[] { TokenType.KEWRD })
            ),

            new SpecialAction("power", new TokenType[0],
                new SpecialAction("consumed", new TokenType[] { TokenType.KEWRD }),
                new SpecialAction("produced", new TokenType[] { TokenType.KEWRD })
            ),

            new SpecialAction("production", new TokenType[0],
                new SpecialAction("startproducing", new TokenType[0]),
                new SpecialAction("stopproducing", new TokenType[0])
            ),

            new SpecialAction("distance", new TokenType[] { TokenType.FLOAT },
                new SpecialAction("changed", new TokenType[] { TokenType.KEWRD }),
                new SpecialAction("arrive", new TokenType[0]),
                new SpecialAction("leave", new TokenType[0])
            ),

            new SpecialAction("shiptool", new TokenType[0],
                new SpecialAction("activated", new TokenType[] { TokenType.KEWRD })
            ),

            //special
            new SpecialAction("door", new TokenType[0],
                new SpecialAction("open", new TokenType[0]),
                new SpecialAction("close", new TokenType[0])
            ),

            new SpecialAction("cockpit", new TokenType[0],
                new SpecialAction("enter", new TokenType[0]),
                new SpecialAction("exit", new TokenType[0])
            ),

            new SpecialAction("landinggear", new TokenType[0],
                new SpecialAction("lock", new TokenType[0]),
                new SpecialAction("unlock", new TokenType[0]),
                new SpecialAction("readylock", new TokenType[0])
            ),
        };

        private static List<SpecialAction> _terminalActions = new List<SpecialAction>()
        {
            new SpecialAction("slider", new TokenType[] { TokenType.INT, TokenType.STR, TokenType.STR, TokenType.FLOAT, TokenType.FLOAT },
                new SpecialAction("changed", new TokenType[] { TokenType.KEWRD })
            ),

            // pos name tooltip
            new SpecialAction("button", new TokenType[] { TokenType.INT, TokenType.STR, TokenType.STR },
                new SpecialAction("pressed", new TokenType[0])
            ),

            // pos name tooltip
            new SpecialAction("onoffswitch", new TokenType[] { TokenType.INT, TokenType.STR, TokenType.STR },
                new SpecialAction("changed", new TokenType[] { TokenType.KEWRD })
            ),

            new SpecialAction("checkbox", new TokenType[] { TokenType.INT, TokenType.STR, TokenType.STR },
                new SpecialAction("changed", new TokenType[] { TokenType.KEWRD })
            ),

            new SpecialAction("color", new TokenType[] { TokenType.INT, TokenType.STR },
                new SpecialAction("changed", new TokenType[] { TokenType.KEWRD })
            ),
        };

        private static List<LibraryDictionary> _libDictionary = new List<LibraryDictionary>()
        {
            new LibraryDictionary("math",
                new MethodDictionary("sin", true, "Value"),
                new MethodDictionary("cos", true, "Value"),

                new MethodDictionary("abs", true, "Value"),

                new MethodDictionary("max", true, "a", "b"),
                new MethodDictionary("min", true, "a", "b"),

                new MethodDictionary("floor", true, "Value"),
                new MethodDictionary("ceiling", true, "Value"),
                new MethodDictionary("round", true, "Value"),

                new MethodDictionary("random", true),
                new MethodDictionary("randomrange", true, "minVal", "maxVal"),
                new MethodDictionary("createvector", true, "x", "y", "z")
            ),

            new LibraryDictionary("api",
                new MethodDictionary("log", false, "Value"),
                new MethodDictionary("startloop", false, "FunctionName", "LoopDelay", "LoopCount", "DelayTime"),
                new MethodDictionary("startloop", false, "FunctionName", "LoopDelay", "LoopCount"),
                new MethodDictionary("stoploop", false, "FunctionName"),
                new MethodDictionary("delayfunction", false, "FunctionName", "FunctionDelay"),

                new MethodDictionary("stopdelays", false),
                new MethodDictionary("assert", false, "a", "b"),

                new MethodDictionary("getlargegridmaxspeed", true),
                new MethodDictionary("getsmallgridmaxspeed", true),

                new MethodDictionary("getinputposition", true),
                new MethodDictionary("getinputrotation", true)
            ),

            new LibraryDictionary("block",
                new MethodDictionary("delay", false, "Value"),

                new MethodDictionary("translate", false, "PositionVector", "Time", "Lerp"),
                new MethodDictionary("scale", false, "ScaleVector", "Time", "Lerp"),
                new MethodDictionary("rotate", false, "AxisVector", "Angle", "Time", "Lerp"),
                new MethodDictionary("rotatearound", false, "AxisVector", "PivotVector", "Angle", "Time", "Lerp"),
                new MethodDictionary("spin", false, "Vector", "Speed", "Time"),
                new MethodDictionary("vibrate", false, "Scale", "Time"),
                new MethodDictionary("setresetpos", false),
                new MethodDictionary("resetpos", false),
                new MethodDictionary("resetrot", false),
                new MethodDictionary("reset", false),

                new MethodDictionary("pilottranslate", false, "PositionVector", "Time", "Lerp"),
                new MethodDictionary("pilotscale", false, "ScaleVector", "Time", "Lerp"),
                new MethodDictionary("pilotrotate", false, "AxisVector", "Angle", "Time", "Lerp"),
                new MethodDictionary("pilotrotatearound", false, "AxisVector", "PivotVector", "Angle", "Time", "Lerp"),
                new MethodDictionary("pilotspin", false, "Vector", "Speed", "Time"),
                new MethodDictionary("pilotvibrate", false, "Scale", "Time"),
                new MethodDictionary("pilotmovetoorigin", false, "Time", "Lerp"),
                new MethodDictionary("pilotreset", false),
                new MethodDictionary("pilotresetpos", false),
                new MethodDictionary("pilotsetresetpos", false),

                new MethodDictionary("currentthrustpercent", true),
                new MethodDictionary("productionitemmodel", true),

                new MethodDictionary("isoccupied", true),
                new MethodDictionary("isworking", true),
                new MethodDictionary("isfunctional", true),

                new MethodDictionary("getgasfilledratio", true),

                new MethodDictionary("isarmed", true),
                new MethodDictionary("iscountingdown", true),
                new MethodDictionary("detonationtime", true),

                //terrible ideas
                new MethodDictionary("poweron", false),
                new MethodDictionary("poweroff", false),

                new MethodDictionary("toggledoor", false),
                new MethodDictionary("closedoor", false),
                new MethodDictionary("opendoor", false),

                new MethodDictionary("togglelock", false),
                new MethodDictionary("lockoff", false),
                new MethodDictionary("lockon", false)

            ),

            new LibraryDictionary("grid",
                new MethodDictionary("isnpc", true),
                new MethodDictionary("getatmosphericdensity", true),
                new MethodDictionary("getplanetaltitude", true),
                new MethodDictionary("getplanetgroundaltitude", true),
                new MethodDictionary("getplanetmaxaltitude", true),
                new MethodDictionary("getspeed", true),
                new MethodDictionary("getnaturalgravity", true),

                new MethodDictionary("geth2fuel", true),
                new MethodDictionary("geto2fuel", true),
                new MethodDictionary("getfuel", true, "fuelString")
            ),

            new LibraryDictionary("weaponcore",
                new MethodDictionary("getactiveammo", true),
                new MethodDictionary("getheatlevel", true),
                new MethodDictionary("getshotsfired", true),
                new MethodDictionary("isshooting", true)
            ),

             new LibraryDictionary("xmlscript",
                new MethodDictionary("run", false, "AnimationId"),
                new MethodDictionary("stop", false),
                new MethodDictionary("reset", false)
            ),

            new LibraryDictionary("subpart",
                new MethodDictionary("delay", false, "Value"),

                new MethodDictionary("setvisible", false, "bool"),
                new MethodDictionary("setmodel", false, "modelFilePath"),
                new MethodDictionary("setemissive", false, "materialName", "r", "g", "b", "a"),

                new MethodDictionary("translate", false, "PositionVector", "Time", "Lerp"),
                new MethodDictionary("translaterelative", false, "PositionVector", "Time", "Lerp"),
                new MethodDictionary("scale", false, "ScaleVector", "Time", "Lerp"),
                new MethodDictionary("rotate", false, "AxisVector", "Angle", "Time", "Lerp"),
                new MethodDictionary("rotaterelative", false, "AxisVector", "Angle", "Time", "Lerp"),
                new MethodDictionary("rotatearound", false, "AxisVector", "PivotVector", "Angle", "Time", "Lerp"),
                new MethodDictionary("spin", false, "Vector", "Speed", "Time"),
                new MethodDictionary("vibrate", false, "Scale", "Time"),
                new MethodDictionary("movetoorigin", false, "Time", "Lerp"),
                new MethodDictionary("setresetpos", false),
                new MethodDictionary("resetpos", false),
                new MethodDictionary("resetrot", false),
                new MethodDictionary("reset", false)
            ),

            new LibraryDictionary("button",
                new MethodDictionary("delay", false, "Value"),

                new MethodDictionary("enabled", false, "bool"),
                new MethodDictionary("interactable", false, "bool"),

                new MethodDictionary("setvisible", false, "bool"),
                new MethodDictionary("setmodel", false, "modelFilePath"),
                new MethodDictionary("setemissive", false, "materialName", "r", "g", "b", "a"),

                new MethodDictionary("translate", false, "PositionVector", "Time", "Lerp"),
                new MethodDictionary("translaterelative", false, "PositionVector", "Time", "Lerp"),
                new MethodDictionary("scale", false, "ScaleVector", "Time", "Lerp"),
                new MethodDictionary("rotate", false, "AxisVector", "Angle", "Time", "Lerp"),
                new MethodDictionary("rotatearound", false, "AxisVector", "PivotVector", "Angle", "Time", "Lerp"),
                new MethodDictionary("spin", false, "Vector", "Speed", "Time"),
                new MethodDictionary("vibrate", false, "Scale", "Time"),
                new MethodDictionary("movetoorigin", false, "Time", "Lerp"),
                new MethodDictionary("setresetpos", false),
                new MethodDictionary("resetpos", false),
                new MethodDictionary("resetrot", false),
                new MethodDictionary("reset", false)
            ),

            new LibraryDictionary("emitter",
                new MethodDictionary("delay", false, "Value"),

                new MethodDictionary("playparticle", false, "particleName", "scale", "life", "vectorScale", "r", "g", "b"),
                new MethodDictionary("playparticle", false, "particleName", "scale", "life", "vectorScale"),
                new MethodDictionary("playparticle", false, "particleName", "scale", "life"),

                new MethodDictionary("stopparticle", false),
                new MethodDictionary("playsound", false, "soundID"),
                new MethodDictionary("stopsound", false)
            ),

            new LibraryDictionary("emissive",
                new MethodDictionary("delay", false, "Value"),

                new MethodDictionary("setcolor", false, "r", "g", "b", "brightness", "setAllSubpartColors"),
                new MethodDictionary("setsubpartcolor", false, "actualSubpartName", "r", "g", "b", "brightness"),

                new MethodDictionary("tocolor", false, "r", "g", "b", "brightness", "setAllSubpartColors", "time", "lerp"),
                new MethodDictionary("subparttocolor", false, "actualSubpartName", "r", "g", "b", "brightness", "time", "lerp")
            ),

            new LibraryDictionary("light",
                new MethodDictionary("delay", false, "Value"),

                new MethodDictionary("setcolor", false, "r", "g", "b"),
                new MethodDictionary("togglelight", false),
                new MethodDictionary("lightoff", false),
                new MethodDictionary("lighton", false)
            ),
        };

        public static bool IsObject(Entity ent, out string error)
        {
            string value = ent.Type.Value.ToString().ToLower();
            int count = _objects.Where(e => value.Equals(e.Name)).Count();

            foreach(var x in _objects)
            {
                if (value.Equals(x.Name))
                {
                    count--;

                    if (ent.Args.Length != x.Args.Length)
                    {
                        if (count == 0)
                        {
                            error = $"{ent.Name.Value} has a different amount of args, expected {x.Args.Length} got {ent.Args.Length}";
                            return false;
                        } 
                        else
                        {
                            continue;
                        }
                    }

                    for(int i = 0; i < x.Args.Length; i++)
                    {
                        if (ent.Args[i].Type != x.Args[i])
                        {
                            error = $"{ent.Name.Value} has a different type at {i + 1}, expected {x.Args[i]} got {ent.Args[i].Type}";
                            return false;
                        }
                    }

                    error = "";
                    return true;
                }
            }
            error = $"Unable to find object by name of {ent.Type.Value}";
            return false;
        }

        public static bool IsAction(ScriptAction node, bool action, out string error)
        {
            foreach (var x in (action ? _specialActions : _terminalActions))
            {
                if (node.TokenName == x.Name)
                {
                    if (node.Paramaters.Length != x.Args.Length)
                    {
                        error = $"{node.TokenName} has a different amount of args, expected {x.Args.Length} got {node.Paramaters.Length}";
                        return false;
                    }

                    for (int i = 0; i < x.Args.Length; i++)
                    {
                        if (node.Paramaters[i].Type != x.Args[i])
                        {
                            error = $"{node.TokenName} has a different type at {i + 1}, expected {x.Args[i]} got {node.Paramaters[i].Type}";
                            return false;
                        }
                    }

                    foreach (var found in node.Funcs)
                    {
                        error = $"Cannot find function named {found.TokenName}";
                        bool varFound = false;
                        foreach (var expected in x.Children)
                        {
                            if (found.TokenName == expected.Name)
                            {
                                varFound = true;
                                if (found.Paramaters.Length != expected.Args.Length)
                                {
                                    error = $"{found.TokenName} has a different amount of args, expected {expected.Args.Length} got {found.Paramaters.Length}";
                                    return false;
                                }

                                for (int i = 0; i < expected.Args.Length; i++)
                                {
                                    if (found.Paramaters[i].Type != expected.Args[i])
                                    {
                                        error = $"{found.TokenName} has a different type at {i + 1}, expected {expected.Args[i]} got {found.Paramaters[i].Type}";
                                        return false;
                                    }
                                }

                                continue;
                            }
                        }
                        if (!varFound)
                        {
                            return false;
                        }
                    }
                    error = "";
                    return true;
                }
            }
            error = $"Unable to find action by name of {node.TokenName}";
            return false;
        }

        public static MethodDictionary? IsMethod(string context, string method, int tokens, out bool match)
        {
            MethodDictionary? result = null;
            foreach (var o in _libDictionary)
            {
                if (o.Name.Equals(context.ToLower()))
                {
                    foreach (var m in o.Methods)
                    {
                        if (m.Name.Equals(method.ToLower()))
                        {
                            result = m;
                            if (m.TokenCount == tokens)
                            {
                                match = true;
                                return m;
                            }
                        }
                    }
                }
            }
            match = false;
            return result;
        }


        private struct LibraryDictionary
        {
            public LibraryDictionary(string name, params MethodDictionary[] methods)
            {
                Name = name;
                Methods = methods;
            }
            public string Name;
            public MethodDictionary[] Methods;
        }


    }

    internal struct SpecialObject
    {
        public SpecialObject(string name, TokenType[] args, params string[] Names)
        {
            this.Name = name;
            this.Args = args;
            this.Names = Names;
        }
        public string Name;
        public TokenType[] Args;
        public string[] Names;
    }

    internal struct SpecialAction
    {
        public SpecialAction(string name, TokenType[] args, params SpecialAction[] children)
        {
            this.Name = name;
            this.Args = args;
            this.Children = children;
        }
        public string Name;
        public TokenType[] Args;
        public SpecialAction[] Children;
    }

    internal struct MethodDictionary
    {
        public MethodDictionary(string name, bool returnable, params string[] tokens)
        {
            Name = name;
            Returnable = returnable;
            TokenCount = tokens.Length;
            Tokens = tokens;
        }
        public string Name;
        public string[] Tokens;
        public int TokenCount;
        public bool Returnable;
    }

}
