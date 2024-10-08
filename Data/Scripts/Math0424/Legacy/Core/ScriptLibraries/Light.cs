﻿using AnimationEngine.Language;
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
    internal class Light : ScriptLib, Initializable, Parentable
    {
        private string dummyName;
        private float radius, falloff, intensity;
        private bool enabledFlare;
        private IMyModelDummy dum;
        private MyLight light;
        private IMyEntity parent;
        private string parentSubpart;

        public string GetParent()
        {
            return parentSubpart;
        }

        public Light(string dummyName, float radius, string parentSubpart, bool enableFlare, float falloff, float intensity)
        {
            this.parentSubpart = parentSubpart;
            this.radius = radius;
            this.enabledFlare = enableFlare;
            this.dummyName = dummyName;
            this.falloff = falloff;
            this.intensity = intensity;

            if (!MyAPIGateway.Utilities.IsDedicated)
            {
                AddMethod("setcolor", SetColor);
                AddMethod("lighton", LightOn);
                AddMethod("lightoff", LightOff);
                AddMethod("togglelight", ToggleLight);
            }
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
            if (ent == null)
                return;

            parent = ent;
            if (!FindDummy(ent))
            {
                //Utils.LogToFile($"Light failed to spawn, could not find dummy '{dummyName}'");
                return;
            }
            //Utils.LogToFile($"Spawned and attached light to '{dummyName}'");

            light = MyLights.AddLight();
            light.Start(dummyName + "_light");

            light.GlareOn = enabledFlare;
            light.LightOn = true;

            light.Color = Color.White;
            light.Range = radius;
            light.Falloff = falloff;
            light.Intensity = intensity;
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
            light.GlareOn = enabledFlare;
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
            if (enabledFlare)
                light.GlareOn = !light.GlareOn;
            light.UpdateLight();
            return null;
        }

    }
}
