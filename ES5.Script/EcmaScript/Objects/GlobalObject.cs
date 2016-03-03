using ES5.Script.EcmaScript.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public partial class GlobalObject : EcmaScriptObject
    {
        public readonly static MethodInfo Method_IncreaseFrame = typeof(GlobalObject).GetMethod("IncreaseFrame");
        public readonly static MethodInfo Method_DecreaseFrame = typeof(GlobalObject).GetMethod("DecreaseFrame");
        public readonly static MethodInfo Method_GetFunction = typeof(GlobalObject).GetMethod("GetFunction");

        ExecutionContext fExecutionContext;
        IDebugSink fDebug;

        internal EcmaScriptCompiler fParser;
        internal List<InternalFunctionDelegate> fDelegates = new List<InternalFunctionDelegate>();

        public EcmaScriptObject ObjectPrototype { get; set; }
        public EcmaScriptObject FunctionPrototype { get; set; }
        public EcmaScriptObject ArrayPrototype { get; set; }
        public EcmaScriptObject NumberPrototype { get; set; }
        public EcmaScriptObject StringPrototype { get; set; }
        public EcmaScriptObject DatePrototype { get; set; }
        public EcmaScriptObject BooleanPrototype { get; set; }
        public EcmaScriptObject RegExpPrototype { get; set; }
        public EcmaScriptObject ErrorPrototype { get; set; }
        public EcmaScriptFunctionObject Thrower { get; set; }
        public EcmaScriptObject NativePrototype { get; set; }
        public EcmaScriptFunctionObject NotStrictGlobalEvalFunc { get; set; }

        public int MaxFrames { get; set; } = 1024;
        public int FrameCount { get; set; }

        public GlobalObject()
            : this(null)
        { }

        public GlobalObject(EcmaScriptCompiler aParser)
            : base(null, null)
        {
            Root = this;
            fParser = aParser;
            Values.Add("NaN", PropertyValue.NotAllFlags(Double.NaN));
            Values.Add("Infinity", PropertyValue.NotAllFlags(Double.PositiveInfinity));
            Values.Add("undefined", PropertyValue.NotAllFlags(Undefined.Instance));

            CreateObject();
            CreateFunction();
            CreateMath();
            CreateArray();
            CreateNumber();
            CreateDate();
            CreateString();
            CreateBoolean();
            CreateRegExp();
            CreateError();
            CreateNativeError();
            CreateJSON();

            this.Prototype = ObjectPrototype;

            Thrower = new EcmaScriptFunctionObject(this, "ThrowTypeError", (ExecutionContext scope, object aSelf, object[] args) =>
            {
                RaiseNativeError(NativeErrorType.TypeError, "caller/arguments not available in strict mode");
                return null;
            }, 0, false);

            NotStrictGlobalEvalFunc = new EcmaScriptEvalFunctionObject(this, "eval", NotStrictGlobalEval, 1);
            // Add function prototype here first!
            Values.Add("eval", PropertyValue.NotEnum(new EcmaScriptEvalFunctionObject(this, "eval", eval, 1)));
            Values.Add("parseInt", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "parseInt", parseInt, 2)));
            Values.Add("parseFloat", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "parseFloat", parseFloat, 1)));
            Values.Add("isNaN", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "isNaN", isNaN, 1)));
            Values.Add("isFinite", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "isFinite", isFinite, 1)));
            Values.Add("decodeURI", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "decodeURI", decodeURI, 1)));
            Values.Add("encodeURI", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "encodeURI", encodeURI, 1)));
            Values.Add("escape", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "escape", escape, 1)));
            Values.Add("unescape", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "unescape", unescape, 1)));
            Values.Add("decodeURIComponent", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "decodeURIComponent", decodeURIComponent, 1)));
            Values.Add("encodeURIComponent", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "encodeURIComponent", encodeURIComponent, 1)));

            NativePrototype = new EcmaScriptObject(this);
            NativePrototype.Extensible = false;
            NativePrototype.Values["toString"] = new PropertyValue(PropertyAttributes.None,
                    new EcmaScriptFunctionObject(this, "toString", NativeToString, 0));
        }

        public ExecutionContext ExecutionContext
        {
            get
            {
                if (fExecutionContext == null)
                    fExecutionContext = new ExecutionContext(new ObjectEnvironmentRecord(null, this), false);
                return fExecutionContext;
            }
            set
            {
                fExecutionContext = value;
            }
        }

        public IDebugSink Debug
        {
            get
            {
                if (fDebug == null) fDebug = new DebugSink(); // dummy one
                return fDebug;
            }
            set
            {
                fDebug = value;
            }
        }

        public EcmaScriptCompiler Parser
        {
            get { return fParser; }
            set { fParser = value; }
        }

        public void IncreaseFrame()
        {
            if ((FrameCount + 1) > MaxFrames)
                RaiseNativeError(NativeErrorType.EvalError, "Stack overflow");

            FrameCount = FrameCount + 1;
        }

        public void DecreaseFrame()
        {
            FrameCount = FrameCount - 1;
        }

        public int StoreFunction(InternalFunctionDelegate aDelegate)
        {
            fDelegates.Add(aDelegate);
            return fDelegates.Count - 1;
        }

        public InternalFunctionDelegate GetFunction(int i)
        {
            return fDelegates[i];
        }

        public object eval(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return InnerEval(aCaller, true, aSelf, args); // strict; this is called through the 
        }

        public object NotStrictGlobalEval(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            if ((aSelf == Undefined.Instance) || (aSelf == null))
            {
                aSelf = aCaller.LexicalScope.ImplicitThisValue();
                if ((aSelf == Undefined.Instance) || (aSelf == null))
                    aSelf = this;
            }

            return InnerEval(aCaller, false, aSelf, args);
        }

        public ExecutionContext InnerExecutionContext(ExecutionContext caller, bool strict)
        {
            if (caller.Strict || strict)
                return new ExecutionContext(new DeclarativeEnvironmentRecord(caller.LexicalScope, this), true);

            return caller;
        }


        public InternalDelegate InnerCompile(bool strict, string script)
        {
            if (String.IsNullOrEmpty(script))
                return null;

            //var lTokenizer = new Tokenizer();
            //var lParser = new Parser();
            //lTokenizer.Error += lParser.fTok_Error;
            //lTokenizer.SetData(script, "<eval>");
            //lTokenizer.Error -= lParser.fTok_Error;

            try
            {
                //var lElement = lParser.Parse(lTokenizer);
                //foreach (var el in lParser.Messages) {
                //    if (el.IsError)
                //        RaiseNativeError(NativeErrorType.SyntaxError, el.ToString());// this will reveal real error
                //}

                return (InternalDelegate)fParser.EvalParse(strict, script);
            }
            catch (ScriptParsingException e) {
                RaiseNativeError(NativeErrorType.SyntaxError, e.Message);
                return null;
            }
        }


        public object InnerEval(ExecutionContext caller, bool strict, object aSelf, params object[] args)
        {
            if ((args.Length < 0) || !(args[0] is String))
                return (args.Length == 0 ? Undefined.Instance : args[0]);

            var lScript = (String)args[0];

            var lContext = InnerExecutionContext(caller, strict);
            var lEval = InnerCompile(lContext.Strict, lScript);

            return lEval(lContext, aSelf, Utilities.EmptyParams);
        }


        public object InnerEval(ExecutionContext caller, bool strict, object aSelf, InternalDelegate evalDelegate)
        {
            if (null == evalDelegate)
                return Undefined.Instance;

            var lContext = InnerExecutionContext(caller, strict);

            return evalDelegate(lContext, aSelf, Utilities.EmptyParams);
        }


        public object InnerEval(bool strict, params object[] args)
        {
            return InnerEval(ExecutionContext, strict, this, args);
        }


        public object InnerEval(bool strict, InternalDelegate evalDelegate)
        {
            return InnerEval(ExecutionContext, strict, this, evalDelegate);
        }


        public object parseInt(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lVal = String.Empty;
            var lRadix = 0;
            if (args.Length < 1)
                lVal = "0";
            else {
                lVal = Utilities.GetArgAsString(args, 0, aCaller);
                if (args.Length < 2)
                    lRadix = 0;
                else if ((args[1] == null) || (args[1] == Undefined.Instance))
                    lRadix = 0;
                else
                    lRadix = Utilities.GetArgAsInteger(args, 1, aCaller);
            }
            var lSign = 1;
            lVal = lVal.Trim();
            if (lVal.StartsWith("-")) {
                lSign = -1;
                lVal = lVal.Substring(1);
            } else if (lVal.StartsWith("+"))
                lVal = lVal.Substring(1);

            if ((lRadix == 0) || (lRadix == 16))
                if (lVal.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase)) { lRadix = 16; lVal = lVal.Substring(2); }
            if (lRadix == 0) lRadix = 10;
            if ((lRadix < 2) || (lRadix > 36)) return Double.NaN;
            for (int i = 0, l = lVal.Length; i < l; i++) {
                var n = BaseString.IndexOf(lVal.Substring(i, 1), 0, lRadix, StringComparison.OrdinalIgnoreCase);
                if (n < 0) {
                    lVal = lVal.Substring(0, i);
                    break;
                }
            }
            if (lVal == "") return Double.NaN;

            //var lRes = 0L;
            /*
            if lRadix = 10 then {
              if Length(lVal) <= 20 then {
                if not Int64.TryParse(lVal, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out lRes) then
                 return Double.NaN;
                 return lSign * lRes;
               }
            end else if lRadix = 16 then {
              if Length(lVal) <= 16 then {
                if not Int64.TryParse(lVal, System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.InvariantInfo, out lRes) then
                 return Double.NaN;
                return lSign * lRes;
              }
            }*/
            var lResD = 0D;

            lResD = 0;
            for (int i = 0, l = lVal.Length; i < l; i++)
            {
                var n = BaseString.IndexOf(lVal.Substring(i, 1), 0, lRadix, StringComparison.InvariantCultureIgnoreCase);
                lResD = lResD * lRadix + n;
            }
            return lSign * lResD;
        }

        public object parseFloat(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lVal = Utilities.GetArgAsString(args, 0, aCaller);
            return Utilities.ParseDouble(lVal, false);
        }

        public object isNaN(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Double.IsNaN(Utilities.GetArgAsDouble(args, 0, aCaller));
        }

        public object isFinite(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lVal = Utilities.GetArgAsDouble(args, 0, aCaller);
            return !Double.IsInfinity(lVal) && !Double.IsNaN(lVal);
        }

        public object ObjectToString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = aSelf as EcmaScriptObject;
            if (lValue == null)
                return "[object null]";
            else
                return "[object " + lValue.Class + "]";
        }

        public object ObjectValueOf(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = aSelf as EcmaScriptObject;
            if (lValue == null)
                return Undefined.Instance;
            else
                return lValue.Value ?? lValue;
        }

        public object ObjectIsPrototypeOf(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lThis = Utilities.ToObject(aCaller, aSelf);
            if (args.Length == 0) return false;
            var lValue = args[0] as EcmaScriptObject;
            if (lValue == null) return null;
            do
            {
                lValue = lValue.Prototype;
                if (lThis == lValue)
                    return true;
            }
            while (lValue != null);
            return false;
        }

        public EcmaScriptObject CreateObject()
        {
            var result = Get("Object") as EcmaScriptObject;
            if (result != null) return result;

            ObjectPrototype = new EcmaScriptObject(this);
            CreateFunctionPrototype();


            result = new EcmaScriptObjectObject(this, "Object");
            Values.Add("Object", PropertyValue.NotEnum(result));

            result.Values["prototype"] = PropertyValue.NotAllFlags(ObjectPrototype);
            result.Values.Add("getPrototypeOf", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getPrototypeOf", ObjectgetPrototypeOf, 1)));
            result.Values.Add("getOwnPropertyDescriptor", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getOwnPropertyDescriptor", ObjectgetOwnPropertyDescriptor, 2)));
            result.Values.Add("getOwnPropertyNames", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getOwnPropertyNames", ObjectgetOwnPropertyNames, 1)));
            result.Values.Add("create", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "create", ObjectCreate, 2)));
            result.Values.Add("defineProperty", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "defineProperty", ObjectdefineProperty, 3)));
            result.Values.Add("defineProperties", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "defineProperties", ObjectdefineProperties, 2)));
            result.Values.Add("seal", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "seal", ObjectSeal, 1)));
            result.Values.Add("freeze", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "freeze", ObjectFreeze, 1)));
            result.Values.Add("preventExtensions", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "preventExtensions", ObjectPreventExtensions, 1)));
            result.Values.Add("isSealed", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "isSealed", ObjectisSealed, 1)));
            result.Values.Add("isFrozen", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "isFrozen", ObjectisFrozen, 1)));
            result.Values.Add("isExtensible", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "isExtensible", ObjectisExtensible, 1)));
            result.Values.Add("keys", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "keys", ObjectKeys, 1)));

            ObjectPrototype.Values["constructor"] = PropertyValue.NotEnum(result);

            ObjectPrototype.Values.Add("toString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toString", ObjectToString, 0, false, true)));
            ObjectPrototype.Values.Add("toLocaleString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toLocaleString", ObjectToLocaleString, 0, false, true)));

            ObjectPrototype.Values.Add("valueOf", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "valueOf", ObjectValueOf, 0, false, true)));

            ObjectPrototype.Values.Add("isPrototypeOf", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "isPrototypeOf", ObjectIsPrototypeOf, 1, false, true)));
            ObjectPrototype.Values.Add("propertyIsEnumerable", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "propertyIsEnumerable", ObjectPropertyIsEnumerable, 1, false, true)));
            ObjectPrototype.Values.Add("hasOwnProperty", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "hasOwnProperty", ObjectHasOwnProperty, 1, false, true)));

            return result;
        }


        public override string ToString()
        {
            return "[object global]";
        }

        public object encodeURI(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var val = Utilities.GetArgAsString(args, 0, aCaller);
            try
            {
                return Utilities.UrlEncode(val);
            }
            catch
            {
                RaiseNativeError(NativeErrorType.URIError, "Invalid input");
                return null;
            }
        }

        public object decodeURI(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var val = Utilities.GetArgAsString(args, 0, aCaller);
            var result = (object)null;
            try
            {
                result = Utilities.UrlDecode(val, false);
            }
            catch
            { }

            if (result == null)
                RaiseNativeError(NativeErrorType.URIError, "Invalid input");

            return result;
        }

        public object encodeURIComponent(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var val = Utilities.GetArgAsString(args, 0, aCaller);
            try
            {
                return Utilities.UrlEncodeComponent(val);
            }
            catch
            {
                RaiseNativeError(NativeErrorType.URIError, "Invalid input");
                return null;
            }
        }

        public object decodeURIComponent(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var val = Utilities.GetArgAsString(args, 0, aCaller);
            var result = (object)null;
            try
            {
                result = Utilities.UrlDecode(val, true);
            }
            catch
            { }

            if (result == null)
                RaiseNativeError(NativeErrorType.URIError, "Invalid input");

            return result;
        }

        public object ObjectgetPrototypeOf(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetArgAsEcmaScriptObject(args, 0, aCaller);
            if (lWork == null)
                RaiseNativeError(NativeErrorType.TypeError, "Type(O) is not a Object");

            return lWork.Prototype;
        }

        public object ObjectgetOwnPropertyDescriptor(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetArgAsEcmaScriptObject(args, 0, aCaller);
            if (lWork == null) RaiseNativeError(NativeErrorType.TypeError, "Type(O) is not Object");
            var lName = Utilities.GetArgAsString(args, 1, aCaller);


            var lPV = (PropertyValue)null;
            if (lWork.Values.TryGetValue(lName, out lPV))
                return FromPropertyDescriptor(lPV);

            return Undefined.Instance;
        }

        public object ObjectgetOwnPropertyNames(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetArgAsEcmaScriptObject(args, 0, aCaller);
            if (lWork == null)
                RaiseNativeError(NativeErrorType.TypeError, "Type(O) is not Object");

            var lRes = (EcmaScriptArrayObject)ArrayCtor(aCaller, 0);
            lRes.AddValues(lWork.Values.Keys.OrderBy(a => a).ToArray());
            return lRes;
        }

        public object ObjectCreate(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetArgAsEcmaScriptObject(args, 0, aCaller);
            if (lWork == null)
                RaiseNativeError(NativeErrorType.TypeError, "Type(O) is not Object");

            var lRes = new EcmaScriptObject(this, lWork);

            var lArgs = Utilities.GetArgAsEcmaScriptObject(args, 1, aCaller);
            if (lArgs != null)
                ObjectdefineProperties(aCaller, null, lRes, lArgs);

            return lRes;
        }

        public object ObjectdefineProperty(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetArgAsEcmaScriptObject(args, 0, aCaller);
            if (lWork == null)
                RaiseNativeError(NativeErrorType.TypeError, "Type(O) is not Object");
            var lName = Utilities.GetArgAsString(args, 1, aCaller);
            var lData = Utilities.GetArgAsEcmaScriptObject(args, 2, aCaller);
            if (lData == null)
                RaiseNativeError(NativeErrorType.TypeError, "Type(O) is not Object");

            var lValue = ToPropertyDescriptor(lData);
            lWork.DefineOwnProperty(lName, lValue, true);
            return lWork;
        }

        public object ObjectdefineProperties(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetArgAsEcmaScriptObject(args, 0, aCaller);
            if (lWork == null) RaiseNativeError(NativeErrorType.TypeError, "Type(O) is not Object");
            var lProps = Utilities.GetArgAsEcmaScriptObject(args, 1, aCaller);
            if (lProps == null) RaiseNativeError(NativeErrorType.TypeError, "Type(Properties) is not Object");

            foreach (var el in lProps.Values.Keys)
            {
                var lValue = Utilities.GetObjAsEcmaScriptObject(lProps.Get(el, aCaller, 0), aCaller);
                if (lValue == null) RaiseNativeError(NativeErrorType.TypeError, "Type(Element) is not Object");
                lWork.DefineOwnProperty(el, ToPropertyDescriptor(lValue), true);
            }

            return lWork;
        }

        public object ObjectSeal(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetArgAsEcmaScriptObject(args, 0, aCaller);
            if (lWork == null) RaiseNativeError(NativeErrorType.TypeError, "Type(O) is not Object");
            foreach (var el in lWork.Values)
                el.Value.Attributes = el.Value.Attributes & ~PropertyAttributes.Enumerable;

            lWork.Extensible = false;

            return lWork;
        }

        public object ObjectFreeze(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetArgAsEcmaScriptObject(args, 0, aCaller);
            if (lWork == null) RaiseNativeError(NativeErrorType.TypeError, "Type(O) is not Object");
            foreach (var el in lWork.Values)
                el.Value.Attributes = el.Value.Attributes & ~(PropertyAttributes.Enumerable | PropertyAttributes.Writable);
            lWork.Extensible = false;

            return lWork;
        }

        public object ObjectPreventExtensions(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetArgAsEcmaScriptObject(args, 0, aCaller);
            if (lWork == null) RaiseNativeError(NativeErrorType.TypeError, "Type(O) is not Object");
            lWork.Extensible = false;

            return lWork;
        }

        public object ObjectisSealed(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetArgAsEcmaScriptObject(args, 0, aCaller);
            if (lWork == null) RaiseNativeError(NativeErrorType.TypeError, "Type(O) is not Object");
            if (lWork.Extensible) return false;
            foreach (var el in lWork.Values)
                if ((PropertyAttributes.Configurable & el.Value.Attributes) != 0) return false;
            return true;
        }

        public object ObjectisFrozen(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetArgAsEcmaScriptObject(args, 0, aCaller);
            if (lWork == null)
                RaiseNativeError(NativeErrorType.TypeError, "Type(O) is not Object");
            if (lWork.Extensible) return false;
            foreach (var el in lWork.Values) {
                if (0 != (PropertyAttributes.Configurable & el.Value.Attributes)) return false;
                if (IsDataDescriptor(el.Value) && (0 != (PropertyAttributes.Writable & el.Value.Attributes)))
                    return false;
            }
            return true;
        }

        public object ObjectisExtensible(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetArgAsEcmaScriptObject(args, 0, aCaller);
            if (lWork == null)
                RaiseNativeError(NativeErrorType.TypeError, "Type(O) is not Object");
            return lWork.Extensible;
        }

        public object ObjectKeys(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetArgAsEcmaScriptObject(args, 0, aCaller);
            if (lWork == null)
                RaiseNativeError(NativeErrorType.TypeError, "Type(O) is not Object");

            var lResult = new EcmaScriptArrayObject(0, this);
            foreach (var el in lWork.Values.Keys)
                lResult.AddValue(el);

            return lResult;
        }


        public object ObjectToLocaleString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetObjAsEcmaScriptObject(aSelf, aCaller);
            if (lWork == null) RaiseNativeError(NativeErrorType.TypeError, "this is not Object");
            var lToString = lWork.Get("toString", aCaller, 0) as EcmaScriptFunctionObject;
            if (lToString == null)
                RaiseNativeError(NativeErrorType.TypeError, "toString is not callable");
            return lToString.CallEx(aCaller, aSelf, Utilities.EmptyParams);
        }

        public object ObjectHasOwnProperty(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetObjAsEcmaScriptObject(aSelf, aCaller);
            if (lWork == null) RaiseNativeError(NativeErrorType.TypeError, "this is not Object");
            return lWork.GetOwnProperty(Utilities.GetArgAsString(args, 0, aCaller)) != null;
        }

        public object ObjectPropertyIsEnumerable(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetObjAsEcmaScriptObject(aSelf, aCaller);
            if (lWork == null)
                RaiseNativeError(NativeErrorType.TypeError, "this is not Object");
            var lProp = lWork.GetOwnProperty(Utilities.GetArgAsString(args, 0, aCaller));

            return (lProp != null) && (PropertyAttributes.Enumerable & lProp.Attributes) != 0;
        }

        public object escape(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetArgAsString(args, 0, aCaller);
            var sb = new StringBuilder();
            for (int i = 0, l = lWork.Length; i < l; i++) {
                if (Utilities.InSets3(lWork[i], 'A', 'Z', 'a', 'z', '0', '9', '@', '*', '_', '+', '-', '.'))
                {
                    sb.Append(lWork[i]);
                }
                else
                {
                    int c = (int)lWork[i];
                    if (i < 256)
                    {
                        sb.Append("%");
                        sb.Append(c.ToString("x2"));
                    }
                    else
                    {
                        sb.Append("%u");
                        sb.Append(c.ToString("x5"));
                    }
                }
            }
            return sb.ToString();
        }

        public object unescape(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lWork = Utilities.GetArgAsString(args, 0, aCaller);
            var sb = new StringBuilder();
            var i = 0;var l = lWork.Length;
            while (i < l) {
                if (lWork[i] == '%')
                {
                    ++i;
                    var lTmp = 0;
                    if (i < lWork.Length)
                    {
                        if ((lWork[i] == 'u') && (i + 4 < lWork.Length))
                        {
                            ++i;
                            if (Int32.TryParse(lWork.Substring(i, 4), System.Globalization.NumberStyles.AllowHexSpecifier, System.Globalization.NumberFormatInfo.InvariantInfo, out lTmp))
                                sb.Append((char)lTmp);
                            i += 4;
                        }
                        else
                        {
                            if (Int32.TryParse(lWork.Substring(i, 2), System.Globalization.NumberStyles.AllowHexSpecifier, System.Globalization.NumberFormatInfo.InvariantInfo, out lTmp))
                                sb.Append((char)lTmp);
                            i += 2;
                        }
                    }
                }
                else
                {
                    sb.Append(lWork[i]);
                    ++i;
                }
            }
            return sb.ToString();
        }

        public object NativeToString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lObject = aSelf as EcmaScriptObjectWrapper;
            if (null == lObject)
                RaiseNativeError(NativeErrorType.ReferenceError, "native toString() is not generic");

            return lObject.Value?.ToString();
        }
    }
}
