using AnimationEngine.Language;
using VRageMath;

namespace AnimationEngine.CoreScript
{
    internal static class SVarUtil
    {
        public static SVariable Convert(Token token)
        {
            switch(token.Type)
            {
                case TokenType.FLOAT: return new SVariableFloat((float)token.Value);
                case TokenType.MVECTOR: return new SVariableVector((Vector3)token.Value);
                case TokenType.BOOL: return new SVariableBool((bool)token.Value);
                case TokenType.INT: return new SVariableInt((int)token.Value);
            }
            return null;
        }
    }

    internal struct SVariableStruct
    {
        private enum ValType
        {
            INT = 1, FLOAT = 2, BOOL = 4, VECTOR = 8, STRING = 16
        }

        readonly ValType type;
        public readonly int intVal;
        public readonly float floatVal;
        public readonly bool boolVal;
        public readonly Vector3 vector3Val;

        public int AsInt() => intVal;
        public float AsFloat() => floatVal;
        public bool AsBool() => boolVal;
        public Vector3 AsVector3() => vector3Val;

        public SVariableStruct(string obj)
        {
            type = ValType.INT;
            intVal = obj.Length;
            floatVal = intVal;
            vector3Val = new Vector3(intVal, intVal, intVal);
            boolVal = intVal != 0;
        }

        public SVariableStruct(int obj)
        {
            type = ValType.INT;
            intVal = (int)obj;
            floatVal = (float)obj;
            vector3Val = new Vector3(obj, obj, obj);
            boolVal = obj != 0;
        }

        public SVariableStruct(float obj)
        {
            type = ValType.FLOAT;
            intVal = (int)obj;
            floatVal = (float)obj;
            vector3Val = new Vector3(obj, obj, obj);
            boolVal = obj != 0;
        }

        public SVariableStruct(bool obj)
        {
            type = ValType.BOOL;
            intVal = obj ? 1 : 0;
            floatVal = intVal;
            vector3Val = new Vector3(intVal, intVal, intVal);
            boolVal = obj;
        }

        public SVariableStruct(Vector3 obj)
        {
            type = ValType.VECTOR;
            intVal = (int)obj.LengthSquared();
            floatVal = obj.LengthSquared();
            vector3Val = obj;
            boolVal = obj.LengthSquared() == 0;
        }

        public bool Equals(SVariableStruct a)
        {
            return a.type == type && (type == ValType.VECTOR ? vector3Val.Equals(a.vector3Val) : floatVal == a.floatVal);
        }

        public SVariableStruct Add(SVariableStruct a)
        {
            if (((int)type | (int)a.type) == 8)
                return new SVariableStruct(vector3Val + a.vector3Val);
            return new SVariableStruct(floatVal + a.floatVal);
        }
        public SVariableStruct Sub(SVariableStruct a)
        {
            if (((int)type | (int)a.type) == 8)
                return new SVariableStruct(vector3Val - a.vector3Val);
            return new SVariableStruct(floatVal - a.floatVal);
        }
        public SVariableStruct Mul(SVariableStruct a)
        {
            if (((int)type | (int)a.type) == 8)
                return new SVariableStruct(vector3Val * a.vector3Val);
            return new SVariableStruct(floatVal * a.floatVal);
        }
        public SVariableStruct Div(SVariableStruct a)
        {
            if (((int)type | (int)a.type) == 8)
                return new SVariableStruct(a.vector3Val / vector3Val);
            return new SVariableStruct(floatVal / a.floatVal);
        }
        public SVariableStruct Mod(SVariableStruct a)
        {
            return new SVariableStruct(a.floatVal % floatVal);
        }

        public override string ToString()
        {
            switch(type)
            {
                case ValType.INT:
                    return intVal.ToString();
                case ValType.FLOAT:
                    return floatVal.ToString();
                case ValType.BOOL:
                    return boolVal.ToString();
                case ValType.VECTOR:
                    return vector3Val.ToString();
                default:
                    return intVal.ToString();
            }
        }
    }

    internal interface SVariable
    {
        public int AsInt();
        public float AsFloat();
        public bool AsBool();
        public Vector3 AsVector3();

        public bool Equals(SVariable a);
        public SVariable Add(SVariable a);
        public SVariable Sub(SVariable a);
        public SVariable Mul(SVariable a);
        public SVariable Div(SVariable a);
        public SVariable Mod(SVariable a);
    }

    internal class SVariableInt : SVariable
    {
        int value;
        public SVariableInt(int value) => this.value = value;

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
        public SVariableFloat(float value) => this.value = value;

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
        public SVariableBool(bool value) => this.value = value;

        public int AsInt() => value ? 1 : 0;
        public float AsFloat() => value ? 1 : 0;
        public bool AsBool() => value;
        public Vector3 AsVector3() => new Vector3(value ? 1 : 0);

        public bool Equals(SVariable a) => a.GetType() == typeof(SVariableBool) && value == ((SVariableBool)a).AsBool();
        public SVariable Add(SVariable a) => new SVariableBool(value | a.AsBool());
        public SVariable Sub(SVariable a) => new SVariableBool(value & a.AsBool()); // dunno
        public SVariable Div(SVariable a) => new SVariableBool(value & a.AsBool()); // dunno
        public SVariable Mod(SVariable a) => new SVariableBool(value & a.AsBool()); // dunno
        public SVariable Mul(SVariable a) => new SVariableBool(value & a.AsBool()); // dunno

        public override string ToString() => value.ToString();
    }

    internal class SVariableVector : SVariable
    {
        Vector3 value;
        public SVariableVector(Vector3 value) => this.value = value;

        public int AsInt() => (int)value.X;
        public float AsFloat() => value.X;
        public bool AsBool() => value.X != 0;
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
        public SVariableString(string value) => this.value = value;

        public int AsInt() => (int)value.Length;
        public float AsFloat() => (float)value.Length;
        public bool AsBool() => value.Length != 0;
        public Vector3 AsVector3() => new Vector3(value.Length);

        public bool Equals(SVariable a) => a.GetType() == typeof(SVariableString) && value == ((SVariableString)a).ToString();
        public SVariable Add(SVariable a) => new SVariableString(value + a.ToString());
        public SVariable Sub(SVariable a) => new SVariableString(value.Replace(a.ToString(), ""));
        public SVariable Div(SVariable a) => new SVariableString(value + a.ToString()); // dunno
        public SVariable Mod(SVariable a) => new SVariableString(value + a.ToString()); // dunno
        public SVariable Mul(SVariable a) => new SVariableString(value + a.ToString()); // dunno

        public override string ToString() => value.ToString();
    }

}
