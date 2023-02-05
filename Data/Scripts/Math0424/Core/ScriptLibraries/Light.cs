using AnimationEngine.Language;
using AnimationEngine.Utility;
using Sandbox.Definitions;
using Sandbox.Game.Lights;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using VRageRender.Lights;

namespace AnimationEngine.Core
{
    internal class Light : ScriptLib, Initializable
    {
        private string dummyName;
        private float radius;
        private IMyModelDummy dum;
        private MyLight light;
        private IMyEntity parent;

        public Light(string dummyName, float radius)
        {
            this.radius = radius;
            this.dummyName = dummyName;
            AddMethod("setcolor", SetColor);
            AddMethod("lighton", LightOn);
            AddMethod("lightoff", LightOff);
            AddMethod("togglelight", ToggleLight);
        }

        public void Close(IMyEntity ent)
        {
            if (light != null)
                MyLights.RemoveLight(light);
            ent.OnClose -= Close;
            Utils.LogToFile($"Closed light attached to '{dummyName}'");
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

        public void Init(IMyEntity ent)
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

        private SVariable SetColor(SVariable[] arr)
        {
            if (light == null && !FindDummy(parent))
                return null;
            light.Color = new Color(arr[0].AsInt(), arr[1].AsInt(), arr[2].AsInt(), 0);
            light.UpdateLight();
            return null;
        }

        private SVariable LightOn(SVariable[] arr)
        {
            if (light == null && !FindDummy(parent))
                return null;
            light.LightOn = true;
            light.GlareOn = true;
            light.UpdateLight();
            return null;
        }

        private SVariable LightOff(SVariable[] arr)
        {
            if (light == null && !FindDummy(parent))
                return null;
            light.LightOn = false;
            light.GlareOn = false;
            light.UpdateLight();
            return null;
        }

        private SVariable ToggleLight(SVariable[] arr)
        {
            if (light == null && !FindDummy(parent))
                return null;
            light.LightOn = !light.LightOn;
            light.GlareOn = !light.GlareOn;
            light.UpdateLight();
            return null;
        }

    }
}
