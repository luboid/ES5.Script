using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public interface IDebugSink
    {
        void DebugLine(string aFilename, int aStartRow, int aStartCol, int aEndRow, int aEndCol);
        void EnterScope(string aName, object aThis, ExecutionContext aContext); // enter method
        void ExitScope(string aName, ExecutionContext aContext, object aResult, bool aExcept); // exit method
        void CaughtException(Exception e); // triggers on a CATCH before the js code itself
        void UncaughtException(Exception e); // triggers when an exception escapes the main method
        void Debugger();
    }
}