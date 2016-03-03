using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptExceptionFunctionObject : EcmaScriptFunctionObject
    {
        public EcmaScriptExceptionFunctionObject(GlobalObject aScope, string aOriginalName, InternalDelegate aDelegate, int aLength, bool aStrict = false, bool aNoProto = false)
            : base(aScope, aOriginalName, aDelegate, aLength, aStrict, aNoProto)
        { }

        public override object Call(ExecutionContext context, params object[] args)
        {
            return Construct(context, args);
        }

        public override object CallEx(ExecutionContext context, object aSelf, params object[] args)
        {
            return Construct(context, args);
        }
    }
}
