using AnimationEngine.Language;
using AnimationEngine.Utility;
using Sandbox.Game.Components;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRageMath;

namespace AnimationEngine.Core
{
    internal class SubpartCore : ScriptLib
    {
        private List<SubpartComponent> components = new List<SubpartComponent>();
        private Mover mover;

        public MyEntitySubpart Subpart { get; private set; }

        public void Init(MyEntitySubpart subpart)
        {
            Subpart = subpart;

            if (!MyAPIGateway.Utilities.IsDedicated)
            {
                AddMethod("setvisible", SetVisibility);
                AddMethod("setmodel", SetModel);

                mover = new Mover(Subpart.PositionComp);
                mover.AddToScriptLib(this, "");
            }
            Subpart.OnClose += Close;
        }

        public void Close()
        {

        }

        public override void Tick(int tick)
        {
            if (Subpart == null || !Subpart.InScene)
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
            mover?.Clear();
            foreach (var component in components)
                component.Close();
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

    }

}
