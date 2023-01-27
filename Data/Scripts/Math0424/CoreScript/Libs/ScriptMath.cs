﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AnimationEngine.CoreScript.Libs
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

            //("cross", 1, true, "Vector", "Vector"),
            //("distance", 1, true, "Vector", "Vector"),
            //("dot", 1, true, "Vector", "Vector"),
            //("magnitude", 1, true, "Vector")
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
            if (var[0].AsFloat() > var[1].AsFloat())
            {
                return var[0];
            }
            return var[1];
        }
        public SVariable min(SVariable[] var)
        {
            if (var[0].AsFloat() < var[1].AsFloat())
            {
                return var[0];
            }
            return var[1];
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