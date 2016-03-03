using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptArrayObjectObject : EcmaScriptFunctionObject
    {
        public EcmaScriptArrayObjectObject(GlobalObject aOwner)
            : base(aOwner, "Array", aOwner.ArrayCtor, 1, false)
        { }


        public override object Construct(ExecutionContext context, params object[] args)
        {
            return Root.ArrayCtor(context, this, args);
        }
    }
}
