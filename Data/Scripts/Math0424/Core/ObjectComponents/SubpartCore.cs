using AnimationEngine.LanguageV1;
using AnimationEngine.LogicV1;
using AnimationEngine.Utility;
using System;
using System.Collections.Generic;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace AnimationEngine.Core
{
    internal class SubpartCore : Actionable
    {
        private Dictionary<Type, SubpartComponent> components = new Dictionary<Type, SubpartComponent>();
        private Mover mover;

        public MyEntitySubpart Subpart { get; private set; }
        private Matrix originMatrix;

        public SubpartCore(MyEntitySubpart subpart)
        {
            this.Subpart = subpart;

            Actions.Add("reset", Reset);
            Actions.Add("resetpos", ResetPos);
            Actions.Add("setresetpos", SetResetPos);

            Actions.Add("scale", Scale);
            Actions.Add("setvisible", SetVisibility);

            mover = new Mover(Subpart.PositionComp);
            Actions["translate"] = mover.Translate;
            Actions["rotate"] = mover.Rotate;
            Actions["rotatearound"] = mover.RotateAround;
            Actions["spin"] = mover.Spin;
            Actions["vibrate"] = mover.Vibrate;
            Subpart.OnClose += Close;
            originMatrix = new Matrix(Subpart.PositionComp.LocalMatrixRef);
        }

        public override void Tick(int tick)
        {
            if (Subpart == null || !Subpart.InScene)
                return;

            foreach (var c in components.Values)
                c.Tick(tick);
            mover.Tick(tick);

            MatrixD parentMat = Subpart.Parent.PositionComp.WorldMatrixRef;
            Subpart.PositionComp.UpdateWorldMatrix(ref parentMat);
        }

        public void SetOriginMatrix()
        {
            originMatrix = new Matrix(Subpart.PositionComp.LocalMatrixRef);
        }

        public void Reset()
        {
            mover?.Clear();
            Subpart?.PositionComp.SetLocalMatrix(ref originMatrix);
        }
        #region actionables

        private void SetResetPos(object[] args)
        {
            SetOriginMatrix();
        }

        private void Reset(object[] args)
        {
            Reset();
        }

        private void ResetPos(object[] args)
        {
            Matrix m = Subpart.WorldMatrix;
            m.Translation = originMatrix.Translation;
            Subpart.PositionComp.SetLocalMatrix(ref m);
        }

        private void Scale(object[] args)
        {
            Vector3 scale = (Vector3)args[0];
            Matrix x = Subpart.PositionComp.LocalMatrixRef.Scale(scale);
            Subpart.PositionComp.SetLocalMatrix(ref x, null, false, ref x);
        }

        private void SetVisibility(object[] args)
        {
            Subpart.Render.Visible = (bool)args[0];
        }
        #endregion


        public void Close(IMyEntity ent)
        {
            Subpart.OnClose -= Close;
            mover?.Clear();
            foreach (var component in components.Values)
                component.Close();
        }

        public void AddComponent<T>(T comp) where T : SubpartComponent
        {
            components.Add(typeof(T), comp);
        }

        public T GetComponent<T>() where T : SubpartComponent
        {
            SubpartComponent t;
            if (components.TryGetValue(typeof(T), out t))
            {
                return (T)t;
            }
            return null;
        }

        public bool HasComponent<T>() where T : SubpartComponent
        {
            return components.ContainsKey(typeof(T));
        }

    }

}
