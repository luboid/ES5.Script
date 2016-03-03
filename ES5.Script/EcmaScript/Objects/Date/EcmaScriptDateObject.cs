using ES5.Script.EcmaScript.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptDateObject : EcmaScriptFunctionObject
    {
        public EcmaScriptDateObject(GlobalObject aScope, string aOriginalName, InternalDelegate aDelegate, int aLength, bool aStrict = false, bool aNoProto = false)
            : base(aScope, aOriginalName, aDelegate, aLength, aStrict, aNoProto)
        {
            Class = "Date";
        }

        public override object Call(ExecutionContext context, params object[] args)
        {
            return Root.DateCall(context, this, args);
        }

        public override object Construct(ExecutionContext context, params object[] args)
        {
            return Root.DateCtor(context, this, args);
        }
    }
}
