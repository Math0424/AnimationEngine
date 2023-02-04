using AnimationEngine.Core;
using AnimationEngine.Language;
using AnimationEngine.Utility;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRageMath;

namespace AnimationEngine
{

    internal class Mover
    {
        private MyPositionComponentBase core;
        private List<BaseAction> movements = new List<BaseAction>();
        private Matrix originMatrix;

        public Mover(MyPositionComponentBase core)
        {
            this.core = core;
            originMatrix = new Matrix(core.LocalMatrixRef);
        }

        public void Tick(int time)
        {
            foreach (var x in movements)
                x?.Tick(time);
            movements.RemoveAll(x => x.IsFinished);
        }

        public SVariable SetResetPos(SVariable[] args)
        {
            originMatrix = new Matrix(core.LocalMatrixRef);
            return null;
        }

        public SVariable ResetPos(SVariable[] args)
        {
            Matrix m = core.WorldMatrixRef;
            m.Translation = originMatrix.Translation;
            core.SetLocalMatrix(ref m);
            return null;
        }

        public SVariable Reset(SVariable[] args)
        {
            Clear();
            core.SetLocalMatrix(ref originMatrix);
            return null;
        }

        //translate([x, y, z], time, lerp)
        public SVariable Translate(SVariable[] args)
        {
            Vector3 val = args[0].AsVector3();
            int time = args[1].AsInt();
            LerpType lerp;
            EaseType ease;
            ((ShortHandLerp)args[2].AsInt()).ShortToLong(out lerp, out ease);
            movements.Add(new TranslateAction(core, time, val, lerp, ease));
            return null;
        }

        //rotate([x, y, z], angle, time, lerp)
        public SVariable Rotate(params SVariable[] args)
        {
            Vector3 val = args[0].AsVector3();
            float angle = args[1].AsFloat() * 0.0174f;
            int time = args[2].AsInt();
            LerpType lerp;
            EaseType ease;
            ((ShortHandLerp)args[3].AsInt()).ShortToLong(out lerp, out ease);
            movements.Add(new RotateAction(core, time, Quaternion.CreateFromAxisAngle(val, angle), lerp, ease));
            return null;
        }

        //rotate([x, y, z], [x, y, z] pivot angle, time, lerp)
        public SVariable RotateAround(SVariable[] args)
        {
            Vector3 val = args[0].AsVector3();
            Vector3 pivot = args[1].AsVector3();
            float angle = args[2].AsFloat() * 0.0174f;
            int time = args[3].AsInt();
            LerpType lerp;
            EaseType ease;
            ((ShortHandLerp)args[4].AsInt()).ShortToLong(out lerp, out ease);
            movements.Add(new RotateAroundAction(core, time, pivot, Quaternion.CreateFromAxisAngle(val, angle), lerp, ease));
            return null;
        }

        //spin([x, y, z], speed, time)
        public SVariable Spin(SVariable[] args)
        {
            Vector3 val = args[0].AsVector3();
            float speed = args[1].AsFloat() * 0.0174f;
            int time = args[2].AsInt();
            movements.Add(new SpinAction(core, time, Quaternion.CreateFromAxisAngle(val, speed)));
            return null;
        }

        //vibrate(time)
        public SVariable Vibrate(SVariable[] args)
        {
            movements.Add(new VibrateAction(core, args[1].AsInt(), args[0].AsFloat()));
            return null;
        }

        public void Clear()
        {
            movements.Clear();
        }

    }

    internal abstract class BaseAction
    {
        public virtual bool IsFinished { get { return true; } }
        public abstract void Tick(int time);
    }

    internal class DelegateAction : BaseAction
    {
        private Action act;
        public DelegateAction(Action act) : base()
        {
            this.act = act;
        }
        public override void Tick(int time)
        {
            act?.Invoke();
        }
    }

    internal abstract class TimedAction : BaseAction
    {
        public override bool IsFinished { get { return frame >= endFrame; } }

        protected int frame, endFrame;
        protected LerpType lerp;
        protected EaseType ease;
        protected double val;

        public TimedAction(int time, LerpType lerp, EaseType ease)
        {
            this.frame = 0;
            this.endFrame = time;
            this.lerp = lerp;
            this.ease = ease;
        }
        public override void Tick(int time)
        {
            frame += time;
            val = endFrame != 0 ? (double)frame / endFrame : 1;
        }
    }

    internal abstract class MoveAction : TimedAction
    {
        protected MyPositionComponentBase core;
        public MoveAction(MyPositionComponentBase core, int time, LerpType lerp, EaseType ease) : base(time, lerp, ease)
        {
            this.core = core;
        }
    }

