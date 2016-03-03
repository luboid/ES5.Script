using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script
{
    public class ScriptDebugExitScopeEventArgs : ScriptDebugEventArgs
    {
        public ScriptDebugExitScopeEventArgs(string aName, PositionPair aSpan, object aResult, Boolean aExcept)
            : base(aName, aSpan)
        {
            Result = aResult;
            WasException = aExcept;
        }

        public bool WasException { get; private set; }
        public object Result { get; private set; }
    }
}
