using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ES5.Script.EcmaScript.Objects
{
    public partial class GlobalObject : EcmaScriptObject
    {
        Random fRandom;

        public EcmaScriptObject CreateMath()
        {
            var result = new EcmaScriptObject(this, null) { Class = "Math" };
            Values["Math"] = PropertyValue.NotEnum(result);


            result.Values.Add("E", PropertyValue.NotAllFlags(Math.E));
            result.Values.Add("PI", PropertyValue.NotAllFlags(Math.PI));
            result.Values.Add("SQRT1_2", PropertyValue.NotAllFlags(Math.Sqrt(0.5)));
            result.Values.Add("SQRT2", PropertyValue.NotAllFlags(Math.Sqrt(2)));

            result.Values.Add("LN10", PropertyValue.NotAllFlags(Math.Log(10)));
            result.Values.Add("LN2", PropertyValue.NotAllFlags(Math.Log(2)));
            result.Values.Add("LOG2E", PropertyValue.NotAllFlags(Math.Log(Math.E, 2)));
            result.Values.Add("LOG10E", PropertyValue.NotAllFlags(Math.Log(Math.E, 10)));

            result.Values.Add("abs", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "abs", Mathabs, 1)));
            result.Values.Add("acos", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "acos", Mathacos, 1)));
            result.Values.Add("asin", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "asin", Mathasin, 1)));
            result.Values.Add("atan", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "atin", Mathatan, 1)));
            result.Values.Add("atan2", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "atin2", Mathatan2, 2)));
            result.Values.Add("ceil", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "ceil", Mathceil, 1)));
            result.Values.Add("cos", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "cos", Mathcos, 1)));
            result.Values.Add("exp", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "exp", Mathexp, 1)));
            result.Values.Add("floor", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "floor", Mathfloor, 1)));
            result.Values.Add("log", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "log", Mathlog, 1)));
            result.Values.Add("max", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "max", MathMax, 2)));
            result.Values.Add("min", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "min", MathMin, 2)));
            result.Values.Add("pow", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "pow", Mathpow, 1)));
            result.Values.Add("random", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "random", Mathrandom, 0)));
            result.Values.Add("round", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "round", Mathround, 1)));
            result.Values.Add("sin", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "sin", Mathsin, 1)));
            result.Values.Add("sqrt", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "sqrt", Mathsqrt, 1)));
            result.Values.Add("tan", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "tan", Mathtan, 1)));

            return result;
        }

        public object Mathabs(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Math.Abs(Utilities.GetArgAsDouble(args, 0, aCaller));
        }

        public object Mathacos(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Math.Acos(Utilities.GetArgAsDouble(args, 0, aCaller));
        }

        public object Mathasin(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Math.Asin(Utilities.GetArgAsDouble(args, 0, aCaller));
        }

        public object Mathatan(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Math.Atan(Utilities.GetArgAsDouble(args, 0, aCaller));
        }

        public object Mathatan2(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Math.Atan2(Utilities.GetArgAsDouble(args, 0, aCaller), Utilities.GetArgAsDouble(args, 1, aCaller));
        }

        public object Mathceil(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Math.Ceiling(Utilities.GetArgAsDouble(args, 0, aCaller));
        }

        public object Mathcos(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Math.Cos(Utilities.GetArgAsDouble(args, 0, aCaller));
        }

        public object Mathexp(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Math.Exp(Utilities.GetArgAsDouble(args, 0, aCaller));
        }

        public object Mathfloor(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Math.Floor(Utilities.GetArgAsDouble(args, 0, aCaller));
        }

        public object Mathlog(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Math.Log(Utilities.GetArgAsDouble(args, 0, aCaller));
        }

        public object MathMax(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            if (args.Length == 0)
                return (Double.NegativeInfinity);

            if (args.Length == 1)
                return (args[0]);

            var lMaxValue = Utilities.GetArgAsDouble(args, 0, aCaller);
            if (Double.IsNaN(lMaxValue))
                return (Double.NaN);

            for (int i = 1, l = args.Length; i < l; i++)
            {
                var lValue = Utilities.GetArgAsDouble(args, i, aCaller);
                if (Double.IsNaN(lValue))
                    return (Double.NaN);

                lMaxValue = Math.Max(lMaxValue, lValue);
            }

            return (lMaxValue);
        }

        public object MathMin(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            if (args.Length == 0)
                return (Double.PositiveInfinity);

            if (args.Length == 1)
                return (args[0]);

            var lMinValue = Utilities.GetArgAsDouble(args, 0, aCaller);

            if (Double.IsNaN(lMinValue))
                return (Double.NaN);

            for (int i = 1, l = args.Length; i < l; i++)
            {
                var lValue = Utilities.GetArgAsDouble(args, i, aCaller);
                if (Double.IsNaN(lValue))
                    return (Double.NaN);

                lMinValue = Math.Min(lMinValue, lValue);
            }

            return (lMinValue);
        }

        public object Mathpow(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Math.Pow(Utilities.GetArgAsDouble(args, 0, aCaller), Utilities.GetArgAsDouble(args, 1, aCaller));
        }

        public object Mathrandom(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            if (fRandom == null) fRandom = new Random();
            return fRandom.NextDouble();
        }

        public object Mathsin(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Math.Sin(Utilities.GetArgAsDouble(args, 0, aCaller));
        }

        public object Mathsqrt(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Math.Sqrt(Utilities.GetArgAsDouble(args, 0, aCaller));
        }

        public object Mathtan(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Math.Tan(Utilities.GetArgAsDouble(args, 0, aCaller));
        }

        public object Mathround(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lVal = Utilities.GetArgAsDouble(args, 0, aCaller);
            // Javascript has a weird kind of rounding
            if ((lVal < 0) && (lVal > -0.5)) return 0;
            return Math.Floor(lVal + 0.5);
        }
    }
}