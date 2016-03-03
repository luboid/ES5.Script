using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public class ObjectEnvironmentRecord : EnvironmentRecord
    {
        EcmaScriptObject fObject;


        public override bool HasBinding(string aName)
        {
            return fObject.HasProperty(aName);
        }

        public override void CreateMutableBinding(string aName, bool aDeleteAfter)
        {
            if (fObject.HasProperty(aName))
                fObject.Root.RaiseNativeError(NativeErrorType.TypeError, "Duplicate property " + aName);

            var lProp = PropertyAttributes.All;
            if (!aDeleteAfter)
                lProp = lProp & ~PropertyAttributes.Configurable;

            fObject.DefineOwnProperty(aName, new PropertyValue(lProp, Undefined.Instance), false);
        }

        public override void SetMutableBinding(string aName, object aValue, bool aStrict)
        {
            fObject.Put(aName, aValue, null, aStrict ? 1 : 0);
        }

        public override object GetBindingValue(string aName, bool aStrict)
        {
            if ((fObject == Global) && (aName == "eval") && !aStrict)
            {
                return Global.NotStrictGlobalEvalFunc;
            }

            if (fObject.HasProperty(aName)) {
                return fObject.Get(aName);
            }

            if (aStrict)
                fObject.Root.RaiseNativeError(NativeErrorType.ReferenceError, aName + " does not exist in this object");

            return Undefined.Instance;
        }

        public override bool DeleteBinding(string aName)
        {
            return fObject.Delete(aName, false);
        }

        public override object ImplicitThisValue()
        {
            if (ProvideThis) return fObject; else return Undefined.Instance;
        }

        public ObjectEnvironmentRecord(EnvironmentRecord aPrevious, EcmaScriptObject aObject, bool aProvideThis = false)
            :base(aPrevious)
        {
            fObject = aObject;
            ProvideThis = aProvideThis;
        }

        public override IEnumerable<string> Names()
        {
            return fObject.Names;
        }

        public bool ProvideThis { get; set; }

        public override GlobalObject Global
        {
            get
            {
                return fObject.Root;
            }
            internal set { }
        }

        public override bool IsDeclarative { get { return false; } }
    }
}
