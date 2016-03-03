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
        public static object Multiply(object aLeft, object aRight, ExecutionContext ec)
        {
            if ((aLeft is Int32) && (aRight is Int32)) {
                var lL = (int)aLeft;
                var lR = (int)aRight;
                var lRes = lL * lR;
                if (lR == 0)
                    return lRes;

                var lNegativeSign = (lL < 0) == (lR < 0);
                if (lNegativeSign) {
                    if (lRes < lL) return lRes;
                } else
                    if (lRes > lL) return lRes;
            }
            var dlL = Utilities.GetObjAsDouble(aLeft, ec);
            var dlR = Utilities.GetObjAsDouble(aRight, ec);
            return dlL * dlR;
        }

        public static object Divide(object aLeft, object aRight, ExecutionContext ec)
        {
            if ((aLeft is Int32) && (aRight is Int32) && (((int)aRight) != 0) && (((int)aLeft) != 0) && (((int)aLeft) % ((int)aRight) == 0)) {
                int result;
                Utilities.DivRem(((int)aLeft), ((int)aRight), out result);
                return result;
            }

            return Utilities.GetObjAsDouble(aLeft, ec) / 
                   Utilities.GetObjAsDouble(aRight, ec);
        }

        public static object Modulus(object aLeft, object aRight, ExecutionContext ec)
        {
            if ((aLeft is Int32) && (aRight is Int32) && (((int)aRight) != 0) && (((int)aLeft) > 0))
                return ((int)aLeft) % ((int)aRight);

            var lLeft = Utilities.GetObjAsDouble(aLeft, ec);
            var lRight = Utilities.GetObjAsDouble(aRight, ec);
            var lWork = lLeft / lRight;
            if (lWork < 0)
                lWork = Math.Ceiling(lWork);
            else
                lWork = Math.Floor(lWork);

            lWork = lLeft - (lRight * lWork);
            if (Double.IsInfinity(lRight) && !Double.IsInfinity(lLeft)) {
                lWork = lLeft;
            } else
                if (((lWork == 0.0) && (lLeft < 0)) || IsNegativeZero(lLeft))
                    lWork = -lWork;

            return lWork;
        }

        public static bool IsNegativeZero(double aValue)
        {
            return (aValue == 0.0) && Double.IsNegativeInfinity(1.0 / aValue);
        }

        public static readonly MethodInfo Method_Multiply = typeof(Operators).GetMethod("Multiply");
        public static readonly MethodInfo Method_Divide = typeof(Operators).GetMethod("Divide");
        public static readonly MethodInfo Method_Modulus = typeof(Operators).GetMethod("Modulus");
    }
}