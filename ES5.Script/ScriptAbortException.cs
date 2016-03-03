using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script
{
    public class ScriptAbortException : Exception
    {
        public ScriptAbortException() : base() { }
        public ScriptAbortException(string message) : base(message) { }
        public ScriptAbortException(string message, Exception innerException) : base(message, innerException) { }
#if !SILVERLIGHT //&& !DNX
        protected ScriptAbortException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) :
            base(info, context)
        { }
#endif
    }
}
