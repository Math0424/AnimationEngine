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

        public XMLScriptGenerator(ModItem mod, string path)
        {
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
                        //case "pressed":
                        //case "pressedon":
                        //case "pressedoff":
                        //    break;
                        //case "leave":
                        //case "arrive":
                        //    act = GetOrAddActionType("distanceaction");
                        //    var funcs = new List<V1Function>(act.Funcs);
                        //    funcs.Add(new V1Function())
                        //    break;
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
                        Name = new Token(TokenType.KEWRD, x.type.ToLower(), 0, 0)
                    });
                    for(int i = 0; i < actions.Count; i++)
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

            if (animation.Triggers.StateTriggers != null)
            {
                foreach (var x in animation.Triggers.StateTriggers)
                {
                    V1ScriptAction act;
                    switch (x.type.ToLower())
                    {
                        case "working":
                            act = GetOrAddActionType("blockaction");
                            break;
                        case "producing":
                            act = GetOrAddActionType("productionaction");
                            break;
                        default:
                            throw new Exception($"cannot parse state trigger '{x.type}'");
                    }
                    callingArray.Add($"{act.ID}_{x.type.ToLower()}", null);
                    var funcs = new List<V1Function>(act.Funcs);
                    funcs.Add(new V1Function()
                    {
                        Name = new Token(TokenType.KEWRD, x.type.ToLower(), 0, 0)
                    });
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
                subparts.Add(new Subpart(x.empty.Substring(8), null));

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
                    int start = y.frame;
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
                                lerp = (int)a;

                        if (z.scale != null) // 0
                        {
                            //method.Add(new Caller()
                            //{
                            //    Args = new Argument[] { 
                            //        new Argument()
                            //        {
                            //            Delay = y.frame,
                            //            Name = "scale",
                            //            Value = new SVariable[] { 
                            //                new SVariableVector(StringToVector(z.scale)),
                            //            }
                            //        }
                            //    },
                            //    Object = x.empty.Substring(8).ToLower()
                            //});
                        }
                        else if (z.rotation != null) // 1
                        {
                            // dunno yet
                        } 
                        else if (z.location != null) // 2
                        {
                            Vector3 change;
                            int nextChange;
                            var arr = x.Keyframes;
                            getNextFrameChange(ref arr, StringToVector(z.location), m + 1, 2, out nextChange, out change);
                            int timetaken = x.Keyframes[nextChange].frame - y.frame;
                            method.Add(new Caller()
                            {
                                Args = new Argument[] {
                                    new Argument()
                                    {
                                        Delay = y.frame,
                                        Name = "translate",
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

            ScriptRunner runner = new ScriptV1Runner(objects, actions, callingArray);
            Log($"|  Registering block");
            AnimationEngine.AddToRegisteredScripts(animation.subtypeId, subparts.ToArray(), runner);
            Log($"|  |  Registered script to '{animation.subtypeId}'");
        }

        private void getNextFrameChange(ref XMLKeyFrame[] arr, Vector3 prev, int start, int type, out int time, out Vector3 change)
        {
            int count = 0;
            Vector3 diff = Vector3.Zero;

            while (start < arr.Length)
            {
                XMLKeyFrame key = arr[start];
                count++;
                foreach(var x in key.Anims)
                {
                    switch(type)
                    {
                        case 0: break;
                        case 1:
                            if (x.rotation != null)
                            {
                                diff = prev - StringToVector(x.location);
                                goto bot;
                            }
                            break;
                        case 2:
                            if (x.location != null)
                            {
                                diff = prev - StringToVector(x.location);
                                goto bot;
                            }
                            break;
                    }
                }
            }
            bot:
            time = count;
            change = diff;
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
