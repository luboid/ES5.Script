using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ES5.Script.EcmaScript.Objects
{
    public partial class GlobalObject : EcmaScriptObject
    {
        public static readonly MethodInfo Method_RaiseNativeError = typeof(GlobalObject).GetMethod("RaiseNativeError");

        public EcmaScriptObject EvalError { get; set; }
        public EcmaScriptObject RangeError { get; set; }
        public EcmaScriptObject ReferenceError { get; set; }
        public EcmaScriptObject SyntaxError { get; set; }
        public EcmaScriptObject TypeError { get; set; }
        public EcmaScriptObject URIError { get; set; }

        public EcmaScriptObject CreateError()
        {
            var result = Get("Error") as EcmaScriptObject;
            if (result != null) return result;

            result = new EcmaScriptExceptionFunctionObject(this, "Error", ErrorCtor, 1) { Class = "Error" };
            Values.Add("Error", PropertyValue.NotEnum(result));

            ErrorPrototype = new EcmaScriptObject(this) { Class = "Error", Prototype = ObjectPrototype };
            ErrorPrototype.Values.Add("constructor", PropertyValue.NotEnum(result));
            result.Values["prototype"] = PropertyValue.NotAllFlags(ErrorPrototype);
            ErrorPrototype.Values.Add("toString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toString", ErrorToString, 0)));
            ErrorPrototype.Values.Add("name", PropertyValue.NotDeleteAndReadOnly("Error"));
            ErrorPrototype.Values.Add("message", PropertyValue.NotDeleteAndReadOnly(string.Empty));

            return result;
        }

        public object ErrorCtor(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lUndef = (0 == args.Length) || (args[0] == Undefined.Instance);
            var lMessage = lUndef ? null : Utilities.GetArgAsString(args, 0, aCaller);
            var lObj = aSelf as EcmaScriptObject;
            if ((lObj == null) || (lObj.Class != "Error"))
                lObj = new EcmaScriptObject(this, ErrorPrototype) { Class = "Error" };
            if (!lUndef)
                lObj.AddValue("message", lMessage);
            return lObj;
        }

        public object ErrorToString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.GetObjAsEcmaScriptObject(aSelf, aCaller);
            var lMsg = lSelf.Get("message", aCaller) == null ? null : Utilities.GetObjAsString(lSelf.Get("message", aCaller, 0), aCaller);
            var lName = Utilities.GetObjAsString(lSelf.Get("name", aCaller), aCaller) ?? "Error";
            if (String.IsNullOrEmpty(lMsg))
                return lName;
            else
                return lName + ": " + lMsg;
        }

        public void CreateNativeError()
        {
            EvalError = new EcmaScriptExceptionFunctionObject(this, "EvalError", EvalErrorCtor, -1);
            Values.Add("EvalError", PropertyValue.NotEnum(EvalError));

            var lPrototype = new EcmaScriptObject(this) { Class = "EvalError", Prototype = ErrorPrototype };
            lPrototype.Values["name"] = PropertyValue.NotDeleteAndReadOnly("EvalError");
            lPrototype.Values.Add("constructor", PropertyValue.Create(EvalError, PropertyAttributes.Writable | PropertyAttributes.Configurable));
            lPrototype.Values.Add("message", PropertyValue.NotDeleteAndReadOnly(string.Empty));
            EvalError.Values["prototype"] = PropertyValue.NotAllFlags(lPrototype);

            RangeError = new EcmaScriptExceptionFunctionObject(this, "RangeError", RangeErrorCtor, -1);
            Values.Add("RangeError", PropertyValue.NotEnum(RangeError));

            lPrototype = new EcmaScriptObject(this) { Class = "RangeError", Prototype = ErrorPrototype };
            lPrototype.Values.Add("constructor", PropertyValue.Create(RangeError, PropertyAttributes.Writable | PropertyAttributes.Configurable));
            lPrototype.Values["name"] = PropertyValue.NotDeleteAndReadOnly("RangeError");
            lPrototype.Values.Add("message", PropertyValue.NotDeleteAndReadOnly(string.Empty));
            RangeError.Values["prototype"] = PropertyValue.NotAllFlags(lPrototype);

            ReferenceError = new EcmaScriptExceptionFunctionObject(this, "ReferenceError", ReferenceErrorCtor, -1);
            Values.Add("ReferenceError", PropertyValue.NotEnum(ReferenceError));

            lPrototype = new EcmaScriptObject(this) { Class = "ReferenceError", Prototype = ErrorPrototype };
            lPrototype.Values["name"] = PropertyValue.NotDeleteAndReadOnly("ReferenceError");
            lPrototype.Values.Add("constructor", PropertyValue.Create(ReferenceError, PropertyAttributes.Writable | PropertyAttributes.Configurable));
            lPrototype.Values.Add("message", PropertyValue.NotDeleteAndReadOnly(string.Empty));
            ReferenceError.Values["prototype"] = PropertyValue.NotAllFlags(lPrototype);

            SyntaxError = new EcmaScriptExceptionFunctionObject(this, "SyntaxError", SyntaxErrorCtor, -1);
            Values.Add("SyntaxError", PropertyValue.NotEnum(SyntaxError));

            lPrototype = new EcmaScriptObject(this) { Class = "SyntaxError", Prototype = ErrorPrototype };
            lPrototype.Values["name"] = PropertyValue.NotDeleteAndReadOnly("SyntaxError");
            lPrototype.Values.Add("constructor", PropertyValue.Create(SyntaxError, PropertyAttributes.Writable | PropertyAttributes.Configurable));
            lPrototype.Values.Add("message", PropertyValue.NotDeleteAndReadOnly(string.Empty));
            SyntaxError.Values["prototype"] = PropertyValue.NotAllFlags(lPrototype);

            TypeError = new EcmaScriptExceptionFunctionObject(this, "TypeError", TypeErrorCtor, -1);
            Values.Add("TypeError", PropertyValue.NotEnum(TypeError));

            lPrototype = new EcmaScriptObject(this) { Class = "TypeError", Prototype = ErrorPrototype };
            lPrototype.Values["name"] = PropertyValue.NotDeleteAndReadOnly("TypeError");
            lPrototype.Values.Add("constructor", PropertyValue.Create(TypeError, PropertyAttributes.Writable | PropertyAttributes.Configurable));
            lPrototype.Values.Add("message", PropertyValue.NotDeleteAndReadOnly(string.Empty));
            TypeError.Values["prototype"] = PropertyValue.NotAllFlags(lPrototype);

            URIError = new EcmaScriptExceptionFunctionObject(this, "URIError", URIErrorCtor, -1);
            Values.Add("URIError", PropertyValue.NotEnum(URIError));

            lPrototype = new EcmaScriptObject(this) { Class = "URIError", Prototype = ErrorPrototype };
            lPrototype.Values["name"] = PropertyValue.NotDeleteAndReadOnly("URIError");
            lPrototype.Values.Add("constructor", PropertyValue.Create(URIError, PropertyAttributes.Writable | PropertyAttributes.Configurable));
            lPrototype.Values.Add("message", PropertyValue.NotDeleteAndReadOnly(string.Empty));
            URIError.Values["prototype"] = PropertyValue.NotAllFlags(lPrototype);
        }


        public void RaiseNativeError(NativeErrorType e, string msg)
        {
            switch (e)
            {
                case NativeErrorType.EvalError: throw new ScriptRuntimeException(EvalErrorCtor(fExecutionContext, null, msg));
                case NativeErrorType.RangeError: throw new ScriptRuntimeException(RangeErrorCtor(fExecutionContext, null, msg));
                case NativeErrorType.ReferenceError: throw new ScriptRuntimeException(ReferenceErrorCtor(fExecutionContext, null, msg));
                case NativeErrorType.SyntaxError: throw new ScriptRuntimeException(SyntaxErrorCtor(fExecutionContext, null, msg));
                case NativeErrorType.TypeError: throw new ScriptRuntimeException(TypeErrorCtor(fExecutionContext, null, msg));
                case NativeErrorType.URIError: throw new ScriptRuntimeException(URIErrorCtor(fExecutionContext, null, msg));
                default: throw new ScriptRuntimeException(ErrorCtor(null, null, new object[] { "Unknown" }));
            }
        }


        public EcmaScriptObject NativeErrorCtor(EcmaScriptObject proto, string arg)
        {
            var lMessage = arg;
            var result = new EcmaScriptObject(this, proto) { Class = proto.Class };
            result.AddValue("message", lMessage);
            return result;
        }

        public object EvalErrorCtor(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lUndef = (0 == args.Length) || (args[0] == Undefined.Instance);
            var lMessage = lUndef ? null : Utilities.GetArgAsString(args, 0, aCaller);
            var lObj = aSelf as EcmaScriptObject;
            if ((lObj == null) || (lObj.Class != "EvalError"))
                lObj = new EcmaScriptObject(this, (EcmaScriptObject)EvalError.Values["prototype"].Value) { Class = "EvalError" };
            if (!lUndef)
                lObj.AddValue("message", lMessage);
            return lObj;
        }

        public object RangeErrorCtor(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lUndef = (0 == args.Length) || (args[0] == Undefined.Instance);
            var lMessage = lUndef ? null : Utilities.GetArgAsString(args, 0, aCaller);
            var lObj = aSelf as EcmaScriptObject;
            if ((lObj == null) || (lObj.Class != "RangeError"))
                lObj = new EcmaScriptObject(this, (EcmaScriptObject)RangeError.Values["prototype"].Value) { Class = "RangeError" };
            if (!lUndef)
                lObj.AddValue("message", lMessage);
            return lObj;
        }

        public object ReferenceErrorCtor(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lUndef = (0 == args.Length) || (args[0] == Undefined.Instance);
            var lMessage = lUndef ? null : Utilities.GetArgAsString(args, 0, aCaller);
            var lObj = aSelf as EcmaScriptObject;
            if ((lObj == null) || (lObj.Class != "ReferenceError"))
                lObj = new EcmaScriptObject(this, (EcmaScriptObject)ReferenceError.Values["prototype"].Value) { Class = "ReferenceError" };
            if (!lUndef)
                lObj.AddValue("message", lMessage);
            return lObj;
        }

        public object SyntaxErrorCtor(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lUndef = (0 == args.Length) || (args[0] == Undefined.Instance);
            var lMessage = lUndef ? null : Utilities.GetArgAsString(args, 0, aCaller);
            var lObj = aSelf as EcmaScriptObject;
            if ((lObj == null) || (lObj.Class != "SyntaxError"))
                lObj = new EcmaScriptObject(this, (EcmaScriptObject)SyntaxError.Values["prototype"].Value) { Class = "SyntaxError" };
            if (!lUndef)
                lObj.AddValue("message", lMessage);
            return lObj;
        }

        public object TypeErrorCtor(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lUndef = (0 == args.Length) || (args[0] == Undefined.Instance);
            var lMessage = lUndef ? null : Utilities.GetArgAsString(args, 0, aCaller);
            var lObj = aSelf as EcmaScriptObject;
            if ((lObj == null) || (lObj.Class != "TypeError"))
                lObj = new EcmaScriptObject(this, (EcmaScriptObject)TypeError.Values["prototype"].Value) { Class = "TypeError" };
            if (!lUndef)
                lObj.AddValue("message", lMessage);
            return lObj;
        }

        public object URIErrorCtor(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lUndef = (0 == args.Length) || (args[0] == Undefined.Instance);
            var lMessage = lUndef ? null : Utilities.GetArgAsString(args, 0, aCaller);
            var lObj = aSelf as EcmaScriptObject;
            if ((lObj == null) || (lObj.Class != "URIError"))
                lObj = new EcmaScriptObject(this, (EcmaScriptObject)URIError.Values["prototype"].Value) { Class = "URIError" };
            if (!lUndef)
                lObj.AddValue("message", lMessage);
            return lObj;
        }
    }
}