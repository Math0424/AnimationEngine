using AnimationEngine.Language;
using AnimationEngine.Utility;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using VRage;
using VRageMath;
using static VRage.Game.MyObjectBuilder_Checkpoint;

namespace AnimationEngine.Core
{
    internal class XMLScriptCore : ScriptLib
    {
        CoreScript core;
        List<sDelay> delays = new List<sDelay>();
        List<sSubpartFrame> subpartFrames = new List<sSubpartFrame>();
        public List<string> subpartNames = new List<string>();

        public XMLScriptCore(CoreScript core, ModItem mod, string path)
        {
            this.core = core;
            AddMethod("run", Run);
            AddMethod("stop", Stop);
            AddMethod("reset", Reset);

            if (MyAPIGateway.Utilities.FileExistsInModLocation(path, mod))
            {
                var RawScript = MyAPIGateway.Utilities.ReadFileInModLocation(path, mod).ReadToEnd();
                XMLScript script = MyAPIGateway.Utilities.SerializeFromXML<XMLScript>(RawScript);

                foreach(var anim in script.Animations)
                {
                    foreach(var sub in anim.Subparts)
                    {
                        string subpartName = sub.empty.Substring(8);
                        List<sDelay> delays = new List<sDelay>();
                        int largestTime = 0;
                        var keyframes = sub.Keyframes;
                        for (int currentKeyframe = 0; currentKeyframe < keyframes.Length; currentKeyframe++)
                        {
                            List<sMovement> movements = new List<sMovement>();

                            var currentFrame = keyframes[currentKeyframe];
                            if (currentFrame.frame > largestTime) largestTime = currentFrame.frame;

                            if (currentFrame.Anims != null)
                            {
                                foreach (var currentAnim in currentFrame.Anims)
                                {
                                    int lerp = 16;
                                    foreach (var a in Enum.GetValues(typeof(LerpType)))
                                        if (a.ToString().ToLower().Equals((currentAnim.lerp ?? "linear").ToLower()))
                                            lerp = (int)a;

                                    int ease = 0;
                                    foreach (var a in Enum.GetValues(typeof(EaseType)))
                                        if (a.ToString().ToLower().Equals((currentAnim.easing ?? "in").ToLower()))
                                            ease = (int)a;

                                    if (currentAnim.type != null)
                                    {
                                        string movementType = currentAnim.type.ToLower();
                                        XMLKeyFrame? nextframe;
                                        XMLAnim? nextAnim;

                                        switch (movementType)
                                        {
                                            case "relativerotation":
                                                GetNextFrame(ref keyframes, currentKeyframe, movementType, out nextframe, out nextAnim);
                                                if (nextframe.HasValue)
                                                {
                                                    Vector3D one;
                                                    double angle;
                                                    StringToQuaternion(nextAnim.Value.args).GetAxisAngle(out one, out angle);
                                                    int time = nextframe.Value.frame - currentFrame.frame;
                                                    movements.Add(new sMovement()
                                                    {
                                                        type = "rotaterelative",
                                                        args = new SVariable[] {
                                                            new SVariableVector(one),
                                                            new SVariableFloat((float)(angle * (180.0/Math.PI))),
                                                            new SVariableInt(time),
                                                            new SVariableInt(ease | lerp),
                                                        }
                                                    });
                                                }
                                                break;
                                            case "relativelocation":
                                                GetNextFrame(ref keyframes, currentKeyframe, movementType, out nextframe, out nextAnim);
                                                if (nextframe.HasValue)
                                                {
                                                    int time = nextframe.Value.frame - currentFrame.frame;
                                                    movements.Add(new sMovement()
                                                    {
                                                        type = "translaterelative",
                                                        args = new SVariable[] {
                                                            new SVariableVector(StringToVector(nextAnim.Value.args)),
                                                            new SVariableInt(time),
                                                            new SVariableInt(ease | lerp),
                                                        }
                                                    });
                                                }
                                                break;
                                            case "rotation":
                                                GetNextFrame(ref keyframes, currentKeyframe, movementType, out nextframe, out nextAnim);
                                                if (nextframe.HasValue)
                                                {
                                                    Vector3D one;
                                                    double angle;

                                                    Vector3 from = StringToVector(currentAnim.args);
                                                    Vector3 to = StringToVector(nextAnim.Value.args);
                                                    (from.EulerToQuat() * QuaternionD.Inverse(to.EulerToQuat())).GetAxisAngle(out one, out angle);

                                                    int time = nextframe.Value.frame - currentFrame.frame;
                                                    movements.Add(new sMovement()
                                                    {
                                                        type = "rotate",
                                                        args = new SVariable[] {
                                                            new SVariableVector(one),
                                                            new SVariableFloat((float)(angle * (180.0/Math.PI))),
                                                            new SVariableInt(time),
                                                            new SVariableInt(ease | lerp),
                                                        }
                                                    });
                                                }
                                                break;
                                            case "scale":
                                            case "location":
                                                GetNextFrame(ref keyframes, currentKeyframe, movementType, out nextframe, out nextAnim);
                                                if (nextframe.HasValue)
                                                {
                                                    int time = nextframe.Value.frame - currentFrame.frame;
                                                    movements.Add(new sMovement()
                                                    {
                                                        type = movementType == "scale" ? movementType : "translate",
                                                        args = new SVariable[] {
                                                            new SVariableVector(StringToVector(nextAnim.Value.args) - StringToVector(currentAnim.args)),
                                                            new SVariableInt(time),
                                                            new SVariableInt(ease | lerp),
                                                        }
                                                    });
                                                }
                                                break;
                                            case "reset":
                                                movements.Add(new sMovement()
                                                {
                                                    type = "reset",
                                                    args = null,
                                                });
                                                break;
                                            default:
                                                throw new Exception($"Unknown movement type {currentAnim.type}");
                                        }
                                    }
                                }
                            } 
                            else
                            {
                                throw new Exception($"{currentFrame} has no animation data in the XML file");
                            }

                            delays.Add(new sDelay()
                            {
                                delay = currentFrame.frame,
                                movements = movements.ToArray(),
                                subpartName = subpartName,
                            });
                        }

                        subpartFrames.Add(new sSubpartFrame()
                        {
                            animationTriggerName = anim.id.ToLower(),
                            timeTaken = largestTime,
                            delays = delays.ToArray(),
                        });
                        subpartNames.Add(subpartName);
                    }

                }
            }
            else
            {
                throw new Exception($"Script file not found! ({path} {mod.Name})");
            }
        }

