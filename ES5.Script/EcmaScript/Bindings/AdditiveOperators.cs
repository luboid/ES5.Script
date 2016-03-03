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
        public static object Add(object aLeft, object aRight, ExecutionContext ec)
        {
            if (aLeft is EcmaScriptObject)
                aLeft = Utilities.GetObjectAsPrimitive(ec, (EcmaScriptObject)aLeft, PrimitiveType.None);

            if (aRight is EcmaScriptObject)
                aRight = Utilities.GetObjectAsPrimitive(ec, (EcmaScriptObject)aRight, PrimitiveType.None);

            if ((aLeft is String) || (aRight is String))
                return (Utilities.GetObjAsString(aLeft, ec) + Utilities.GetObjAsString(aRight, ec));

            if ((aLeft is Int32) && (aRight is Int32))
                return ((Int32)aLeft + (Int32)aRight);

            return (Utilities.GetObjAsDouble(aLeft, ec) + Utilities.GetObjAsDouble(aRight, ec));
        }


        public static object Subtract(object aLeft, object aRight, ExecutionContext ec)
        {
            if ((aLeft is Int32) && (aRight is Int32))
                return ((Int32)aLeft - (Int32)aRight);

            return (Utilities.GetObjAsDouble(aLeft, ec) - Utilities.GetObjAsDouble(aRight, ec));
        }

        public static readonly MethodInfo Method_Add = typeof(Operators).GetMethod("Add");
        public static readonly MethodInfo Method_Subtract = typeof(Operators).GetMethod("Subtract");

    }
}