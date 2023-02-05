using AnimationEngine.Language;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRageMath;

namespace AnimationEngine.Core
{
    internal class Emissive : ScriptLib, Initializable
    {
        private string materialID;
        IMyEntity ent;

        public Emissive(string materialID)
        {
            this.materialID = materialID;
            AddMethod("setcolor", SetColor);
            AddMethod("setsubpartcolor", SetSubpartColor);
        }

        public void Init(IMyEntity ent)
        {
            this.ent = ent;
        }

        private SVariable SetColor(SVariable[] arr)
        {
            if (arr.Length == 4)
            {
                ent.SetEmissiveParts(materialID, new Color(arr[0].AsInt(), arr[1].AsInt(), arr[2].AsInt()), arr[3].AsFloat());
            }
            else if (arr.Length == 5 && arr[4].AsBool())
            {
                ent.SetEmissiveParts(materialID, new Color(arr[0].AsInt(), arr[1].AsInt(), arr[2].AsInt()), arr[3].AsFloat());
                ent.SetEmissivePartsForSubparts(materialID, new Color(arr[0].AsInt(), arr[1].AsInt(), arr[2].AsInt()), arr[3].AsFloat());
            }
            return null;
            //TODO, broadcast to clients
        }

        private SVariable SetSubpartColor(SVariable[] arr)
        {
            MyEntitySubpart x = ent.GetSubpart(arr[0].ToString());
            if (x != null)
            {
                x.SetEmissiveParts(materialID, new Color(arr[1].AsInt(), arr[2].AsInt(), arr[3].AsInt()), arr[4].AsFloat());
            }
            return null;
            //TODO, broadcast to clients
        }

    }
}
