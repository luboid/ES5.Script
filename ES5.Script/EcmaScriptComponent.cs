using ES5.Script.EcmaScript;
using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script
{
    public class EcmaScriptComponent : ScriptComponent
    {
        EcmaScriptCompiler fCompiler;
        ScriptScope fScope;
        ExecutionContext fRoot;
        GlobalObject fGlobalObject;
        Boolean fJustFunctions;

        public ExecutionContext RootContext
        {
            get
            {
                return fRoot;
            }
        }

        public bool JustFunctions
        {
            get
            {
                return fJustFunctions;
            }
            set
            {
                if (value != fJustFunctions)
                {
                    fJustFunctions = value;
                    fCompiler.JustFunctions = value;
                }
            }
        }

        public override ScriptScope Globals
        {
            get
            {
                return fScope;
            }
        }

        public GlobalObject GlobalObject
        {
            get
            {
                return fGlobalObject;
            }
        }

        protected override object IntRun()
        {
            Status = fEntryStatus;
            fEntryStatus = ScriptStatus.Running;
            try
            {
                if (String.IsNullOrEmpty(SourceFileName))
                    SourceFileName = "main.js";
                if (Source == null)
                    Source = string.Empty;

                fGlobalObject.FrameCount = 0;

                var lCallback = fCompiler.Parse(SourceFileName, Source);

                return lCallback(fRoot, fGlobalObject, Utilities.EmptyParams);
            }
            catch (ScriptRuntimeException ex)
            {
                SetRunException(ex);
                throw;
            }
            catch (ScriptAbortException)
            {
                return Undefined.Instance;
            }
            finally
            {
                Status = ScriptStatus.Stopped;
            }
        }

        public override bool HasFunction(string aName)
        {
            return fGlobalObject.Get(aName) is EcmaScriptBaseFunctionObject;
        }


        public override void ExposeType(Type type, string name)
        {
            if (String.IsNullOrEmpty(name))
                name = type.Name;

            fGlobalObject.AddValue(name, new EcmaScriptObjectWrapper(null, type, fGlobalObject));
        }

        public void Include(string aFileName, string aData)
        {
            if (String.IsNullOrEmpty(SourceFileName))
                SourceFileName = "include.js";

            if (String.IsNullOrEmpty(aData))
                return;

            var lCallback = fCompiler.Parse(aFileName, aData);
            lCallback(fRoot, fGlobalObject, Utilities.EmptyParams);
        }

        public override bool Debug
        {
            get
            {
                return base.Debug;
            }
            set
            {
                if (base.Debug != value)
                {
                    base.Debug = value;
                    Clear();
                }
            }
        }

        public override void Clear(bool aGlobals = false)
        {
            fGlobalObject = new GlobalObject();
            if (aGlobals || (fScope == null))
                fScope = new EcmaScriptScope(null, fGlobalObject);
            else
                fScope.Global = fGlobalObject;

            if (Debug)
                fGlobalObject.Debug = this;

            var lRoot = new ObjectEnvironmentRecord(fScope, fGlobalObject, false);

            fRoot = new ExecutionContext(lRoot, false);
            fGlobalObject.ExecutionContext = fRoot;

            fCompiler = new EcmaScriptCompiler(
                new EcmaScriptCompilerOptions {
                    EmitDebugCalls = Debug,
                    GlobalObject = fGlobalObject,
                    Context = fRoot.LexicalScope,
                    JustFunctions = fJustFunctions
                });

            fGlobalObject.Parser = fCompiler;
        }


        public override object RunFunction(ScriptStatus initialStatus, string name, params object[] args)
        {
            try
            {
                var lItem = fGlobalObject.Get(name) as EcmaScriptBaseFunctionObject;

                if (lItem == null)
                    throw new ScriptComponentException(String.Format(ES5.Script.Properties.Resources.eNoSuchFunction, name));

                if (args == null)
                    args = Utilities.EmptyParams;

                if (initialStatus == ScriptStatus.StepInto)
                {
                    Status = initialStatus;
                    fLastFrame = fStackList.Count;
                }
                else
                {
                    Status = ScriptStatus.Running;
                }

                return lItem.Call(fRoot, args.Select(a => EcmaScriptScope.DoTryWrap(fGlobalObject, a)).ToArray());
            }
            catch (ScriptRuntimeException ex)
            {
                SetRunException(ex);
                throw;
            }
            finally
            {
                Status = ScriptStatus.Stopped;
            }
        }

        void SetRunException(ScriptRuntimeException ex)
        {
            RunException = ex;
            var w = ex.Original as EcmaScriptObjectWrapper;
            if (null != w)
            {
                var e = w.Value as Exception;
                if (e == null)
                    RunException = ex;
                else
                    RunException = e;
            }
            else
            {
                var eo = ex.Original as EcmaScriptObject;
                if (null != eo)
                {
                    RunException = new ScriptRuntimeException(eo.ToString());
                }
            }
        }
    }
}