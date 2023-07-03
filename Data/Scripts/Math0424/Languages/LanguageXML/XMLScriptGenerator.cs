using AnimationEngine.Core;
using AnimationEngine.Language;
using AnimationEngine.LanguageV1;
using AnimationEngine.LogicV1;
using AnimationEngine.Utility;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using VRageMath;
using static VRage.Game.MyObjectBuilder_Checkpoint;

namespace AnimationEngine.LanguageXML
{
    internal class XMLScriptGenerator
    {
        ModItem mod;

        public XMLScriptGenerator(ModItem mod, string path)
        {
            this.mod = mod;
            if (MyAPIGateway.Utilities.FileExistsInModLocation(path, mod))
            {
                var RawScript = MyAPIGateway.Utilities.ReadFileInModLocation(path, mod).ReadToEnd();
                long start = DateTime.Now.Ticks;
                Log($"Reading script {Path.GetFileName(path)} for {mod.Name}");
                Log($"|  Loaded {RawScript.Length * 8} bytes");
                XMLScript script = MyAPIGateway.Utilities.SerializeFromXML<XMLScript>(RawScript);
                Log($"|  Running Generator XML version '{script.ver}'");

                foreach (var x in script.Animations)
                    GenerateScript(x);

                Log($"Compiled script(s) ({(DateTime.Now.Ticks - start) / TimeSpan.TicksPerMillisecond}ms)");
            }
            else
            {
                throw new Exception($"Script file not found! ({path} {mod.Name})");
            }
        }

        List<V1ScriptAction> actions;
        private V1ScriptAction GetOrAddActionType(string action, params object[] args)
        {
            foreach(var x in actions)
            {
                if (x.Name.Value.ToString().Equals(action.ToLower()))
                {
                    return x;
                }
            }
            var act = new V1ScriptAction();
            act.ID = actions.Count;
            act.Name = new Token(TokenType.KEWRD, action.ToLower(), 0, 0);
            act.Funcs = new V1Function[0];
            actions.Add(act);
            return act;
        }

