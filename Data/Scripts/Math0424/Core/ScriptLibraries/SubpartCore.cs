using AnimationEngine.Language;
using AnimationEngine.Utility;
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

            AddMethod("scale", Scale);
            AddMethod("setvisible", SetVisibility);

            mover = new Mover(Subpart.PositionComp);
            AddMethod("translate", mover.Translate);
            AddMethod("rotate", mover.Rotate);
            AddMethod("rotatearound", mover.RotateAround);
            AddMethod("spin", mover.Spin);
            AddMethod("vibrate", mover.Vibrate);
            AddMethod("reset", mover.Reset);
            AddMethod("resetpos", mover.ResetPos);
            AddMethod("setresetpos", mover.SetResetPos);
            AddMethod("stop", mover.Stop);
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
            mover.Tick(tick);

            foreach (var x in components)
                x.Tick(tick);

            MatrixD parentMat = Subpart.Parent.PositionComp.WorldMatrixRef;
            Subpart.PositionComp.UpdateWorldMatrix(ref parentMat);
        }

        private SVariable Scale(SVariable[] args)
        {
            Vector3 scale = args[0].AsVector3();
            Matrix x = Subpart.PositionComp.LocalMatrixRef.Scale(scale);
            Subpart.PositionComp.SetLocalMatrix(ref x, null, false, ref x);
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
