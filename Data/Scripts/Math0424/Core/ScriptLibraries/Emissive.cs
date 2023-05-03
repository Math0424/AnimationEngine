using AnimationEngine.Language;
using AnimationEngine.Utility;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRageMath;

namespace AnimationEngine.Core
{
    internal class Emissive : ScriptLib, Initializable
    {
        private string materialID;
        IMyEntity ent;

        Vector4 CurrentColor;
        Vector4 NewColor;
        
        string subpartName;
        
        int timeRemaining;
        int timeStart;
        
        bool transitionColor;
        bool transitionAllColors;

        ShortHandLerp lerp;

        public Emissive(string materialID)
        {
            this.materialID = materialID;
            AddMethod("setcolor", SetColor);
            AddMethod("setsubpartcolor", SetSubpartColor);

            AddMethod("tocolor", transitionBlockColor);
            AddMethod("subparttocolor", transitionSubpartColor);
        }

        public override void Tick(int time) 
        { 
            if (!transitionColor)
                return;

            LerpType lerp2;
            EaseType ease;
            lerp.ShortToLong(out lerp2, out ease);
            var newColor = (Vector4)lerp2.Lerp(ease, NewColor, CurrentColor, ((float)timeRemaining / timeStart));
            timeRemaining -= time;

            if (timeRemaining <= 0)
                transitionColor = false;

            if (subpartName != null)
            {
                MyEntitySubpart x = ent.GetSubpart(subpartName);
                if (x != null)
                    x.SetEmissiveParts(materialID, newColor, newColor.Z);
            } 
            else
            {
                ent.SetEmissiveParts(materialID, newColor, newColor.Z);
                if (transitionAllColors)
                    ent.SetEmissivePartsForSubparts(materialID, newColor, newColor.Z);
            }
        }

        public void Init(IMyEntity ent)
        {
            this.ent = ent;
        }

        //"actualSubpartName", "r", "g", "b", "brightness" "time" "lerp"
        private SVariable transitionSubpartColor(SVariable[] arr)
        {
            subpartName = arr[0].ToString();
            CurrentColor = NewColor;
            NewColor = new Vector4(arr[1].AsFloat() / 255, arr[2].AsFloat() / 255, arr[3].AsFloat() / 255, arr[4].AsFloat());
            timeRemaining = arr[5].AsInt();
            lerp = (ShortHandLerp)arr[6].AsInt();
            timeStart = timeRemaining;

            transitionColor = true;
            return null;
        }

        //"r", "g", "b", "brightness", "setAllSubpartColors" "time" "lerp"
        private SVariable transitionBlockColor(SVariable[] arr)
        {
            subpartName = null;
            CurrentColor = NewColor;
            NewColor = new Vector4(arr[0].AsFloat() / 255, arr[1].AsFloat() / 255, arr[2].AsFloat() / 255, arr[3].AsFloat());
            transitionAllColors = arr[4].AsBool();
            timeRemaining = arr[5].AsInt();
            lerp = (ShortHandLerp)arr[6].AsInt();
            timeStart = timeRemaining;

            transitionColor = true;
            return null;
        }

        private SVariable SetColor(SVariable[] arr)
        {
            transitionColor = false;
            CurrentColor = new Vector4(arr[0].AsFloat() / 255, arr[1].AsFloat() / 255, arr[2].AsFloat() / 255, arr[3].AsFloat()); 
            ent.SetEmissiveParts(materialID, CurrentColor, CurrentColor.Z);
            if (arr.Length == 5 && arr[4].AsBool())
            {
                ent.SetEmissivePartsForSubparts(materialID, CurrentColor, CurrentColor.Z);
            }
            return null;
            //TODO, broadcast to clients
        }

        private SVariable SetSubpartColor(SVariable[] arr)
        {
            transitionColor = false;
            MyEntitySubpart x = ent.GetSubpart(arr[0].ToString());
            if (x != null)
            {
                CurrentColor = new Vector4(arr[1].AsFloat() / 255, arr[2].AsFloat() / 255, arr[3].AsFloat() / 255, arr[4].AsFloat());
                x.SetEmissiveParts(materialID, CurrentColor, CurrentColor.Z);
            }
            return null;
            //TODO, broadcast to clients
        }

    }
}
