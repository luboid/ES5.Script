using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace ES5.Script.EcmaScript.Objects
{
    public partial class GlobalObject : EcmaScriptObject
    {
        public EcmaScriptObject CreateString()
        {
            var result = Get("String") as EcmaScriptObject;
            if (result != null) return result;

            result = new EcmaScriptStringObject(this, "String", StringCall, 1) { Class = "String" };
            Values.Add("String", PropertyValue.NotEnum(result));

            StringPrototype = new EcmaScriptObject(this) { Class = "String" };
            StringPrototype.Values.Add("constructor", PropertyValue.NotEnum(result));
            StringPrototype.Prototype = ObjectPrototype;
            result.Values["prototype"] = PropertyValue.NotAllFlags(StringPrototype);

            result.Values.Add("fromCharCode", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "fromCharCode", StringFromCharCode, 1)));
            //StringPrototype.Values.Add("length", PropertyValue.NotAllFlags(0));
            StringPrototype.Values.Add("toString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toString", StringToString, 0)));
            StringPrototype.Values.Add("valueOf", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "valueOf", StringValueOf, 0)));
            StringPrototype.Values.Add("charAt", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "charAt", StringCharAt, 1)));
            StringPrototype.Values.Add("charCodeAt", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "charCodeAt", StringCharCodeAt, 1)));
            StringPrototype.Values.Add("concat", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "concat", StringConcat, 1)));
            StringPrototype.Values.Add("indexOf", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "indexOf", StringIndexOf, 1)));
            StringPrototype.Values.Add("lastIndexOf", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "lastIndexOf", StringLastIndexOf, 1)));

            StringPrototype.Values.Add("match", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "match", StringMatch, 1))); // dep}s on regex support
            StringPrototype.Values.Add("replace", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "replace", StringReplace, 2)));
            StringPrototype.Values.Add("search", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "search", StringSearch, 1))); // dep}s on regex support
            StringPrototype.Values.Add("slice", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "slice", StringSlice, 2)));
            StringPrototype.Values.Add("split", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "split", StringSplit, 2)));
            StringPrototype.Values.Add("substring", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "substring", StringSubString, 2)));
            StringPrototype.Values.Add("substr", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "substr", StringSubStr, 2)));
            StringPrototype.Values.Add("toLowerCase", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toLowerCase", StringToLowerCase, 0)));
            StringPrototype.Values.Add("toUpperCase", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toUpperCase", StringToUpperCase, 0)));
            StringPrototype.Values.Add("toLocaleLowerCase", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toLocaleLowerCase", StringToLocaleLowerCase, 0)));
            StringPrototype.Values.Add("toLocaleUpperCase", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toLocaleUpperCase", StringToLocaleUpperCase, 0)));

            StringPrototype.Values.Add("localeCompare", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "localeCompare", StringLocaleCompare, 1)));
            StringPrototype.Values.Add("trim", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "trim", StringTrim, 0)));

            return result;
        }

        public object StringCall(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Utilities.GetArgAsString(args, 0, aCaller) ?? string.Empty;
        }

        public object StringCtor(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lVal = args.Length == 0 ? string.Empty : (Utilities.GetArgAsString(args, 0, aCaller) ?? string.Empty);

            var lObj = new EcmaScriptObject(this, StringPrototype) { Class = "String", Value = lVal };
            lObj.Values.Add("length", PropertyValue.NotDeleteAndReadOnly(lVal.Length));

            return (lObj);
        }

        public object StringFromCharCode(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lRes = new Char[args.Length];

            for (int i = 0, l = lRes.Length; i < l; i++)
                lRes[i] = (char)Utilities.GetArgAsInteger(args, i, aCaller);

            return (new String(lRes));
        }


        public object StringToString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            if (aSelf is String)
                return (aSelf);

            var s = aSelf as EcmaScriptObject;
            if ((s != null) && (s.Class == "String"))
                return (s.Value);

            RaiseNativeError(NativeErrorType.TypeError, "String.prototype.toString is not generic");

            return null;
        }


        public object StringValueOf(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            if (aSelf is String)
                return (aSelf);

            var s = aSelf as EcmaScriptObject;
            if ((s != null) && (s.Class == "String"))
                return (s.Value);

            RaiseNativeError(NativeErrorType.TypeError, "String.prototype.valueOf is not generic");

            return null;
        }


        public object StringCharAt(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.GetObjAsString(aSelf, aCaller) ?? string.Empty;
            var lIndex = Utilities.GetArgAsInteger(args, 0, aCaller);

            if ((lIndex < 0) || (lIndex >= lSelf.Length))
                return (string.Empty);

            return (new String(lSelf[lIndex], 1));
        }


        public object StringCharCodeAt(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.GetObjAsString(aSelf, aCaller) ?? string.Empty;
            var lIndex = Utilities.GetArgAsInteger(args, 0, aCaller);

            if ((lIndex < 0) || (lIndex >= lSelf.Length))
                return (Double.NaN);

            return ((int)lSelf[lIndex]);
        }


        public object StringConcat(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.GetObjAsString(aSelf, aCaller) ?? string.Empty;

            if (args.Length == 0)
                return (lSelf);

            if (args.Length == 1)
                return (lSelf + Utilities.GetArgAsString(args, 0, aCaller));

            if (args.Length == 2)
                return (lSelf + Utilities.GetArgAsString(args, 0, aCaller) + Utilities.GetArgAsString(args, 1, aCaller));

            var fsb = new StringBuilder();
            fsb.Append(lSelf);

            for (int i = 0, l = args.Length; i < l; i++)
                fsb.Append(Utilities.GetArgAsString(args, i, aCaller));

            return (fsb.ToString());
        }


        public object StringIndexOf(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.GetObjAsString(aSelf, aCaller) ?? string.Empty;
            var lNeedle = Utilities.GetArgAsString(args, 0, aCaller) ?? string.Empty;
            var lIndex = Utilities.GetArgAsInteger(args, 1, aCaller);

            if (lIndex >= lSelf.Length)
                return (-1);

            return (lSelf.IndexOf(lNeedle, lIndex));
        }


        public object StringLastIndexOf(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.GetObjAsString(aSelf, aCaller) ?? string.Empty;
            var lNeedle = Utilities.GetArgAsString(args, 0, aCaller) ?? string.Empty;
            var lIndex = Utilities.GetArgAsInteger(args, 1, aCaller);

            if ((lIndex >= lSelf.Length) || (lIndex == 0))
                lIndex = lSelf.Length;

            return (lSelf.LastIndexOf(lNeedle, lIndex));
        }


        public object StringReplace(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.GetObjAsString(aSelf, aCaller) ?? string.Empty;
            if (0 == args.Length)
                return lSelf;

            var lNewValue = (string)null;
            var lCallBack = Utilities.GetArg(args, 1) as EcmaScriptInternalFunctionObject;

            if (null == lCallBack)
                lNewValue = Utilities.GetArgAsString(args, 1, aCaller) ?? string.Empty;

            var lPattern = args[0] as EcmaScriptRegexpObject;
            if (lPattern == null && lCallBack == null)
            {
                // this will replace all occurence
                return lSelf.Replace(Utilities.GetArgAsString(args, 0, aCaller), lNewValue);
            }

            if (lPattern == null && lCallBack != null)
            {
                lPattern = new EcmaScriptRegexpObject(aCaller.Global, Utilities.GetArgAsString(args, 0, aCaller), string.Empty);
            }

            if (lCallBack != null)
            {
                object[] lCallBackArgs = null; int groups = 0;
                MatchEvaluator evaluator = (Match match) =>
                {
                    if (null == lCallBackArgs)
                    {
                        groups = match.Groups.Count;
                        lCallBackArgs = new object[groups + 2];
                        lCallBackArgs[lCallBackArgs.Length - 1] = lSelf;
                    }

                    for (var i = 0; i < groups; i++)
                    {
                        if (i == 0)
                        {
                            lCallBackArgs[lCallBackArgs.Length - 2] = match.Groups[i].Index;
                        }
                        lCallBackArgs[i] = match.Groups[i].Success ? (object)match.Groups[i].Value : Undefined.Instance;
                    }

                    var replacment = Utilities.GetObjAsString(lCallBack.CallEx(aCaller, aCaller.Global, lCallBackArgs), aCaller);

                    return replacment;
                };

                if (lPattern.GlobalVal)
                {
                    return lPattern.Regex.Replace(lSelf, evaluator);
                }
                else
                {
                    return lPattern.Regex.Replace(lSelf, evaluator, 1);
                }
            }
            else
            {
                if (lPattern.GlobalVal)
                    return (lPattern.Regex.Replace(lSelf, lNewValue));
                else
                    return (lPattern.Regex.Replace(lSelf, lNewValue, 1));
            }
        }

        public object StringSlice(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.GetObjAsString(aSelf, aCaller) ?? string.Empty;

            if (lSelf == null)
                return Undefined.Instance;

            var lStart = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lObj = Utilities.GetArg(args, 1);
            var lEnd = ((lObj == null) || (lObj == Undefined.Instance) ? Int32.MaxValue : Utilities.GetObjAsInteger(lObj, aCaller));

            if (lStart < 0)
            {
                lStart = lSelf.Length + lStart;
                if (lStart < 0)
                    lStart = 0;
            }

            if (lEnd < 0)
            {
                lEnd = lSelf.Length + lEnd;
                if (lEnd < 0)
                    lEnd = 0;
            }

            if (lEnd < lStart)
                lEnd = lStart;

            if (lStart > lSelf.Length)
                lStart = lSelf.Length;

            if (lEnd > lSelf.Length)
                lEnd = lSelf.Length;

            return lSelf.Substring(lStart, lEnd - lStart);
        }


        public object StringSplit(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.GetObjAsString(aSelf, aCaller) ?? string.Empty;
            var lNeedle = Utilities.GetArgAsString(args, 0, aCaller) ?? string.Empty;
            var lMax = Utilities.GetArgAsInteger(args, 1, aCaller);

            if (lMax <= 0)
                lMax = Int32.MaxValue;

#if SILVERLIGHT
              var lValues = lSelf.Split(new [] {lNeedle}, StringSplitOptions.None);
              if (lValues.Length > lMax) {
                var result = new EcmaScriptArrayObject(this, 0);
                for(int i=0;i<lMax;i++) {
                  result.AddValue(lValues[i]);
                }
                return result;
              } else
                return new EcmaScriptArrayObject(this, 0).AddValues(lValues);
#else
            return (new EcmaScriptArrayObject(this, 0).AddValues(lSelf.Split(new[] { lNeedle }, lMax, StringSplitOptions.None)));
#endif
        }


        public object StringSubString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.GetObjAsString(aSelf, aCaller) ?? string.Empty;
            if (lSelf == null)
                return Undefined.Instance;

            if (lSelf.Length == 0)
                return string.Empty;

            var lStart = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lObj = Utilities.GetArg(args, 1);
            var lEnd = ((lObj == null) || (lObj == Undefined.Instance) ? Int32.MaxValue : Utilities.GetObjAsInteger(lObj, aCaller));

            if (lStart < 0)
                lStart = 0;

            if (lEnd < 0)
                lEnd = 0;

            if (lStart == lEnd)
                return string.Empty;

            if (lEnd < lStart)
            { // swap indeces
                var bufEnd = lEnd;
                lEnd = lStart;
                lStart = bufEnd;
            }

            if (lStart > (lSelf.Length - 1))
                return string.Empty;

            if (lEnd > lSelf.Length)
                lEnd = lSelf.Length;

            return lSelf.Substring(lStart, lEnd - lStart);
        }


        public object StringSubStr(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.GetObjAsString(aSelf, aCaller) ?? string.Empty;

            if (null == lSelf)
                return Undefined.Instance;

            if (lSelf.Length == 0)
                return string.Empty;

            var lStart = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lObj = Utilities.GetArg(args, 1);
            var lEnd = ((lObj == null) || (lObj == Undefined.Instance) ? Int32.MaxValue : Utilities.GetObjAsInteger(lObj, aCaller));

            if (lEnd <= 0)
                return string.Empty;

            if (lStart < 0)
            {
                lStart = lSelf.Length + lStart;
                if (lStart < 0)
                    lStart = 0;
            }

            if (lSelf.Length <= lStart)
                return string.Empty;

            return lSelf.Substring(lStart, Math.Min(lEnd, lSelf.Length - lStart));
        }


        public object StringToLowerCase(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = (aSelf as String);

            if ((lSelf == null) && (aSelf is EcmaScriptObject))
                lSelf = (String)(((EcmaScriptObject)aSelf).Value);

            if (lSelf == null)
                RaiseNativeError(NativeErrorType.TypeError, "String.prototype.toLowerCase is not generic");

            return (lSelf.ToLowerInvariant());
        }

        public object StringToUpperCase(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = (aSelf as String);

            if ((lSelf == null) && (aSelf is EcmaScriptObject))
                lSelf = (String)(((EcmaScriptObject)aSelf).Value);

            if (lSelf == null)
                RaiseNativeError(NativeErrorType.TypeError, "String.prototype.toUpperCase is not generic");

            return (lSelf.ToUpperInvariant());
        }

        public object StringToLocaleLowerCase(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = (aSelf as String);

            if ((lSelf == null) && (aSelf is EcmaScriptObject))
                lSelf = (String)(((EcmaScriptObject)aSelf).Value);

            if (lSelf == null)
                RaiseNativeError(NativeErrorType.TypeError, "String.prototype.toLocaleLowerCase is not generic");

            return (lSelf.ToLower());
        }

        public object StringToLocaleUpperCase(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = (aSelf as String);

            if ((lSelf == null) && (aSelf is EcmaScriptObject))
                lSelf = (String)(((EcmaScriptObject)aSelf).Value);

            if (lSelf == null)
                RaiseNativeError(NativeErrorType.TypeError, "String.prototype.toLocaleUpperCase is not generic");

            return (lSelf.ToUpper());
        }


        public object StringSearch(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.GetObjAsString(aSelf, aCaller) ?? string.Empty;
            var lObj = (EcmaScriptRegexpObject)null;

            if ((args.Length == 0) || !(args[0] is EcmaScriptRegexpObject))
                lObj = new EcmaScriptRegexpObject(this, Utilities.GetArgAsString(args, 0, aCaller), "");
            else
                lObj = (EcmaScriptRegexpObject)args[0];

            try
            {
                var lMatch = lObj.Regex.Match(lSelf);
                return (((lMatch == null) || !lMatch.Success ? -1 : lMatch.Index));
            }
            catch (Exception ex)
            {
                RaiseNativeError(NativeErrorType.SyntaxError, ex.Message);
                return null;
            }
        }

        public object StringMatch(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.GetObjAsString(aSelf, aCaller) ?? string.Empty;
            var lObj = (EcmaScriptRegexpObject)null;

            if ((args.Length == 0) || !(args[0] is EcmaScriptRegexpObject))
                lObj = new EcmaScriptRegexpObject(this, Utilities.GetArgAsString(args, 0, aCaller), "");
            else
                lObj = (EcmaScriptRegexpObject)args[0];

            if (!lObj.GlobalVal)
                return (RegExpExec(aCaller, lObj, lSelf));

            var lRealResult = new EcmaScriptArrayObject(this, 0);

            lObj.LastIndex = 0;
            var lLastMatch = true;
            var lPrevLastIndex = 0;
            while (lLastMatch)
            {
                var lRes = (EcmaScriptArrayObject)RegExpExec(aCaller, lObj, lSelf);
                if (lRes == null)
                    lLastMatch = false;
                else
                {
                    var lThisIndex = Utilities.GetObjAsInteger(lObj.Get("lastIndex", aCaller), aCaller);
                    if (lThisIndex == lPrevLastIndex)
                    {
                        lPrevLastIndex = lThisIndex + 1;
                        lObj.LastIndex = lPrevLastIndex;
                    }
                    else
                        lPrevLastIndex = lThisIndex;

                    lRealResult.AddValue(lRes.Get("0", aCaller, 2));//lMatchStr
                }
            }

            if (lRealResult.Length == 0)
                return (null);

            return (lRealResult);
        }

        public object StringLocaleCompare(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            if ((aSelf == null) || (aSelf == Undefined.Instance))
                RaiseNativeError(NativeErrorType.TypeError, "null/undefined not coercible");

            return string.Compare(
                Utilities.GetObjAsString(aSelf, aCaller),
                Utilities.GetArgAsString(args, 0, aCaller),
                StringComparison.CurrentCulture);
        }


        public object StringTrim(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            if ((aSelf == null) || (aSelf == Undefined.Instance))
                RaiseNativeError(NativeErrorType.TypeError, "null/undefined not coercible");

            var s = Utilities.GetObjAsString(aSelf, aCaller).Trim();
            while (s.Length != 0)
            {
                if (s[0] == '\uFEFF' || s[s.Length - 1] == '\uFEFF')
                {
                    s = s.Trim(new[] { '\uFEFF' }).Trim();
                }
                else
                {
                    break;
                }
            }
            return s;
        }
    }
}