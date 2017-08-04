using ES5.Script.EcmaScript;
using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;


namespace ES5.Script
{
    public abstract class ScriptComponent :
#if !SILVERLIGHT //&& !DNX
        Component,
#endif
        IDebugSink, IDisposable
    {
        System.Threading.Thread fWorkThread;
        object fRunResult;
        bool fRunInThread;
        bool fDebug;
        //ScriptRuntimeSetup fSetup;
        volatile bool fTracing;
        ScriptStatus fStatus;
        ReadOnlyCollection<ScriptStackFrame> fStackItems;
        PositionPair fDebugLastPos;
        System.Threading.ManualResetEvent fWaitEvent = new System.Threading.ManualResetEvent(true);

        protected int fLastFrame;
        protected List<ScriptStackFrame> fStackList = new List<ScriptStackFrame>();
        protected ScriptScope fGlobals;
        protected ScriptStatus fEntryStatus = ScriptStatus.Running;
        protected abstract object IntRun();

        void IDebugSink.DebugLine(string aFilename, int aStartRow, int aStartCol, int aEndRow, int aEndCol)
        {
            fDebugLastPos = new PositionPair(0, aStartRow, aStartCol, 0, aEndRow, aEndCol, aFilename);
            if (Status == ScriptStatus.Stopping) throw new ScriptAbortException();
            if (fTracing) return;
            fTracing = true;
            try
            {
                if (DebugTracePoint != null)
                    DebugTracePoint(this, new ScriptDebugEventArgs(fStackList[fStackItems.Count - 1].Method, new PositionPair(0, aStartRow, aStartCol, 0, aEndRow, aEndCol, aFilename)));

                if ((Status == ScriptStatus.StepInto) ||
                      ((Status == ScriptStatus.StepOver) && (fLastFrame == fStackList.Count)))
                    Status = ScriptStatus.Pausing;

                CheckShouldPause();
            }
            finally
            {
                fTracing = false;
            }
        }

        void IDebugSink.EnterScope(string aName, object athis, ExecutionContext aContext)
        {
            fStackList.Add(new ScriptStackFrame(aName, athis, aContext.LexicalScope));
            if (Status == ScriptStatus.Stopping) throw new ScriptAbortException();
            if (fTracing) return;
            fTracing = true;
            try
            {
                if (DebugFrameEnter != null)
                    DebugFrameEnter(this, new ScriptDebugEventArgs(fStackList[fStackItems.Count - 1].Method, new PositionPair()));
                if (Status == ScriptStatus.StepInto)
                    Status = ScriptStatus.Pausing;
                CheckShouldPause();
            }
            finally
            {
                fTracing = false;
            }
        }

        void IDebugSink.ExitScope(string aName, ExecutionContext aContext, object aResult, bool aExcept)
        {
            var lFrame = fStackList[fStackList.Count - 1];
            fStackList.RemoveAt(fStackList.Count - 1);

            if (fTracing) return;
            fTracing = true;

            try
            {
                if (DebugFrameExit != null)
                    DebugFrameExit(this, new ScriptDebugExitScopeEventArgs(lFrame.Method, new PositionPair(), aResult, aExcept));

                if ((Status == ScriptStatus.StepOut) && (fLastFrame < fStackList.Count))
                    Status = ScriptStatus.Pausing;

                CheckShouldPause();
            }
            finally
            {
                fTracing = false;
            }
        }

        void IDebugSink.CaughtException(Exception e)
        {
            if (fTracing) return;
            fTracing = true;

            try
            {
                if (DebugException != null)
                    DebugException(this, new ScriptDebugEventArgs(fStackList[fStackList.Count - 1].Method, fDebugLastPos));

                CheckShouldPause();
            }
            finally
            {
                fTracing = false;
            }
        }

        void IDebugSink.UncaughtException(Exception e)
        {
            if (fTracing) return;
            fTracing = true;

            try
            {
                if (DebugExceptionUnwind != null)
                    DebugExceptionUnwind(this, new ScriptDebugEventArgs(null, fDebugLastPos));
            }
            finally
            {
                fTracing = false;
            }
        }

        void IDebugSink.Debugger()
        {
            if (fTracing) return;
            fTracing = true;
            try
            {
                if (DebugDebugger != null)
                    DebugDebugger(this, new ScriptDebugEventArgs(fStackList[fStackList.Count - 1].Method, fDebugLastPos));
                CheckShouldPause();
            }
            finally
            {
                fTracing = false;
            }
        }

        void Idle()
        {
            if (Status == ScriptStatus.Pausing)
                Status = ScriptStatus.Paused;

            while (Status == ScriptStatus.Paused) {
                if (NonThreadedIdle != null) NonThreadedIdle(this, EventArgs.Empty);
                else
                    System.Threading.Thread.Sleep(10);
            }
        }

        void CheckShouldPause()
        {
            if (Status == ScriptStatus.Paused || Status == ScriptStatus.Pausing)
            {
                if (fRunInThread)
                {
                    Status = ScriptStatus.Paused;
                    fWaitEvent.Reset();
                    fWaitEvent.WaitOne();
                }
                else
                    Idle();
            }
        }

        public ScriptStatus Status
        {
            get { return fStatus; }
            protected set
            {
                fStatus = value;
                if (StatusChanged != null)
                    StatusChanged(this, EventArgs.Empty);
            }
        }

        [Category("Script")]
        public bool RunInThread
        {
            get { return fRunInThread; }
            set
            {
                if (Status == ScriptStatus.Stopped)
                    fRunInThread = value;
                else
                    throw new ScriptComponentException(Properties.Resources.eRunInThreadCannotBeModifiedWhenScriptIsRunning);
            }
        }

        [Category("Script")]
        public virtual bool Debug
        {
            get { return fDebug; }
            set
            {
                fDebug = value;
            }
        }

        [Category("Script")]
        public string SourceFileName { get; set; }
        [Category("Script")]
        public string Source { get; set; }

        [Browsable(false)]
        public ReadOnlyCollection<ScriptStackFrame> CallStack
        {
            get
            {
                return fStackItems;
            }
        }

        [Browsable(false)]
        public abstract ScriptScope Globals { get; }

        public object RunResult { get { return fRunResult; } }
        public Exception RunException { get; protected set; }
        public PositionPair DebugLastPos { get { return fDebugLastPos; } }

        public event EventHandler<ScriptDebugEventArgs>  DebugFrameEnter;
        public event EventHandler<ScriptDebugExitScopeEventArgs>  DebugFrameExit;
        //public event EventHandler<ScriptDebugEventArgs> DebugThreadExit;
        public event EventHandler<ScriptDebugEventArgs> DebugTracePoint;
        public event EventHandler<ScriptDebugEventArgs> DebugDebugger;
        public event EventHandler<ScriptDebugEventArgs> DebugException;
        public event EventHandler<ScriptDebugEventArgs> DebugExceptionUnwind;
        public event EventHandler StatusChanged;
        public event EventHandler NonThreadedIdle;

        public ScriptComponent()
        {
            fStackItems = new ReadOnlyCollection<ScriptStackFrame>(fStackList);
            Clear();
        }

        public void StepInto()
        {
            lock (this)
            {
                if (Status == ScriptStatus.Stopped)
                {
                    fEntryStatus = ScriptStatus.StepInto;
                    fLastFrame = fStackList.Count;
                    Run();
                    return;
                }

                if (Status == ScriptStatus.Paused || Status == ScriptStatus.Pausing || Status == ScriptStatus.Running)
                {
                    if (Status == ScriptStatus.Paused)
                    {
                        Status = ScriptStatus.StepInto;
                        fWaitEvent.Set();
                    }
                    else
                        Status = ScriptStatus.StepInto;
                    fLastFrame = fStackList.Count;
                }
            }
        }

        public void StepOver()
        {
            lock (this)
            {
                if (Status == ScriptStatus.Stopped)
                {
                    fEntryStatus = ScriptStatus.StepInto;
                    fLastFrame = fStackList.Count;
                    Run();
                    return;
                }
                if (Status == ScriptStatus.Paused || Status == ScriptStatus.Pausing || Status == ScriptStatus.Running)
                {
                    if (Status == ScriptStatus.Paused)
                    {
                        Status = ScriptStatus.StepOver;
                        fWaitEvent.Set();
                    }
                    else
                        Status = ScriptStatus.StepOver;
                    fLastFrame = fStackList.Count;
                }
            }
        }

        public void StepOut()
        {
            lock (this)
            {
                if (Status == ScriptStatus.Stopped)
                {
                    fEntryStatus = ScriptStatus.StepInto;
                    fLastFrame = fStackList.Count;
                    Run();
                    return;
                }

                if (Status == ScriptStatus.Paused || Status == ScriptStatus.Pausing || Status == ScriptStatus.Running)
                {
                    if (Status == ScriptStatus.Paused)
                    {
                        Status = ScriptStatus.StepOut;
                        fWaitEvent.Set();
                    }
                    else
                        Status = ScriptStatus.StepOut;
                    fLastFrame = fStackList.Count;
                }
            }
        }

        public void Stop()
        {
            lock (this)
            {
                switch (Status)
                {
                    case ScriptStatus.Paused:
                        Status = ScriptStatus.Stopping;
                        fWaitEvent.Set();
                        break;
                    case ScriptStatus.Stopping:
                    case ScriptStatus.Pausing:
                    case ScriptStatus.Running:
                    case ScriptStatus.StepInto:
                    case ScriptStatus.StepOut:
                    case ScriptStatus.StepOver:
                        Status = ScriptStatus.Stopping; break;
                    case ScriptStatus.Stopped:
                        break;
                } // case
            }
        }

        public void Run()
        {
            if (fRunInThread) {
                lock (this)
                {
                    if (Status == ScriptStatus.StepInto || Status == ScriptStatus.StepOut || Status == ScriptStatus.StepOver)
                    {
                        Status = ScriptStatus.Running;
                        return;
                    }
                    else if (Status == ScriptStatus.Paused)
                    {
                        Status = ScriptStatus.Running;
                        fWaitEvent.Set();
                        return;
                    }
                    else if (Status != ScriptStatus.Stopped)
                        throw new ScriptComponentException(ES5.Script.Properties.Resources.eAlreadyRunning);
                    Status = ScriptStatus.Running;
                }

                RunException = null;
                fWorkThread = new System.Threading.Thread(() => {
                    try {
                        fRunResult = IntRun();
                    } catch (Exception e) {
                        this.RunException = e;
                    }
                });

                try
                {
                    fWorkThread.Start();
                }
                catch
                {
                    Status = ScriptStatus.Stopped;
                    throw;
                }
            } else {
                if (Status == ScriptStatus.Paused) { Status = ScriptStatus.Running; return; }
                if (Status != ScriptStatus.Stopped)
                    throw new ScriptComponentException(ES5.Script.Properties.Resources.eAlreadyRunning);
                fRunResult = IntRun();
            }
        }

        public void Pause()
        {
            lock (this)
            {
                if (Status == ScriptStatus.Running)
                {
                    Status = ScriptStatus.Pausing;
                }
            }
        }

        public object RunFunction(string name, params object[] args)
        {
            return RunFunction(ScriptStatus.Running, name, args);
        }

        public abstract void ExposeType(Type type, string name = null);
        public abstract void Clear(bool aGlobals = false);
        public abstract bool HasFunction(string aName);
        public abstract object RunFunction(ScriptStatus initialStatus, string name, params object[] args);


#if SILVERLIGHT
        public void Dispose()
#else
        protected override void Dispose(bool disposing)
#endif
        {
#if !SILVERLIGHT
            if (disposing)
#endif
                fWaitEvent.Close();
        }
    }
}