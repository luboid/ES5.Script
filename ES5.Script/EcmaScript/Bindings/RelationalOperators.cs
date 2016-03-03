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
        public static object InstanceOf(object aLeft, object aRight, ExecutionContext ec)
        {
            var lLeft = aLeft as EcmaScriptObject;
            var lRight = aRight as EcmaScriptObject;
            lRight = lRight?.Get("prototype", ec) as EcmaScriptObject;
            if (lRight == null)
                ec.Global.RaiseNativeError(NativeErrorType.TypeError, "Not an object");

            if (lLeft == null) return false;

            do
            {
                if (lLeft == lRight) return true;
                lLeft = lLeft.Prototype;
            }
            while (lLeft != null);

            return false;
        }

        public static object In(object aLeft, object aRight, ExecutionContext ec)
        {
            var lObj = aRight as EcmaScriptObject;
            if (lObj == null)
                ec.Global.RaiseNativeError(NativeErrorType.TypeError, "Not an object");

            return lObj.HasProperty(Utilities.GetObjAsString(aLeft, ec));
        }

        public static object LessThan(object aLeft, object aRight, ExecutionContext ec)
        {
            if (aLeft is EcmaScriptObject)
                aLeft = Utilities.GetObjectAsPrimitive(ec, (EcmaScriptObject)aLeft, PrimitiveType.Number);
            if (aRight is EcmaScriptObject)
                aRight = Utilities.GetObjectAsPrimitive(ec, (EcmaScriptObject)aRight, PrimitiveType.Number);

            if ((aLeft is String) && (aRight is String))
                return string.CompareOrdinal((String)aLeft, (String)aRight) < 0;

            var l = Utilities.GetObjAsDouble(aLeft, ec);
            var r = Utilities.GetObjAsDouble(aRight, ec);
            if (Double.IsNaN(l) || Double.IsNaN(r))
                return false;

            return l < r;
        }

        public static object GreaterThan(object aLeft, object aRight, ExecutionContext ec)
        {
            if (aLeft is EcmaScriptObject)
                aLeft = Utilities.GetObjectAsPrimitive(ec, (EcmaScriptObject)aLeft, PrimitiveType.Number);
            if (aRight is EcmaScriptObject)
                aRight = Utilities.GetObjectAsPrimitive(ec, (EcmaScriptObject)aRight, PrimitiveType.Number);

            if ((aLeft is String) && (aRight is String))
                return string.CompareOrdinal((String)aLeft, (String)aRight) > 0;

            var l = Utilities.GetObjAsDouble(aLeft, ec);
            var r = Utilities.GetObjAsDouble(aRight, ec);
            if (Double.IsNaN(l) || Double.IsNaN(r))
                return false;

            return l > r;
        }

        public static object LessThanOrEqual(object aLeft, object aRight, ExecutionContext ec)
        {
            if (aLeft is EcmaScriptObject)
                aLeft = Utilities.GetObjectAsPrimitive(ec, (EcmaScriptObject)aLeft, PrimitiveType.Number);
            if (aRight is EcmaScriptObject)
                aRight = Utilities.GetObjectAsPrimitive(ec, (EcmaScriptObject)aRight, PrimitiveType.Number);

            if ((aLeft is String) && (aRight is String))
                return string.CompareOrdinal((String)aLeft, (String)aRight) <= 0;

            var l = Utilities.GetObjAsDouble(aLeft, ec);
            var r = Utilities.GetObjAsDouble(aRight, ec);
            if (Double.IsNaN(l) || Double.IsNaN(r))
                return false;

            return l <= r;
        }

        public static object GreaterThanOrEqual(object aLeft, object aRight, ExecutionContext ec)
        {
            if (aLeft is EcmaScriptObject)
                aLeft = Utilities.GetObjectAsPrimitive(ec, (EcmaScriptObject)aLeft, PrimitiveType.Number);
            if (aRight is EcmaScriptObject)
                aRight = Utilities.GetObjectAsPrimitive(ec, (EcmaScriptObject)aRight, PrimitiveType.Number);

            if ((aLeft is String) && (aRight is String))
                return string.CompareOrdinal((String)aLeft, (String)aRight) >= 0;

            var l = Utilities.GetObjAsDouble(aLeft, ec);
            var r = Utilities.GetObjAsDouble(aRight, ec);
            if (Double.IsNaN(l) || Double.IsNaN(r))
                return false;

            return l >= r;
        }

        public static readonly MethodInfo Method_LessThan = typeof(Operators).GetMethod("LessThan");
        public static readonly MethodInfo Method_GreaterThan = typeof(Operators).GetMethod("GreaterThan");
        public static readonly MethodInfo Method_LessThanOrEqual = typeof(Operators).GetMethod("LessThanOrEqual");
        public static readonly MethodInfo Method_GreaterThanOrEqual = typeof(Operators).GetMethod("GreaterThanOrEqual");
        public static readonly MethodInfo Method_InstanceOf = typeof(Operators).GetMethod("InstanceOf");
        public static readonly MethodInfo Method_In = typeof(Operators).GetMethod("In");
    }
}