using VRage.Game.Entity;
using VRage.ModAPI;
using VRageMath;

namespace AnimationEngine.Core
{
    internal class Emissive : Actionable, Initializable
    {
        private string materialID;
        private string parent;
        IMyEntity ent;

        public Emissive(string materialID, string parent)
        {
            this.materialID = materialID;
            this.parent = parent;
            Actions.Add("setcolor", SetColor);
            Actions.Add("setsubpartcolor", SetSubpartColor);
        }

        public string GetParent()
        {
            return parent;
        }

        public void Initalize(IMyEntity ent)
        {
            this.ent = ent;
        }

        private void SetColor(object[] arr)
        {
            if (arr.Length == 4)
            {
                ent.SetEmissiveParts(materialID, new Color((int)arr[0], (int)arr[1], (int)arr[2]), (float)arr[3]);
            }
            else if (arr.Length == 5 && (bool)arr[4])
            {
                ent.SetEmissiveParts(materialID, new Color((int)arr[0], (int)arr[1], (int)arr[2]), (float)arr[3]);
                ent.SetEmissivePartsForSubparts(materialID, new Color((int)arr[0], (int)arr[1], (int)arr[2]), (float)arr[3]);
            }
            //TODO, broadcast to clients
        }

        private void SetSubpartColor(object[] arr)
        {
            MyEntitySubpart x = ent.GetSubpart((string)arr[0]);
            if (x != null)
            {
                x.SetEmissiveParts(materialID, new Color((int)arr[1], (int)arr[2], (int)arr[3]), (float)arr[4]);
            }
            //TODO, broadcast to clients
        }

    }
}
