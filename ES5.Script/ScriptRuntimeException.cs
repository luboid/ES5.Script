using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ES5.Script
{
    public class ScriptRuntimeException : ScriptException
    {
        public static readonly MethodInfo Method_Unwrap = typeof(ScriptRuntimeException).GetMethod("Unwrap");
        public static readonly MethodInfo Method_Wrap = typeof(ScriptRuntimeException).GetMethod("Wrap");

        readonly object fOriginal;

        public ScriptRuntimeException(object aOriginal) :
            base(ScriptRuntimeException.SafeEcmaScriptToObject(aOriginal))
        {
            fOriginal = aOriginal;
        }

        public object Original
        {
            get
            {
                return fOriginal;
            }
        }

        public override string ToString()
        {
            return this.Message;
        }

        public static string SafeEcmaScriptToObject(object o)
        {
            string value = "Error";
            if (null != o)
            {
                try
                {
                    value = o.ToString();
                }
                catch { }
            }
            return value;
        }

        public static Exception Wrap(object arg)
        {
            var lResult = arg as Exception;
            if (lResult != null)
                return lResult;
            else
                return new ScriptRuntimeException(arg);
        }

        public static object Unwrap(object arg)
        {
            var e = arg as ScriptRuntimeException;
            if (arg != null)
                return e.Original;

            return arg;
        }
    }
}
