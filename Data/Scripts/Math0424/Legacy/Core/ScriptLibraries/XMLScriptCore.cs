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
        List<sDelay> delays = new List<sDelay>();
        List<sSubpartFrame> subpartFrames = new List<sSubpartFrame>();
        List<string> subpartNames = new List<string>();
        Action<string, MyTuple<string, SVariable[]>> args;

        public XMLScriptCore(ModItem mod, string path)
        {
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
                                            case "rotate":
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
                                            default:
                                                throw new Exception($"Unknown movement type {currentAnim.type}");
                                        }
                                    }
                                }
                            }

                            delays.Add(new sDelay()
                            {
                                delay = currentFrame.frame,
                                movements = movements.ToArray(),
                            });
                        }

                        subpartFrames.Add(new sSubpartFrame()
                        {
                            animationTriggerName = anim.id.ToLower(),
                            timeTaken = largestTime,
                            delays = delays.ToArray(),
                            subpartName = sub.empty.Substring(8)
                        });
                        subpartNames.Add(sub.empty.Substring(8));
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
                    if (x.type == type)
                    {
                        frame = key;
                        found = x;
                    }
                }
                start++;
            }
            frame = null;
            found = null;
        }

        public SVariable Run(SVariable[] animationName)
        {
            foreach(var x in subpartFrames)
                if (x.animationTriggerName == animationName[0].ToString().ToLower())
                    delays.AddArray(x.delays);
            return null;
        }

        public override void Tick(int time)
        {
            delays.ForEach(d => d.delay -= time);
            foreach (var d in delays)
                if (d.delay < 0)
                    foreach (var m in d.movements)
                        args?.Invoke(d.subpartName, new MyTuple<string, SVariable[]>(m.type, m.args));
            delays.RemoveAll(d => d.delay < 0);
        }

        public SVariable Stop(SVariable[] animationName)
        {
            delays.Clear();
            return null;
        }

        public SVariable Reset(SVariable[] animationName)
        {
            foreach(var x in subpartNames)
                args?.Invoke(x, new MyTuple<string, SVariable[]>("reset", null));
            return null;
        }

        //assumed format [x,y,z]
        private Vector3 StringToVector(string str)
        {
            string[] arr = str.Substring(1, str.Length - 2).Split(',');
            return new Vector3(float.Parse(arr[0]), float.Parse(arr[1]), float.Parse(arr[2]));
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
        public string subpartName;
        public int timeTaken;
        public sDelay[] delays;
    }

    public struct sDelay
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
