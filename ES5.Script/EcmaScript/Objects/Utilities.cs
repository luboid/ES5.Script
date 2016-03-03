using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public static class Utilities
    {
        public static readonly object[] EmptyParams = new object[0];
        static readonly Encoding fEncoding = Encoding.UTF8;
        static readonly char fChar0 = (char)0;

        const Double EPSILON = 0.00000000001;
        const Int32 PRECISION = 18;

        static char[] SplitNumberToCharArray(long value)
        {
            if (value == 0L)
                return (new[] { '0' });

            var buffer = new StringBuilder(32);
            long remainder;
            while (value > 0)
            {
                value = DivRem(value, 10, out remainder);
                buffer.Append((byte)remainder);
            }

            return buffer.ToString().ToCharArray().Reverse().ToArray();
        }

        static string DoubleToString(double value)
        {
            // Border cases
            if (Double.IsNaN(value))
                return ("NaN");

            if ((value >= -EPSILON) && (value <= EPSILON))
                return ("0");

            if (Double.IsNegativeInfinity(value))
                return "-Infinity";

            if (Double.IsPositiveInfinity(value))
                return ("Infinity");

            var lResult = new StringBuilder(128);
            if (value < 0)
            {
                value = -value;
                lResult.Append("-");
            }

            // At this point we know that value > EPSILON
            // Specification step 5. Determine n, k and s
            var lRoundedValueLog10 = Convert.ToInt32(Math.Floor(Math.Log10(value))) + 1;
            var n_k = 0; // this is value of (n-k)

            // Integer part
            // First thing is to double-check for overflow
            double s_temp = value;
            var lIntegertPartPower = lRoundedValueLog10;
            if (lIntegertPartPower >= PRECISION)
            {
                for (var i = 0; i <= (lIntegertPartPower - PRECISION - 1); i++)
                {
                    s_temp = s_temp / 10;
                    n_k++;
                }
                s_temp = Math.Round(s_temp);
            }

            // Let"s check is s divisible by 10 or not
            var s = Convert.ToInt64(s_temp);
            if (lIntegertPartPower > 0)
            {
                var remainder = 0L;
                while (true)
                {
                    var s0 = Utilities.DivRem(s, 10, out remainder);
                    if (remainder == 0L)
                    {
                        s = s0;
                        ++n_k;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Now we know s
            // Next step is to determine its length
            int s_length = Math.Max(0, lRoundedValueLog10 + n_k);

            // Check if there is any place for floating point part
            if (s_length < 19)
            {
                // Optimization
                // Cut the border for really small numbers
                if (value < 1.0)
                {
                    n_k = 0;
                    var lFractionalPower = Convert.ToInt32(Math.Floor(Math.Log10(value))) + 1;
                    for (var i = -1; i >= lFractionalPower; i--)
                    {
                        s_temp = s_temp * 10.0;
                        --n_k;
                    }
                }

                var s_double = Math.Floor(value);
                var n_k_fractional = 0;
                var lIsFractionalPartPresent = false;

                var multiplier = 10L;
                while ((Math.Abs(s_temp - s_double) > EPSILON) && (s_length < (19 - 1)))
                {
                    s_temp = value * multiplier;
                    multiplier = multiplier * 10;
                    s_double = Math.Floor(s_temp + EPSILON);
                    lIsFractionalPartPresent = true;
                    --n_k_fractional;
                    ++s_length;
                }

                if (lIsFractionalPartPresent)
                {
                    n_k = n_k_fractional;
                    s = Convert.ToInt64(s_double);
                    if (Math.Abs(s_temp - s_double) >= 0.5) ++s;
                }
            }

            // so we know values for s and (n-k)
            // let"s think about k
            // we split s into chars so we"ll immediately know both k and String representation of this number

            var s_char_array = SplitNumberToCharArray(s);

            var k = s_char_array.Length;
            var n = n_k + k;


            // At this point s, n & k are known

            // Specification step 6. Result
            if ((k <= n) && (n <= 21))
            {
                for (var i = 0; i <= (k - 1); i++)
                    lResult.Append(s_char_array[i]);

                for (var i = 0; i <= (n_k - 1); i++)
                    lResult.Append("0");

                return (lResult.ToString());
            }


            // Specification step 7. Result
            if ((0 < n) && (n <= 21))
            {
                for (var i = 0; i <= (n - 1); i++)
                    lResult.Append(s_char_array[i]);

                lResult.Append(".");

                for (var i = n; i <= (k - 1); i++)
                {
                    if (AllZeroesAhead(s_char_array, i))
                        break;

                    lResult.Append(s_char_array[i]);
                }

                return (lResult.ToString());
            }

            // Specification step 8. Result
            if ((-6 < n) && (n <= 0))
            {
                lResult.Append("0");
                lResult.Append(".");

                for (var i = n; i <= (-n - 1); i++)
                    lResult.Append("0");

                for (var i = 0; i <= (k - 1); i++)
                {
                    if (AllZeroesAhead(s_char_array, i))
                        break;

                    lResult.Append(s_char_array[i]);
                }

                return (lResult.ToString());
            }

            // Specification step 9. Result
            if (k == 1)
            {
                lResult.Append(s_char_array[0]);
                lResult.Append("e");
                lResult.Append(n - 1 > 0 ? "+" : "-");
                lResult.Append(Math.Abs(n - 1).ToString(CultureInfo.InvariantCulture));

                return (lResult.ToString());
            }

            // Specification step 10. Result
            lResult.Append(s_char_array[0]);
            lResult.Append(".");
            for (var i = 1; i <= (k - 1); i++)
            {
                if (AllZeroesAhead(s_char_array, i)) break;
                lResult.Append(s_char_array[i]);
            }
            lResult.Append("e");
            lResult.Append((n - 1 > 0 ? "+" : "-"));
            lResult.Append(Math.Abs(n - 1).ToString(CultureInfo.InvariantCulture));

            return (lResult.ToString());
        }

        //class method AllZeroesAhead(arr: array of Char; i: Int32): Boolean;
        public static bool AllZeroesAhead(char[] arr, int i)
        {
            for (var n = i; n < arr.Length; i++)
                if (arr[n] != '0') return false;
            return true;
        }

        public static object GetObjectAsPrimitive(ExecutionContext ec, EcmaScriptObject arg, PrimitiveType aPrimitive)
        {
            if (aPrimitive == PrimitiveType.None)
                aPrimitive = arg.Class == "Date" ?
                    PrimitiveType.String : PrimitiveType.Number;

            if (aPrimitive == PrimitiveType.String)
            {
                var func = (EcmaScriptBaseFunctionObject)arg.Get("toString");
                if (func != null)
                {
                    var result = func.CallEx(ec, arg, new char[0]);
                    if (IsPrimitive(result)) return result;
                }

                func = (EcmaScriptBaseFunctionObject)arg.Get("valueOf");
                if (func != null)
                {
                    var result = func.CallEx(ec, arg, new char[0]);
                    if (IsPrimitive(result)) return result;
                }

            }
            else
            {
                var func = (EcmaScriptBaseFunctionObject)arg.Get("valueOf");
                if (func != null)
                {
                    var result = func.CallEx(ec, arg, new char[0]);
                    if (IsPrimitive(result)) return result;
                }

                func = (EcmaScriptBaseFunctionObject)arg.Get("toString");
                if (func != null)
                {
                    var result = func.CallEx(ec, arg, new char[0]);
                    if (IsPrimitive(result)) return result;
                }
            }

            ec.Global.RaiseNativeError(NativeErrorType.TypeError, "toString/valueOf does not return a value primitive value");
            return null;
        }


        public static uint GetObjAsCardinal(object arg, ExecutionContext ec)
        {
            if (arg is EcmaScriptObject)
                arg = GetObjectAsPrimitive(ec, (EcmaScriptObject)arg, PrimitiveType.Number);

            if (arg == null) return 0;

            uint result = 0; bool parse;
            switch (Type.GetTypeCode(arg.GetType()))
            {
                case TypeCode.Boolean: result = (uint)((bool)arg ? 1 : 0); break;
                case TypeCode.Byte: result = (byte)arg; break;
                case TypeCode.Char: result = (uint)(int)((char)arg); break;
                case TypeCode.Decimal: result = (uint)(int)((decimal)arg); break;
                case TypeCode.Double:
                    var lVal = (double)arg;
                    if (Double.IsNaN(lVal) || (Double.IsInfinity(lVal)))
                        return 0;

                    result = ((uint)(Math.Sign(lVal) * Math.Floor(Math.Abs(lVal))));
                    break;
                case TypeCode.Int16: result = (uint)(Int16)arg; break;
                case TypeCode.Int32: result = (uint)(Int32)arg; break;
                case TypeCode.Int64: result = (uint)(Int64)arg; break;
                case TypeCode.SByte: result = (uint)(SByte)arg; break;
                case TypeCode.Single: result = (uint)(Int32)((Single)arg); break;
                case TypeCode.String:
                    arg = ((String)arg)?.Trim();
                    if (((string)arg).StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                        parse = UInt32.TryParse(((string)arg).Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier, System.Globalization.NumberFormatInfo.InvariantInfo, out result);
                    else
                        parse = UInt32.TryParse((string)arg, out result);

                    if (!parse)
                    {
                        var lWork = ParseDouble((string)arg);
                        if (Double.IsNaN(lWork))
                            result = 0;
                        else
                            result = (uint)(Math.Sign(lWork) * Math.Floor(Math.Abs(lWork)));
                    }
                    break;
                case TypeCode.UInt16: result = (uint)(UInt16)arg; break;
                case TypeCode.UInt32: result = (uint)(UInt32)arg; break;
                case TypeCode.UInt64: result = (uint)(UInt64)arg; break;
            } // case
            return result;
        }

        public static bool IsPrimitive(object arg)
        {
            if ((arg == null) || (arg == Undefined.Instance))
                return true;

            switch (Type.GetTypeCode(arg.GetType()))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.String:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64: return true;
            } // case
            return false;
        }

        public static double ParseDouble(string s, bool allowHex = true)
        {
            s = s?.Trim();//my add
            if (String.IsNullOrEmpty(s))
                return 0;

            var lNegative = false;
            if (s.StartsWith("+"))
                s = s.Substring(1);
            else
                if (s.StartsWith("-"))
            {
                s = s.Substring(1);
                lNegative = true;
            }

            if (s.StartsWith("Infinity", StringComparison.InvariantCulture))
            {
                if (lNegative)
                    return Double.NegativeInfinity;
                else
                    return Double.PositiveInfinity;
            }

            if (allowHex && s.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
            {
                Int64 v;
                if (Int64.TryParse(s.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier, System.Globalization.NumberFormatInfo.InvariantInfo, out v))
                    return v;
            }

            var lcleaned = false;
            var lp = s.Length;
            s = s.Trim();

            if (s.Length != lp)
            {
                lcleaned = true;
                lp = s.Length;
            }

            if (!allowHex)
                for (var j = s.Length - 1; j <= 0; j--)
                {
                    var c = s[j];
                    if (!(('0' <= c && c <= '9') || c == '.' || c == 'e' || c == 'E' || c == '+' || c == '-'))
                    {
                        lp = j;
                    }
                    else
                    {
                        if (s[j] == '+' && s[j] == '-' && (j > 0) && !(s[j - 1] == 'e' || s[j - 1] == 'E'))
                        {
                            lp = j;
                        }
                        else
                        {
                            if ((s[j] == 'e' || s[j] == 'E') && s.IndexOfAny(new char[] { 'e', 'E' }) != j)
                            {
                                lp = j;
                            }
                        }
                    }
                }

            if ((s.IndexOf('.') != s.LastIndexOf('.')) && (s.LastIndexOf('.') < lp))
                lp = s.LastIndexOf(".");

            if (lp != s.Length)
            {
                s = s.Substring(0, lp);
                lcleaned = true;
            }

            var lExp = s.IndexOfAny(new char[] { 'e', 'E' });
            if (lExp != -1)
            {
                var lTmp = s.Substring(lExp + 1);
                s = s.Substring(0, lExp);
                if ((lTmp == "") || (lTmp == "+") || (lTmp == "-"))
                    lExp = 0;
                else
                    if (!Int32.TryParse(lTmp, out lExp))
                    return Double.NaN;
            }
            else
                lExp = 0;

            if (s == "")
            {
                if (lcleaned || !allowHex) return Double.NaN;
                if (lNegative)
                    return -0;
                else
                    return 0;
            }

            double result;
            if (!Double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out result))
                return Double.NaN;

            double lIncVal;
            if (lExp < 0)
            {
                lIncVal = 0.1;
                lExp = -lExp;
            }
            else
                lIncVal = 10.0;

            while (lExp > 0)
            {
                result = result * lIncVal;
                --lExp;
            }

            if (lNegative)
                result = -result;

            return result;
        }

        public static EcmaScriptObject ToObject(ExecutionContext ec, object o)
        {
            var result = o as EcmaScriptObject;
            if (result == null)
            {
                if (o is Boolean)
                    return ec.Global.BooleanCtor(ec, null, new[] { o }) as EcmaScriptObject;
                if (o is String)
                    return ec.Global.StringCtor(ec, null, new[] { o }) as EcmaScriptObject;
                if ((o is Int32) || (o is Double))
                    return ec.Global.NumberCtor(ec, null, new[] { o }) as EcmaScriptObject;

                ec.Global.RaiseNativeError(NativeErrorType.TypeError, "Object expected");
            }
            return result;
        }


        public static string UrlEncodeComponent(String s)
        {
            s = s?.Trim();
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            var bytes = fEncoding.GetBytes(s);
            var res = new StringBuilder(); byte c;
            for (var i = 0; i < bytes.Length; i++)
            {
                c = bytes[i];
                if (InSets3(c,
                    (byte)'A', (byte)'Z',//set 1
                    (byte)'a', (byte)'z',//set 2
                    (byte)'0', (byte)'9',//set 3
                    (byte)'!', (byte)'~', (byte)'*', (byte)'(', (byte)')',
                    (byte)'.', (byte)'_', (byte)'-', (byte)'\''))
                {
                    res.Append((char)c);
                }
                else
                {
                    res.Append('%');
                    res.Append(((c >> 4) & 15).ToString("X"));
                    res.Append(((c) & 15).ToString("X"));
                }
            }
            return res.ToString();
        }

        public static bool InSet<T>(T value, params T[] array)
        {
            return Array.IndexOf<T>(array, value) > -1;
        }

        public static bool InSets1<T>(T value, T a0, T a1, params T[] values)
            where T : IComparable<T>
        {
            //Less than zero    => This instance precedes other in the sort order.
            //Zero              => This instance occurs in the same position in the sort order as other.
            //Greater than zero => This instance follows other in the sort order.
            if (a0.CompareTo(value) <= 0 && value.CompareTo(a1) <= 0)
            {
                return true;
            }
            else
            {
                return Array.IndexOf<T>(values, value) > -1;
            }
        }

        public static bool InSets2<T>(T value, T a0, T a1, T b0, T b1, params T[] values)
            where T : IComparable<T>
        {
            if ((a0.CompareTo(value) <= 0 && value.CompareTo(a1) <= 0))
            {
                return true;
            }
            else
            {
                if ((b0.CompareTo(value) <= 0 && value.CompareTo(b1) <= 0))
                {
                    return true;
                }
                else
                {
                    return Array.IndexOf<T>(values, value) > -1;
                }
            }
        }

        public static bool InSets3<T>(T value, T a0, T a1, T b0, T b1, T c0, T c1, params T[] values)
            where T : IComparable<T>
        {
            if ((a0.CompareTo(value) <= 0 && value.CompareTo(a1) <= 0))
            {
                return true;
            }
            else
            {
                if ((b0.CompareTo(value) <= 0 && value.CompareTo(b1) <= 0))
                {
                    return true;
                }
                else
                {
                    if ((c0.CompareTo(value) <= 0 && value.CompareTo(c1) <= 0))
                    {
                        return true;
                    }
                    else
                    {
                        return Array.IndexOf<T>(values, value) > -1;
                    }
                }
            }
        }

        public static string UrlDecode(string s, bool aComponent)
        {
            s = s?.Trim();
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            byte b; char c0 = fChar0;
            var ms = new StringBuilder();
            var i = 0; byte[] Octets;

            while (i < s.Length)
            {
                //if not aComponent and (s[i] = '+') then begin
                //  ms.Append(#32);
                //  inc(i);
                //end else
                if (s[i] == '%')
                {
                    if (!(i + 2 < s.Length))
                        return null;

                    if ((s[i + 1] != c0) && (s[i + 2] != c0) && Byte.TryParse(String.Concat(s[i + 1], s[i + 2]), System.Globalization.NumberStyles.AllowHexSpecifier, System.Globalization.NumberFormatInfo.InvariantInfo, out b))
                    {
                        if (!aComponent && InSet(b, 0x3b, 0x2f, 0x3f, 0x3a, 0x40, 0x26, 0x3d, 0x2b, 0x24, 0x2c, 0x23))
                        {
                            ms.Append('%');
                            ms.Append(s[i + 1]);
                            ms.Append(s[i + 2]);
                            i += 3;
                        }
                        else
                        {
                            i += 3;
                            if (0 == (b & 0x80))
                            {
                                ms.Append((char)b);
                            }
                            else
                            {
                                // 4.d.vii.1
                                var n = 0;
                                if (0 == (b & 0x40))
                                    return null;
                                else
                                    if (0 == (b & 0x20))
                                    n = 2;
                                else
                                    if (0 == (b & 0x10))
                                    n = 3;
                                else
                                    if (0 == (b & 0x8))
                                    n = 4;
                                else
                                    return null;

                                if (i + (3 * (n - 1)) > s.Length)
                                    return null;

                                Octets = new Byte[n];
                                Octets[0] = b;
                                for (var j = 1; j < n; j++)
                                {
                                    if (s[i] != '%') return null;

                                    if ((!InSets3(s[i + 1], '0', '9', 'A', 'F', 'a', 'f') ||
                                        (!InSets3(s[i + 2], '0', '9', 'A', 'F', 'a', 'f'))))
                                        return null;

                                    if (!Byte.TryParse(String.Concat(s[i + 1], s[i + 2]), System.Globalization.NumberStyles.AllowHexSpecifier, System.Globalization.NumberFormatInfo.InvariantInfo, out b))
                                        return null;

                                    if (0x80 != (b & 0xc0))
                                        return null;

                                    Octets[j] = b;
                                    i += 3;
                                }

                                int w = 0;
                                switch (n)
                                {
                                    case 2:
                                        w = (Int32)((Octets[0] & 0x1F) << 6) | (Octets[1] & 0x3F);
                                        break;
                                    case 3:
                                        w = (Int32)((Octets[0] & 0x0F) << 12) | ((Octets[1] & 0x3F) << 6) | (Octets[2] & 0x3F);
                                        break;
                                    case 4:
                                        w = (Int32)((Octets[0] & 0x07) << 18) | ((Octets[1] & 0x3F) << 12) | ((Octets[2] & 0x3F) << 6) | (Octets[3] & 0x3F);
                                        break;
                                } // case

                                if ((w == 0) || (w < 0x80) || ((w < 0x800) && (n != 2)) ||
                                    ((w >= 0x800) && (w < 0x10000) && (n != 3)) ||
                                    ((w >= 0x10000) && (w < 0x110000) && (n != 4)))
                                    return null;

                                if (0xD800 <= w && w <= 0xDFFF) return null;

                                if (w <= 0xFFFF)
                                {
                                    ms.Append((char)w);
                                }
                                else
                                {
                                    w = w - 0x10000; // reencode to utf16
                                    ms.Append((char)(0xD800 + (w >> 10)));
                                    ms.Append((char)(0xDC00 + (w & 0x3ff)));
                                }

                                /*
                                  From RFC 3629
                                  Char.number range  |        UTF-8 octet sequence
                                     (hexadecimal)    |              (binary)
                                   --------------------+---------------------------------------------
                                   0000 0000-0000 007F | 0xxxxxxx
                                   0000 0080-0000 07FF | 110xxxxx 10xxxxxx
                                   0000 0800-0000 FFFF | 1110xxxx 10xxxxxx 10xxxxxx
                                   0001 0000-0010 FFFF | 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx


                                   Implementations of the decoding algorithm above MUST protect against
                                   decoding invalid sequences.For instance, a naive implementation may
                                 decode the overlong UTF-8 sequence C0 80 into the character U+0000,
                                 or the surrogate pair ED A1 8C ED BE B4 into U+233B4.Decoding
                                 invalid sequences may have security consequences or cause other
                                   problems.See Security Considerations(Section 10) below.
                             */
                            }
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    ms.Append(s[i]);
                    ++i;
                }
            }
            return ms.ToString();
        }

        public static string UrlEncode(string s)
        {
            s = s?.Trim();
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            var bytes = fEncoding.GetBytes(s);
            var res = new StringBuilder();
            for (int i = 0, l = bytes.Length; i < l; i++)
            {
                if (InSets3(bytes[i],
                    (byte)'A', (byte)'Z',//set 1
                    (byte)'a', (byte)'z',//set 2
                    (byte)'0', (byte)'9',//set 3
                    (byte)';', (byte)'/', (byte)'?', (byte)':', (byte)'@', (byte)'&', (byte)'=', (byte)'+', (byte)'$', (byte)',',
                    (byte)'-', (byte)'_', (byte)'.', (byte)'!', (byte)'~', (byte)'*', (byte)'\'', (byte)'(', (byte)')',
                    (byte)'#',
                    (byte)'.', (byte)'_', (byte)'-', (byte)'~'))
                {
                    res.Append((char)bytes[i]);
                }
                else
                {
                    res.Append('%');
                    res.Append(((bytes[i] >> 4) & 15).ToString("X"));
                    res.Append(((bytes[i]) & 15).ToString("X"));
                }
            }
            return res.ToString();
        }

        public static bool IsCallable(object o)
        {
            var result = (o is MulticastDelegate);
            if (result && (o is EcmaScriptObject))
                result = result && (o is EcmaScriptBaseFunctionObject);
            return result;
        }

        public static object GetArg(object[] arg, int index = 0)
        {
            if ((null == arg) || (index < 0) || (index >= arg.Length))
            {
                return Undefined.Instance;
            }
            else
            {
                return arg[index];
            }
        }

        public static string GetObjAsString(object arg, ExecutionContext ec)
        {
            var lOrg = arg;
            if (arg is EcmaScriptObject)
                arg = GetObjectAsPrimitive(ec, (EcmaScriptObject)arg, PrimitiveType.String);

            if (arg == Undefined.Instance)
                return "undefined";

            if (arg == null)
                return "null";

            string result = null;
            switch (Type.GetTypeCode(arg.GetType()))
            {
                case TypeCode.Boolean: result = (bool)arg ? "true" : "false"; break;
                case TypeCode.Byte: result = ((byte)arg).ToString(); break;
                case TypeCode.Char: result = ((char)arg).ToString(); break;
                case TypeCode.Decimal: result = ((decimal)arg).ToString(System.Globalization.NumberFormatInfo.InvariantInfo); break;
                case TypeCode.Double: result = DoubleToString((double)arg); break;
                case TypeCode.Int16: result = ((Int16)arg).ToString(); break;
                case TypeCode.Int32: result = ((Int32)arg).ToString(); break;
                case TypeCode.Int64: result = ((Int64)arg).ToString(); break;
                case TypeCode.SByte: result = ((SByte)arg).ToString(); break;
                case TypeCode.Single: result = ((Single)arg).ToString(System.Globalization.NumberFormatInfo.InvariantInfo); break;
                case TypeCode.String: result = (String)arg; break;
                case TypeCode.UInt16: result = ((UInt16)arg).ToString(); break;
                case TypeCode.UInt32: result = ((UInt32)arg).ToString(); break;
                case TypeCode.UInt64: result = ((UInt64)arg).ToString(); break;
            }
            return result;
        }

        public static bool GetObjAsBoolean(object arg, ExecutionContext ec)
        {
            //if (arg is EcmaScriptObject)and (EcmaScriptObject(arg).Class = 'Boolean') then arg := GetObjectAsPrimitive(ec, EcmaScriptObject(arg), PrimitiveType.Number);
            if (arg is EcmaScriptObject)
                return true;

            if ((arg == null) || (arg == Undefined.Instance))
                return false;

            bool result = false;
            switch (Type.GetTypeCode(arg.GetType()))
            {
                case TypeCode.Boolean: result = (Boolean)arg; break;
                case TypeCode.Byte: result = (Byte)arg != 0; break;
                case TypeCode.Char: result = (Char)arg != fChar0; break;
                case TypeCode.Decimal: result = (Decimal)arg != 0; break;
                case TypeCode.Double: result = !Double.IsNaN((double)arg) ? (Double)arg != 0 : false; break;
                case TypeCode.Int16: result = (Int16)arg != 0; break;
                case TypeCode.Int32: result = (Int32)arg != 0; break;
                case TypeCode.Int64: result = (Int64)arg != 0; break;
                case TypeCode.SByte: result = (SByte)arg != 0; break;
                case TypeCode.Single: result = (Single)arg != 0; break;
                case TypeCode.String: result = (String)arg != ""; break;
                case TypeCode.UInt16: result = (UInt16)arg != 0; break;
                case TypeCode.UInt32: result = (UInt32)arg != 0; break;
                case TypeCode.UInt64: result = (UInt64)arg != 0; break;
            } // case

            return result;
        }

        public static double GetObjAsDouble(object arg, ExecutionContext ec)
        {
            if (arg is EcmaScriptObject)
                arg = GetObjectAsPrimitive(ec, (EcmaScriptObject)arg, PrimitiveType.Number);

            if (arg == null) return 0;

            if (arg == Undefined.Instance)
                return Double.NaN;

            double result = 0;
            switch (Type.GetTypeCode(arg.GetType()))
            {
                case TypeCode.Boolean: result = (Boolean)arg ? 1 : 0; break;
                case TypeCode.Byte: result = (Byte)arg; break;
                case TypeCode.Char: result = (int)((Char)arg); break;
                case TypeCode.Decimal: result = (Double)((Decimal)arg); break;
                case TypeCode.Double: result = (Double)arg; break;
                case TypeCode.Int16: result = (Int16)arg; break;
                case TypeCode.Int32: result = (Int32)arg; break;
                case TypeCode.Int64: result = (Int64)arg; break;
                case TypeCode.SByte: result = (SByte)arg; break;
                case TypeCode.Single: result = (Single)arg; break;
                case TypeCode.String: result = ParseDouble(((String)arg).Trim()); break;
                case TypeCode.UInt16: result = (UInt16)arg; break;
                case TypeCode.UInt32: result = (UInt32)arg; break;
                case TypeCode.UInt64: result = (UInt64)arg; break;
            }
            return result;
        }

        public static long GetObjAsInt64(object arg, ExecutionContext ec)
        {
            if (arg is EcmaScriptObject)
                arg = GetObjectAsPrimitive(ec, (EcmaScriptObject)arg, PrimitiveType.Number);

            if (arg == null) return 0;

            long result = 0L;
            switch (Type.GetTypeCode(arg.GetType()))
            {
                case TypeCode.Boolean: result = (Boolean)arg ? 1 : 0; break;
                case TypeCode.Byte: result = (Byte)arg; break;
                case TypeCode.Char: result = (Int32)((char)arg); break;
                case TypeCode.Decimal: result = (Int64)((decimal)arg); break;
                case TypeCode.Double: result = (Int64)((Double)arg); break;
                case TypeCode.Int16: result = (Int16)arg; break;
                case TypeCode.Int32: result = (Int32)arg; break;
                case TypeCode.Int64: result = (Int64)arg; break;
                case TypeCode.SByte: result = (SByte)arg; break;
                case TypeCode.Single: result = (Int64)((Single)arg); break;
                case TypeCode.String:
                    var s = ((String)arg)?.Trim(); bool parse;
                    if (s.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                        parse = Int64.TryParse(s.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier, System.Globalization.NumberFormatInfo.InvariantInfo, out result);
                    else
                        parse = Int64.TryParse(s, out result);

                    if (!parse)
                        result = 0;

                    break;
                case TypeCode.UInt16: result = (UInt16)arg; break;
                case TypeCode.UInt32: result = (UInt32)arg; break;
                case TypeCode.UInt64: result = (Int64)((UInt64)arg); break;
            } // case
            return result;
        }

        public static int GetObjAsInteger(object arg, ExecutionContext ec, bool aTreatInfinity = false)
        {
            if (arg is EcmaScriptObject)
                arg = GetObjectAsPrimitive(ec, (EcmaScriptObject)arg, PrimitiveType.Number);

            if (arg == null) return 0;

            int result = 0;
            switch (Type.GetTypeCode(arg.GetType()))
            {
                case TypeCode.Boolean: result = (Boolean)arg ? 1 : 0; break;
                case TypeCode.Byte: result = (Byte)arg; break;
                case TypeCode.Char: result = (Int32)((Char)arg); break;
                case TypeCode.Decimal: result = (Int32)((Decimal)arg); break;
                case TypeCode.Double:
                    var lVal = (Double)arg;
                    if (aTreatInfinity)
                    {
                        if (Double.IsPositiveInfinity(lVal)) return Int32.MaxValue;
                        else
                            if (Double.IsNegativeInfinity(lVal)) return Int32.MinValue;
                    }
                    else
                    {
                        if (Double.IsNaN(lVal) || (Double.IsInfinity(lVal)))
                            return 0;
                    }
                    result = (Int32)((UInt32)(Math.Sign(lVal) * Math.Floor(Math.Abs(lVal))));
                    break;
                case TypeCode.Int16: result = (Int16)arg; break;
                case TypeCode.Int32: result = (Int32)arg; break;
                case TypeCode.Int64: result = (Int32)((Int64)arg); break;
                case TypeCode.SByte: result = (SByte)arg; break;
                case TypeCode.Single: result = (Int32)((Single)arg); break;
                case TypeCode.String:
                    var s = ((String)arg)?.Trim(); bool parse;
                    if (s.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                        parse = Int32.TryParse(s.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier, System.Globalization.NumberFormatInfo.InvariantInfo, out result);
                    else
                        parse = Int32.TryParse(s, out result);

                    if (!parse)
                    {
                        var lWork = ParseDouble(s);
                        if (Double.IsNaN(lWork))
                            result = 0;
                        else
                            result = (Int32)((UInt32)(Math.Sign(lWork) * Math.Floor(Math.Abs(lWork))));
                    }
                    break;
                case TypeCode.UInt16: result = (UInt16)arg; break;
                case TypeCode.UInt32: result = (Int32)(UInt32)arg; break;
                case TypeCode.UInt64: result = (Int32)(UInt64)arg; break;
            }
            return result;
        }

        public static string GetArgAsString(object[] arg, int index, ExecutionContext ec)
        {
            var lValue = GetArg(arg, index);

            return GetObjAsString(lValue, ec);
        }

        public static EcmaScriptObject GetObjAsEcmaScriptObject(object arg, ExecutionContext ec)
        {
            return (EcmaScriptObject)arg;
        }

        public static long GetArgAsCardinal(object[] arg, int index, ExecutionContext ec)
        {
            var lValue = GetArg(arg, index);
            if ((lValue == null) || (lValue == Undefined.Instance))
                return 0;
            else
                return GetObjAsCardinal(lValue, ec);
        }

        public static long GetArgAsInt64(object[] arg, int index, ExecutionContext ec)
        {
            var lValue = GetArg(arg, index);
            if ((lValue == null) || (lValue == Undefined.Instance))
                return 0;
            else
                return GetObjAsInt64(lValue, ec);
        }

        public static double GetArgAsDouble(object[] arg, int index, ExecutionContext ec)
        {
            var lValue = GetArg(arg, index);
            return GetObjAsDouble(lValue, ec);
        }

        public static bool GetArgAsBoolean(object[] arg, int index, ExecutionContext ec)
        {
            var lValue = GetArg(arg, index);
            if ((lValue == null) || (lValue == Undefined.Instance))
                return false;
            else
                return GetObjAsBoolean(lValue, ec);
        }

        public static EcmaScriptObject GetArgAsEcmaScriptObject(object[] arg, int index, ExecutionContext ec)
        {
            var lValue = GetArg(arg, index);
            return lValue as EcmaScriptObject;
        }

        public static int GetArgAsInteger(object[] arg, int index, ExecutionContext ec, bool aTreatInfinity = false)
        {
            var lValue = GetArg(arg, index);
            if ((lValue == null) || (lValue == Undefined.Instance))
                return 0;
            else
                return GetObjAsInteger(lValue, ec, aTreatInfinity);
        }

        public static MethodInfo Method_GetObjAsBoolean = typeof(Utilities).GetMethod("GetObjAsBoolean"); 
        public static MethodInfo Method_GetObjAsString = typeof(Utilities).GetMethod("GetObjAsString");


        public static long DivRem(long a, long b, out long result)
        {
#if SILVERLIGHT //|| DNX
            long c = (long)Math.Floor(a / b);
            result = a - (b * c);
            return c;
#else
            return Math.DivRem(a, b, out result);
#endif
        }

        public static int DivRem(int a, int b, out int result)
        {
#if SILVERLIGHT //|| DNX
            int c = (int)Math.Floor(a / b);
            result = a - (b * c);
            return c;
#else
            return Math.DivRem(a, b, out result);
#endif
        }

        public static ConstructorInfo GetConstructorInfo(this Type type, Type[] parameters)
        {
//#if DNX
//            return System.Reflection.TypeExtensions.GetConstructor(type, parameters);
//#else
            return type.GetConstructor(parameters);
//#endif
        }
    }
}