using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public class EcmaScriptCompilerOptions
    {
        public bool StackOverflowProtect { get; set; } = true;
        public bool EmitDebugCalls { get; set; }
        public bool JustFunctions { get; set; }
        public EnvironmentRecord Context { get; set; }
        public GlobalObject GlobalObject { get; set; }
    }
}