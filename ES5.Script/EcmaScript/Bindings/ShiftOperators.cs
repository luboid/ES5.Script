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
        public static object ShiftLeft(object aLeft, object aRight, ExecutionContext ec)
        {
            return Utilities.GetObjAsInteger(aLeft, ec) << Utilities.GetObjAsInteger(aRight, ec);
        }

        public static object ShiftRight(object aLeft, object aRight, ExecutionContext ec)
        {
            return Utilities.GetObjAsInteger(aLeft, ec) >> Utilities.GetObjAsInteger(aRight, ec);
        }

        public static object ShiftRightUnsigned(object aLeft, object aRight, ExecutionContext ec)
        {
            var l = (uint)Utilities.GetObjAsInteger(aLeft, ec);
            var r = Utilities.GetObjAsInteger(aRight, ec);
            var u = (Int64)(l >> r);
            return (double)u;
        }

        public static readonly MethodInfo Method_ShiftLeft = typeof(Operators).GetMethod("ShiftLeft");
        public static readonly MethodInfo Method_ShiftRight = typeof(Operators).GetMethod("ShiftRight");
        public static readonly MethodInfo Method_ShiftRightUnsigned = typeof(Operators).GetMethod("ShiftRightUnsigned");
    }
}