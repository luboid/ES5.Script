using ES5.Script.EcmaScript.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public partial class GlobalObject : EcmaScriptObject
    {
        public EcmaScriptObject CreateFunction()
        {
            var result = Get("Function") as EcmaScriptObject;
            if (result != null) return result;

            result = new EcmaScriptFunctionObject(this, null, FunctionCtor, 1) { Class = "Function" };
            Values.Add("Function", PropertyValue.NotEnum(result));

            result.Values["prototype"] = PropertyValue.NotAllFlags(FunctionPrototype);

            FunctionPrototype.Values["constructor"] = PropertyValue.NotEnum(result);
            FunctionPrototype.Values.Add("toString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toString", FunctionToString, 0, false, true)));
            FunctionPrototype.Values.Add("apply", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "apply", FunctionApply, 2, false, true)));
            FunctionPrototype.Values.Add("call", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "call", FunctionCall, 1, false, true)));
            FunctionPrototype.Values.Add("bind", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "bind", FunctionBind, 1, false, true)));

            return result;
        }


        public object FunctionCtor(ExecutionContext aCaller, object athis, params object[] args)
        {
            var lNames = "";
            var lBody = "";
            if (args.Length != 0) {
                for (int i = 0, l = args.Length - 2; i < l; i++)
                {
                    if (i == 0) lNames = Utilities.GetArgAsString(args, i, aCaller);
                    else
                        lNames = lNames + "," + Utilities.GetArgAsString(args, i, aCaller);
                }
                lBody = Utilities.GetArgAsString(args, args.Length - 1, aCaller);
            }

            var lTokenizer = new Tokenizer();
            var lParser = new Parser();
            lTokenizer.Error += lParser.fTok_Error;
            lTokenizer.SetData(lNames, "Function Constructor Names");
            lTokenizer.Error -= lParser.fTok_Error;

            var lParams = new List<ParameterDeclaration>();
            if (lTokenizer.Token != TokenKind.EOF)
            {
                while (true) {
                    if (lTokenizer.Token != TokenKind.Identifier) {
                        RaiseNativeError(NativeErrorType.SyntaxError, "Unknown token in parameter names");
                    }
                    lParams.Add(new ParameterDeclaration(lTokenizer.PositionPair, lTokenizer.TokenStr));
                    lTokenizer.Next();
                    if (lTokenizer.Token == TokenKind.Comma) {
                        lTokenizer.Next();
                    } else if (lTokenizer.Token == TokenKind.EOF) {
                        lTokenizer.Next();
                        break;
                    } else {
                        RaiseNativeError(NativeErrorType.SyntaxError, "Unknown token in parameter names");
                    }
                }
            }

            foreach (var el in lParser.Messages)
                if (el.IsError)
                    RaiseNativeError(NativeErrorType.SyntaxError, el.IntToString());

            lTokenizer.Error += lParser.fTok_Error;
            lTokenizer.SetData(lBody, "Function Body");
            lTokenizer.Error -= lParser.fTok_Error;

            var lCode = lParser.Parse(lTokenizer);
            foreach (var el in lParser.Messages)
                if (el.IsError)
                    RaiseNativeError(NativeErrorType.SyntaxError, el.IntToString());

            // var lFunc = new FunctionDeclarationElement(lCode.PositionPair, FunctionDeclarationType.None, null, lParams, lCode);
            var lFunc = new FunctionDeclarationElement(lCode.PositionPair, FunctionDeclarationType.None, null, lParams, lCode.Items);

            try
            {
                var func = (InternalFunctionDelegate)fParser.Parse(lFunc, false, null, lCode.Items);
                return new EcmaScriptInternalFunctionObject(this, fParser.fRoot, null, func, lFunc.Parameters.Count, lBody, aCaller.Strict);
            }
            catch(ScriptParsingException ex)
            {
                RaiseNativeError(NativeErrorType.SyntaxError, ex.Message);
                return null;
            }
        }

        public object FunctionToString(ExecutionContext aCaller, object athis, params object[] args)
        {
            var lthis = (EcmaScriptBaseFunctionObject)athis;
            if (lthis == null)
                RaiseNativeError(NativeErrorType.TypeError, "Function.prototype.toString() is not generic");

            if (lthis is EcmaScriptInternalFunctionObject)
                return ((EcmaScriptInternalFunctionObject)lthis).OriginalBody;

            return "function " + lthis?.Class + "() { }";
        }

        public object FunctionApply(ExecutionContext aCaller, object athis, params object[] args)
        {
            if (!(athis is EcmaScriptObject))
                RaiseNativeError(NativeErrorType.TypeError, "Function.prototype.apply is not generic");

            var lthis = (Object)Undefined.Instance;
            if (args.Length != 0)
                lthis = args[0];

            //if ((args.Length == 0) || !(args[0] is EcmaScriptObject))
            //    lthis = this;
            //else
            //    lthis = args[0];

            var largs = (object[])null;
            if (args.Length < 2)
            {
                largs = Utilities.EmptyParams;
            }
            else
            {
                var lArgObj = args[1] as EcmaScriptArrayObject;
                if (null == lArgObj)
                    RaiseNativeError(NativeErrorType.TypeError, "Array expected as second parameter.");

                largs = new Object[lArgObj.Length];
                for (uint i = 0, l = lArgObj.Length; i < l; i++)
                    largs[i] = lArgObj.Get(i.ToString(), aCaller, 2);
            }
            return ((EcmaScriptObject)athis).CallEx(aCaller, lthis, largs);
        }

        public object FunctionCall(ExecutionContext aCaller, object athis, params object[] args)
        {
            if (!(athis is EcmaScriptObject))
                RaiseNativeError(NativeErrorType.TypeError, "Function.prototype.call is not generic.");

            // only proxy parameters all of this need to be done in concrete functions
            // TestCases/chapter15/15.5/15.5.4/15.5.4.20/15.5.4.20-1-2.js
            // String.prototype.trim throws TypeError when string is null

            //var lthis = (Object)null;
            //if ((args.Length == 0) /*|| !(args[0] is EcmaScriptObject) DO NOT WORK 15.5.4.20-1-7.js BECAUSE OF THIS */)
            //    lthis = this;
            //else
            //    lthis = args[0];

            //// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Function/call
            //if (!aCaller.Strict && (null == lthis || lthis == Undefined.Instance))
            //    lthis = aCaller.Global;

            var lthis = (Object)Undefined.Instance;
            if (args.Length != 0)
                lthis = args[0];

            var largs = new Object[args.Length < 1 ? 0 : args.Length - 1];
            if (largs.Length > 0)
                Array.Copy(args, 1, largs, 0, largs.Length);

            return ((EcmaScriptObject)athis).CallEx(aCaller, lthis, largs);
        }

        public object FunctionBind(ExecutionContext aCaller, object athis, params object[] args)
        {
            var lthis = athis as EcmaScriptInternalFunctionObject;
            if (lthis == null) {
                var lthis2 = athis as EcmaScriptBaseFunctionObject;
                if (lthis2 != null) {
                    return new EcmaScriptBoundFunctionObject(aCaller.LexicalScope, this, lthis2, args);
                }
                RaiseNativeError(NativeErrorType.TypeError, "Target is not callable.");
            }
            return new EcmaScriptBoundFunctionObject(this, lthis, args);
        }

        public void CreateFunctionPrototype()
        {
            FunctionPrototype = new EcmaScriptFunctionObject(this, "Function", FunctionProtoCtor, -1, false, true) { Class = "Function" };
            FunctionPrototype.Prototype = ObjectPrototype;
        }

        public object FunctionProtoCtor(ExecutionContext aCaller, object athis, params object[] args)
        {
            return Undefined.Instance;
        }
    }
}
