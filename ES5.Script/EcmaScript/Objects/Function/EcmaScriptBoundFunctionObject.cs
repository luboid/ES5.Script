using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptBoundFunctionObject : EcmaScriptBaseFunctionObject
    {
        InternalFunctionDelegate fFunc;
        EcmaScriptBaseFunctionObject fFunc2;
        object fNewSelf;
        object[] fNewArgs;
        EcmaScriptInternalFunctionObject fOriginal;

        public EcmaScriptBoundFunctionObject(GlobalObject aGlobal, EcmaScriptInternalFunctionObject aFunc, object[] args)
            : base(aGlobal)
        {
            Prototype = aGlobal.FunctionPrototype;
            fFunc = ((EcmaScriptInternalFunctionObject)aFunc)?.Delegate;
            Class = "Function";
            Scope = aFunc.Scope;
            var lProto = new EcmaScriptObject(aGlobal);
            lProto.DefineOwnProperty("constructor", new PropertyValue(PropertyAttributes.Writable | PropertyAttributes.Configurable, this));
            var lLength = Utilities.GetObjAsInteger(aFunc.Get("length"), aGlobal.ExecutionContext);

            fOriginal = aFunc;
            lLength = lLength - (args.Length - 1);
            if (lLength < 0) lLength = 0;
            fNewSelf = Utilities.GetArg(args, 0);
            if (args.Length == 0)
                fNewArgs = Utilities.EmptyParams;
            else
            {
                fNewArgs = new Object[args.Length - 1];
                Array.Copy(args, 1, fNewArgs, 0, fNewArgs.Length);
            };
            Values.Add("length", PropertyValue.NotAllFlags(lLength));
            DefineOwnProperty("prototype", new PropertyValue(PropertyAttributes.Writable, lProto));
            DefineOwnProperty("caller", new PropertyValue(PropertyAttributes.None, aGlobal.Thrower, aGlobal.Thrower));
            DefineOwnProperty("arguments", new PropertyValue(PropertyAttributes.None, aGlobal.Thrower, aGlobal.Thrower));
        }

        public EcmaScriptBoundFunctionObject(EnvironmentRecord aScope, GlobalObject aGlobal, EcmaScriptBaseFunctionObject aFunc, object[] args)
            : base(aGlobal)
        {
            fFunc2 = aFunc;
            Class = "Function";
            Scope = aScope;

            var lProto = new EcmaScriptObject(aGlobal);
            lProto.DefineOwnProperty("constructor", new PropertyValue(PropertyAttributes.Writable | PropertyAttributes.Configurable, this));
            var lLength = Utilities.GetObjAsInteger(aFunc.Get("length"), aGlobal.ExecutionContext);


            lLength = lLength - (args.Length - 1);
            if (lLength < 0) lLength = 0;

            fNewSelf = Utilities.GetArg(args, 0);
            if (args.Length == 0)
                fNewArgs = Utilities.EmptyParams;
            else
            {
                fNewArgs = new Object[args.Length - 1];
                Array.Copy(args, 1, fNewArgs, 0, fNewArgs.Length);
            }

            Values.Add("length", PropertyValue.NotAllFlags(lLength));
            DefineOwnProperty("prototype", new PropertyValue(PropertyAttributes.Writable, lProto));
            DefineOwnProperty("caller", new PropertyValue(PropertyAttributes.None, aGlobal.Thrower, aGlobal.Thrower));
            DefineOwnProperty("arguments", new PropertyValue(PropertyAttributes.None, aGlobal.Thrower, aGlobal.Thrower));
        }

        public override object Call(ExecutionContext context, params object[] args)
        {
            var callArgs = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(fNewArgs, args));
            if (fFunc2 == null)
                return fFunc(new ExecutionContext(Scope, false), fNewSelf, callArgs, fOriginal);

            return fFunc2.CallEx(new ExecutionContext(Scope, false), fNewSelf, callArgs);
        }

        public override object CallEx(ExecutionContext context, object athis, params object[] args)
        {
            return Call(context, args);
        }
    }
}
