using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptNumberObject : EcmaScriptFunctionObject
    {
        public EcmaScriptNumberObject(GlobalObject aScope, string aOriginalName, InternalDelegate aDelegate, int aLength, bool aStrict = false, bool aNoProto = false)
            : base(aScope, aOriginalName, aDelegate, aLength, aStrict, aNoProto)
        {

        }

        public override object Call(ExecutionContext context, params object[] args)
        {
            return Root.NumberCall(context, this, args);
        }

        public override object Construct(ExecutionContext context, params object[] args)
        {
            return Root.NumberCtor(context, this, args);
        }
    }
}