using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

namespace ES5.Script.EcmaScript.Internal
{
    class DebugSink : IDebugSink
    {
        public virtual void DebugLine(string aFilename, int aStartRow, int aStartCol, int aEndRow, int aEndCol) { }
        public virtual void EnterScope(string aName, object aThis, ExecutionContext aContext) { }// enter method
        public virtual void ExitScope(string aName, ExecutionContext aContext, object aResult, bool aExcept) { }// exit method
        public virtual void CaughtException(Exception e) { }// triggers on a CATCH before the js code itself
        public virtual void UncaughtException(Exception e) { }// triggers when an exception escapes the main method
        public virtual void Debugger() { }

        public static readonly MethodInfo Method_Debugger = typeof(IDebugSink).GetMethod("Debugger");
        public static readonly MethodInfo Method_DebugLine = typeof(IDebugSink).GetMethod("DebugLine");
        public static readonly MethodInfo Method_EnterScope = typeof(IDebugSink).GetMethod("EnterScope");
        public static readonly MethodInfo Method_ExitScope = typeof(IDebugSink).GetMethod("ExitScope");
        //class var Method_CaughtException: System.Reflection.MethodInfo := typeOf(IDebugSink).GetMethod('CaughtException'); readonly;
        public static readonly MethodInfo Method_UncaughtException = typeof(IDebugSink).GetMethod("UncaughtException");
    }
}