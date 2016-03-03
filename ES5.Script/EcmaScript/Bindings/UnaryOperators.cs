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
        public static object BitwiseNot(object aData, ExecutionContext ec)
        {
            return ~Utilities.GetObjAsInteger(aData, ec);
        }

        public static object LogicalNot(object aData, ExecutionContext ec)
        {
            return !Utilities.GetObjAsBoolean(aData, ec);
        }

        public static object Minus(object aData, ExecutionContext ec)
        {
            if (aData is int)
            {
                if (((int)aData) == 0)
                {
                    var d = 0.0;
                    d = -d;
                    return d;
                }
                return -((int)aData);
            };
            return -Utilities.GetObjAsDouble(aData, ec);
        }

        public static object Plus(object aData, ExecutionContext ec)
        {
            if (aData is int)
            {
                return aData;
            };
            return Utilities.GetObjAsDouble(aData, ec);
        }

        public static readonly MethodInfo Method_BitwiseNot = typeof(Operators).GetMethod("BitwiseNot");
        public static readonly MethodInfo Method_LogicalNot = typeof(Operators).GetMethod("LogicalNot");
        public static readonly MethodInfo Method_Minus = typeof(Operators).GetMethod("Minus");
        public static readonly MethodInfo Method_Plus = typeof(Operators).GetMethod("Plus");
    }
}