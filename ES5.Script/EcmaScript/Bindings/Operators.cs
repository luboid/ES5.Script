using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ES5.Script.EcmaScript.Bindings
{
    public partial class Operators
    {
        static bool DoubleCompare(double aLeft, double aRight)
        {
            if (Double.IsNegativeInfinity(aLeft) && Double.IsNegativeInfinity(aRight)) return true;
            if (Double.IsPositiveInfinity(aLeft) && Double.IsPositiveInfinity(aRight)) return true;
            if (Double.IsNegativeInfinity(aLeft) && Double.IsPositiveInfinity(aRight)) return false;
            if (Double.IsPositiveInfinity(aLeft) && Double.IsNegativeInfinity(aRight)) return false;
            if (Double.IsNaN(aLeft)) return false;
            if (Double.IsNaN(aRight)) return false;
            if (Double.IsInfinity(aLeft) || Double.IsInfinity(aRight)) return false;
            //var bits := BitConverter.DoubleToInt64Bits(aLeft);
            // Note that the shift is sign-extended, hence the test against -1 not 1
            return aLeft == aRight;
        }

        // OR, AND, ?: have side effects in evaluation and are not specified here.
        public static readonly MethodInfo Method_SameValue = typeof(Operators).GetMethod("SameValue");
        public static readonly MethodInfo Method_Equal = typeof(Operators).GetMethod("Equal");
        public static readonly MethodInfo Method_NotEqual = typeof(Operators).GetMethod("NotEqual");
        public static readonly MethodInfo Method_StrictEqual = typeof(Operators).GetMethod("StrictEqual");
        public static readonly MethodInfo Method_StrictNotEqual = typeof(Operators).GetMethod("StrictNotEqual");
        public static readonly MethodInfo Method_TypeOf = typeof(Operators).GetMethod("_TypeOf");

        public static SimpleType Type(object o)
        {
            if (o == null) return SimpleType.Null;
            if (o == Undefined.Instance) return SimpleType.Undefined;
            switch (System.Type.GetTypeCode(o.GetType())) {
                case TypeCode.Boolean: return SimpleType.Boolean;
                case TypeCode.Int32:
                case TypeCode.Double: return SimpleType.Number;
                case TypeCode.String: return SimpleType.String;
                default: return SimpleType.Object;
            } // case
        }


        public static object Equal(object aLeft, object aRight, ExecutionContext ec)
        {
            var lLeft = Type(aLeft);
            var lRight = Type(aRight);
            if (lLeft == lRight) {
                switch (lLeft) {
                    case SimpleType.Boolean:
                        return Utilities.GetObjAsBoolean(aLeft, ec) == Utilities.GetObjAsBoolean(aRight, ec);
                    case SimpleType.Undefined:
                    case SimpleType.Null: return true;
                    case SimpleType.Number: {
                            if ((aLeft is Int32) && (aRight is Int32))
                                return (Int32)aLeft == (Int32)aRight;
                            if ((aLeft is Int64) && (aRight is Int64))
                                return (Int64)aLeft == (Int64)aRight;

                            return DoubleCompare(Utilities.GetObjAsDouble(aLeft, ec), Utilities.GetObjAsDouble(aRight, ec));
                        }
                    case SimpleType.String:
                        return Utilities.GetObjAsString(aLeft, ec) == Utilities.GetObjAsString(aRight, ec);
                    default: // object
                        return (EcmaScriptObject)aLeft == (EcmaScriptObject)aRight;
                } // case
            }

            if (((lLeft == SimpleType.Undefined) && (lRight == SimpleType.Null)) ||
                ((lRight == SimpleType.Undefined) && (lLeft == SimpleType.Null)))
                return true;

            if ((lLeft == SimpleType.Number) && (lRight == SimpleType.String))
                return Equal(aLeft, Utilities.GetObjAsDouble(aRight, ec), ec);

            if ((lRight == SimpleType.Number) && (lLeft == SimpleType.String))
                return Equal(Utilities.GetObjAsDouble(aLeft, ec), aRight, ec);

            if (lLeft == SimpleType.Boolean)
                return Equal(Utilities.GetObjAsDouble(aLeft, ec), aRight, ec);
            if (lRight == SimpleType.Boolean)
                return Equal(aLeft, Utilities.GetObjAsDouble(aRight, ec), ec);

            if ((lLeft == SimpleType.String || lLeft == SimpleType.Number) &&
                (lRight == SimpleType.Object))
                return Equal(aLeft, Utilities.GetObjectAsPrimitive(ec, (EcmaScriptObject)aRight, PrimitiveType.None), ec);

            if ((lRight == SimpleType.String || lRight == SimpleType.Number) &&
                (lLeft == SimpleType.Object))
                return Equal(Utilities.GetObjectAsPrimitive(ec, (EcmaScriptObject)aLeft, PrimitiveType.None), aRight, ec);

            return false;
        }

        static readonly TypeCode[] StrictEqual_Types = new[] {TypeCode.SByte,
                TypeCode.Int16,
                TypeCode.Int32,
                TypeCode.Int64,
                TypeCode.Byte,
                TypeCode.UInt16,
                TypeCode.UInt32,
                TypeCode.UInt64,
                TypeCode.Single,
                TypeCode.Double};

        public static object StrictEqual(object aLeft, object aRight, ExecutionContext ec)
        {
            if ((aLeft == null) && (aRight == null)) return true;
            if ((aLeft == null) || (aRight == null)) return false;

            var tLeft = System.Type.GetTypeCode(aLeft.GetType());
            var tRight = System.Type.GetTypeCode(aRight.GetType());
            if (Array.IndexOf<TypeCode>(StrictEqual_Types, tLeft) > -1 &&
                Array.IndexOf<TypeCode>(StrictEqual_Types, tRight) > -1) {
                return DoubleCompare(
                    Utilities.GetObjAsDouble(aLeft, ec),
                    Utilities.GetObjAsDouble(aRight, ec));
            }

            if (aLeft.GetType() != aRight.GetType()) return false;
            if (aLeft == Undefined.Instance) return true;

            return Equals(aLeft, aRight);
        }

        public static object StrictNotEqual(object aLeft, object aRight, ExecutionContext ec)
        {
            return !(bool)StrictEqual(aLeft, aRight, ec);
        }

        public static bool SameValue(object aLeft, object aRight, ExecutionContext ec)
        {
            return (aLeft == aRight) || (bool)(StrictEqual(aLeft, aRight, ec));
        }

        public static string _TypeOf(object aValue, ExecutionContext ec)
        {
            var lRef = aValue as Reference;
            if ((lRef != null) && (lRef.Base == Undefined.Instance)) return "undefined";
            aValue = Reference.GetValue(aValue, ec);

            if (aValue == null) return "object";
            if (aValue == Undefined.Instance) return "undefined";
            var lObj = (aValue as EcmaScriptObject);

            if (lObj != null) {
                if (lObj is EcmaScriptBaseFunctionObject)
                    return "function";
                return "object";
            }

            switch (System.Type.GetTypeCode(aValue.GetType()))
            {
                case TypeCode.Boolean: return "boolean";
                case TypeCode.Char: return "string";
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single: return "number";
                case TypeCode.String: return "string";
            } // case
            return "object";
        }

        public static object NotEqual(object aLeft, object aRight, ExecutionContext ec)
        {
            return !(bool)Equal(aLeft, aRight, ec);
        }
    }
}