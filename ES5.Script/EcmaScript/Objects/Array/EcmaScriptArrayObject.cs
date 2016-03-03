using ES5.Script.EcmaScript.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptArrayObject : EcmaScriptObject
    {
        public EcmaScriptArrayObject(GlobalObject aRoot, object aLength)
            : base(aRoot, aRoot.ArrayPrototype)
        {
            Class = "Array";
            base.DefineOwnProperty("length", new PropertyValue(PropertyAttributes.Writable, 0), false);
            DefineOwnProperty("length", new PropertyValue(PropertyAttributes.Writable, aLength), false);
        }


        public EcmaScriptArrayObject(uint aCapacity, GlobalObject aRoot)
                    : base(aRoot, aRoot.ArrayPrototype)
        {
            Class = "Array";
            base.DefineOwnProperty("length", new PropertyValue(PropertyAttributes.Writable, 0), false);
        }


        public static bool TryGetArrayIndex(string s, out uint val)
        {
            return (UInt32.TryParse(s, out val) && (val != uint.MaxValue));
        }


        public override bool DefineOwnProperty(string aName, PropertyValue aValue, bool aThrow = true)
        {
            var lOldLenDesc = GetOwnProperty("length");
            var lOldLen = Utilities.GetObjAsCardinal(lOldLenDesc?.Value, Root.ExecutionContext);
            if (aName == "length") {
                if ((PropertyAttributes.HasValue & aValue.Attributes) == 0)
                    return base.DefineOwnProperty(aName, aValue, aThrow);

                var lNewLen = Utilities.GetObjAsCardinal(aValue.Value, Root.ExecutionContext);

                var lLenVal = (lNewLen < ((uint)Int32.MaxValue)) ? (Double)((Int32)lNewLen) : (Double)((Int64)lNewLen);

                if (!Utilities.GetObjAsBoolean(Operators.Equal(lLenVal, Utilities.GetObjAsDouble(aValue.Value, Root.ExecutionContext), Root.ExecutionContext), Root.ExecutionContext))
                    Root.RaiseNativeError(NativeErrorType.RangeError, "Index out of range");

                aValue.Value = lLenVal;
                if (lNewLen > lOldLen)
                    return base.DefineOwnProperty(aName, aValue, aThrow);

                if ((~PropertyAttributes.Writable & lOldLenDesc.Attributes) == 0) {
                    if (aThrow)
                        Root.RaiseNativeError(NativeErrorType.TypeError, "Value not writable");
                    return true;
                }

                var lNewWritable = (PropertyAttributes.Writable & aValue.Attributes) != 0;
                aValue.Attributes = aValue.Attributes | PropertyAttributes.Writable;
                if (!base.DefineOwnProperty(aName, aValue, aThrow))
                    return false; // set the actual length

                while (lNewLen < lOldLen) {
                    lOldLen = lOldLen - 1;
                    if (!Delete(lOldLen.ToString(), false)) {
                        lOldLen = lOldLen + 1;
                        aValue.Value = lOldLen < ((uint)Int32.MaxValue) ? (Double)((Int32)lOldLen) : (Double)lOldLen;
                        base.DefineOwnProperty(aName, aValue, false);
                        if (aThrow)
                            Root.RaiseNativeError(NativeErrorType.TypeError, "Element " + (lOldLen - 1) + " cannot be removed");
                        return true;
                    }
                }
                if (!lNewWritable) {
                    lOldLenDesc.Attributes = lOldLenDesc.Attributes & ~PropertyAttributes.Writable;
                }
                return true;
            }
            var lIndex = (uint)0;
            if (TryGetArrayIndex(aName, out lIndex)) {
                if ((PropertyAttributes.Writable & lOldLenDesc.Attributes) == 0 && (lIndex >= lOldLen)) {
                    if (aThrow)
                        Root.RaiseNativeError(NativeErrorType.TypeError, "Element out of range of array and length is readonly");
                    return true;
                }

                if (!base.DefineOwnProperty(aName, aValue, false)) {
                    if (aThrow)
                        Root.RaiseNativeError(NativeErrorType.TypeError, "Cannot write element " + aName);
                    return true;
                }

                if (lIndex >= lOldLen) {
                    lOldLen = lIndex + 1;
                    lOldLenDesc.Value = (lOldLen < ((uint)Int32.MaxValue)) ? (Double)((Int32)lOldLen) : (Double)lOldLen;
                }

                return true;
            }

            return base.DefineOwnProperty(aName, aValue, aThrow);
        }

        public uint Length
        {
            get
            {
                return Utilities.GetObjAsCardinal(GetOwnProperty("length").Value, Root.ExecutionContext);
            }
        }

        public EcmaScriptArrayObject AddValues(object[] items)
        {
            var lLen = Length;
            DefineOwnProperty("length", new PropertyValue(PropertyAttributes.Writable, lLen + items.Length), true);

            for (int i = 0, l = items.Length; i < l; i++)
                DefineOwnProperty((lLen + i).ToString(), new PropertyValue(PropertyAttributes.All, items[i]), true);

            return this;
        }


        public void AddValue(object aItem)
        {
            DefineOwnProperty(Length.ToString(), new PropertyValue(PropertyAttributes.All, aItem), true);
        }

        new public static readonly ConstructorInfo ConstructorInfo = typeof(EcmaScriptArrayObject).GetConstructor(new[] { typeof(uint), typeof(GlobalObject) });
        new public static readonly MethodInfo Method_AddValue = typeof(EcmaScriptArrayObject).GetMethod("AddValue", new[] { typeof(object) });
    }
}
