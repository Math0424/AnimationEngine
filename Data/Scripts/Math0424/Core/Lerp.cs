using System;
using VRageMath;

namespace AnimationEngine
{

    public enum ShortHandLerp
    {
        Instant,
        Linear,

        InBack,
        OutBack,
        InOutBack,

        InBounce,
        OutBounce,
        InOutBounce,

        InElastic,
        OutElastic,
        InOutElastic,

        InSine,
        OutSine,
        InOutSine,

        InQuad,
        OutQuad,
        InOutQuad,

        InCubic,
        OutCubic,
        InOutCubic,

        InQuart,
        OutQuart,
        InOutQuart,

        InQuint,
        OutQuint,
        InOutQuint,

        InExpo,
        OutExpo,
        InOutExpo, 

        InCirc,
        OutCirc,
        InOutCirc,

    }


    public enum LerpType
    {
        Instant,
        Linear,

        Back,
        Bounce,
        Elastic,

        Sine,
        Quad,
        Cubic,
        Quart,
        Quint,
        Expo,
        Circ,
    }

    public enum EaseType
    {
        In,
        Out,
        InOut,
    }

    public static class LerpExtensions
    {

        public static void ShortToLong(this ShortHandLerp shl, out LerpType lerp, out EaseType ease)
        {
            switch(shl)
            {
                case ShortHandLerp.Linear:
                    lerp = LerpType.Linear;
                    ease = EaseType.InOut;
                    return;
                case ShortHandLerp.Instant:
                    lerp = LerpType.Instant;
                    ease = EaseType.InOut;
                    return;

                case ShortHandLerp.InBounce:
                    lerp = LerpType.Bounce;
                    ease = EaseType.In;
                    return;
                case ShortHandLerp.OutBounce:
                    lerp = LerpType.Bounce;
                    ease = EaseType.Out;
                    return;
                case ShortHandLerp.InOutBounce:
                    lerp = LerpType.Bounce;
                    ease = EaseType.InOut;
                    return;

                case ShortHandLerp.InElastic:
                    lerp = LerpType.Elastic;
                    ease = EaseType.In;
                    return;
                case ShortHandLerp.OutElastic:
                    lerp = LerpType.Elastic;
                    ease = EaseType.Out;
                    return;
                case ShortHandLerp.InOutElastic:
                    lerp = LerpType.Elastic;
                    ease = EaseType.InOut;
                    return;

                case ShortHandLerp.InExpo:
                    lerp = LerpType.Expo;
                    ease = EaseType.In;
                    return;
                case ShortHandLerp.OutExpo:
                    lerp = LerpType.Expo;
                    ease = EaseType.Out;
                    return;
                case ShortHandLerp.InOutExpo:
                    lerp = LerpType.Expo;
                    ease = EaseType.InOut;
                    return;

                case ShortHandLerp.InCubic:
                    lerp = LerpType.Cubic;
                    ease = EaseType.In;
                    return;
                case ShortHandLerp.OutCubic:
                    lerp = LerpType.Cubic;
                    ease = EaseType.Out;
                    return;
                case ShortHandLerp.InOutCubic:
                    lerp = LerpType.Cubic;
                    ease = EaseType.InOut;
                    return;

                case ShortHandLerp.InBack:
                    lerp = LerpType.Back;
                    ease = EaseType.In;
                    return;
                case ShortHandLerp.OutBack:
                    lerp = LerpType.Back;
                    ease = EaseType.Out;
                    return;
                case ShortHandLerp.InOutBack:
                    lerp = LerpType.Back;
                    ease = EaseType.InOut;
                    return;

                case ShortHandLerp.InSine:
                    lerp = LerpType.Sine;
                    ease = EaseType.In;
                    return;
                case ShortHandLerp.OutSine:
                    lerp = LerpType.Sine;
                    ease = EaseType.Out;
                    return;
                case ShortHandLerp.InOutSine:
                    lerp = LerpType.Sine;
                    ease = EaseType.InOut;
                    return;

                case ShortHandLerp.InQuad:
                    lerp = LerpType.Quad;
                    ease = EaseType.In;
                    return;
                case ShortHandLerp.OutQuad:
                    lerp = LerpType.Quad;
                    ease = EaseType.Out;
                    return;
                case ShortHandLerp.InOutQuad:
                    lerp = LerpType.Quad;
                    ease = EaseType.InOut;
                    return;

                case ShortHandLerp.InQuart:
                    lerp = LerpType.Quart;
                    ease = EaseType.In;
                    return;
                case ShortHandLerp.OutQuart:
                    lerp = LerpType.Quart;
                    ease = EaseType.Out;
                    return;
                case ShortHandLerp.InOutQuart:
                    lerp = LerpType.Quart;
                    ease = EaseType.InOut;
                    return;

                case ShortHandLerp.InQuint:
                    lerp = LerpType.Quint;
                    ease = EaseType.In;
                    return;
                case ShortHandLerp.OutQuint:
                    lerp = LerpType.Quint;
                    ease = EaseType.Out;
                    return;
                case ShortHandLerp.InOutQuint:
                    lerp = LerpType.Quint;
                    ease = EaseType.InOut;
                    return;

                case ShortHandLerp.InCirc:
                    lerp = LerpType.Circ;
                    ease = EaseType.In;
                    return;
                case ShortHandLerp.OutCirc:
                    lerp = LerpType.Circ;
                    ease = EaseType.Out;
                    return;
                case ShortHandLerp.InOutCirc:
                    lerp = LerpType.Circ;
                    ease = EaseType.InOut;
                    return;

                default:
                    lerp = LerpType.Instant;
                    ease = EaseType.In;
                    return;
            }
        }

