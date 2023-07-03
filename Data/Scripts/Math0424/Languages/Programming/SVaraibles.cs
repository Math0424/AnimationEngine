using System;
using VRageMath;

namespace AnimationEngine.Language
{
    internal static class SVarUtil
    {
        public static SVariable Convert(Token token)
        {
            switch (token.Type)
            {
                case TokenType.FLOAT: return new SVariableFloat((float)token.Value);
                case TokenType.MVECTOR: return new SVariableVector((Vector3)token.Value);
                case TokenType.BOOL: return new SVariableBool((bool)token.Value);
                case TokenType.INT: return new SVariableInt((int)token.Value);
                case TokenType.STR: return new SVariableString((string)token.Value);
                case TokenType.LERP:
                    foreach (var x in Enum.GetValues(typeof(ShortHandLerp)))
                        if (x.ToString().ToLower().Equals(token.Value.ToString().ToLower()))
                            return new SVariableInt((int)x);
                    return null;
            }
            return null;
        }
    }

    public interface SVariable
    {
        int AsInt();
        float AsFloat();
        bool AsBool();
        Vector3 AsVector3();

        bool Equals(SVariable a);
        SVariable Add(SVariable a);
        SVariable Sub(SVariable a);
        SVariable Mul(SVariable a);
        SVariable Div(SVariable a);
        SVariable Mod(SVariable a);
    }

    internal class SVariableInt : SVariable
    {
        int value;
        public SVariableInt(int value)
        {
            this.value = value;
        }

        public int AsInt() => value;
        public float AsFloat() => (float)value;
        public bool AsBool() => value != 0;
        public Vector3 AsVector3() => new Vector3(value);

        public bool Equals(SVariable a) => a.GetType() == typeof(SVariableInt) && value == ((SVariableInt)a).AsInt();
        public SVariable Add(SVariable a) => new SVariableInt(value + a.AsInt());
        public SVariable Sub(SVariable a) => new SVariableInt(value - a.AsInt());
        public SVariable Div(SVariable a) => new SVariableInt(value / a.AsInt());
        public SVariable Mod(SVariable a) => new SVariableInt(value % a.AsInt());
        public SVariable Mul(SVariable a) => new SVariableInt(value * a.AsInt());

        public override string ToString() => value.ToString();
    }

    internal class SVariableFloat : SVariable
    {
        float value;
        public SVariableFloat(float value)
        {
            this.value = value;
        }

        public int AsInt() => (int)value;
        public float AsFloat() => (float)value;
        public bool AsBool() => value != 0;
        public Vector3 AsVector3() => new Vector3(value);

        public bool Equals(SVariable a) => a.GetType() == typeof(SVariableFloat) && value == ((SVariableFloat)a).AsFloat();
        public SVariable Add(SVariable a) => new SVariableFloat(value + a.AsFloat());
        public SVariable Sub(SVariable a) => new SVariableFloat(value - a.AsFloat());
        public SVariable Div(SVariable a) => new SVariableFloat(value / a.AsFloat());
        public SVariable Mod(SVariable a) => new SVariableFloat(value % a.AsFloat());
        public SVariable Mul(SVariable a) => new SVariableFloat(value * a.AsFloat());

        public override string ToString() => value.ToString();
    }

    internal class SVariableBool : SVariable
    {
        bool value;
        public SVariableBool(bool value)
        {
            this.value = value;
        }

        public int AsInt() => value ? 1 : 0;
        public float AsFloat() => value ? 1 : 0;
        public bool AsBool() => value;
        public Vector3 AsVector3() => new Vector3(value ? 1 : 0);

        public bool Equals(SVariable a) => a.GetType() == typeof(SVariableBool) && value == ((SVariableBool)a).AsBool();
        public SVariable Add(SVariable a) => new SVariableInt(AsInt() + a.AsInt());
        public SVariable Sub(SVariable a) => new SVariableInt(AsInt() - a.AsInt()); 
        public SVariable Div(SVariable a) => new SVariableInt(AsInt() / a.AsInt()); 
        public SVariable Mod(SVariable a) => new SVariableInt(AsInt() % a.AsInt()); 
        public SVariable Mul(SVariable a) => new SVariableInt(AsInt() * a.AsInt()); 

        public override string ToString() => value.ToString();
    }

    internal class SVariableVector : SVariable
    {
        Vector3 value;
        public SVariableVector(Vector3 value)
        {
            this.value = value;
        }

        public int AsInt() => (int)value.Length();
        public float AsFloat() => value.Length();
        public bool AsBool() => value.Length() != 0;
        public Vector3 AsVector3() => value;

        public bool Equals(SVariable a) => a.GetType() == typeof(SVariableVector) && value == ((SVariableVector)a).AsVector3();
        public SVariable Add(SVariable a) => new SVariableVector(value + a.AsVector3());
        public SVariable Sub(SVariable a) => new SVariableVector(value - a.AsVector3());
        public SVariable Div(SVariable a) => new SVariableVector(value / a.AsVector3());
        public SVariable Mod(SVariable a) => new SVariableVector(value / a.AsVector3()); // dunno
        public SVariable Mul(SVariable a) => new SVariableVector(value * a.AsVector3());

        public override string ToString() => value.ToString();
    }

    internal class SVariableString : SVariable
    {
        string value;
        public SVariableString(string value)
        {
            this.value = value;
        }

        public int AsInt() => (int)value.Length;
        public float AsFloat() => (float)value.Length;
        public bool AsBool() => value.Length != 0;
        public Vector3 AsVector3() => new Vector3(value.Length);

        public bool Equals(SVariable a) => a.GetType() == typeof(SVariableString) && value == ((SVariableString)a).ToString();
        public SVariable Add(SVariable a) => new SVariableString(value + a.ToString());
        public SVariable Sub(SVariable a) => new SVariableString(value.Replace(a.ToString(), ""));
        public SVariable Div(SVariable a) => new SVariableString(value + a.ToString()); // dunno
        public SVariable Mod(SVariable a) => new SVariableString(value + a.ToString()); // dunno
        public SVariable Mul(SVariable a)
        {
            string x = value;
            for(int i = 0; i < a.AsInt(); i++)
                x += value;
            return new SVariableString(value);
        }

        public override string ToString() => value.ToString();
    }

}
