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
        public static object And(object aLeft, object aRight, ExecutionContext ec)
        {
            return (Utilities.GetObjAsInteger(aLeft, ec) & Utilities.GetObjAsInteger(aRight, ec));
        }

        public static object Or(object aLeft, object aRight, ExecutionContext ec)
        {
            return (Utilities.GetObjAsInteger(aLeft, ec) | Utilities.GetObjAsInteger(aRight, ec));
        }

        public static object Xor(object aLeft, object aRight, ExecutionContext ec)
        {
            return (Utilities.GetObjAsInteger(aLeft, ec) ^ Utilities.GetObjAsInteger(aRight, ec));
        }

        public static readonly MethodInfo Method_And = typeof(Operators).GetMethod("And");
        public static readonly MethodInfo Method_Or = typeof(Operators).GetMethod("Or");
        public static readonly MethodInfo Method_Xor = typeof(Operators).GetMethod("Xor");
    }
}