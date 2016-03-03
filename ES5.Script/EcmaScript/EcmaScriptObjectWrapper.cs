using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ES5.Script.EcmaScript.Objects;
using ES5.Script.Properties;
using System.Collections;

namespace ES5.Script.EcmaScript
{
    public class EcmaScriptObjectWrapper : EcmaScriptBaseFunctionObject
    {
        class MethodEntry
        {
            public MethodBase Method { get; set; }
            public Type ParamsType { get; set; } // if it"s an is params 
            public ParameterInfo[] Args { get; set; }
        }

        object fValue;
        Type fType;
        Dictionary<string, MemberInfo> _fields;
        Dictionary<string, List<MethodBase>> _methods;

        public Type Type { get { return fType; } }
        public new object Value { get { return fValue; } }
        public bool Static { get { return fValue == null; } }

        public EcmaScriptObjectWrapper(object aValue, Type aType, GlobalObject aGlobal)
            : base(aGlobal, aGlobal.NativePrototype)
        {
            Class = "Native " + aType;
            fValue = aValue;
            fType = aType;
        }


        static void FindBestMatchingMethod(List<MethodBase> aMethods, object[] aParameters)
        {
            var lWork = new List<MethodEntry>();
            foreach (var el in aMethods) {
                var lPars = el.GetParameters() ?? new ParameterInfo[0];
                if (lPars.Any(b => b.ParameterType.IsByRef)) continue;

                Type lArrType = null;
                if (aParameters.Length != lPars.Length) {
                    if ((aParameters.Length >= (lPars.Length - 1)) && (lPars.Length > 0) &&
                        (lPars[lPars.Length - 1].GetCustomAttributes(typeof(ParamArrayAttribute), true).Length > 0)) {
                        lArrType = lPars[lPars.Length - 1].ParameterType.GetElementType();
                    }
                    else
                        continue;
                }
                else
                {
                    if ((lPars.Length > 0) &&
                        (lPars[lPars.Length - 1].GetCustomAttributes(typeof(ParamArrayAttribute), true).Length > 0) &&
                        (!(aParameters[aParameters.Length - 1] is EcmaScriptArrayObject)))
                        lArrType = lPars[lPars.Length - 1].ParameterType.GetElementType();
                }

                for (int i = 0, l = aParameters.Length; i < l; i++) {
                    if (!IsCompatibleType(aParameters[i]?.GetType(), i < lPars.Length ? (lArrType ?? lPars[i].ParameterType) : lArrType)) {
                        lPars = null;
                        break;
                    }
                }

                if (lPars == null)
                    continue;

                lWork.Add(new MethodEntry { Method = el, ParamsType = lArrType, Args = lPars });
            }

            var lResult = -1;
            for (int i = 0, l = lWork.Count; i < l; i++) {
                if (lResult == -1) {
                    lResult = i;
                    continue;
                }

                if (BetterFunctionMember(lWork[lResult], lWork[i], aParameters))
                    lResult = i;
            }

            if (lResult != -1) {
                var n = lWork[lResult];
                aMethods.Clear();
                aMethods.Add(n.Method);
            }
        }

        static int IsMoreSpecific(Type aBest, Type aCurrent)
        {
            if (aBest.IsGenericParameter && !aCurrent.IsGenericParameter) return -1;
            if (!aBest.IsGenericParameter && aCurrent.IsGenericParameter) return 1;

            return 0;
        }

        static bool IsInteger(Type o)
        {
            switch (Type.GetTypeCode(o))
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64: return true;
                default: return false;
            }
        }

        static bool IsFloat(Type o)
        {
            switch (Type.GetTypeCode(o))
            {
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double: return true;
                default: return false;
            }
        }

