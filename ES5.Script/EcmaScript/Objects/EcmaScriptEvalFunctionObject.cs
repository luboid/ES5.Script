using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptEvalFunctionObject : EcmaScriptFunctionObject
    {
        public EcmaScriptEvalFunctionObject(GlobalObject aScope, string aOriginalName, InternalDelegate aDelegate, int aLength, bool aStrict = false, bool aNoProto = false)
            : base(aScope, aOriginalName, aDelegate, aLength, aStrict, aNoProto)
        { }
    }
}
