using AnimationEngine.Utility;
using System.Xml.Serialization;

namespace AnimationEngine.LanguageXML
{
    internal class XMLScriptCreator
    {
        //public XMLScriptCreator(string path)
        //{
        //    if (File.Exists(path))
        //    {
        //        long start = DateTime.Now.Ticks;
        //        Log($"Compiling XML script {Path.GetFileName(path)} for {{mod.Name}}");
        //        
        //        XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLScript));
        //        using (var file = File.Open(path, FileMode.Open))
        //            xmlSerializer.Deserialize(file);
        //
        //        Log($"Compiled script ({(DateTime.Now.Ticks - start) / TimeSpan.TicksPerMillisecond}ms)");
        //    }
        //    else
        //    {
        //        throw new Exception($"Script file not found! ({path} {{mod.Name}})");
        //    }
        //}

        public void Log(object msg)
        {
            Utils.LogToFile(msg);
        }
    }

    [XmlRoot("Animations")]
    public struct XMLScript
    {
        public string ver;
        public XMLAnimation[] Animations;
    }

    [XmlType("Animation")]
    public struct XMLAnimation
    {
        public string id;
        public string subtypeId;
        public XMLTrigers Triggers;

        public XMLSubpart[] Subparts;
    }

    [XmlRoot("Triggers")]
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
        public string empty;
        public XMLKeyFrame[] Keyframes;
    }

    [XmlType("Keyfrane")]
    public struct XMLKeyFrame
    {
        public int time;
        [XmlArray]
        public XMLAnim[] Anims;
        [XmlArray]
        public XMLFunction[] Functions;
    }

    [XmlType("Anim")]
    public struct XMLAnim
    {
        public string rotation;
        public string lerp;
        public string easing;
    }

    [XmlType("Trigger")]
    public struct XMLEvent
    {
        public string type;
        public float distance;
        public string empty;
    }

    [XmlType("Function")]
    public struct XMLFunction
    {
        public string rgb;
        public string type;
        public string empty;
        public string subtypeid;
        public string material;
        public float brightness;
    }

    [XmlType("State")]
    public struct XMLState
    {
        public string type;
        [XmlElement("bool")]
        public bool value;
        public bool loop;
    }

}