        static bool BetterFunctionMember(MethodEntry aBest, MethodEntry aCurrent, object[] aParameters)
        {
            var lAtLeastOneBetterConversion = false;
            for (int i = 0, l = aParameters.Length; i < l; i++)
            {
                var lBestParam = i >= aBest.Args.Length ? aBest.ParamsType : aBest.Args[i].ParameterType;
                var lCurrentParam = i >= aCurrent.Args.Length ? aCurrent.ParamsType : aCurrent.Args[i].ParameterType;
                switch (BetterConversionFromExpression(aParameters[i], lBestParam, lCurrentParam))
                {
                    case 1: return false;
                    case -1: lAtLeastOneBetterConversion = true; break;
                }
            }
            if (lAtLeastOneBetterConversion)
                return true;

            if ((aBest.Method.GetGenericArguments().Length != 0) && (aCurrent.Method.GetGenericArguments().Length == 0)) return true;
            if ((aBest.Method.GetGenericArguments().Length == 0) && (aCurrent.Method.GetGenericArguments().Length != 0)) return false; // return if the reverse is true
            if (aBest.Args.Length > aCurrent.Args.Length) return true;
            if (aBest.Args.Length < aCurrent.Args.Length) return false;

            for (int i = 0, l = aParameters.Length; i < l; i++)
            {
                var lBestParam = i >= aBest.Args.Length ? aBest.ParamsType : aBest.Args[i].ParameterType;
                var lCurrentParam = i >= aCurrent.Args.Length ? aCurrent.ParamsType : aCurrent.Args[i].ParameterType;
                switch (IsMoreSpecific(lBestParam, lCurrentParam))
                {
                    case 1: return false;
                    case -1: lAtLeastOneBetterConversion = true; break;
                }
            }
            return lAtLeastOneBetterConversion;
        }

        static int BetterConversionFromExpression(object aMine, Type aBest, Type aCurrent)
        {
            if (aBest == aCurrent)
                return 0;

            var lGT = aMine?.GetType();
            if (lGT == aBest) return 1;
            if (lGT == aCurrent) return -1;

            if (IsCompatibleType(aBest, aCurrent) && !IsCompatibleType(aCurrent, aBest)) return 1;
            if (IsCompatibleType(aCurrent, aBest) && !IsCompatibleType(aBest, aCurrent)) return -1;

            if (IsCompatibleType(lGT, aBest) && !IsCompatibleType(lGT, aCurrent)) return 1;
            if (IsCompatibleType(lGT, aCurrent) && !IsCompatibleType(lGT, aBest)) return -1;

            if ((lGT == null) && (aBest.IsValueType) && (!aCurrent.IsValueType)) return -1;
            if ((lGT == null) && (!aBest.IsValueType) && (aCurrent.IsValueType)) return 1;

            if (lGT != null)
            {
                if (IsFloat(lGT) && IsFloat(aCurrent) && !IsFloat(aBest)) return -1;
                if (IsFloat(lGT) && !IsFloat(aCurrent) && IsFloat(aBest)) return 1;

                if (IsInteger(lGT) && IsInteger(aCurrent) && !IsInteger(aBest)) return -1;
                if (IsInteger(lGT) && !IsInteger(aCurrent) && IsInteger(aBest)) return 1;
            }

            return 0;
        }

        public static bool IsCompatibleType(Type sourceType, Type targetType)
        {
            if ((sourceType == null) || (sourceType == typeof(Undefined)))
                return (!targetType.IsValueType);

            if (targetType.IsAssignableFrom(sourceType))
                return (true);

            if (targetType.IsGenericParameter)
                return true;

            if (((sourceType == typeof(Double)) || (sourceType == typeof(Int32))) &&
                          Utilities.InSet(Type.GetTypeCode(targetType), 
                            TypeCode.Byte, 
                            TypeCode.Char, 
                            TypeCode.DateTime, 
                            TypeCode.Decimal,
                            TypeCode.Double, 
                            TypeCode.Int16, 
                            TypeCode.Int32, 
                            TypeCode.Int64, 
                            TypeCode.SByte,
                            TypeCode.Single, 
                            TypeCode.UInt16, 
                            TypeCode.UInt32, 
                            TypeCode.UInt64))
                return (true);

            if (targetType == typeof(String))
                return (true);

            return (false);
        }

        public static object UnwrapValue(object value)
        {
            // Preferred method is EcmaScriptObjectWrapper.ConvertTo(Object, type): Object;
            // This method uses way less sophiscated approach, so some of the JS-specific value transitions are lost

            // if provided object was wrapped before then we should unwrap it
            var objectWrapper = value as EcmaScriptObjectWrapper;
            if (objectWrapper != null)
                return objectWrapper.Value;

            // DateTime handling
            var scriptObject = value as EcmaScriptObject;
            if (null != scriptObject)
            {
                if (scriptObject.Class != "Date")
                    return scriptObject.Value ?? value;

                if (scriptObject.Value.GetType() == typeof(DateTime))
                    return ((DateTime)scriptObject.Value).ToLocalTime();

                return GlobalObject.UnixToDateTime(Convert.ToInt64(scriptObject.Value)).ToLocalTime();
            }
            return value;
        }