        private void GetNextFrame(ref XMLKeyFrame[] arr, int start, string type, out XMLKeyFrame? frame, out XMLAnim? found)
        {
            start++;
            while (start < arr.Length)
            {
                XMLKeyFrame key = arr[start];
                foreach (var x in key.Anims)
                {
                    if (x.type.ToLower() == type)
                    {
                        frame = key;
                        found = x;
                        return;
                    }
                }
                start++;
            }
            frame = null;
            found = null;
        }

        private void Execute(string name, string arg, SVariable[] args)
        {
            if (core.SubpartArr.ContainsKey(name))
            {
                foreach (var x in core.SubpartArr[name])
                    x.Execute(arg, args);
            }
        }

        public SVariable Run(SVariable[] animationName)
        {
            foreach (var x in subpartFrames)
                if (x.animationTriggerName == animationName[0].ToString().ToLower())
                {
                    foreach(var y in x.delays)
                    {
                        delays.Add(new sDelay()
                        {
                            delay = y.delay,
                            movements = y.movements,
                            subpartName = y.subpartName
                        });
                    }
                }
            return null;
        }

        public override void Tick(int time)
        {
            delays.ForEach(d => d.delay -= time);
            foreach (var d in delays)
                if (d.delay < 0)
                    foreach (var m in d.movements)
                        Execute(d.subpartName, m.type, m.args);
            delays.RemoveAll(d => d.delay < 0);
        }

        public SVariable Stop(SVariable[] animationName)
        {
            delays.Clear();
            return null;
        }

        public SVariable Reset(SVariable[] animationName)
        {
            foreach (var x in subpartNames)
                Execute(x, "reset", null);
            return null;
        }

        // BLENDER -> SE [x z y] -> [x,y,z]
        private Vector3 StringToVector(string str)
        {
            string[] arr = str.Substring(1, str.Length - 2).Split(',');
            return new Vector3(float.Parse(arr[0]), float.Parse(arr[2]), float.Parse(arr[1]));
        }

        private QuaternionD StringToQuaternion(string str)
        {
            string[] arr = str.Substring(1, str.Length - 2).Split(',');
            return new QuaternionD(float.Parse(arr[0]), float.Parse(arr[1]), float.Parse(arr[2]), float.Parse(arr[3]));
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
        [XmlArray("Subparts")]
        public XMLSubpart[] Subparts;
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
    }

    [XmlType("Anim")]
    public struct XMLAnim
    {
        [XmlAttribute]
        public string type;
        [XmlAttribute]
        public string args;
        [XmlAttribute]
        public string lerp;
        [XmlAttribute]
        public string easing;
    }

    public struct sSubpartFrame
    {
        public string animationTriggerName;
        public int timeTaken;
        public sDelay[] delays;
    }

    public class sDelay
    {
        public string subpartName;
        public int delay;
        public sMovement[] movements;
    }

    public struct sMovement
    {
        public string type;
        public SVariable[] args;
    }

}
