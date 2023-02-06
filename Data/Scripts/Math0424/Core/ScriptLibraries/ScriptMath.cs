using AnimationEngine.Core;
using AnimationEngine.Utility;
using System;

namespace AnimationEngine.Language.Libs
{
    internal class ScriptMath : ScriptLib
    {
        public ScriptMath()
        {
            AddMethod("sin", sin);
            AddMethod("cos", cos);
            AddMethod("abs", abs);
            AddMethod("max", max);
            AddMethod("min", min);
            AddMethod("floor", floor);
            AddMethod("ceiling", ceiling);

            AddMethod("makevector", makeVector);
            //("cross", 1, true, "Vector", "Vector"),
            //("distance", 1, true, "Vector", "Vector"),
            //("dot", 1, true, "Vector", "Vector"),
            //("magnitude", 1, true, "Vector")
        }

        public SVariable makeVector(SVariable[] var)
        {
            return new SVariableVector(new VRageMath.Vector3(var[0].AsFloat(), var[1].AsFloat(), var[2].AsFloat()));
        }

        public SVariable sin(SVariable[] var)
        {
            return new SVariableFloat((float)Math.Sin(var[0].AsFloat()));
        }
        public SVariable cos(SVariable[] var)
        {
            return new SVariableFloat((float)Math.Cos(var[0].AsFloat()));
        }
        public SVariable abs(SVariable[] var)
        {
            return new SVariableFloat((float)Math.Abs(var[0].AsFloat()));
        }
        public SVariable max(SVariable[] var)
        {
            return new SVariableFloat(var[0].AsFloat() > var[1].AsFloat() ? var[0].AsFloat() : var[1].AsFloat());
        }
        public SVariable min(SVariable[] var)
        {
            return new SVariableFloat(var[0].AsFloat() < var[1].AsFloat() ? var[0].AsFloat() : var[1].AsFloat());
        }
        public SVariable floor(SVariable[] var)
        {
            return new SVariableFloat((float)Math.Floor(var[0].AsFloat()));
        }
        public SVariable ceiling(SVariable[] var)
        {
            return new SVariableFloat((float)Math.Ceiling(var[0].AsFloat()));
        }
    }
}