        private void GenerateScript(XMLAnimation animation)
        {
            Log($"|  Generating script for {animation.subtypeId}");
            List<Subpart> subparts = new List<Subpart>();

            actions = new List<V1ScriptAction>();
            List<ObjectDef> objects = new List<ObjectDef>();
            Dictionary<string, Caller[]> callingArray = new Dictionary<string, Caller[]>();

            if (animation.Triggers.EventTriggers != null)
            {
                foreach (var x in animation.Triggers.EventTriggers)
                {
                    V1ScriptAction act;
                    switch (x.type.ToLower())
                    {
                        case "pressed":
                        case "pressedon":
                        case "pressedoff":
                            act = GetOrAddActionType("buttonaction");
                            break;
                        case "leave":
                        case "arrive":
                            act = GetOrAddActionType("distanceaction");
                            break;
                        case "startproducing":
                        case "stopproducing":
                            act = GetOrAddActionType("productionaction");
                            break;
                        case "working":
                        case "notworking":
                            act = GetOrAddActionType("blockaction");
                            break;
                        case "readylock":
                        case "unlock":
                        case "lock":
                            act = GetOrAddActionType("landinggearaction");
                            break;
                        case "exit":
                        case "enter":
                            act = GetOrAddActionType("cockpitaction");
                            break;
                        case "close":
                        case "open":
                            act = GetOrAddActionType("dooraction");
                            break;
                        case "build":
                        case "create":
                            act = GetOrAddActionType("blockaction");
                            break;
                        default:
                            throw new Exception($"cannot parse state trigger '{x.type}'");
                    }
                    callingArray.Add($"{act.ID}_{x.type.ToLower()}", null);
                    var funcs = new List<V1Function>(act.Funcs);
                    funcs.Add(new V1Function()
                    {
                        Name = new Token(TokenType.KEWRD, x.type.ToLower(), 0, 0),
                        Paramaters = x.distance == 0 ? null : new Token[] { new Token(TokenType.FLOAT, x.distance, 0, 0) },
                    });
                    for(int i = 0; i < actions.Count; i++)
                    {
                        if (actions[i].ID == act.ID)
                        {
                            var y = actions[i];
                            y.Funcs = funcs.ToArray();
                            y.Paramaters = x.empty == null ? null : new Token[] { new Token(TokenType.KEWRD, x.empty, 0, 0) };
                            actions[i] = y;
                        }
                    }
                }
            }

            if (animation.Triggers.StateTriggers != null)
            {
                foreach (var x in animation.Triggers.StateTriggers)
                {
                    List<V1Function> funcs = new List<V1Function>();
                    V1ScriptAction act;
                    switch (x.type.ToLower())
                    {
                        case "working":
                            act = GetOrAddActionType("blockaction");
                            if (x.loop)
                            {
                                funcs.Add(new V1Function() { Name = new Token(TokenType.KEWRD, "workingloop", 0, 0) });
                                callingArray.Add($"{act.ID}_workingloop", null);
                            }
                            break;
                        case "producing":
                            act = GetOrAddActionType("productionaction");
                            if (x.loop)
                            {
                                funcs.Add(new V1Function() { Name = new Token(TokenType.KEWRD, "producingloop", 0, 0) });
                                callingArray.Add($"{act.ID}_producingloop", null);
                            }
                            break;
                        default:
                            throw new Exception($"cannot parse state trigger '{x.type}'");
                    }
                    funcs.Union(act.Funcs);
                    for (int i = 0; i < actions.Count; i++)
                    {
                        if (actions[i].ID == act.ID)
                        {
                            var y = actions[i];
                            y.Funcs = funcs.ToArray();
                            actions[i] = y;
                        }
                    }
                }
            }
            Log($"|  |  Loaded {actions.Count} action triggers");

            if (actions.Count == 0)
                throw new Exception($"Animation has no action types?");

            if (animation.Subparts == null)
                throw new Exception($"Animation has no Animation?");

            int animationLength = 0;
            List<Caller> method = new List<Caller>();
            Log($"|  |  Loading {animation.Subparts.Length} subparts");
            foreach (var x in animation.Subparts)
            {
                if (x.Keyframes == null)
                    throw new Exception($"Subpart '{x.empty.Substring(8)}' has no Animation?");

                Log($"|  |  |  Reading '{x.empty.Substring(8)}' {x.Keyframes.Length} animations");
                subparts.Add(new Subpart(x.empty.Substring(8), x.empty.Substring(8), null));

                objects.Add(new ObjectDef()
                {
                    Name = x.empty.Substring(8),
                    Type = "subpart",
                });

                for (int m = 0; m < x.Keyframes.Length; m++)
                {
                    var y = x.Keyframes[m];
                    if (y.Anims == null)
                        throw new Exception($"Subpart '{x.empty.Substring(8)}' frame {y.frame} has no data?");

                    foreach(var z in y.Anims)
                    {
                        if (y.frame > animationLength)
                            animationLength = y.frame;

                        int lerp = 16;
                        foreach (var a in Enum.GetValues(typeof(LerpType)))
                            if (a.ToString().ToLower().Equals((z.lerp ?? "linear").ToLower()))
                                lerp = (int)a;

                        int ease = 0;
                        foreach (var a in Enum.GetValues(typeof(EaseType)))
                            if (a.ToString().ToLower().Equals((z.easing ?? "in").ToLower()))
                                ease = (int)a;

                        var arr = x.Keyframes;
                        if (z.rotation != null) // 1
                        {
                            Vector3 to;
                            int changedFrame = GetNextFrameChange(ref arr, m, 1, out to);
                            if (changedFrame == -1)
                                continue;
                            Vector3 from = StringToVector(z.rotation);

                            QuaternionD diff = from.EulerToQuat() * QuaternionD.Inverse(to.EulerToQuat());
                            Vector3D one;
                            double angle;
                            diff.GetAxisAngle(out one, out angle);

                            int timetaken = x.Keyframes[changedFrame].frame - y.frame;
                            method.Add(new Caller()
                            {
                                Args = new Argument[] {
                                    new Argument()
                                    {
                                        Delay = y.frame,
                                        Name = "rotate",
                                        Value = new SVariable[] {
                                            new SVariableVector(one),
                                            new SVariableFloat((float)(angle * (180.0/Math.PI))),
                                            new SVariableInt(timetaken),
                                            new SVariableInt(ease | lerp),
                                        }
                                    }
                                },
                                Object = x.empty.Substring(8).ToLower()
                            });
                        }
                        else if (z.location != null || z.scale != null) // 2 : 0
                        {
                            Vector3 change;
                            int changedFrame = GetNextFrameChange(ref arr, m, z.location == null ? 0 : 2, out change);
                            if (changedFrame == -1)
                                continue;
                            change -= StringToVector(z.location);
                            int timetaken = x.Keyframes[changedFrame].frame - y.frame;
                            method.Add(new Caller()
                            {
                                Args = new Argument[] {
                                    new Argument()
                                    {
                                        Delay = y.frame,
                                        Name = z.location == null ? "scale" : "translate",
                                        Value = new SVariable[] {
                                            new SVariableVector(change),
                                            new SVariableInt(timetaken),
                                            new SVariableInt(ease | lerp),
                                        }
                                    }
                                },
                                Object = x.empty.Substring(8).ToLower()
                            });
                        }
                    }
                }
            }
            Log($"|  |  Generated {method.Count} subpart animations");

            string methodCallName = new Random().Next().ToString();
            for(int i = 0; i < callingArray.Count; i++)
            {
                var meth = callingArray.ElementAt(i);
                var x = new Caller[]
                {
                    new Caller()
                    {
                        FuncCall = true,
                        Object = methodCallName,
                    }
                };
                callingArray[meth.Key] = x;
            }
            callingArray[methodCallName] = method.ToArray();


            //work on adding the looping variables
            if (animation.Triggers.StateTriggers != null)
            {
                foreach (var x in animation.Triggers.StateTriggers)
                {
                    if (x.loop == true)
                    {
                        for (int i = 0; i < actions.Count; i++)
                        {
                            if (actions[i].Name.Value.Equals("blockaction") || actions[i].Name.Value.Equals("productionaction"))
                            {
                                var y = actions[i];
                                for(int j = 0; j < y.Funcs.Length; j++)
                                {
                                    if (y.Funcs[j].Name.Value.Equals("workingloop") || y.Funcs[j].Name.Value.Equals("producingloop"))
                                        y.Funcs[j].Paramaters = new Token[] { new Token(TokenType.INT, animationLength, 0, 0) };
                                }
                                actions[i] = y;
                            }
                        }
                    }
                }
            }

            ScriptRunner runner = new ScriptV1Runner(mod.Name, objects, actions, callingArray);
            Log($"|  Registering block");
            AnimationEngine.AddToRegisteredScripts(animation.subtypeId, subparts.ToArray(), runner);
            Log($"|  |  Registered script to '{animation.subtypeId}'");
        }