    internal class TranslateAction : MoveAction
    {
        private Vector3D prev, end;
        public TranslateAction(MyPositionComponentBase core, int time, Vector3D end, LerpType lerp, EaseType ease) : base(core, time, lerp, ease)
        {
            this.prev = Vector3D.Zero;
            this.end = end;
        }
        public override void Tick(int time)
        {
            base.Tick(time);

            Matrix matrix = core.LocalMatrixRef;
            
            Vector3D temp = prev;
            prev = lerp.Lerp(ease, Vector3D.Zero, end, val);
            temp = temp - prev;

            matrix.Translation += temp;

            core.SetLocalMatrix(ref matrix, null, false, ref matrix);
        }
    }

    internal class SpinAction : MoveAction
    {
        private Quaternion rot;
        public SpinAction(MyPositionComponentBase core, int time, Quaternion rot) : base(core, time, LerpType.Linear, EaseType.InOut)
        {
            this.rot = rot;
        }
        public override void Tick(int time)
        {
            base.Tick(time);

            Matrix local = core.LocalMatrixRef;
            
            Quaternion q = rot;
            if (time > 1)
            {
                for (int i = 0; i < time - 1; i++)
                {
                    float W = q.W;
                    float X = q.X;
                    float Y = q.Y;
                    float Z = q.Z;
                    q.W = W * rot.W - X * rot.X - Y * rot.Y - Z * rot.Z;
                    q.X = W * rot.X + X * rot.W + Y * rot.Z - Z * rot.Y;
                    q.Y = W * rot.Y - X * rot.Z + Y * rot.W + Z * rot.X;
                    q.Z = W * rot.Z + X * rot.Y - Y * rot.X + Z * rot.W;
                }
            }

            Matrix matrix = MatrixD.Transform(local, q);
            matrix.Translation = local.Translation;

            core.SetLocalMatrix(ref matrix, null, false, ref matrix);
        }
    }

    internal class RotateAroundAction : MoveAction
    {
        private Quaternion prevQ, end;
        private Vector3D prevV, pivot;
        public RotateAroundAction(MyPositionComponentBase core, int time, Vector3 pivot, Quaternion end, LerpType lerp, EaseType ease) : base(core, time, lerp, ease)
        {
            this.pivot = pivot;
            prevV = pivot;

            this.prevQ = Quaternion.Identity;
            this.end = end;
        }
        public override void Tick(int time)
        {
            base.Tick(time);

            Quaternion temp = prevQ;
            prevQ = lerp.Lerp(ease, Quaternion.Identity, end, val);
            temp = temp * Quaternion.Inverse(prevQ);
            
            Matrix matrix = core.LocalMatrixRef;

            Vector3D tempV = prevV;
            prevV = Vector3D.Transform(pivot, prevQ);

            Vector3D tmp = matrix.Translation + tempV - prevV;
            
            matrix = MatrixD.Transform(matrix, Quaternion.Inverse(temp));
            matrix.Translation = tmp;

            core.SetLocalMatrix(ref matrix, null, false, ref matrix);
        }
    }

    internal class RotateAction : MoveAction
    {
        private Quaternion prev, end;
        public RotateAction(MyPositionComponentBase core, int time, Quaternion end, LerpType lerp, EaseType ease) : base(core, time, lerp, ease)
        {
            this.prev = Quaternion.Identity;
            this.end = end;
        }
        public override void Tick(int time)
        {
            base.Tick(time);

            Quaternion temp = prev;
            prev = lerp.Lerp(ease, Quaternion.Identity, end, val);
            temp = temp * Quaternion.Inverse(prev);

            Matrix local = core.LocalMatrixRef;
            Matrix matrix = MatrixD.Transform(local, temp); //TODO Quaternion.Inverse(temp). this method is reversed.
            matrix.Translation = local.Translation;

            core.SetLocalMatrix(ref matrix, null, false, ref matrix);
        }
    }

    internal class VibrateAction : MoveAction
    {
        private static readonly Random rand = new Random();
        private Vector3D vec;
        public VibrateAction(MyPositionComponentBase core, int time, float scale) : base(core, time, LerpType.Instant, EaseType.InOut)
        {
            this.vec = new Vector3D(rand.NextDouble(), rand.NextDouble(), rand.NextDouble()) * scale;
        }
        public override void Tick(int time)
        {
            base.Tick(time);

            Matrix matrix = core.LocalMatrixRef;
            matrix.Translation = vec;
            vec = -vec;
            core.SetLocalMatrix(ref matrix, null, false, ref matrix);
        }
    }


}
