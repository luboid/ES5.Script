using ES5.Script.EcmaScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script
{
    public class ScriptStackFrame
    {
        Object fThis;
        String fMethod;
        EnvironmentRecord fFrame;

        internal ScriptStackFrame(string aMethod, object aThis, EnvironmentRecord aFrame)
        {
            fMethod = aMethod;
            fFrame = aFrame;
            fThis = aThis;
        }

        public EnvironmentRecord Frame { get; private set; }
        public object This { get; private set; }
        public string Method { get; private set; }

    }
}
