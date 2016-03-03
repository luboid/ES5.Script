using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptRegexpObject : EcmaScriptObject
    {
        Regex fRegex;

        public bool GlobalVal { get; set; }

        public RegexOptions Options
        {
            get { return fRegex.Options; }
        }

        public Regex Regex
        {
            get { return fRegex; }
        }

        public int LastIndex
        {
            get { return Utilities.GetObjAsInteger(Get("lastIndex"), Root.ExecutionContext); }
            set { Put("lastIndex", value); }
        }

        internal void Recreate(string aPattern, RegexOptions aSettings)
        {
            fRegex = new Regex(aPattern, aSettings);
        }

        public EcmaScriptRegexpObject(GlobalObject aGlobal, string aPattern, string aFlags)
            : base(aGlobal, aGlobal.RegExpPrototype)
        {
            Class = "RegExp";
            var lOpt = RegexOptions.ECMAScript;
            if (aFlags != null)
            {
                foreach (var el in aFlags)
                {
                    if (el == 'i')
                    {
                        if ((RegexOptions.IgnoreCase & lOpt) != 0)
                            aGlobal.RaiseNativeError(NativeErrorType.SyntaxError, "invalid flags");
                        lOpt = lOpt | RegexOptions.IgnoreCase;
                    }
                    else
                    if (el == 'm')
                    {
                        if ((RegexOptions.Multiline & lOpt) != 0)
                            aGlobal.RaiseNativeError(NativeErrorType.SyntaxError, "invalid flags");
                        lOpt = lOpt | RegexOptions.Multiline;
                    }
                    else
                    if (el == 'g')
                    {
                        if (GlobalVal) aGlobal.RaiseNativeError(NativeErrorType.SyntaxError, "invalid flags");
                        GlobalVal = true;
                    }
                    else
                        aGlobal.RaiseNativeError(NativeErrorType.SyntaxError, "invalid flags");
                }
            }
            Values["source"] = PropertyValue.NotAllFlags(aPattern);
            Values["global"] = PropertyValue.NotAllFlags(GlobalVal);
            Values["ignoreCase"] = PropertyValue.NotAllFlags((RegexOptions.IgnoreCase & lOpt) != 0);
            Values["multiline"] = PropertyValue.NotAllFlags((RegexOptions.Multiline & lOpt) != 0);
            Values["lastIndex"] = new PropertyValue(PropertyAttributes.Writable, Undefined.Instance);
            try
            {
                fRegex = new Regex(aPattern, lOpt);
            }
            catch (Exception e)
            {
                aGlobal.RaiseNativeError(NativeErrorType.SyntaxError, e.Message);
            }
        }
    }
}