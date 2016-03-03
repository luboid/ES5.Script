using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ES5.Script
{
    public class ScriptComponentException : Exception
    {
        public ScriptComponentException() : base() { }
        public ScriptComponentException(string message) : base(message) { }
        public ScriptComponentException(string message, Exception innerException) : base(message, innerException) { }
#if !SILVERLIGHT //&& !DNX
        protected ScriptComponentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) :
            base(info, context)
        { }
#endif
    }
}
