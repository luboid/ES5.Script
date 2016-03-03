using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ES5.Script.EcmaScript.Internal;

namespace ES5.Script.EcmaScript.Objects
{
    public partial class GlobalObject : EcmaScriptObject
    {
        public EcmaScriptObject CreateBoolean()
        {
            var result = Get("Boolean") as EcmaScriptObject;
            if (result != null)
                return result;

            result = new EcmaScriptBooleanObject(this, "Boolean", BooleanCall, 1) { Class = "Boolean" };
            Values.Add("Boolean", PropertyValue.NotEnum(result));

            BooleanPrototype = new EcmaScriptObject(this) { Class = "Boolean", Prototype = ObjectPrototype };
            BooleanPrototype.Values.Add("constructor", PropertyValue.NotEnum(result));
            BooleanPrototype.Values.Add("toString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toString", BooleanToString, 0)));
            BooleanPrototype.Values.Add("valueOf", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "valueOf", BooleanValueOf, 0)));

            result.Values["prototype"] = PropertyValue.NotAllFlags(BooleanPrototype);
            return result;
        }


        public object BooleanCall(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Utilities.GetArgAsBoolean(args, 0, aCaller);
        }

        public object BooleanCtor(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lVal = Utilities.GetArgAsBoolean(args, 0, aCaller);
            var lObj = new EcmaScriptObject(this, BooleanPrototype) { Class = "Boolean", Value = lVal };

            return (lObj);
        }

        public object BooleanToString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            if (aSelf is Boolean)
                return (bool)aSelf ? "true" : "false";

            var El = aSelf as EcmaScriptObject;

            if ((El == null) || (El.Class != "Boolean"))
                RaiseNativeError(NativeErrorType.TypeError, "Boolean.toString() is not generic");

            return Utilities.GetObjAsBoolean(El.Value, aCaller) ? "true" : "false";
        }

        public object BooleanValueOf(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            if (aSelf is Boolean)
                return (bool)aSelf;

            var El = aSelf as EcmaScriptObject;
            if ((El == null) || (El.Class != "Boolean"))
                RaiseNativeError(NativeErrorType.TypeError, "Boolean.toString() is not generic");

            return Utilities.GetObjAsBoolean(El.Value, aCaller);
        }
    }
}
