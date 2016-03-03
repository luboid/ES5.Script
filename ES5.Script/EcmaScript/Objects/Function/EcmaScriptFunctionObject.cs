using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptFunctionObject : EcmaScriptBaseFunctionObject
    {
        InternalDelegate fDelegate;

        public InternalDelegate Delegate { get { return fDelegate; } }

        // TestCases/chapter15/15.7/15.7.3/15.7.3-2.js
        // it was new EcmaScriptObject(aScope, aScope.Root.FunctionPrototype)
        public EcmaScriptFunctionObject(GlobalObject aScope, string aOriginalName, InternalDelegate aDelegate, int aLength, bool aStrict = false, bool aNoProto = false)
            : base(aScope, aScope.Root.FunctionPrototype)
        {
            Class = "Function";
            fOriginalName = aOriginalName;
            fDelegate = aDelegate;

            if (aLength > -1)
                Values.Add("length", PropertyValue.NotAllFlags(aLength));

            if (aNoProto)
            {
                //DefineOwnProperty("prototype", new PropertyValue(PropertyAttributes.Writable, Undefined.Instance)); If it is do not exists it is undefined
            }
            else
            {
                var lProto = new EcmaScriptObject(aScope);
                lProto.DefineOwnProperty("constructor", new PropertyValue(PropertyAttributes.Writable | PropertyAttributes.Configurable, this));
                DefineOwnProperty("prototype", new PropertyValue(PropertyAttributes.Writable, lProto));
            }

            if (aStrict)
            {
                DefineOwnProperty("caller", new PropertyValue(PropertyAttributes.None, aScope.Thrower, aScope.Thrower));
                DefineOwnProperty("arguments", new PropertyValue(PropertyAttributes.None, aScope.Thrower, aScope.Thrower));
            }
        }


        public override object Construct(ExecutionContext context, params object[] args)
        {
            var lRes = new EcmaScriptObject(Root);
            lRes.Prototype = (EcmaScriptObject)Get("prototype", context);
            if (lRes.Prototype == null)
                Root.RaiseNativeError(NativeErrorType.TypeError, "No construct method");

            var result = fDelegate(context, lRes, args);
            if (!(result is EcmaScriptObject)) result = lRes;
            return result;
        }



        public override object CallEx(ExecutionContext context, object athis, params object[] args)
        {
            return fDelegate(context, athis, args);
        }


        public override object Call(ExecutionContext context, params object[] args)
        {
            return fDelegate(context, this, args);
        }
    }
}
