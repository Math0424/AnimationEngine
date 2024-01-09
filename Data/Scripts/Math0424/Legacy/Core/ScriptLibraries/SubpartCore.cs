using AnimationEngine.Language;
using AnimationEngine.Utility;
using Sandbox.Game.Components;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace AnimationEngine.Core
{
    internal class SubpartCore : ScriptLib
    {
        private List<SubpartComponent> components = new List<SubpartComponent>();
        private Mover mover;
        private readonly string _subpartName;
        
        public bool Valid { get; private set; }
        public MyEntitySubpart Subpart { get; private set; }
        public SubpartCore(MyEntitySubpart part, string subpartName)
        {
            _subpartName = subpartName;
            Valid = false;
            SetSubpart(part);

            if (!MyAPIGateway.Utilities.IsDedicated)
            {
                AddMethod("setvisible", SetVisibility);
                AddMethod("setmodel", SetModel);
            }
        }

        public void SetSubpart(MyEntitySubpart part)
        {
            if (part == null)
                return;
            
            if(Subpart != null)
                Subpart.OnClose -= Close;
            
            if (part.Render.GetType() != typeof(MyRenderComponent) && part.Model != null)
            {
                string asset = ((IMyModel)part.Model).AssetName;
                var model = part.Render.ModelStorage;
                var matrix = part.PositionComp.LocalMatrixRef;
                var physics = part.Physics;
                var render = part.Render;
                var parent = part.Parent;
                var oldPart = part;
                
                part = new MyEntitySubpart();
                part.Render.EnableColorMaskHsv = render.EnableColorMaskHsv;
                part.Render.ColorMaskHsv = render.ColorMaskHsv;
                part.Render.TextureChanges = render.TextureChanges;
                part.Render.MetalnessColorable = render.MetalnessColorable;
                part.Physics = physics;
                
                part.Init(null, asset, parent, null, null);
                part.OnAddedToScene(parent);
                part.PositionComp.SetLocalMatrix(ref matrix, null, true);

                parent.Subparts[_subpartName] = part;

                // man I dunno why its crashing
                // just make it go away...
                parent.Hierarchy.RemoveChild(oldPart);
                oldPart.Render.Visible = false;
                oldPart.WorldMatrix = MatrixD.Zero;
                //oldPart.Close();
            }

            Subpart = part;
            Valid = true;

            mover?.Clear();
            mover = new Mover(Subpart.PositionComp);
            mover.AddToScriptLib(this, "");

            Subpart.OnClose += Close;
            part.Name = $"{part.EntityId}:{_subpartName}";
        }

        public override void Close()
        {
            Valid = false;
            mover?.Clear();
            foreach (var component in components)
                component.Close();
        }

        public override void Tick(int tick)
        {
            if (Subpart == null)
            {
                Valid = false;
                return;
            }
            if (!Subpart.InScene || Subpart.MarkedForClose)
                return;

            foreach (var c in components)
                c.Tick(tick);
            mover?.Tick(tick);

            MatrixD parentMat = Subpart.Parent.PositionComp.WorldMatrixRef;
            Subpart.PositionComp.UpdateWorldMatrix(ref parentMat);
        }

        private SVariable SetModel(SVariable[] args)
        {
            if (Subpart.Render != null && Subpart.Render is MyRenderComponent)
            {
                Subpart.RefreshModels(args[0].ToString(), null);
            }
            return null;
        }

        private SVariable SetVisibility(SVariable[] args)
        {
            Subpart.Render.Visible = args[0].AsBool();
            return null;
        }

        private void Close(IMyEntity ent)
        {
            Subpart.OnClose -= Close;
            Close();
        }

        public void AddComponent<T>(T comp) where T : SubpartComponent
        {
            components.Add(comp);
        }

        public T GetFirstComponent<T>() where T : SubpartComponent
        {
            foreach (var x in components)
                if (x.GetType() == typeof(T))
                    return (T)x;
            return null;
        }

        public bool HasComponent<T>() where T : SubpartComponent
        {
            foreach (var x in components)
                if (x.GetType() == typeof(T))
                    return true;
            return false;
        }

        public bool RemoveComponent<T>() where T : SubpartComponent
        {
            foreach (var x in components)
                if (x.GetType() == typeof(T))
                {
                    x.Close();
                    components.Remove(x);
                    return true;
                }
            return false;
        }

    }

}
