using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public class DeclarativeEnvironmentRecord : EnvironmentRecord
    {
        GlobalObject fGlobal;
        Dictionary<String, PropertyValue> fBag = new Dictionary<String, PropertyValue>();

        public DeclarativeEnvironmentRecord(EnvironmentRecord aPrevious, GlobalObject aGlobal)
            : base(aPrevious)
        {
            fGlobal = aGlobal;
        }

        public override object ImplicitThisValue()
        {
            return null;
        }

        public override bool HasBinding(string aName)
        {
            return fBag.ContainsKey(aName);
        }

        public override void CreateMutableBinding(string aName, bool aDeleteAfter)
        {
            if (fBag.ContainsKey(aName))
                fGlobal.RaiseNativeError(NativeErrorType.TypeError, "Duplicate property: " + aName);

            fBag.Add(aName, new PropertyValue(Objects.PropertyAttributes.Writable | Objects.PropertyAttributes.Configurable, Undefined.Instance));
        }

        public override void SetMutableBinding(string aName, object aValue, bool aStrict)
        {
            PropertyValue lVal;
            if (!fBag.TryGetValue(aName, out lVal)) {
                fGlobal.RaiseNativeError(NativeErrorType.TypeError, "Unknown property: " + aName);
            }

            if ((Objects.PropertyAttributes.Writable & lVal.Attributes) == 0)
                fGlobal.RaiseNativeError(NativeErrorType.TypeError, "Property is immutable: " + aName);

            lVal.Value = aValue;
        }

        public override object GetBindingValue(string aName, bool aStrict)
        {
            PropertyValue lVal;
            if (!fBag.TryGetValue(aName, out lVal))
                fGlobal.RaiseNativeError(NativeErrorType.TypeError, "Unknown property: " + aName);

            if ((lVal.Attributes == Objects.PropertyAttributes.Configurable) && (lVal.Value == Undefined.Instance) && aStrict)  // immutable but not set yet
                fGlobal.RaiseNativeError(NativeErrorType.ReferenceError, "Property not initialized: " + aName);

            return lVal.Value;
        }

        public override bool DeleteBinding(string aName)
        {
            PropertyValue lVal;
            if (!fBag.TryGetValue(aName, out lVal))
                return true;

            if (lVal.Attributes != (Objects.PropertyAttributes.Configurable | Objects.PropertyAttributes.Writable))
                return false;

            var p = fBag[aName];
            if (null != p)
            {
                p.Deleted = true;
                var o = p.Value as EcmaScriptObject;
                if (null != o)
                {
                    o.Deleted = true;
                }
            }
            return fBag.Remove(aName);
        }

        public void CreateImmutableBinding(string aName)
        {
            if (fBag.ContainsKey(aName))
                fGlobal.RaiseNativeError(NativeErrorType.TypeError, "Duplicate property: " + aName);

            fBag.Add(aName, new PropertyValue(Objects.PropertyAttributes.Configurable, Undefined.Instance));
        }

        public void InitializeImmutableBinding(string aName, object aValue)
        {
            PropertyValue lVal;
            if (!fBag.TryGetValue(aName, out lVal))
                fGlobal.RaiseNativeError(NativeErrorType.TypeError, "Unknown property: " + aName);

            if ((Objects.PropertyAttributes.Configurable | Objects.PropertyAttributes.HasValue) != lVal.Attributes)
                fGlobal.RaiseNativeError(NativeErrorType.TypeError, "Property not an unitialized immutable: " + aName);

            lVal.Attributes = Objects.PropertyAttributes.None;
            lVal.Value = aValue;
        }

        public static EcmaScriptBaseFunctionObject SetAndInitializeImmutable(EcmaScriptBaseFunctionObject val, string aName)
        {
            var lSelf = (val.Scope as DeclarativeEnvironmentRecord);
            lSelf.CreateImmutableBinding(aName);
            lSelf.InitializeImmutableBinding(aName, val);
            return val;
        }

        public override IEnumerable<string> Names()
        {
            return fBag.Keys;
        }

        public override GlobalObject Global
        {
            get
            {
                return fGlobal;
            }
            internal set
            {
                fGlobal = value;
            }
        }

        public Dictionary<String, PropertyValue> Bag
        {
            get
            {
                return fBag;
            }
        }

        public override bool IsDeclarative { get { return true; } }

        public static readonly ConstructorInfo ConstructorInfo = typeof(DeclarativeEnvironmentRecord).GetConstructor(new[] { typeof(EnvironmentRecord), typeof(GlobalObject) });
        public static readonly MethodInfo Method_SetAndInitializeImmutable = typeof(DeclarativeEnvironmentRecord).GetMethod("SetAndInitializeImmutable");
    }
}