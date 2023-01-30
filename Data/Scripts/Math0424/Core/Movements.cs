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

        public Mover(MyPositionComponentBase core)
        {
            this.core = core;
        }

        public void Tick(int time)
        {
            foreach (var x in movements)
                x?.Tick(time);
            movements.RemoveAll(x => x.IsFinished);
        }

        //translate([x, y, z], time, lerp)
        public void Translate(object[] args)
        {
            Vector3 val = (Vector3)args[0];
            int time = (int)args[1];
            LerpType lerp;
            EaseType ease;
            ((ShortHandLerp)args[2]).ShortToLong(out lerp, out ease);
            movements.Add(new TranslateAction(core, time, val, lerp, ease));
        }

        //rotate([x, y, z], angle, time, lerp)
        public void Rotate(params object[] args)
        {
            Vector3 val = (Vector3)args[0];
            float angle = (float)args[1] * 0.0174f;
            int time = (int)args[2];
            LerpType lerp;
            EaseType ease;
            ((ShortHandLerp)args[3]).ShortToLong(out lerp, out ease);
            movements.Add(new RotateAction(core, time, Quaternion.CreateFromAxisAngle(val, angle), lerp, ease));
        }

        //rotate([x, y, z], [x, y, z] pivot angle, time, lerp)
        public void RotateAround(object[] args)
        {
            Vector3 val = (Vector3)args[0];
            Vector3 pivot = (Vector3)args[1];
            float angle = (float)args[2] * 0.0174f;
            int time = (int)args[3];
            LerpType lerp;
            EaseType ease;
            ((ShortHandLerp)args[4]).ShortToLong(out lerp, out ease);
            movements.Add(new RotateAroundAction(core, time, pivot, Quaternion.CreateFromAxisAngle(val, angle), lerp, ease));
        }

        //spin([x, y, z], speed, time)
        public void Spin(object[] args)
        {
            Vector3 val = (Vector3)args[0];
            float speed = (float)args[1] * 0.0174f;
            int time = (int)args[2];
            movements.Add(new SpinAction(core, time, Quaternion.CreateFromAxisAngle(val, speed)));
        }

        //vibrate(time)
        public void Vibrate(object[] args)
        {
            movements.Add(new VibrateAction(core, (int)args[1], (float)args[0]));
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
