using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script
{
    public enum ScriptStatus
    {

        Stopped,
        StepInto,
        StepOver,
        StepOut,
        Running,
        Stopping,
        Paused,
        Pausing
    }
}
