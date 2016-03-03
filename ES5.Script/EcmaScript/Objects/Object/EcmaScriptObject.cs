using ES5.Script.EcmaScript.Bindings;
using ES5.Script.EcmaScript.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptObject
    {
        PropertyValues fValues;
        GlobalObject fGlobal;

        public EcmaScriptObject(GlobalObject obj):
            this(obj, obj?.ObjectPrototype)
        { }

        public EcmaScriptObject(GlobalObject obj, EcmaScriptObject aProto)
        {
            fGlobal = obj;
            Prototype = aProto;
            fValues = new PropertyValues(this);
        }

        public ulong Ticks { get; set; } = 0;
        public bool Deleted { get; set; } = false;
        public bool Extensible { get; set; } = true;
        public GlobalObject Root { get { return fGlobal; } set { fGlobal = value; } }

        public Dictionary<String, PropertyValue> Values
        {
            get
            {
                return fValues;
            }
        }

        public EcmaScriptObject Prototype { get; set; }
        public string Class { get; set; } = "Object";
        public object Value { get; set; }
        public IEnumerable<string> Names
        {
            get
            {
                return Values.Keys;
            }
        }

        public virtual PropertyValue GetOwnProperty(string name, bool getPropertyValue = true)
        {
            PropertyValue lResult;
            if (fValues.TryGetValue(name, out lResult))
                return lResult;

            return null;
        }

        public virtual PropertyValue GetProperty(string name)
        {
            var lSelf = this; PropertyValue lResult;
            while (lSelf != null)
            {
                lResult = lSelf.GetOwnProperty(name);
                if (lResult != null)
                    return lResult;

                lSelf = lSelf.Prototype;
            }
            return null;
        }

        public virtual object Get(string aName, ExecutionContext aExecutionContext = null, int aFlags = 0)
        {
            var lDesc = GetProperty(aName);
            return Get(lDesc, aExecutionContext, aFlags);
        }

        public virtual object Get(PropertyValue aDesc, ExecutionContext aExecutionContext = null, int aFlags = 0)
        {
            if (aDesc == null || aDesc.Deleted)
                return Undefined.Instance;

            if (IsDataDescriptor(aDesc))
                return aDesc.Value;

            if (IsAccessorDescriptor(aDesc) && (aDesc.Get != null))
                return aDesc.Get.CallEx(aExecutionContext ?? Root.ExecutionContext, this);

            return Undefined.Instance;
        }

        public virtual bool CanPut(string name)
        {
            var lValue = GetOwnProperty(name, false);
            if (lValue != null)
            {
                if (IsAccessorDescriptor(lValue))
                    return lValue.Set != null;

                if (IsDataDescriptor(lValue))
                    return (PropertyAttributes.Writable & lValue.Attributes) != 0;
            }

            var lProperty = Prototype?.GetProperty(name);
            if (lProperty != null)
            {
                if (IsAccessorDescriptor(lProperty))
                    return (lProperty.Set != null);

                if (IsDataDescriptor(lProperty))
                    return (PropertyAttributes.Writable & lProperty.Attributes) != 0;
            }

            return Extensible;
        }

        public virtual object Put(string propertyName, object propertyValue, ExecutionContext context = null, int flags = 0)
        {
            if (!CanPut(propertyName))
            {
                if (0 != (flags & 1))
                    Root.RaiseNativeError(NativeErrorType.TypeError, "Property " + propertyName + " cannot be written to");

                return Undefined.Instance;
            }

            var lOwn = GetOwnProperty(propertyName, false);
            if ((lOwn != null) && IsDataDescriptor(lOwn))
            {
                if (DefineOwnProperty(propertyName, new PropertyValue(lOwn.Attributes, propertyValue), (0 != (flags & 1))))
                    return propertyValue;

                return Undefined.Instance;
            }

            lOwn = GetProperty(propertyName);
            if ((lOwn != null) && IsAccessorDescriptor(lOwn) && (lOwn.Set != null))
            {
                return lOwn.Set.CallEx(context ?? Root.ExecutionContext, this, new[] { propertyValue });
            }

            if (DefineOwnProperty(propertyName, new PropertyValue(PropertyAttributes.All, propertyValue), (0 != (flags & 1))))
                return propertyValue;

            return Undefined.Instance;
        }

        public EcmaScriptObject AddValue(string aValue, object aData)
        {
            Values[aValue] = new PropertyValue(PropertyAttributes.All, aData) { Ticks = ++Ticks };
            return this;
        }

        public EcmaScriptObject AddValues(string[] aValue, object[] aData)
        {
            if (aValue.Length != aData.Length)
                throw new ArgumentException();

            ++Ticks;
            for (int i = 0, l = aValue.Length; i < l; i++)
                Values[aValue[i]] = new PropertyValue(PropertyAttributes.All, aData[i]) { Ticks = Ticks };

            return this;
        }

        public EcmaScriptObject ObjectLiteralSet(string aName, FunctionDeclarationType aMode, object aData, bool aStrict)
        {
            PropertyValue lDescr;
            switch (aMode) {
                case FunctionDeclarationType.Get:
                    lDescr = new PropertyValue(PropertyAttributes.Configurable | PropertyAttributes.Enumerable, (EcmaScriptBaseFunctionObject)aData, null);
                    break;
                case FunctionDeclarationType.Set:
                    lDescr = new PropertyValue(PropertyAttributes.Configurable | PropertyAttributes.Enumerable, null, (EcmaScriptBaseFunctionObject)aData);
                    break;
                default: // Internal.FunctionDeclarationType.None
                    lDescr = new PropertyValue(PropertyAttributes.All, aData);
                    break;
            } // case

            //if aStrict and ((aName = 'eval') or (aName = 'arguments')) then begin
            //  Root.RaiseNativeError(NativeErrorType.SyntaxError, 'eval and arguments not allowed as object literals when using strict mode');
            //end;
            var lOwn = GetOwnProperty(aName);
            if (lOwn != null)
            {
                if (aStrict && IsDataDescriptor(lOwn) && IsDataDescriptor(lDescr))
                    Root.RaiseNativeError(NativeErrorType.SyntaxError, "Duplicate property");

                if (IsDataDescriptor(lOwn) && IsAccessorDescriptor(lDescr))
                    Root.RaiseNativeError(NativeErrorType.SyntaxError, "Duplicate property");

                if (IsAccessorDescriptor(lOwn) && IsDataDescriptor(lDescr))
                    Root.RaiseNativeError(NativeErrorType.SyntaxError, "Duplicate property");

                if (IsAccessorDescriptor(lOwn) && IsAccessorDescriptor(lDescr) && (((lOwn.Get != null) == (lDescr.Get != null)) || (lOwn.Set != null) == (lDescr.Set != null)))
                    Root.RaiseNativeError(NativeErrorType.SyntaxError, "Duplicate property");
            }
            DefineOwnProperty(aName, lDescr, false);
            return this;
        }

        public virtual bool HasProperty(string aName)
        {
            return GetProperty(aName) != null;
        }

        public virtual bool Delete(string aName, bool aThrow)
        {
            var lValue = GetOwnProperty(aName);
            if (lValue == null) return true;
            if ((PropertyAttributes.Configurable | lValue.Attributes) != 0)
            {
                var p = fValues[aName];
                if (null != p)
                {
                    p.Deleted = true;
                    var o = p.Value as EcmaScriptObject;
                    if (null != o)
                    {
                        o.Deleted = true;
                    }
                }
                return fValues.Remove(aName);
            }

            if (aThrow)
                Root.RaiseNativeError(NativeErrorType.TypeError, "Cannot delete property " + aName + ".");

            return false;
        }

        public virtual bool DefineOwnProperty(string aName, PropertyValue aValue, bool aThrow = true)
        {
            var lCurrent = GetOwnProperty(aName);
            return DefineOwnProperty(aName, lCurrent, aValue, aThrow);
        }

        public virtual bool DefineOwnProperty(string aName, PropertyValue aCurrent, PropertyValue aValue, bool aThrow = true)
        {
            if (aCurrent == null) {
                if (Extensible) {
                    aValue.Ticks = ++Ticks;
                    fValues[aName] = aValue;
                    return true;
                } else {
                    if (aThrow)
                        Root.RaiseNativeError(NativeErrorType.TypeError, "Object not extensible");
                    return false;
                }
            }

            // this is not always true 
            // chapter15/15.2/15.2.3/15.2.3.6/15.2.3.6-4-20.js
            // if (IsGenericDescriptor(aValue) && (aValue.Attributes == PropertyAttributes.None))
            //    return true;

            if ((aValue.Attributes == aCurrent.Attributes) && (Operators.SameValue(aValue.Value, aCurrent.Value, Root.ExecutionContext)) && (aValue.Get == aCurrent.Get) && (aValue.Set == aCurrent.Set))
                return true;

            if ((PropertyAttributes.Configurable & aCurrent.Attributes) == 0)
            {
                if ((PropertyAttributes.Configurable & aValue.Attributes) != 0)
                {
                    if (aThrow) Root.RaiseNativeError(NativeErrorType.TypeError, "Property " + aName + " not configurable");
                    return false;
                }
                if ((PropertyAttributes.Enumerable & aValue.Attributes) != (PropertyAttributes.Enumerable & aCurrent.Attributes))
                {
                    if (aThrow) Root.RaiseNativeError(NativeErrorType.TypeError, "Property " + aName + " enumerable mismatch");
                    return false;
                }
            }

            if (!IsGenericDescriptor(aValue)) {
                if (IsDataDescriptor(aValue) != IsDataDescriptor(aCurrent)) {
                    if ((PropertyAttributes.Configurable & aCurrent.Attributes) == 0) {
                        if (aThrow) Root.RaiseNativeError(NativeErrorType.TypeError, "Property " + aName + " not configurable");
                        return false;
                    }
                    if (IsDataDescriptor(aCurrent)) {
                        aCurrent.Attributes = aCurrent.Attributes & ~PropertyAttributes.Writable;
                        aCurrent.Set = aValue.Set;
                        aCurrent.Get = aValue.Set;
                    } else {
                        aCurrent.Attributes = aCurrent.Attributes & ~PropertyAttributes.Writable | aValue.Attributes;
                        aCurrent.Set = aValue.Set;
                        aCurrent.Get = aValue.Set;
                    }
                } else
                    if (IsDataDescriptor(aValue) && IsDataDescriptor(aCurrent)) {
                    if ((PropertyAttributes.Configurable & aCurrent.Attributes) == 0) {
                        if (((PropertyAttributes.Writable & aCurrent.Attributes) == 0) && ((PropertyAttributes.Writable & aValue.Attributes) != 0)) {
                            if (aThrow) Root.RaiseNativeError(NativeErrorType.TypeError, "Property " + aName + " not writable");
                            return false;
                        }
                        if (((PropertyAttributes.Writable & aCurrent.Attributes) == 0) && !Operators.SameValue(aValue.Value, aCurrent.Value, Root.ExecutionContext)) {
                            if (aThrow) Root.RaiseNativeError(NativeErrorType.TypeError, "Property " + aName + " not writable");
                            return false;
                        }
                    }
                } else
                if (IsAccessorDescriptor(aValue) && IsAccessorDescriptor(aCurrent)) {
                    if ((PropertyAttributes.Configurable & aCurrent.Attributes) == 0) {
                        if ((aCurrent.Get != aValue.Get) || (aCurrent.Set != aValue.Set)) {
                            if (aThrow) Root.RaiseNativeError(NativeErrorType.TypeError, "Property " + aName + " not writable");
                            return false;
                        }
                    }
                }
            }

            if (aCurrent.Value == Undefined.Instance)
            {
                aCurrent.Ticks = ++Ticks;
            }

            aCurrent.Value = aValue.Value;
            aCurrent.Set = aValue.Set;
            aCurrent.Get = aValue.Get;
            aCurrent.Attributes = aCurrent.Attributes | aValue.Attributes;

            return true;
        }

        public virtual object Construct(ExecutionContext context, params object[] args)
        {
            Root?.RaiseNativeError(NativeErrorType.TypeError, "Object is not a function");
            return null;
        }

        public virtual object Call(ExecutionContext context, params object[] args)
        {
            Root?.RaiseNativeError(NativeErrorType.TypeError, "Object is not a function");
            return null;
        }

        public virtual object CallEx(ExecutionContext context, object aSelf, params object[] args)
        {
            Root.RaiseNativeError(NativeErrorType.TypeError, "Object " + ToString() + " is not a function");
            return null;
        }

        public override string ToString()
        {
            var lFunc = (EcmaScriptObject)Get("toString");
            if (null != lFunc)
                return Utilities.GetObjAsString(lFunc.CallEx(null, this), Root.ExecutionContext);

            return "[object " + Class + "]";
        }

        public bool IsAccessorDescriptor(PropertyValue aProp)
        {
            return (aProp.Get != null) || (aProp.Set != null);
        }

        public bool IsDataDescriptor(PropertyValue aProp)
        {
            return ((PropertyAttributes.Writable & aProp.Attributes) != 0) || ((PropertyAttributes.HasValue & aProp.Attributes) != 0);
        }

        public bool IsGenericDescriptor(PropertyValue aProp)
        {
            return !IsAccessorDescriptor(aProp) && !IsDataDescriptor(aProp);
        }

        public EcmaScriptObject FromPropertyDescriptor(PropertyValue aProp)
        {
            var lRes = new EcmaScriptObject(Root, Root.ObjectPrototype);
            lRes.Put("value", aProp.Value);
            lRes.Put("writable", (PropertyAttributes.Writable & aProp.Attributes) != 0);
            lRes.Put("enumerable", (PropertyAttributes.Enumerable & aProp.Attributes) != 0);
            lRes.Put("configurable", (PropertyAttributes.Configurable & aProp.Attributes) != 0);
            if (aProp.Get != null) lRes.Put("get", aProp.Get);
            if (aProp.Set != null) lRes.Put("set", aProp.Set);
            return lRes;
        }

        public PropertyValue ToPropertyDescriptor(EcmaScriptObject aProp)
        {
            if ((aProp.HasProperty("get") || aProp.HasProperty("set")) && (aProp.HasProperty("writable") || aProp.HasProperty("value")))
                Root.RaiseNativeError(NativeErrorType.TypeError, "Descriptor has 'get/set' and 'writable/value' present.");

            var result = new PropertyValue(PropertyAttributes.None, null);
            if (aProp.HasProperty("enumerable"))
                if (Utilities.GetObjAsBoolean(aProp.Get("enumerable"), Root.ExecutionContext))
                    result.Attributes = result.Attributes | PropertyAttributes.Enumerable;

            if (aProp.HasProperty("configurable"))
                if (Utilities.GetObjAsBoolean(aProp.Get("configurable"), Root.ExecutionContext))
                    result.Attributes = result.Attributes | PropertyAttributes.Configurable;

            result.Value = Undefined.Instance;
            if (aProp.HasProperty("value"))
            {
                result.Value = aProp.Get("value");
            }
            else
            {
                if (aProp.HasProperty("get") || aProp.HasProperty("set"))
                {
                    result.Attributes = result.Attributes & ~PropertyAttributes.HasValue;
                }
            }
            if (aProp.HasProperty("writable"))
                if (Utilities.GetObjAsBoolean(aProp.Get("writable"), Root.ExecutionContext))
                    result.Attributes = result.Attributes | PropertyAttributes.Writable;

            if (aProp.HasProperty("get")) {
                var lGet = aProp.Get("get");
                if (lGet != Undefined.Instance) {
                    if (!(lGet is EcmaScriptBaseFunctionObject))
                        Root.RaiseNativeError(NativeErrorType.TypeError, "get is not callable");

                    result.Get = (EcmaScriptBaseFunctionObject)lGet;
                }
            }

            if (aProp.HasProperty("set")) {
                var lset = aProp.Get("set");
                if (lset != Undefined.Instance) {
                    if (!(lset is EcmaScriptBaseFunctionObject))
                        Root.RaiseNativeError(NativeErrorType.TypeError, "set is not callable");

                    result.Set = (EcmaScriptBaseFunctionObject)lset;
                }
            }

            return result;
        }

        public virtual IEnumerator<string> GetNames()
        {
            return IntGetNames().GetEnumerator();
        }

        public IEnumerable<string> IntGetNames()
        {
            var lItems = new List<String>();
            var lCurr = this;
            while (null != lCurr)
            {
                foreach (var el in lCurr.Values)
                {
                    if ((PropertyAttributes.Enumerable & el.Value.Attributes) != 0)
                    {
                        if (!lItems.Contains(el.Key))
                            lItems.Add(el.Key);
                    }
                }
                lCurr = lCurr.Prototype;
            }
            return System.Linq.Enumerable.Where(lItems, a => HasProperty(a));
        }

        public static object CallHelper(object Ref, object aSelf, object[] arg, ExecutionContext ec)
        {
            var lRef = Ref as Reference;
            object lThis;
            if (lRef != null)
            {
                var lEr = lRef.Base as EnvironmentRecord;
                if (lEr != null) {
                    lThis = lEr.ImplicitThisValue();
                    if ((lThis == null) || (lThis == Undefined.Instance))
                        lThis = aSelf;
                } else
                    lThis = lRef.Base;
            } else
                lThis = null;

            var lVal = Reference.GetValue(Ref, ec);
            if ((lVal == null) || (lVal == Undefined.Instance)) {
                if (lRef == null)
                    ec.Global.RaiseNativeError(NativeErrorType.TypeError, "Cannot call non-object value");

                ec.Global.RaiseNativeError(NativeErrorType.TypeError, "Object " + lRef.Base?.ToString() + " has no method \"" + lRef.Name + "\"");
            }
            var lFunc = lVal as EcmaScriptBaseFunctionObject;
            if (lFunc == null) {
                if (lRef == null)
                    ec.Global.RaiseNativeError(NativeErrorType.TypeError, "Cannot call non-object value");

                ec.Global.RaiseNativeError(NativeErrorType.TypeError, "Property \"" + lRef.Name + "\" of object " + lRef.Base?.ToString() + " is not callable");
            }

            return lFunc.CallEx(ec, lThis, arg);
        }

        public static readonly ConstructorInfo ConstructorInfo = typeof(EcmaScriptObject).GetConstructor(new[] { typeof(GlobalObject) });
        public static readonly MethodInfo Method_ObjectLiteralSet = typeof(EcmaScriptObject).GetMethod("ObjectLiteralSet");
        public static readonly MethodInfo Method_GetNames = typeof(EcmaScriptObject).GetMethod("GetNames");
        public static readonly MethodInfo Method_AddValue = typeof(EcmaScriptObject).GetMethod("AddValue");
        public static readonly MethodInfo Method_Construct = typeof(EcmaScriptObject).GetMethod("Construct");
        public static readonly MethodInfo Method_Call = typeof(EcmaScriptObject).GetMethod("Call");
        public static readonly MethodInfo Method_CallEx = typeof(EcmaScriptObject).GetMethod("CallEx");
        public static readonly MethodInfo Method_CallHelper = typeof(EcmaScriptObject).GetMethod("CallHelper");
    }
}