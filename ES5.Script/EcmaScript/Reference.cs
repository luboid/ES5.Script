using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public class Reference
    {
        public Reference(object aBase, string aName, int aStrict)
        {
            Base = aBase;
            Name = aName;
            Flags = aStrict;
        }

        public object Base { get; set; } // Undefined, Simple, IEnvironmentRecord or Object
        public string Name { get; set; }
        public int Flags { get; set; }
        public bool Strict
        {
            get
            {
                return 0 != (1 & Flags);
            }
        }

        public bool ArrayAccess
        {
            get
            {
                return 0 != (2 & Flags);
            }
        }

        public static object GetValue(object aReference, ExecutionContext aExecutionContext)
        {
            var lRef = aReference as Reference;
            if (lRef == null)
            {
                return aReference;
            }

            if (lRef.Base == Undefined.Instance)
                aExecutionContext.Global.RaiseNativeError(NativeErrorType.ReferenceError, lRef.Name + " is not defined");

            if (lRef.Base == null)
                return aExecutionContext.Global.ObjectPrototype.Get(lRef.Name, aExecutionContext);

            var lObj = lRef.Base as EcmaScriptObject;
            if (lObj != null)
                return lObj.Get(lRef.Name, aExecutionContext, lRef.Flags);

            var lExec = lRef.Base as EnvironmentRecord;
            if (lExec != null)
                return lExec.GetBindingValue(lRef.Name, lRef.Strict);

            if (lRef.Base is Boolean) return aExecutionContext.Global.BooleanPrototype.Get(lRef.Name, aExecutionContext);
            if (lRef.Base is Int32) return aExecutionContext.Global.NumberPrototype.Get(lRef.Name, aExecutionContext);
            if (lRef.Base is Double) return aExecutionContext.Global.NumberPrototype.Get(lRef.Name, aExecutionContext);
            if (lRef.Base is String)
            {
                if (lRef.Name == "length")
                    return ((String)lRef.Base).Length;

                return aExecutionContext.Global.StringPrototype.Get(lRef.Name, aExecutionContext);
            }

            return null;
        }

        public static object SetValue(object aReference, object aValue, ExecutionContext aExecutionContext)
        {
            var lRef = aReference as Reference;
            if (lRef == null)
            {
                aExecutionContext.Global.RaiseNativeError(NativeErrorType.ReferenceError, "Invalid left-hand side in assignment");
                return lRef;
            }

            if (lRef.Base == Undefined.Instance)
                if (lRef.Strict)
                    aExecutionContext.Global.RaiseNativeError(NativeErrorType.TypeError, "Cannot call " + lRef.Name + " on undefined");
                else
                    return aExecutionContext.Global.Put(lRef.Name, aValue, aExecutionContext, lRef.Flags);

            var lObj = lRef.Base as EcmaScriptObject;
            if (lObj != null)
                return lObj.Put(lRef.Name, aValue, aExecutionContext, lRef.Flags);

            var lExec = lRef.Base as EnvironmentRecord;
            if (lExec != null)
            {
                lExec.SetMutableBinding(lRef.Name, aValue, lRef.Strict);
                return aValue;
            }

            if (lRef.Strict)
                aExecutionContext.Global.RaiseNativeError(NativeErrorType.TypeError, "Cannot set value on transient object");

            return aValue; // readonly so the on the fly Object 
        }

        public static bool Delete(object aReference, ExecutionContext aExecutionContext)
        {
            var lRef = aReference as Reference;
            if (lRef == null) return true;
            if ((lRef.Base == null) || (lRef.Base == Undefined.Instance))
            {
                if (lRef.Strict)
                    aExecutionContext.Global.RaiseNativeError(NativeErrorType.SyntaxError, "Cannot delete undefined reference");

                return true;
            }

            var lObj = lRef.Base as EcmaScriptObject;
            if (lObj != null)
                return lObj.Delete(lRef.Name, lRef.Strict);

            var lExec = (lRef.Base as EnvironmentRecord);
            if (lExec != null)
            {
                if (lRef.Strict)
                    aExecutionContext.Global.RaiseNativeError(NativeErrorType.SyntaxError, "Cannot delete execution context element");

                return lExec.DeleteBinding(lRef.Name);
            }

            if (lRef.Strict)
                aExecutionContext.Global.RaiseNativeError(NativeErrorType.SyntaxError, "Cannot delete transient object");

            return false;
        }

        public static Reference CreateReference(object aBase, object aSub, ExecutionContext aExecutionContext, int aStrict)
        {
            if (aBase == null)
                aExecutionContext.Global.RaiseNativeError(NativeErrorType.TypeError, "Cannot get property on null");
            if (aBase == Undefined.Instance)
                aExecutionContext.Global.RaiseNativeError(NativeErrorType.TypeError, "Cannot get property of undefined");

            return new Reference(aBase, Utilities.GetObjAsString(aSub, aExecutionContext), aStrict);
        }

        public static readonly MethodInfo Method_GetValue = typeof(Reference).GetMethod("GetValue");
        public static readonly MethodInfo Method_SetValue = typeof(Reference).GetMethod("SetValue");
        public static readonly MethodInfo Method_Delete = typeof(Reference).GetMethod("Delete");
        public static readonly MethodInfo Method_Create = typeof(Reference).GetMethod("CreateReference");
    }
}