        public static object ConvertTo(object value, Type type)
        {
            // Undefined -> Double is Double.NaN, not just nil
            // Null -> Double is 0.0, not just nil
            if (type == typeof(Double))
            {
                if (value == Undefined.Instance)
                    return double.NaN;

                if (value == null)
                    return 0;
            }

            if (value == null)
                return null;

            //if (value.GetType() == type)
            //    return value;

            if (value == Undefined.Instance)
                return type == typeof(Object) ? Undefined.Instance : null;

            var objectWrapper = value as EcmaScriptObjectWrapper;
            if (objectWrapper != null)
                return ConvertTo(objectWrapper.Value, type);

            // Unwrap EcmaScriptObject before conversion
            var wrapper = value as EcmaScriptObject;
            if (null != wrapper)
            {
                if (wrapper.Class == "Date")
                {
                    if (wrapper.Value.GetType() == typeof(DateTime))
                        value = ((DateTime)wrapper.Value).ToLocalTime();
                    else
                        value = GlobalObject.UnixToDateTime(Convert.ToInt64(wrapper.Value)).ToLocalTime();

                    return ConvertTo(value, type);
                }

                // For arbitrary EcmaScriptObject objects their .toString() method is called
                if (type == typeof(String))
                    return value.ToString();

                if (wrapper.Value != null)
                    return ConvertTo(wrapper.Value, type);
            }

            if (type.IsAssignableFrom(value.GetType()))
                return value;

            // Special cases
            #region Double -> DateTime
            // Double -> DateTime conversion
            // type in [ .. ] doesn"t compile on Silverlight
            var lValueType = value.GetType();
            if ((type == typeof(DateTime)) &&
                              ((lValueType == typeof(Double)) ||
                               (lValueType == typeof(Int32)) ||
                               (lValueType == typeof(Int64)) ||
                               (lValueType == typeof(UInt32)) ||
                               (lValueType == typeof(UInt64))))
                return GlobalObject.UnixToDateTime(Convert.ToInt64(value)).ToLocalTime();

            // Implicitly convert Date to its String representation while sending it to .NET code
            if (value.GetType() == typeof(DateTime))
            {
                if (type == typeof(String))
                    return Convert.ChangeType(value, type, System.Globalization.CultureInfo.CurrentCulture);

                // Implicit DateTime -> Double conversion
                if (type == typeof(Double))
                    return GlobalObject.DateTimeToUnix((DateTime)value);
            }
            #endregion

            #region Boolean
            // Special-case Boolean conversions
            if (type == typeof(Boolean))
            {
                // Number.NaN equals to false in JS while .NET converts it to true
                if ((value.GetType() == typeof(Double)) && double.IsNaN((double)value))
                    return false;

                // Arbitrary Strings are converted to Boolean using simple rule - empty string is False, anything else is True
                if (value.GetType() == typeof(String))
                    return !String.IsNullOrEmpty((String)value);

                if (value is EcmaScriptObject)
                    return true;
            }

            // In JS Boolean .toString is always lowercased, while .NET returs "True" or "False"
            if ((value.GetType() == typeof(Boolean)) && (type == typeof(String)))
                return (bool)value ? "true" : "false";
            #endregion

            #region Double
            // Special-case Double conversions
            if (type == typeof(Double))
            {
                // Arbitrary strings are converted to Double.NaN
                if (value.GetType() == typeof(String))
                {
                    double lResult;
                    if (double.TryParse((String)value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out lResult))
                        return lResult;

                    return Double.NaN;
                }

                // Special rules for Boolean -> Double conversion (JS supports this)
                if (value.GetType() == typeof(Boolean))
                    return (bool)value ? 1.0 : 0.0;
            }
            #endregion

