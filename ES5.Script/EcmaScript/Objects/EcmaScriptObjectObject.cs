using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptObjectObject : EcmaScriptBaseFunctionObject
    {
        public EcmaScriptObjectObject(GlobalObject aOwner, string aName) :
            base(aOwner, new EcmaScriptObject(aOwner, aOwner.FunctionPrototype))
        {            
            Class = "Function";
            fOriginalName = aName;
            Values.Add("length", PropertyValue.NotAllFlags(1));
        }

        public override object Call(ExecutionContext context, params object[] args)
        {
            var lVal = Utilities.GetArg(args, 0);
            if ((lVal != null) || (lVal == Undefined.Instance))
                return Construct(context, null, args);

            return Utilities.ToObject(context, lVal);
        }

        public override object Construct(ExecutionContext context, params object[] args)
        {
            if ((args.Length != 0) && (args[0] != null) && (args[0] != Undefined.Instance))
            {
                if (args[0] is EcmaScriptObject)
                    return args[0];

                if ((args[0] is String) ||
                    (args[0] is Int32) ||
                    (args[0] is Int64) ||
                    (args[0] is Double) ||
                    (args[0] is Boolean))
                    return Utilities.ToObject(context, args[0]);
            }
            return new EcmaScriptObject(Root);
        }

        public override object CallEx(ExecutionContext context, object aSelf, params object[] args)
        {
            var lVal = Utilities.GetArg(args, 0);
            if ((lVal != null) || (lVal == Undefined.Instance)) 
                return Construct(context, null, args);

            return Utilities.ToObject(context, lVal);
        }

    }
}
