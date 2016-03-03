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
        static void ThrowStrict(Reference aLeft, ExecutionContext aExec, string message)
        {
            if (aLeft.Strict && (aLeft.Base is EnvironmentRecord) && (aLeft.Name == "eval" || aLeft.Name == "arguments"))
                aExec.Global.RaiseNativeError(NativeErrorType.SyntaxError, message);
        }

        public static object PostDecrement(object aLeft, ExecutionContext aExec)
        {
            var lRef = aLeft as Reference;

            if (lRef != null) {
                ThrowStrict(lRef, aExec, "eval/arguments cannot be used in post decrement operator");
                aLeft = Reference.GetValue(lRef, aExec);
            }

            var lOldValue = aLeft;
            if (aLeft is int) {
                lOldValue = (int)aLeft;
                aLeft = (int)aLeft - 1;
            }
            else
            {
                lOldValue = Utilities.GetObjAsDouble(aLeft, aExec);
                aLeft = Utilities.GetObjAsDouble(aLeft, aExec) - 1.0;
            }

            Reference.SetValue(lRef, aLeft, aExec);

            return lOldValue;
        }

        public static object PostIncrement(object aLeft, ExecutionContext aExec)
        {
            var lRef = aLeft as Reference;

            if (lRef != null)
            {
                ThrowStrict(lRef, aExec, "eval/arguments cannot be used in post increment operator");
                aLeft = Reference.GetValue(lRef, aExec);
            }

            var lOldValue = aLeft;
            if (aLeft is int)
            {
                lOldValue = (int)aLeft;
                aLeft = (int)aLeft + 1;
            }
            else
            {
                lOldValue = Utilities.GetObjAsDouble(aLeft, aExec);
                aLeft = Utilities.GetObjAsDouble(aLeft, aExec) + 1.0;
            }

            Reference.SetValue(lRef, aLeft, aExec);

            return lOldValue;
        }

        public static object PreDecrement(object aLeft, ExecutionContext aExec)
        {
            var lRef = aLeft as Reference;

            if (lRef != null) {
                ThrowStrict(lRef, aExec, "eval/arguments cannot be used in pre decrement operator");
                aLeft = Reference.GetValue(lRef, aExec);
            }

            if (aLeft is int)
                aLeft = (int)aLeft - 1;
            else
                aLeft = Utilities.GetObjAsDouble(aLeft, aExec) - 1.0;

            return Reference.SetValue(lRef, aLeft, aExec);
        }

        public static object PreIncrement(object aLeft, ExecutionContext aExec)
        {
            var lRef = aLeft as Reference;

            if (lRef != null)
            {
                ThrowStrict(lRef, aExec, "eval/arguments cannot be used in pre increment operator");
                aLeft = Reference.GetValue(lRef, aExec);
            }

            if (aLeft is int)
                aLeft = (int)aLeft + 1;
            else
                aLeft = Utilities.GetObjAsDouble(aLeft, aExec) + 1.0;

            return Reference.SetValue(lRef, aLeft, aExec);
        }

        // OR, AND, ?: have side effects in evaluation and are not specified here.
        public static readonly MethodInfo Method_PostDecrement = typeof(Operators).GetMethod("PostDecrement");
        public static readonly MethodInfo Method_PostIncrement = typeof(Operators).GetMethod("PostIncrement");
        public static readonly MethodInfo Method_PreDecrement = typeof(Operators).GetMethod("PreDecrement");
        public static readonly MethodInfo Method_PreIncrement = typeof(Operators).GetMethod("PreIncrement");
    }
}