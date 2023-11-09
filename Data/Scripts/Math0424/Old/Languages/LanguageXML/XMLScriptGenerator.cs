using AnimationEngine.Language;
using AnimationEngine.Utility;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using VRageMath;
using static VRage.Game.MyObjectBuilder_Checkpoint;

namespace AnimationEngine.LanguageXML
{
    internal class XMLScriptGenerator
    {
        List<sSubpartFrame> subpartFrames = new List<sSubpartFrame>();

        public XMLScriptGenerator(ModItem mod, string path)
        {
            if (MyAPIGateway.Utilities.FileExistsInModLocation(path, mod))
            {
                var RawScript = MyAPIGateway.Utilities.ReadFileInModLocation(path, mod).ReadToEnd();
                XMLScript script = MyAPIGateway.Utilities.SerializeFromXML<XMLScript>(RawScript);

                foreach(var anim in script.Animations)
                {
                    foreach(var sub in anim.Subparts)
                    {
                        List<sXMLDelay> delays = new List<sXMLDelay>();
                        int largestTime = 0;
                        var keyframes = sub.Keyframes;
                        for (int currentKeyframe = 0; currentKeyframe < keyframes.Length; currentKeyframe++)
                        {
                            List<sXMLMovement> movements = new List<sXMLMovement>();

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
                                                    movements.Add(new sXMLMovement()
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
                                                    movements.Add(new sXMLMovement()
                                                    {
                                                        type = "relativetranslate",
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
                                                    movements.Add(new sXMLMovement()
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
                                                    movements.Add(new sXMLMovement()
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

                            delays.Add(new sXMLDelay()
                            {
                                delay = currentFrame.frame,
                                movements = movements.ToArray(),
                            });
                        }

                        subpartFrames.Add(new sSubpartFrame()
                        {
                            timeTaken = largestTime,
                            delays = delays.ToArray(),
                            name = sub.empty.Substring(8)
                        });
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
        public string empty; //formatted parent/parent/name
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

}
