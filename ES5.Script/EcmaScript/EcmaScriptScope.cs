using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public class EcmaScriptScope : ScriptScope
    {
        public EcmaScriptScope(EnvironmentRecord aPrevious, GlobalObject aGlobal)
            : base(aPrevious, aGlobal)
        { }

        public override object TryWrap(object aValue)
        {
            return DoTryWrap(Global, aValue);
        }

        public static object DoTryWrap(GlobalObject Global, object aValue)
        {
            if ((aValue == null) || (aValue is EcmaScriptObject))
                return (aValue);

            if (aValue == Undefined.Instance)
                return (aValue);

            var lType = aValue.GetType();
            switch (Type.GetTypeCode(lType))
            {
                case TypeCode.Boolean: return(aValue);
                case TypeCode.Byte: return(Convert.ToInt32((byte)aValue));
                case TypeCode.Char: return(((char)aValue).ToString());
                case TypeCode.DateTime: return(Global.CreateDateObject(((DateTime)aValue).ToUniversalTime()));
                case TypeCode.Decimal: return(Convert.ToDouble((Decimal)aValue));
                case TypeCode.Double: return(aValue);
                case TypeCode.Int16: return(Convert.ToInt32((Int16)aValue));
                case TypeCode.Int32: return(aValue);
                case TypeCode.Int64: return(Convert.ToDouble((Int64)aValue));
                case TypeCode.SByte: return(Convert.ToInt32((SByte)aValue));
                case TypeCode.Single: return(Convert.ToDouble((Single)aValue));
                case TypeCode.String: return(aValue);
                case TypeCode.UInt16: return(Convert.ToInt32((UInt16)aValue));
                case TypeCode.UInt32: return(Convert.ToInt32((UInt32)aValue));
                case TypeCode.UInt64: return(Convert.ToDouble((UInt64)aValue));
            }// case
            return (new EcmaScriptObjectWrapper(aValue, lType, Global));
        }

    }
}
