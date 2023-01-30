using AnimationEngine.LanguageV1;
using AnimationEngine.Utility;
using Sandbox.Definitions;
using Sandbox.Game.Lights;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using VRageRender.Lights;

namespace AnimationEngine.Core
{
    internal class Light : Actionable, Initializable
    {
        private string dummyName;
        private float radius;
        private IMyModelDummy dum;
        private IMyEntity parent;
        private MyLight light;

        public Light(string dummyName, float radius)
        {
            this.radius = radius;
            this.dummyName = dummyName;
            Actions.Add("setcolor", SetColor);
            Actions.Add("lighton", LightOn);
            Actions.Add("lightoff", LightOff);
            Actions.Add("togglelight", ToggleLight);
        }

        public string GetParent()
        {
            return dummyName;
        }

        public void Close(IMyEntity ent)
        {
            if (light != null)
                MyLights.RemoveLight(light);
            ent.OnClose -= Close;
            Utility.LogToFile($"Closed light attached to '{dummyName}'");
        }

        private bool FindDummy(IMyEntity ent)
        {
            Dictionary<string, IMyModelDummy> dummies = new Dictionary<string, IMyModelDummy>();
            ent.Model.GetDummies(dummies);
            foreach (var dum in dummies)
            {
                if (dum.Value.Name.Equals(dummyName))
                {
                    this.dum = dum.Value;
                    return true;
                }
            }
            //Utils.LogToFile($"Emitter failed to spawn, could not find dummy '{dummyName}'");
            return false;
        }

        public void Initalize(IMyEntity ent)
        {
            parent = ent;
            if (!FindDummy(ent))
            {
                //Utils.LogToFile($"Light failed to spawn, could not find dummy '{dummyName}'");
                return;
            }
            //Utils.LogToFile($"Spawned and attached light to '{dummyName}'");

            light = MyLights.AddLight();
            light.Start(dummyName + "_light");

            light.GlareOn = true;
            light.LightOn = true;

            light.Color = Color.White;
            light.Range = 5f;
            light.Falloff = 1f;
            light.Intensity = 2f;
            light.PointLightOffset = 0f;

            light.GlareSize = new Vector2(radius, radius);
            light.GlareIntensity = 2;
            light.GlareMaxDistance = 50;

            var flareDef = MyDefinitionManager.Static.GetDefinition(new MyDefinitionId(typeof(MyObjectBuilder_FlareDefinition), "InteriorLight")) as MyFlareDefinition;
            light.SubGlares = flareDef.SubGlares;
            light.GlareType = MyGlareTypeEnum.Normal;
            light.GlareQuerySize = 1f;
            light.GlareQueryShift = 1f;

            if (ent is IMyCubeBlock)
            {
                IMyCubeGrid grid = ((IMyCubeBlock)ent).CubeGrid;

                light.ParentID = grid.Render.GetRenderObjectID();
                light.Position = Vector3D.Transform(Vector3D.Transform(dum.Matrix.Translation, ent.WorldMatrix), grid.WorldMatrixInvScaled);
            }

            light.UpdateLight();

            ent.OnClose += Close;
            parent = ent;
        }

        private void SetColor(object[] arr)
        {
            if (light == null && !FindDummy(parent))
                return;
            light.Color = new Color((int)arr[0], (int)arr[1], (int)arr[2], 0);
            light.UpdateLight();
        }

        private void LightOn(object[] arr)
        {
            if (light == null && !FindDummy(parent))
                return;
            light.LightOn = true;
            light.GlareOn = true;
            light.UpdateLight();
        }

        private void LightOff(object[] arr)
        {
            if (light == null && !FindDummy(parent))
                return;
            light.LightOn = false;
            light.GlareOn = false;
            light.UpdateLight();
        }

        private void ToggleLight(object[] arr)
        {
            if (light == null && !FindDummy(parent))
                return;
            light.LightOn = !light.LightOn;
            light.GlareOn = !light.GlareOn;
            light.UpdateLight();
        }

    }
}
