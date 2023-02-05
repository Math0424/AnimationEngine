using AnimationEngine.Language;
using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace AnimationEngine.Language
{
    internal static class LanguageDictionary
    {

        private static List<SpecialAction> _specialActions = new List<SpecialAction>()
        {
            new SpecialAction("button", new TokenType[] { TokenType.KEWRD },
                new SpecialAction("pressed", new TokenType[] { TokenType.KEWRD })
            ),

            new SpecialAction("block", new TokenType[0],
                new SpecialAction("create", new TokenType[0]),
                new SpecialAction("build", new TokenType[0]),
                new SpecialAction("working", new TokenType[0]),
                new SpecialAction("notworking", new TokenType[0])
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

            new SpecialAction("thruster", new TokenType[0],
                new SpecialAction("thrust", new TokenType[] { TokenType.KEWRD })
            ),
        };

        private static List<SpecialAction> _terminalActions = new List<SpecialAction>()
        {
            new SpecialAction("slider", new TokenType[] { TokenType.INT, TokenType.STR, TokenType.FLOAT, TokenType.FLOAT },
                new SpecialAction("changed", new TokenType[] { TokenType.KEWRD })
            ),

            new SpecialAction("button", new TokenType[] { TokenType.INT, TokenType.STR },
                new SpecialAction("pressed", new TokenType[0])
            ),

            new SpecialAction("onoffswitch", new TokenType[] { TokenType.INT, TokenType.STR, TokenType.STR, TokenType.STR },
                new SpecialAction("changed", new TokenType[] { TokenType.KEWRD })
            ),

            new SpecialAction("checkbox", new TokenType[] { TokenType.INT, TokenType.STR },
                new SpecialAction("changed", new TokenType[] { TokenType.KEWRD })
            ),

            new SpecialAction("color", new TokenType[] { TokenType.INT, TokenType.STR },
                new SpecialAction("changed", new TokenType[] { TokenType.KEWRD })
            ),
        };

        private static List<ObjectDictionary> _libDictionary = new List<ObjectDictionary>()
        {
            new ObjectDictionary("math",
                new MethodDictionary("sin", true, "Value"),
                new MethodDictionary("cos", true, "Value"),

                new MethodDictionary("abs", true, "Value"),

                new MethodDictionary("max", true, "a", "b"),
                new MethodDictionary("min", true, "a", "b"),

                new MethodDictionary("floor", true, "Value"),
                new MethodDictionary("ceiling", true, "Value")
            ),

            new ObjectDictionary("block",
                new MethodDictionary("null", false, "Value")
            ),

            new ObjectDictionary("api",
                new MethodDictionary("log", false, "Value"),
                new MethodDictionary("startloop", false, "FunctionName", "LoopDelay", "LoopCount"),
                new MethodDictionary("stoploop", false, "FunctionName")
            ),

            new ObjectDictionary("subpart",
                new MethodDictionary("spin", false, "Vector", "Speed", "Time")
                //TODO
            ),

            new ObjectDictionary("block",
                new MethodDictionary("spin", false, "Vector", "Speed", "Time")
                //TODO
            ),
        };

        public static bool IsAction(ScriptAction node, bool action, out string error)
        {
            foreach(var x in (action ? _specialActions : _terminalActions))
            {
                if (node.TokenName == x.Name)
                {
                    if (node.Paramaters.Length != x.Args.Length)
                    {
                        error = $"{node.TokenName} has a different amount of args, expected {x.Args.Length} got {node.Paramaters.Length}";
                        return false;
                    }

                    for(int i = 0; i < x.Args.Length; i++)
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
            foreach(var o in _libDictionary)
            {
                if (o.Name.Equals(context.ToLower()))
                {
                    foreach(var m in o.Methods)
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


        private struct ObjectDictionary
        {
            public ObjectDictionary(string name, params MethodDictionary[] methods)
            {
                Name = name;
                Methods = methods;
            }
            public string Name;
            public MethodDictionary[] Methods;
        }


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