            #region Int32, Int64, UInt32, UInt64
            // type in [ .. ] doesn"t compile on Silverlight
            if ((type == typeof(Int32)) || (type == typeof(Int64)) || (type == typeof(UInt32)) || (type == typeof(UInt64)))
            {
                // Convert String to Double first, and then to the target type
                if (value.GetType() == typeof(String))
                {
                    double lResult;
                    if (double.TryParse((String)value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out lResult))
                        return System.Convert.ChangeType(lResult, type, System.Globalization.CultureInfo.InvariantCulture);

                    throw new FormatException("Cannot convert provided String value to an Integer value");
                }

                // Throw away fraction part
                if (value.GetType() == typeof(Double))
                {
#if SILVERLIGHT
                    return System.Convert.ChangeType(Math.Abs((Double)value) * Math.Floor(Math.Abs((Double)value)), type, System.Globalization.CultureInfo.InvariantCulture);
#else
                    return Convert.ChangeType(Math.Truncate((Double)value), type, System.Globalization.CultureInfo.InvariantCulture);
#endif
                }
            }
            #endregion

            return Convert.ChangeType(value, type, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static object FindAndCallBestOverload(List<MethodBase> methods, GlobalObject root, string methodName, object self, object[] parameters)
        {
            var lMethods = methods;
            for (int i = 0, l = parameters.Length; i < l; i++)
                parameters[i] = EcmaScriptObjectWrapper.UnwrapValue(parameters[i]);

            FindBestMatchingMethod(lMethods, parameters);

            if (lMethods.Count > 1)
                root.RaiseNativeError(NativeErrorType.TypeError, String.Format(Resources.Ambigious_overloaded_method_0_with_1_parameters, methodName, parameters.Length));

            if (lMethods.Count == 0)
                root.RaiseNativeError(NativeErrorType.TypeError, String.Format(Resources.No_overloaded_method_0_with_1_parameters, methodName, parameters.Length));

            var lMeth = lMethods[0];
            var lParams = lMeth.GetParameters();
            var lReal = new Object[lParams.Length];
            var lParamStart = -1;

            if (((lParams.Length > 0) && (lParams[lParams.Length - 1].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)))
                lParamStart = lParams.Length - 1;

            for (int j = 0, l = parameters.Length; j < l; j++)
            {
                if ((lParamStart != -1) && (j >= lParamStart))
                {
                    if (j == lParamStart)
                        lReal[j] = Array.CreateInstance(lParams[lParams.Length - 1].ParameterType.GetElementType(), parameters.Length - lParamStart);

                    ((Array)lReal[lParamStart]).SetValue(EcmaScriptObjectWrapper.ConvertTo(parameters[j], lParams[lParams.Length - 1].ParameterType.GetElementType()), j - lParamStart);
                }
                else
                {
                    lReal[j] = EcmaScriptObjectWrapper.ConvertTo(parameters[j], lParams[j].ParameterType);
                }
            }

            for (int j = parameters.Length, l = (lReal.Length - 1); j < l; j++)
            {
                var lParameter = lParams[j];
                if ((lParamStart != -1) && (j >= lParamStart))
                {
                    lReal[j] = Array.CreateInstance(lParams[lParams.Length - 1].ParameterType.GetElementType(), 0);// call method with empty array
                    break;// create empty array and return no more parameters
                }
                else
                {
                    if (ParameterAttributes.HasDefault == (lParameter.Attributes & ParameterAttributes.HasDefault))
                    {
                        lReal[j] = lParameter.RawDefaultValue;
                    }
                    else
                    {
                        if (System.Type.GetTypeCode(lParameter.ParameterType) == TypeCode.Object)
                            lReal[j] = Undefined.Instance;
                    }
                }
            }

            try
            {
                if (lMeth is ConstructorInfo)
                    return (EcmaScriptScope.DoTryWrap(root, ((ConstructorInfo)lMeth).Invoke(lReal)));

                return (EcmaScriptScope.DoTryWrap(root, lMeth.Invoke(self, lReal)));
            }
            catch (ScriptRuntimeException)
            {
                throw;
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is ScriptRuntimeException)
                    throw ex.InnerException;

                throw new ScriptRuntimeException(EcmaScriptScope.DoTryWrap(root, ex.InnerException) as EcmaScriptObject);
            }
            catch (Exception ex)
            {
                throw new ScriptRuntimeException(EcmaScriptScope.DoTryWrap(root, ex) as EcmaScriptObject);
            }
        }

        MemberInfo GetMember(string aName)
        {
            if (_fields == null)
            {
                _fields = fType.GetMembers(BindingFlags.Public | BindingFlags.FlattenHierarchy | (Static ? BindingFlags.Static : BindingFlags.Instance))
                .Where(a => a.MemberType == MemberTypes.Field || a.MemberType == MemberTypes.Property)
                .ToDictionary(a => a.Name, a => a, StringComparer.InvariantCulture);
            }
            MemberInfo result;
            if (_fields.TryGetValue(aName, out result))
                return result;
            else
                return null;
        }