        public static float Lerp(this LerpType type, EaseType ease, float one, float two, double val)
        {
            return MathHelper.Lerp(one, two, (float)type.LerpVal(val, ease));
        }

        public static Vector3D Lerp(this LerpType type, EaseType ease, Vector3D one, Vector3D two, double val)
        {
            return Vector3D.Lerp(one, two, (float)type.LerpVal(val, ease));
        }

        public static MatrixD Lerp(this LerpType type, EaseType ease, ref MatrixD one, ref MatrixD two, double val)
        {
            return MatrixD.Slerp(one, two, (float)type.LerpVal(val, ease));
        }

        public static Quaternion Lerp(this LerpType type, EaseType ease, Quaternion one, Quaternion two, double val)
        {
            return Quaternion.Slerp(one, two, (float)type.LerpVal(val, ease));
        }

        public static double LerpVal(this LerpType lerp, double val, EaseType ease)
        {
            switch (lerp)
            {
                case LerpType.Instant: return 1;
                case LerpType.Linear: return val;

                case LerpType.Back: return Back(val, ease);
                case LerpType.Bounce: return Bounce(val, ease);
                case LerpType.Elastic: return Elastic(val, ease);

                case LerpType.Sine: return Sine(val, ease);
                case LerpType.Quad: return Quad(val, ease);
                case LerpType.Cubic: return Cubic(val, ease);
                case LerpType.Quart: return Quart(val, ease);
                case LerpType.Quint: return Quint(val, ease);
                case LerpType.Expo: return Expo(val, ease);
                case LerpType.Circ: return Circ(val, ease);
                default: return 0;
            }
        }

        //Functions
        private static double Bounce(double x, EaseType type)
        {
            const double n1 = 7.5625;
            const double d1 = 2.75;

            switch (type)
            {
                case EaseType.In:
                    return 1 - Bounce(1 - x, EaseType.Out);
                case EaseType.Out:
                    if (x < 1 / d1)
                    {
                        return n1 * x * x;
                    }
                    else if (x < 2 / d1)
                    {
                        return n1 * (x -= 1.5 / d1) * x + 0.75;
                    }
                    else if (x < 2.5 / d1)
                    {
                        return n1 * (x -= 2.25 / d1) * x + 0.9375;
                    }
                    else
                    {
                        return n1 * (x -= 2.625 / d1) * x + 0.984375;
                    }
                case EaseType.InOut:
                    return x < 0.5 ? (1 - Bounce(1 - 2 * x, EaseType.Out)) / 2 : (1 + Bounce(2 * x - 1, EaseType.Out)) / 2;
            }
            return x;
        }

