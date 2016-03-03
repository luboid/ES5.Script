using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptStringObject : EcmaScriptFunctionObject
    {
        public EcmaScriptStringObject(GlobalObject aScope, string aOriginalName, InternalDelegate aDelegate, int aLength, bool aStrict = false, bool aNoProto = false)
            : base(aScope, aOriginalName, aDelegate, aLength, aStrict, aNoProto)
        { }

        public override object Call(ExecutionContext context, params object[] args)
        {
            return Root.StringCall(context, this, args);
        }

        public override object Construct(ExecutionContext context, params object[] args)
        {
            return Root.StringCtor(context, this, args);
        }
    }
}