        public override bool DefineOwnProperty(string aName, PropertyValue aValue, bool aThrow)
        {
            var lItem = GetMember(aName);
            if (lItem == null)
            {
                return base.DefineOwnProperty(aName, aValue, aThrow);
            }
            else
            {
                switch (lItem.MemberType)
                {
                    case MemberTypes.Field:
                        var f = (FieldInfo)lItem;
                        if (f.IsInitOnly)
                        {
                            if (aThrow)
                                Root.RaiseNativeError(NativeErrorType.ReferenceError, "Readonly field");
                            return (false);
                        }

                        f.SetValue(fValue, EcmaScriptObjectWrapper.ConvertTo(aValue.Value, f.FieldType));

                        return (true);
                    case MemberTypes.Property:
                        var p = (PropertyInfo)lItem;
                        if (!p.CanWrite)
                        {
                            if (aThrow)
                                Root.RaiseNativeError(NativeErrorType.ReferenceError, "Readonly property");

                            return (false);
                        }

                        p.SetValue(fValue, EcmaScriptObjectWrapper.ConvertTo(aValue.Value, p.PropertyType), Utilities.EmptyParams);

                        return (true);
                }
            }

            if (aThrow)
                Root.RaiseNativeError(NativeErrorType.ReferenceError, "Readonly property");

            return (false);
        }

        public override PropertyValue GetOwnProperty(string aName, bool getPropertyValue)
        {
            List<MethodBase> methods = null;
            MemberInfo lItem = GetMember(aName);
            if (null == lItem)
                methods = GetMethods(aName);

            if (null == lItem && null == methods)
            {
                return base.GetOwnProperty(aName, getPropertyValue);
            }
            else
            {
                if (null == lItem)
                {
                    var aValue = new EcmaScriptObjectWrapper(new Overloads(fValue, methods), typeof(Overloads), Root);
                    return new PropertyValue(Objects.PropertyAttributes.None, aValue);
                }
                else
                {
                    object val = null;
                    switch (lItem.MemberType)
                    {
                        case MemberTypes.Field:
                            var f = (FieldInfo)lItem;
                            if (getPropertyValue)
                            {
                                val = f.GetValue(fValue);
                                val = EcmaScriptScope.DoTryWrap(Root, val);
                            }
                            return new PropertyValue(
                                          f.IsInitOnly ? Objects.PropertyAttributes.None : Objects.PropertyAttributes.Writable,
                                          val);
                        case MemberTypes.Property:
                            var p = (PropertyInfo)lItem;
                            if (p.CanRead && getPropertyValue)
                            {
                                val = p.GetValue(fValue, Utilities.EmptyParams);
                                val = EcmaScriptScope.DoTryWrap(Root, val);
                            }
                            return new PropertyValue(
                                p.CanWrite ? Objects.PropertyAttributes.Writable : Objects.PropertyAttributes.None,
                                val);
                    }
                }
            }
            return null;
        }

        List<MethodBase> GetMethods(string aName)
        {
            if (_methods == null)
            {
                _methods = fType
                    .GetMembers(BindingFlags.Public | BindingFlags.FlattenHierarchy | (Static ? BindingFlags.Static : BindingFlags.Instance))
                    .Where(a => a.MemberType == MemberTypes.Method)
                    .Cast<MethodBase>()
                    .Where(a => !((a.Attributes & MethodAttributes.SpecialName) !=0 && (a.Name.StartsWith("get_")|| a.Name.StartsWith("set_"))))
                    .GroupBy(a => a.Name)
                    .ToDictionary(a => a.Key, a => a.OrderBy(i => i.GetParameters().Length).ToList(), StringComparer.InvariantCulture);
            }

            List<MethodBase> result = null;
            if (_methods.TryGetValue(aName, out result) && result.Count != 0)
                return result;
            else
                return null;
        }

