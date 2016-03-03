using ES5.Script.EcmaScript.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    /**
    * Да се пренапишат всички операции по итерация върху масива като ...
    */
    public partial class GlobalObject : EcmaScriptObject
    {
        public EcmaScriptFunctionObject DefaultCompareInstance { get; set; }

        public EcmaScriptObject CreateArray()
        {
            var result = Get("Array") as EcmaScriptObject;
            if (result != null) return result;

            result = new EcmaScriptArrayObjectObject(this) { Class = "Function" };
            result.Prototype = FunctionPrototype;
            Values.Add("Array", PropertyValue.NotEnum(result));

            ArrayPrototype = new EcmaScriptObject(this) { Class = "Array" };
            ArrayPrototype.Prototype = ObjectPrototype;

            result.Values["prototype"] = PropertyValue.NotAllFlags(ArrayPrototype);
            result.Values.Add("isArray", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "isArray", ArrayIsArray, 1)));

            DefaultCompareInstance = new EcmaScriptFunctionObject(this, "defaultCompare", DefaultCompare, 2);
            ArrayPrototype.Values["constructor"] = PropertyValue.NotEnum(result);
            //ArrayPrototype.Values["length"] = PropertyValue.NotAllFlags(0);
            ArrayPrototype.Values.Add("toString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toString", ArrayToString, 0, false, true)));
            ArrayPrototype.Values.Add("toLocaleString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toLocaleString", ArrayToLocaleString, 0, false, true)));
            ArrayPrototype.Values.Add("concat", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "concat", ArrayConcat, 1, false, true)));
            ArrayPrototype.Values.Add("join", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "join", ArrayJoin, 1, false, true)));
            ArrayPrototype.Values.Add("pop", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "pop", ArrayPop, 0, false, true)));
            ArrayPrototype.Values.Add("push", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "push", ArrayPush, 1, false, true)));
            ArrayPrototype.Values.Add("reverse", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "reverse", ArrayReverse, 0, false, true)));
            ArrayPrototype.Values.Add("shift", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "shift", ArrayShift, 0, false, true)));
            ArrayPrototype.Values.Add("slice", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "slice", ArraySlice, 2, false, true)));
            ArrayPrototype.Values.Add("sort", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "sort", ArraySort, 1, false, true)));
            ArrayPrototype.Values.Add("splice", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "splice", ArraySplice, 2, false, true)));
            ArrayPrototype.Values.Add("unshift", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "unshift", ArrayUnshift, 1, false, true)));

            ArrayPrototype.Values.Add("indexOf", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "indexOf", ArrayIndexOf, 1, false, true)));
            ArrayPrototype.Values.Add("lastIndexOf", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "lastIndexOf", ArrayLastIndexOf, 1, false, true)));
            ArrayPrototype.Values.Add("every", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "every", ArrayEvery, 1, false, true)));
            ArrayPrototype.Values.Add("some", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "some", ArraySome, 1, false, true)));
            ArrayPrototype.Values.Add("forEach", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "forEach", ArrayForeach, 1, false, true)));
            ArrayPrototype.Values.Add("map", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "map", ArrayMap, 1, false, true)));
            ArrayPrototype.Values.Add("filter", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "filter", ArrayFilter, 1, false, true)));
            ArrayPrototype.Values.Add("reduce", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "reduce", ArrayReduce, 1, false, true)));
            ArrayPrototype.Values.Add("reduceRight", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "reduceRight", ArrayReduceRight, 1, false, true)));

            return result;
        }


        public object ArrayCtor(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            if ((args.Length == 1) && ((args[0] is Int32) || (args[0] is Double)))
                return (new EcmaScriptArrayObject(this, args[0])); // create a new array of length arg

            return (new EcmaScriptArrayObject(this, 0).AddValues(args));
        }


        public object ArrayToString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.ToObject(aCaller, aSelf);
            var lJoin = lSelf.Get("join") as EcmaScriptBaseFunctionObject;
            if (lJoin == null)
                return ObjectToString(aCaller, aSelf);

            return lJoin.CallEx(aCaller, aSelf);
        }


        public object ArrayConcat(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.ToObject(aCaller, aSelf);
            EcmaScriptArrayObject lRes;
            var lLen = Utilities.GetObjAsInteger(lSelf.Get("length"), aCaller);
            lRes = new EcmaScriptArrayObject(this, 0);
            for (var i = 0; i < lLen; i++)
            {
                lRes.AddValue(lSelf.Get(i.ToString(), aCaller, 3));
            }

            foreach (var el in args)
            {
                if (el is EcmaScriptArrayObject)
                {
                    var a = (EcmaScriptArrayObject)el;
                    for (uint i = 0, l = a.Length; i < l; i++)
                    {
                        var lVal = a.GetOwnProperty(i.ToString());
                        if (lVal != null)
                            lRes.AddValue(lVal.Value);
                    }
                }
                else
                    lRes.AddValue(el);
            }
            return lRes;
        }


        public object ArrayJoin(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSep = args.Length == 0 ? null : Utilities.GetArgAsString(args, 0, aCaller);
            if (lSep == null) lSep = ",";
            var lSelf = Utilities.ToObject(aCaller, aSelf);
            var lRes = new StringBuilder();
            for (int i = 0, l = Utilities.GetObjAsInteger(lSelf.Get("length"), aCaller); i < l; i++) {
                if (i != 0)
                    lRes.Append(lSep);

                var member = lSelf.Get(i.ToString(), aCaller, 3);
                var lItem = member == lSelf ? null : Utilities.GetObjAsString(member, aCaller);
                if (lItem == null)
                    lItem = "";

                lRes.Append(lItem);
            }
            return lRes.ToString();
        }


        public object ArrayPop(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.ToObject(aCaller, aSelf);
            var lLen = Utilities.GetObjAsCardinal(lSelf.Get("length", aCaller), aCaller);
            if (lLen == 0)
            {
                lSelf.Put("length", 0, aCaller, 1);
                return Undefined.Instance;
            }
            var indx = (lLen - 1).ToString();
            var el = lSelf.Get(indx, aCaller);
            lSelf.Delete(indx, true);
            lLen = (lLen - 1);
            lSelf.Put("length", indx, aCaller);
            return el;
        }

        public object ArrayPush(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.ToObject(aCaller, aSelf);
            var lLen = Utilities.GetObjAsCardinal(lSelf.Get("length", aCaller), aCaller);
            foreach (var el in args) {
                lSelf.Put(lLen.ToString(), el, aCaller);
                ++lLen;
            }
            var length = lLen < (uint)Int32.MaxValue ? (int)lLen : (double)lLen;
            return Put("length", length, aCaller);
        }


        public object ArrayReverse(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.ToObject(aCaller, aSelf);
            var lLen = Utilities.GetObjAsCardinal(lSelf.Get("length", aCaller), aCaller);
            var lMid = lLen / 2;
            var lLow = 0;
            while (lLow != lMid) {
                var lUp = lLen - lLow - 1;
                var lUpP = lUp.ToString();
                var lLowP = lLow.ToString();
                var lLowVal = lSelf.Get(lLowP, aCaller);
                var lUpVal = lSelf.Get(lUpP, aCaller);
                var lLowerExists = lSelf.HasProperty(lLowP);
                var lUpperExists = lSelf.HasProperty(lUpP);
                if (lLowerExists && lUpperExists) {
                    lSelf.Put(lLowP, lUpVal, aCaller, 1);
                    lSelf.Put(lUpP, lLowVal, aCaller, 1);
                } else if (!lLowerExists && lUpperExists) {
                    lSelf.Put(lLowP, lUpVal, aCaller, 1);
                    lSelf.Delete(lUpP, true);
                } else if (!lLowerExists && !lUpperExists) {
                    lSelf.Delete(lLowP, true);
                    lSelf.Put(lUpP, lLowVal, aCaller, 1);
                };

                lLow++;
            }
            return lSelf;
        }


        public object ArrayShift(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.ToObject(aCaller, aSelf);
            var lLen = Utilities.GetObjAsCardinal(lSelf.Get("length", aCaller), aCaller);

            if (lLen == 0) {
                lSelf.Put("length", 0, aCaller, 1);
                return (Undefined.Instance);
            }

            var first = lSelf.Get("0", aCaller);
            var k = (uint)1;
            while (k < lLen) {
                var lFrom = k.ToString();
                var lTo = (k - 1).ToString();
                if (lSelf.HasProperty(lFrom))
                    lSelf.Put(lTo, lSelf.Get(lFrom, aCaller), aCaller);
                else
                    lSelf.Delete(lTo, true);
                ++k;
            }

            var lNewLen = (lLen - 1).ToString();
            lSelf.Delete(lNewLen, true);
            lSelf.Put("length", lNewLen, aCaller);

            return (first);
        }

        public object ArraySlice(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.ToObject(aCaller, aSelf);
            var lLen = Utilities.GetObjAsCardinal(lSelf.Get("length", aCaller), aCaller);

            var lRelStart = Utilities.GetArgAsDouble(args, 0, aCaller);
            var k = (uint)0;
            if (lRelStart < 0)
                k = (uint)((Int64)(Math.Max((double)(lLen + lRelStart), 0)));
            else
                k = (uint)((Int64)(Math.Min(lRelStart, (Double)((Int64)lLen))));

            var lRelEnd = ((args.Length < 2) || (args[1] == Undefined.Instance)) ? lLen : Utilities.GetArgAsDouble(args, 1, aCaller);

            var lFinal = lRelEnd < 0 ? (uint)(Math.Max(lLen + (int)lRelEnd, 0)) : (uint)((Int64)(Math.Min(lRelEnd, (Double)lLen)));
            var a = new EcmaScriptArrayObject(Root, 0);
            var n = 0;
            while (k < lFinal)
            {
                var pk = k.ToString();
                if (lSelf.HasProperty(pk))
                {
                    a.Put(n.ToString(), lSelf.Get(pk, aCaller), aCaller, 0);
                }

                ++(n);
                ++(k);
            }

            return a;
        }

        public object ArraySort(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.ToObject(aCaller, aSelf);
            var lLen = Utilities.GetObjAsCardinal(lSelf.Get("length", aCaller), aCaller);

            var lFunc = Utilities.GetArg(args, 0) as EcmaScriptBaseFunctionObject;
            if (lFunc == null)
                lFunc = DefaultCompareInstance;

            Sort(lSelf, 0, (int)(lLen - 1),
              (EcmaScriptObject aList, int L, int R) => {
                  if (L != R) this.Swap(this.ExecutionContext, aList, L, R);
              },
              (EcmaScriptObject aList, int L, int R) =>
              {
                  var jString = L.ToString();
                  var kString = R.ToString();
                  var hasJ = aList.HasProperty(jString);
                  var hasK = aList.HasProperty(kString);
                  if (!hasJ && !hasK) return 0;
                  if (!hasJ) return 1;
                  if (!hasK) return -1;
                  var x = aList.Get(jString, aCaller, 0);
                  var y = aList.Get(kString, aCaller, 0);
                  if ((x == Undefined.Instance) && (y == Undefined.Instance)) return 0;
                  if (x == Undefined.Instance) return 1;
                  if (y == Undefined.Instance) return -1;
                  return Utilities.GetObjAsInteger(lFunc.CallEx(aCaller, aList, x, y), aCaller);
              });
            return lSelf;
        }

        public object ArraySplice(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.ToObject(aCaller, aSelf);
            var A = new EcmaScriptArrayObject(this, 0);
            var lLen = Utilities.GetObjAsCardinal(lSelf.Get("length", aCaller, 0), aCaller);

            var lRelativeStart = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lActualStart = lRelativeStart < 0 ? Math.Max(lLen + lRelativeStart, 0) : Math.Min(lRelativeStart, (int)lLen);

            var lActualDeleteCount = Math.Min(Math.Max(Utilities.GetArgAsInteger(args, 1, aCaller), 0), lLen - lActualStart);
            var k = 0L;
            while (k < lActualDeleteCount) {
                var lFrom = (lRelativeStart + k).ToString();
                if (lSelf.HasProperty(lFrom)) {
                    A.Put(k.ToString(), lSelf.Get(lFrom, aCaller), aCaller);
                };
                ++(k);
            }

            if ((args.Length - 2) < lActualDeleteCount) {
                k = lActualStart;
                while (k < (lLen - lActualDeleteCount))
                {
                    var lFrom = (k + lActualDeleteCount).ToString();
                    var lTo = (k + args.Length - 2).ToString();
                    if (lSelf.HasProperty(lFrom))
                        lSelf.Put(lTo, lSelf.Get(lFrom, aCaller), aCaller);
                    else
                        lSelf.Delete(lTo, true);
                    ++(k);
                }
            } else if ((args.Length - 2) > lActualDeleteCount) {
                k = lLen - lActualDeleteCount;
                while (k > lActualStart)
                {
                    var lFrom = (k + lActualDeleteCount - 1).ToString();
                    var lTo = (k + args.Length - 3).ToString();
                    if (lSelf.HasProperty(lFrom))
                        lSelf.Put(lTo, lSelf.Get(lFrom, aCaller), aCaller);
                    else
                        lSelf.Delete(lTo, true);
                    --(k);
                }
            }
            k = lActualStart;

            for (int i = 2, l = args.Length; i < l; i++)
            {
                lSelf.Put(k.ToString(), args[i], aCaller);
                ++k;
            }

            lSelf.Put("length", (int)(lLen - lActualDeleteCount + (args.Length - 2)), aCaller);

            return (A);
        }

        public object ArrayUnshift(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lSelf = Utilities.ToObject(aCaller, aSelf);
            var lLen = Utilities.GetObjAsCardinal(lSelf.Get("length", aCaller), aCaller);
            var lArgCount = args.Length;
            var k = lLen;
            while (k > 0) {
                var lFrom = (k - 1).ToString();
                var lTo = (k + lArgCount - 1).ToString();
                if (lSelf.HasProperty(lFrom)) {
                    lSelf.Put(lTo, lSelf.Get(lFrom, aCaller), aCaller);
                } else
                    lSelf.Delete(lTo, true);
                --(k);
            }

            for (int j = 0, l = args.Length; j < l; j++)
                lSelf.Put(j.ToString(), args[j], aCaller, 1);

            lSelf.Put("length", ((int)lLen) + lArgCount, aCaller);

            return (lSelf);
        }


        public object DefaultCompare(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lLeft = args[0];
            var lRight = args[1];
            if (lLeft == lRight) return 0;
            if (lLeft == Undefined.Instance) return 1;
            if (lRight == Undefined.Instance) return -1;
            if (lLeft == null) return 1;
            if (lRight == null) return -1;

            if ((lLeft is String) || (lRight is String))
                return String.Compare(Utilities.GetObjAsString(lLeft, aCaller),
                                      Utilities.GetObjAsString(lRight, aCaller),
                                      StringComparison.Ordinal);

            if (lLeft is EcmaScriptObject)
                return (lRight is EcmaScriptObject ? 1 : 0);
            if (lRight is EcmaScriptObject)
                return -1;

            return Utilities.GetObjAsDouble(lLeft, aCaller).CompareTo(Utilities.GetObjAsDouble(lRight, aCaller));
        }


        public object ArrayToLocaleString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lObj = Utilities.ToObject(aCaller, aSelf);
            if (lObj == null)
                RaiseNativeError(NativeErrorType.ReferenceError, "Object type expected");

            var lLen = Utilities.GetObjAsInteger(lObj.Get("length", aCaller, 0), aCaller);
            var lRes = new StringBuilder();
            for (int i = 0; i < lLen; i++)
            {
                var lVal = Utilities.ToObject(aCaller, lObj.Get(i.ToString(), aCaller, 2));
                string lData;
                if (lVal == null)
                {
                    lData = String.Empty;
                }
                else
                {
                    var lLocale = lVal.Get("toLocaleString") as EcmaScriptFunctionObject;

                    if (lLocale == null)
                        RaiseNativeError(NativeErrorType.ReferenceError, "element " + i + " in array does not have a callable toLocaleString");

                    lData = Utilities.GetObjAsString(lLocale.CallEx(aCaller, lVal), aCaller);
                };

                if (i != 0)
                    lRes.Append(",");

                lRes.Append(lData);
            }

            return (lRes.ToString());
        }


        public bool Sort<T>(T aList, int aStart, int aEnd, Action<T, int, int> aSwap, Func<T, int, int, int> aCompare)
        {
            int I; bool result = false;
            do {
                I = aStart;
                var J = aEnd;
                var P = (aStart + aEnd) >> 1;

                do {
                    while (aCompare(aList, I, P) < 0) ++I;

                    while (aCompare(aList, J, P) > 0) --J;

                    if (I <= J)
                    {
                        result = true;
                        aSwap(aList, I, J);

                        if (P == I)
                            P = J;
                        else if (P == J)
                            P = I;

                        ++(I);
                        --(J);
                    }
                } while (!(I > J));

                if (aStart < J)
                    result = Sort<T>(aList, aStart, J, aSwap, aCompare) || result;

                aStart = I;
            } while (!(I >= aEnd));

            return result;
        }


        public object ArrayIsArray(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var obj = Utilities.GetArg(args, 0) as EcmaScriptObject;
            if (obj != null)
            {
                if (obj is EcmaScriptArrayObject)
                {
                    return true;
                }
                return obj == ArrayPrototype;
            }
            return false;
        }

        public object ArrayIndexOf(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lObj = Utilities.ToObject(aCaller, aSelf);
            var lLen = Utilities.GetObjAsCardinal(lObj.Get("length"), aCaller);
            var lElement = Utilities.GetArg(args, 0);
            var lStart = Utilities.GetArgAsInt64(args, 1, aCaller);

            if (lStart >= lLen)
                return (-1);

            if (lStart < 0)
                lStart = lLen + lStart;

            while (lStart < lLen) {
                var lIndex = lStart.ToString();
                if (lObj.HasProperty(lIndex))
                    if ((bool)(Operators.StrictEqual(lObj.Get(lIndex, aCaller, 2), lElement, aCaller)))
                        return (lStart);
                lStart = lStart + 1;
            }

            return (-1);
        }

        public object ArrayEvery(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lObj = Utilities.ToObject(aCaller, aSelf);
            var lLen = Utilities.GetObjAsCardinal(lObj.Get("length"), aCaller);
            var lCallback = Utilities.GetArg(args, 0) as EcmaScriptBaseFunctionObject;
            var lCallbackThis = Utilities.GetArg(args, 1) ?? Undefined.Instance;
            if (lCallback == null)
                RaiseNativeError(NativeErrorType.TypeError, "Delegate expected");

            var ticks = GetTicks(lObj); PropertyValue value;
            for (uint i = 0; i < lLen; i++)
            {
                if (lObj.Deleted)
                    break;

                var lIndex = i.ToString();
                if (null != (value = lObj.GetProperty(lIndex)) && value.Ticks <= ticks[value.Owner])
                    if (!Utilities.GetObjAsBoolean(lCallback.CallEx(aCaller, lCallbackThis, lObj.Get(value, aCaller, 2), i, lObj), aCaller))
                        return false;
            }
            return true;
        }

        public object ArrayLastIndexOf(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lObj = Utilities.ToObject(aCaller, aSelf);
            var lLen = Utilities.GetObjAsCardinal(lObj.Get("length"), aCaller);
            var lElement = Utilities.GetArg(args, 0);
            var lStart = (args.Length >= 2) ? Utilities.GetArgAsInt64(args, 1, aCaller) : lLen;
            if (lLen == 0) return -1;
            if (lStart >= lLen) lStart = lLen - 1;
            if (lStart < 0) lStart = lLen + lStart;
            while (lStart >= 0) {
                var lIndex = lStart.ToString();
                if (lObj.HasProperty(lIndex))
                    if ((bool)Operators.StrictEqual(lObj.Get(lIndex, aCaller, 2), lElement, aCaller))
                        return lStart;
                lStart = lStart - 1;
            }
            return -1;
        }

        public object ArraySome(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lObj = Utilities.ToObject(aCaller, aSelf);
            var lLen = (uint)0;
            // some strange behaviours
            var lArrayLen = lObj.Get("length") as EcmaScriptArrayObject;
            if (lArrayLen != null)
            {
                lLen = lArrayLen.Length;
                if (lLen == 0)
                {
                    return false;
                }
                else
                {
                    lLen = Utilities.GetObjAsCardinal(lArrayLen, aCaller);// TODO: Try if it cant be converted, tihs always return 0
                    if (0 == lLen)
                    {
                        return -1;
                    }
                }
            }
            else
            {
                lLen = Utilities.GetObjAsCardinal(lObj.Get("length"), aCaller);
            }
            //if (lLen == 0)
            //    return true;

            var lCallback = Utilities.GetArg(args, 0) as EcmaScriptBaseFunctionObject;
            var lCallbackThis = Utilities.GetArg(args, 1) ?? Undefined.Instance;
            if (lCallback == null)
                RaiseNativeError(NativeErrorType.TypeError, "Delegate expected");

            var ticks = GetTicks(lObj); PropertyValue value;
            for (uint i = 0; i < lLen; i++)
            {
                if (lObj.Deleted)
                    break;

                var lIndex = i.ToString();
                if (null != (value = lObj.GetProperty(lIndex)) && value.Ticks <= ticks[value.Owner])
                    if (Utilities.GetObjAsBoolean(lCallback.CallEx(aCaller, lCallbackThis, lObj.Get(value, aCaller, 2), i, lObj), aCaller))
                        return true;
            }
            return false;
        }

        public object ArrayMap(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lObj = Utilities.ToObject(aCaller, aSelf);
            var lLen = Utilities.GetObjAsCardinal(lObj.Get("length"), aCaller);
            var lCallback = Utilities.GetArg(args, 0) as EcmaScriptBaseFunctionObject;
            var lCallbackThis = Utilities.GetArg(args, 1) ?? Undefined.Instance;
            if (lCallback == null)
                RaiseNativeError(NativeErrorType.TypeError, "Delegate expected");
            var ticks = GetTicks(lObj); PropertyValue value; 

            var lRes = new EcmaScriptArrayObject(lLen, this);
            for (uint i = 0; i < lLen; i++)
            {
                if (lObj.Deleted)
                {
                    lRes.AddValue(Undefined.Instance);
                    break;
                }

                var lIndex = i.ToString();
                if (null != (value = lObj.GetProperty(lIndex)) && value.Ticks <= ticks[value.Owner])
                {
                    lRes.AddValue(lCallback.CallEx(aCaller, lCallbackThis, lObj.Get(value, aCaller, 2), i, lObj));
                }
            }

            //chapter15/15.4/15.4.4/15.4.4.19/15.4.4.19-8-7.js
            //chapter15\15.4\15.4.4\15.4.4.19\15.4.4.19-8-b-1.js
            lRes.Put("length", lLen);

            return lRes;
        }

        public object ArrayForeach(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lObj = Utilities.ToObject(aCaller, aSelf);
            var lLen = Utilities.GetObjAsCardinal(lObj.Get("length"), aCaller);
            var lCallback = Utilities.GetArg(args, 0) as EcmaScriptBaseFunctionObject;
            var lCallbackThis = Utilities.GetArg(args, 1) ?? Undefined.Instance;
            if (lCallback == null)
                RaiseNativeError(NativeErrorType.TypeError, "Delegate expected");

            PropertyValue value;
            for (uint i = 0; i < lLen; i++) {
                if (lObj.Deleted)
                    break;

                var lIndex = i.ToString();
                if (null != (value = lObj.GetProperty(lIndex)))
                    lCallback.CallEx(aCaller, lCallbackThis, lObj.Get(value, aCaller, 2), i, lObj);
            }
            return Undefined.Instance;
        }


        public object ArrayFilter(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lObj = Utilities.ToObject(aCaller, aSelf);
            var lLen = Utilities.GetObjAsCardinal(lObj.Get("length"), aCaller);
            var lCallback = Utilities.GetArg(args, 0) as EcmaScriptBaseFunctionObject;
            var lCallbackThis = Utilities.GetArg(args, 1) ?? Undefined.Instance;
            if (lCallback == null) RaiseNativeError(NativeErrorType.TypeError, "Delegate expected");
            var lRes = new EcmaScriptArrayObject(lLen, this); PropertyValue value;
            var ticks = GetTicks(lObj);

            for (uint i = 0; i < lLen; i++) {
                if (lObj.Deleted)
                    break;

                var lIndex = i.ToString();
                if (null != (value = lObj.GetProperty(lIndex)) && value.Ticks <= ticks[value.Owner]) {
                    var lGet = lObj.Get(value, aCaller, 2);
                    if (Utilities.GetObjAsBoolean(lCallback.CallEx(aCaller, lCallbackThis, lGet, i, lObj), aCaller))
                        lRes.AddValue(lGet);
                };
            };
            return lRes;
        }

        public object ArrayReduce(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lObj = Utilities.ToObject(aCaller, aSelf);
            var lLen = Utilities.GetObjAsCardinal(lObj.Get("length"), aCaller);
            var lCallback = Utilities.GetArg(args, 0) as EcmaScriptBaseFunctionObject;
            if (lCallback == null)
                RaiseNativeError(NativeErrorType.TypeError, "Delegate expected");

            var lInitialValue = Utilities.GetArg(args, 1);
            var lGotInitial = false;
            if (args.Length >= 2)
                lGotInitial = true;

            var k = 0; PropertyValue value; string lKey;
            if (!lGotInitial)
            {
                while (k < lLen) {
                    lKey = k.ToString();
                    ++(k);
                    if (null != (value = lObj.GetProperty(lKey))) {
                        lGotInitial = true;
                        lInitialValue = lObj.Get(value, aCaller, 2);
                        break;
                    }
                }
            }

            if (k == lLen)
            {
                if (lGotInitial)
                {
                    return lInitialValue;
                }
                else
                {
                    RaiseNativeError(NativeErrorType.TypeError, "Empty array.");
                }
            }

            var ticks = GetTicks(lObj);
            while (k < lLen)
            {
                if (lObj.Deleted)
                {
                    break;
                }

                lKey = k.ToString();
                if (null != (value = lObj.GetProperty(lKey)) && value.Ticks <= ticks[value.Owner])
                {
                    lInitialValue = lCallback.CallEx(aCaller, Undefined.Instance, lInitialValue, lObj.Get(value, aCaller, 2), k, lObj);
                }
                ++(k);
            }
            return lInitialValue;
        }

        Dictionary<EcmaScriptObject, ulong> GetTicks(EcmaScriptObject obj)
        {
            var ticks = new Dictionary<EcmaScriptObject, ulong>();
            while (obj != null)
            {
                ticks[obj] = obj.Ticks;
                obj = obj.Prototype;
            }
            return ticks;
        }

        public object ArrayReduceRight(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lObj = Utilities.ToObject(aCaller, aSelf);
            var lLen = Utilities.GetObjAsCardinal(lObj.Get("length"), aCaller);
            var lCallback = Utilities.GetArg(args, 0) as EcmaScriptBaseFunctionObject;
            if (lCallback == null)
                RaiseNativeError(NativeErrorType.TypeError, "Delegate expected");

            var lInitialValue = Utilities.GetArg(args, 1);
            var lGotInitial = false;
            if (args.Length >= 2)
                lGotInitial = true;


            var k = lLen; PropertyValue value; string lKey;
            if (!lGotInitial && k != 0)
            {
                do
                {
                    --k;
                    lKey = k.ToString();
                    if (null != (value = lObj.GetProperty(lKey)))
                    {
                        lGotInitial = true;
                        lInitialValue = lObj.Get(value, aCaller, 2);
                        break;
                    }
                }
                while (k != 0);
            }

            if (0 == k)
            {
                if (lGotInitial)
                {
                    return lInitialValue;
                }
                else
                {
                    RaiseNativeError(NativeErrorType.TypeError, "Empty array.");
                }
            }

            var ticks = GetTicks(lObj);
            do
            {
                if (lObj.Deleted)
                {
                    break;
                }

                --k;
                lKey = k.ToString();
                if (null != (value = lObj.GetProperty(lKey)) && value.Ticks <= ticks[value.Owner])
                {
                    lInitialValue = lCallback.CallEx(aCaller, Undefined.Instance, lInitialValue, lObj.Get(value, aCaller, 2), k, lObj);
                }
            }
            while (k != 0);

            return lInitialValue;
        }


        void Swap(ExecutionContext ec, EcmaScriptObject aSelf, Int32 L, Int32 R)
        {
            var LStr = L.ToString();
            var LVal = aSelf.Get(LStr, ec);
            var RStr = R.ToString();
            var RVal = aSelf.Get(RStr, ec);

            var lHasL = aSelf.HasProperty(LStr);
            var lHasR = aSelf.HasProperty(RStr);

            if (lHasL && lHasR) {
                aSelf.Put(LStr, RVal, ec, 1);
                aSelf.Put(RStr, LVal, ec, 1);
                return;
            }

            if (!lHasL && lHasR) {
                aSelf.Put(LStr, RVal, ec, 1);
                aSelf.Delete(RStr, true);
                return;
            }

            if (!lHasL && !lHasR)
            {
                aSelf.Delete(LStr, true);
                aSelf.Put( RStr, LVal, ec, 1);
            }
        }
    }
}