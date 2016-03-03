using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public partial class GlobalObject : EcmaScriptObject
    {
        public readonly static string BaseString = "0123456789abcdefghijklmnopqrstuvwxyz";

        public EcmaScriptObject CreateNumber()
        {
            var result = Get("Number") as EcmaScriptObject;
            if (result != null) return result;

            result = new EcmaScriptNumberObject(this, "Number", NumberCall, 1) { Class = "Number" };
            Values.Add("Number", PropertyValue.NotEnum(result));
            result.Values.Add("MAX_VALUE", PropertyValue.NotAllFlags(Double.MaxValue));
            result.Values.Add("MIN_VALUE", PropertyValue.NotAllFlags(Double.Epsilon));
            result.Values.Add("NaN", PropertyValue.NotAllFlags(Double.NaN));
            result.Values.Add("NEGATIVE_INFINITY", PropertyValue.NotAllFlags(Double.NegativeInfinity));
            result.Values.Add("POSITIVE_INFINITY", PropertyValue.NotAllFlags(Double.PositiveInfinity));

            NumberPrototype = new EcmaScriptObject(this) { Class = "Number", Prototype = ObjectPrototype };
            NumberPrototype.Values.Add("constructor", PropertyValue.NotEnum(result));
            NumberPrototype.Values.Add("toString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toString", NumberToString, 0)));
            NumberPrototype.Values.Add("toLocaleString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toLocaleString", NumberLocaleString, 0)));
            NumberPrototype.Values.Add("toFixed", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toFixed", NumberToFixed, 1)));
            NumberPrototype.Values.Add("toExponential", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toExponential", NumberToExponential, 1)));
            NumberPrototype.Values.Add("toPrecision", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toPrecision", NumberToPrecision, 1)));
            NumberPrototype.Values.Add("valueOf", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "valueOf", NumberValueOf, 0)));

            result.Values["prototype"] = PropertyValue.NotAllFlags(NumberPrototype);

            return result;
        }

        public object NumberCall(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return args.Length == 0 ? 0.0 : Utilities.GetArgAsDouble(args, 0, aCaller);
        }

        public object NumberCtor(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lVal = args.Length == 0 ? 0.0 : Utilities.GetArgAsDouble(args, 0, aCaller);
            return new EcmaScriptObject(this, NumberPrototype) { Class = "Number", Value = lVal };
        }

        public object NumberToString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lVal = aSelf as EcmaScriptObject;
            if ((lVal == null) && ((aSelf is Double) || (aSelf is int)))
                lVal = (EcmaScriptObject)NumberCtor(aCaller, NumberPrototype, aSelf);

            if ((lVal == null) || (lVal.Class != "Number"))
                RaiseNativeError(NativeErrorType.TypeError, "number.prototype.valueOf is not generic");

            var lRadix = 10;
            if (args.Length > 0) lRadix = Utilities.GetArgAsInteger(args, 0, aCaller);
            if ((lRadix < 2) || (lRadix > 36))
                RaiseNativeError(NativeErrorType.RangeError, "Radix parameter should be between 2 and 36");

            if (lRadix == 10)
                return Utilities.GetObjAsDouble(lVal.Value, aCaller)
                    .ToString(System.Globalization.NumberFormatInfo.InvariantInfo);

            if (lRadix == 2 || lRadix == 16 || lRadix == 8)
                return Convert.ToString(Utilities.GetObjAsInt64(lVal.Value, aCaller), lRadix);

            //var value = (ulong)Utilities.GetObjAsInt64(lVal.Value, aCaller);
            var value = Utilities.GetObjAsInt64(lVal.Value, aCaller);
            if (value == 0)
                return "0";

            var sign = "";
            if (value < 0)
            {
                sign = "-";
                value = -value;
            }

            var result = "";
            while (value != 0)
            {
                result = BaseString[(int)(value % lRadix)] + result;
                Utilities.DivRem(value, lRadix, out value);
            }
            return sign + result;
        }


        public object NumberValueOf(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            // &self.GetType() in [ .. ] doesn"t compile on Silverlight
            var lValueType = aSelf.GetType();

            if (lValueType == typeof(Double))
                return (double)aSelf;

            if ((lValueType == typeof(Int32)) || (lValueType == typeof(Int64)) || (lValueType == typeof(UInt32)) || (lValueType == typeof(UInt64)))
                return Convert.ChangeType(aSelf, typeof(Double), System.Globalization.CultureInfo.InvariantCulture);

            var lValue = aSelf as EcmaScriptObject;
            if ((null == lValue) || (lValue.Class != "Number"))
                RaiseNativeError(NativeErrorType.TypeError, "Number.prototype.valueOf is not generic");

            return lValue.Value;
        }


        public object NumberLocaleString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Utilities.GetObjAsDouble(aSelf, aCaller).ToString();
        }

        public object NumberToFixed(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lFrac = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lValue = Utilities.GetObjAsDouble(aSelf, aCaller);
            return lValue.ToString("F" + lFrac.ToString(), System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public object NumberToExponential(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lFrac = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lValue = Utilities.GetObjAsDouble(aSelf, aCaller);
            return lValue.ToString("E" + lFrac.ToString(), System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public object NumberToPrecision(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lFrac = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lValue = Utilities.GetObjAsDouble(aSelf, aCaller);
            return lValue.ToString("N" + lFrac.ToString(), System.Globalization.NumberFormatInfo.InvariantInfo);
        }
    }
}