        private int GetNextFrameChange(ref XMLKeyFrame[] arr, int start, int type, out Vector3 change)
        {
            change = Vector3.Zero;
            start++;
            while (start < arr.Length)
            {
                XMLKeyFrame key = arr[start];
                foreach(var x in key.Anims)
                {
                    switch(type)
                    {
                        case 0: break;
                        case 1:
                            if (x.rotation != null)
                            {
                                change = StringToVector(x.rotation);
                                return start;
                            }
                            break;
                        case 2:
                            if (x.location != null)
                            {
                                change = StringToVector(x.location);
                                return start;
                            }
                            break;
                    }
                }
                start++;
            }
            return -1;
        }

        //assumed format [x,y,z]
        private Vector3 StringToVector(string str)
        {
            string[] arr = str.Substring(1, str.Length - 2).Split(',');
            return new Vector3(float.Parse(arr[0]), float.Parse(arr[1]), float.Parse(arr[2]));
        }

        public void Log(object msg)
        {
            Utils.LogToFile(msg);
        }


        [XmlRoot("Animations")]
        public struct XMLScript
        {
            [XmlAttribute]
            public string ver;
            [XmlElement("Animation")]
            public XMLAnimation[] Animations;
        }

        [XmlType("Animation")]
        public struct XMLAnimation
        {
            [XmlAttribute]
            public string id;
            [XmlAttribute]
            public string subtypeId;
            [XmlElement("Triggers")]
            public XMLTrigers Triggers;
            [XmlArray("Subparts")]
            public XMLSubpart[] Subparts;
        }

        [XmlType("Triggers")]
        public struct XMLTrigers
        {
            [XmlElement("Event")]
            public XMLEvent[] EventTriggers;
            [XmlElement("State")]
            public XMLState[] StateTriggers;
        }

        [XmlType("Subpart")]
        public struct XMLSubpart
        {
            [XmlAttribute]
            public string empty;
            [XmlArray("Keyframes")]
            public XMLKeyFrame[] Keyframes;
        }

        [XmlType("Keyframe")]
        public struct XMLKeyFrame
        {
            [XmlAttribute]
            public int frame;
            [XmlElement("Anim")]
            public XMLAnim[] Anims;
            [XmlElement("Function")]
            public XMLFunction[] Functions;
        }

        [XmlType("Anim")]
        public struct XMLAnim
        {
            [XmlAttribute]
            public string scale;
            [XmlAttribute]
            public string location;
            [XmlAttribute]
            public string rotation;
            [XmlAttribute]
            public string lerp;
            [XmlAttribute]
            public string easing;
        }

        [XmlType("Trigger")]
        public struct XMLEvent
        {
            [XmlAttribute]
            public string type;
            [XmlAttribute]
            public float distance;
            [XmlAttribute]
            public string empty;
        }

        [XmlType("Function")]
        public struct XMLFunction
        {
            [XmlAttribute]
            public string rgb;
            [XmlAttribute]
            public string type;
            [XmlAttribute]
            public string empty;
            [XmlAttribute]
            public string subtypeid;
            [XmlAttribute]
            public string material;
            [XmlAttribute]
            public float brightness;
        }

        [XmlType("State")]
        public struct XMLState
        {
            [XmlAttribute]
            public string type;
            [XmlAttribute("bool")]
            public bool value;
            [XmlAttribute]
            public bool loop;
        }

    }


}
