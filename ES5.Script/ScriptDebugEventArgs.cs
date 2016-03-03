using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script
{
    public class ScriptDebugEventArgs : EventArgs
    {
        public ScriptDebugEventArgs(string aName, PositionPair aSpan)
        {
            Name = aName;
            SourceSpan = aSpan;
        }

        public ScriptDebugEventArgs(string aName, PositionPair aSpan, Exception ex)
                : this(aName, aSpan)
        {
            Exception = ex;
        }

        public string Name { get; private set; }
        public Exception Exception { get; private set; }
        public PositionPair SourceSpan { get; private set; }
        public string SourceFileName { get { return SourceSpan.File; } }
    }
}