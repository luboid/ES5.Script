using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace ES5.Script.EcmaScript.Objects
{
    public partial class GlobalObject : EcmaScriptObject
    {
        public EcmaScriptObject CreateRegExp()
        {
            var result = Get("RegExp") as EcmaScriptObject;
            if (result != null) return result;

            result = new EcmaScriptFunctionObject(this, "RegExp", RegExpCtor, 1) { Class = "RegExp" };
            Values.Add("RegExp", PropertyValue.NotEnum(result));

            RegExpPrototype = new EcmaScriptFunctionObject(this, "RegExp", RegExpCtor, 1) { Class = "RegExp" };
            RegExpPrototype.Prototype = ObjectPrototype;
            RegExpPrototype.Values["source"] = PropertyValue.NotAllFlags(Undefined.Instance);
            RegExpPrototype.Values["global"] = PropertyValue.NotAllFlags(false);
            RegExpPrototype.Values["ignoreCase"] = PropertyValue.NotAllFlags(false);
            RegExpPrototype.Values["multiline"] = PropertyValue.NotAllFlags(false);
            RegExpPrototype.Values["lastIndex"] = new PropertyValue(PropertyAttributes.Writable, Undefined.Instance);

            result.Values["prototype"] = PropertyValue.NotAllFlags(RegExpPrototype);

            RegExpPrototype.Values["constructor"] = PropertyValue.NotEnum(RegExpPrototype);
            RegExpPrototype.Values.Add("toString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toString", RegExpToString, 0)));
            RegExpPrototype.Values.Add("test", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "test", RegExpTest, 1)));
            RegExpPrototype.Values.Add("compile", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "test", RegExpCompile, 2)));
            RegExpPrototype.Values.Add("exec", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "exec", RegExpExec, 1)));

            return result;
        }

        public object RegExpCtor(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return new EcmaScriptRegexpObject(this,
                (args.Length == 0) || (args[0] == Undefined.Instance) ? "" : Utilities.GetArgAsString(args, 0, aCaller),
                (args.Length < 2) || (args[1] == Undefined.Instance) ? "" : Utilities.GetArgAsString(args, 1, aCaller));
        }

        public object RegExpExec(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = aSelf as EcmaScriptRegexpObject;
            var lIndex = lSelf.GlobalVal ? lSelf.LastIndex : 0;
            var lInput = Utilities.GetArgAsString(args, 0, aCaller) ?? String.Empty;
            try
            {
                var lMatch = lSelf.Regex.Matches(lInput, lIndex);

                return MatchToArray(lSelf, lInput, lMatch);
            }
            catch (Exception e)
            {
                RaiseNativeError(NativeErrorType.SyntaxError, e.Message);
                return null;//only to compile
            }
        }


        public EcmaScriptArrayObject MatchToArray(EcmaScriptRegexpObject aSelf, string aInput, MatchCollection aMatch)
        {
            var lObj = new EcmaScriptArrayObject(this, 0);
            lObj.AddValue("input", aInput);
            if (aMatch.Count > 0)
            {
                lObj.AddValue("index", aMatch[0].Index);
                if (!aSelf.GlobalVal)
                {
                    for (int i = 0, l = aMatch[0].Groups.Count; i < l; i++)
                        lObj.AddValue(aMatch[0].Groups[i].Value);
                }
                else
                { // global
                    for (int i = 0, l = aMatch.Count; i < l; i++)
                        lObj.AddValue(aMatch[i].Value);

                    aSelf.LastIndex = aMatch[aMatch.Count - 1].Index + aMatch[aMatch.Count - 1].Length;
                }
            }
            else
            {
                if (aSelf.GlobalVal)
                    aSelf.LastIndex = 0;
                return null;
            }
            return lObj;
        }

        public object RegExpTest(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return RegExpExec(aCaller, aSelf, args) != null;
        }

        public object RegExpToString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = aSelf as EcmaScriptRegexpObject;
            if (lSelf == null)
            {
                return "RegEx Class";
            }

            var result = "/" + lSelf.Get("source").ToString() + "/";
            if (lSelf.GlobalVal) result = (String)result + "g";
            if ((RegexOptions.IgnoreCase & lSelf.Options) != 0) result = (String)result + "i";
            if ((RegexOptions.Multiline & lSelf.Options) != 0) result = (String)result + "m";
            return result;
        }



        public object RegExpCompile(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lObj = aSelf as EcmaScriptRegexpObject;
            if (lObj == null)
                RaiseNativeError(NativeErrorType.TypeError, "this is not a RegEx object");
            var lOpt = RegexOptions.ECMAScript;
            var aFlags = Utilities.GetArgAsString(args, 1, aCaller);
            var aPattern = Utilities.GetArgAsString(args, 0, aCaller);
            if ((aFlags != null) && aFlags.Contains("i")) lOpt = lOpt | RegexOptions.IgnoreCase;
            if ((aFlags != null) && aFlags.Contains("m")) lOpt = lOpt | RegexOptions.Multiline;
            if ((aFlags != null) && aFlags.Contains("g")) lObj.GlobalVal = true;
            lObj.Values["source"] = PropertyValue.NotAllFlags(aPattern);
            lObj.Values["global"] = PropertyValue.NotAllFlags(lObj.GlobalVal);
            lObj.Values["ignoreCase"] = PropertyValue.NotAllFlags((RegexOptions.IgnoreCase & lOpt) != 0);
            lObj.Values["multiline"] = PropertyValue.NotAllFlags((RegexOptions.Multiline & lOpt) != 0);
            lObj.Values["lastIndex"] = new PropertyValue(PropertyAttributes.Writable, Undefined.Instance);
            try
            {
                lObj.Recreate(aPattern, lOpt);
            }
            catch (Exception e)
            {
                RaiseNativeError(NativeErrorType.SyntaxError, e.Message);
            }
            return lObj;
        }
    }
}