using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptBaseFunctionObject : EcmaScriptObject
    {
        protected string fOriginalName;

        public EcmaScriptBaseFunctionObject(GlobalObject obj):
            base(obj)
        { }

        public EcmaScriptBaseFunctionObject(GlobalObject obj, EcmaScriptObject aProto):
            base(obj, aProto)
        { }

        public EnvironmentRecord Scope { get; set; }
        public string OriginalName { get { return fOriginalName; } }
    }
}