        private static double Elastic(double x, EaseType type)
        {
            const double c4 = (2 * Math.PI) / 3;
            const double c5 = (2 * Math.PI) / 4.5;

            switch (type)
            {
                case EaseType.In:
                    return x == 0 ? 0 : x == 1 ? 1 : Math.Pow(2, -10 * x) * Math.Sin((x * 10 - 0.75) * c4) + 1;
                case EaseType.Out:
                    return x == 0 ? 0 : x == 1 ? 1 : Math.Pow(2, -10 * x) * Math.Sin((x * 10 - 0.75) * c4) + 1;
                case EaseType.InOut:
                    return x == 0 ? 0 : x == 1 ? 1 : x < 0.5 ? -(Math.Pow(2, 20 * x - 10) * Math.Sin((20 * x - 11.125) * c5)) / 2 : (Math.Pow(2, -20 * x + 10) * Math.Sin((20 * x - 11.125) * c5)) / 2 + 1; ;
            }
            return x;
        }

        private static double Expo(double x, EaseType type)
        {
            switch (type)
            {
                case EaseType.In:
                    return x == 0 ? 0 : Math.Pow(2, 10 * x - 10);
                case EaseType.Out:
                    return x == 1 ? 1 : 1 - Math.Pow(2, -10 * x);
                case EaseType.InOut:
                    return x == 0 ? 0 : x == 1 ? 1 : x < 0.5 ? Math.Pow(2, 20 * x - 10) / 2 : (2 - Math.Pow(2, -20 * x + 10)) / 2;
            }
            return x;
        }

        private static double Cubic(double x, EaseType type)
        {
            switch (type)
            {
                case EaseType.In:
                    return x * x * x;
                case EaseType.Out:
                    return 1 - Math.Pow(1 - x, 3);
                case EaseType.InOut:
                    return x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;
            }
            return x;
        }

        private static double Back(double x, EaseType type)
        {
            const double c1 = 1.70158;
            const double c2 = c1 * 1.525;
            const double c3 = c1 + 1;

            switch (type)
            {
                case EaseType.In:
                    return c3 * x * x * x - c1 * x * x;
                case EaseType.Out:
                    return 1 + c3 * Math.Pow(x - 1, 3) + c1 * Math.Pow(x - 1, 2);
                case EaseType.InOut:
                    return x < 0.5 ? (Math.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2 : (Math.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
            }
            return x;
        }

        private static double Sine(double x, EaseType type)
        {
            switch (type)
            {
                case EaseType.In:
                    return 1 - Math.Cos((x * Math.PI) / 2);
                case EaseType.Out:
                    return Math.Sin((x * Math.PI) / 2);
                case EaseType.InOut:
                    return -(Math.Cos(Math.PI * x) - 1) / 2;
            }
            return x;
        }

        private static double Quad(double x, EaseType type)
        {
            switch (type)
            {
                case EaseType.In:
                    return x * x;
                case EaseType.Out:
                    return 1 - (1 - x) * (1 - x);
                case EaseType.InOut:
                    return x < 0.5 ? 2 * x * x : 1 - Math.Pow(-2 * x + 2, 2) / 2;
            }
            return x;
        }

        public static double Quart(double x, EaseType type)
        {
            switch (type)
            {
                case EaseType.In:
                    return x * x * x * x;
                case EaseType.Out:
                    return 1 - Math.Pow(1 - x, 4);
                case EaseType.InOut:
                    return x < 0.5 ? 8 * x * x * x * x : 1 - Math.Pow(-2 * x + 2, 4) / 2;
            }
            return x;
        }

        public static double Quint(double x, EaseType type)
        {
            switch (type)
            {
                case EaseType.In:
                    return x * x * x * x * x;
                case EaseType.Out:
                    return 1 - Math.Pow(1 - x, 5);
                case EaseType.InOut:
                    return x < 0.5 ? 16 * x * x * x * x * x : 1 - Math.Pow(-2 * x + 2, 5) / 2;
            }
            return x;
        }


        public static double Circ(double x, EaseType type)
        {
            switch (type)
            {
                case EaseType.In:
                    return 1 - Math.Sqrt(1 - Math.Pow(x, 2));
                case EaseType.Out:
                    return Math.Sqrt(1 - Math.Pow(x - 1, 2));
                case EaseType.InOut:
                    return x < 0.5 ? (1 - Math.Sqrt(1 - Math.Pow(2 * x, 2))) / 2 : (Math.Sqrt(1 - Math.Pow(-2 * x + 2, 2)) + 1) / 2;
            }
            return x;
        }

    }
}
