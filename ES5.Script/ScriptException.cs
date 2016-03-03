using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script
{
    public class ScriptException : Exception
    {
        public ScriptException() : base() { }
        public ScriptException(string message) : base(message) { }
        public ScriptException(string message, Exception innerException) : base(message, innerException) { }
#if !SILVERLIGHT //&& !DNX
        protected ScriptException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) :
            base(info, context)
        { }
#endif
    }
}