        public override object Call(ExecutionContext context, params object[] args)
        {
            if (typeof(MulticastDelegate).IsAssignableFrom(fType)) {
                var lMeth = fType.GetMethod("Invoke");
                if (lMeth != null)
                    return (FindAndCallBestOverload(new List<MethodBase>(new[] { lMeth }), Root, "Delegate Invoke", fValue, args));
            }

            if (typeof(Overloads) == fType) 
                return (FindAndCallBestOverload(((Overloads)fValue).Items, Root, ((Overloads)fValue).Items[0].Name, ((Overloads)fValue).Instance, args));

            Root.RaiseNativeError(NativeErrorType.ReferenceError, fType.ToString() + " not callable");

            return null;
        }

        public override object CallEx(ExecutionContext context, object aSelf, params object[] args)
        {
            return Call(context, args);
        }

        public override object Construct(ExecutionContext context, params object[] args)
        {
            if (!Static)
                Root.RaiseNativeError(NativeErrorType.ReferenceError, "Cannot call new on instance");

            var a = fType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Cast<MethodBase>()
                .ToList();

            return EcmaScriptObjectWrapper.FindAndCallBestOverload(a, Root, "<constructor>", null, args);
        }

        public override object Get(string aName, ExecutionContext aExecutionContext =  null, int aFlags =0)
        {
            if (((aFlags & 2) != 0) && !Static) {
                // default property
                var lItems = fType.GetDefaultMembers()
                               .Where(a => a.MemberType == MemberTypes.Property)
                               .Cast<PropertyInfo>()
                               .Where(a => a.GetIndexParameters().Length == 1)
                               .ToArray();

                var lIntValue = 0;
                var lIsInt = Int32.TryParse(aName, out lIntValue);
                MethodInfo lCall;
                if (lIsInt) {
                    lCall = lItems.Where(a => a.GetIndexParameters()[0].ParameterType == typeof(int))
                                      .FirstOrDefault()?.GetGetMethod();

                    if (lCall != null)
                        return (EcmaScriptScope.DoTryWrap(Root, lCall.Invoke(Value, new[] { (object)lIntValue })));
                }

                lCall = lItems.Where(a => a.GetIndexParameters()[0].ParameterType == typeof(String)).FirstOrDefault()?.GetGetMethod();
                if (lCall != null)
                    return EcmaScriptScope.DoTryWrap(Root, lCall.Invoke(Value, new[] { (object)aName }));

                Root.RaiseNativeError(NativeErrorType.ReferenceError, "No default indexer with string or integer parameter");
            }

            return base.Get(aName, aExecutionContext, aFlags);
        }

        public override object Put(string aName, object aValue, ExecutionContext aExecutionContext = null, int aFlags = 0)
        {
            if (((aFlags & 2) != 0) && !Static) {
                // default property
                if (aValue is EcmaScriptObjectWrapper)
                    aValue = ((EcmaScriptObjectWrapper)aValue).Value;

                var lItems = fType.GetDefaultMembers()
                                .Where(a => a.MemberType == MemberTypes.Property).Cast<PropertyInfo>()
                                .Where(a => a.GetIndexParameters().Length == 1)
                                .ToArray();

                var lIntValue = 0;
                var lIsInt = Int32.TryParse(aName, out lIntValue);
                MethodInfo lCall;
                if (lIsInt) {
                    lCall = lItems.Where(a => a.GetIndexParameters()[0].ParameterType == typeof(int))
                            .FirstOrDefault()?.GetSetMethod();
                    if (lCall != null)
                        return (EcmaScriptScope.DoTryWrap(Root, lCall.Invoke(Value, new[] { lIntValue, aValue })) ?? Undefined.Instance);
                }

                lCall = lItems.Where(a => a.GetIndexParameters()[0].ParameterType == typeof(String))
                        .FirstOrDefault()?.GetSetMethod();
                if (lCall != null)
                    return ((EcmaScriptScope.DoTryWrap(Root, lCall.Invoke(Value, new[] { aName, aValue })) ?? Undefined.Instance));

                Root.RaiseNativeError(NativeErrorType.ReferenceError, "No default indexer setter with string or integer parameter");
            }

            return base.Put(aName, aValue, aExecutionContext, aFlags);
        }

        public override IEnumerator<String> GetNames()
        {
            return Enumerable.Concat(IntGetNames(), GetOwnNames()).GetEnumerator();
        }


        public IEnumerable<String> GetOwnNames()
        {
            return fType.GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | (Static ? BindingFlags.Static : BindingFlags.Instance))
                   .Where(a => 0 == a.GetIndexParameters().Length)
                   .Select(a => a.Name);
        }

    }
}
