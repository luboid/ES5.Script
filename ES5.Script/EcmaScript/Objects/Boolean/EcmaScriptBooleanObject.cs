using ES5.Script.EcmaScript.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptBooleanObject : EcmaScriptFunctionObject
    {
        public EcmaScriptBooleanObject(GlobalObject aScope, string aOriginalName, InternalDelegate aDelegate, int aLength, bool aStrict = false, bool aNoProto = false)
            : base(aScope, aOriginalName, aDelegate, aLength, aStrict, aNoProto)
        {
            Class = "Boolean";
        }

        public override object Call(ExecutionContext context, params object[] args)
        {
            return Root.BooleanCall(context, this, args);
        }

        public override object Construct(ExecutionContext context, params object[] args)
        {
            return Root.BooleanCtor(context, this, args);
        }
    }
}
