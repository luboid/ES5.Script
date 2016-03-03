using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public static class JSON
    {
        public static string QuoteString(string value)
        {
            var sb = new StringBuilder();

            sb.Append('"');
            for (int i = 0, l = value.Length; i < l; i++) {
                switch (value[i]) {
                    case '"': sb.Append('\"'); break;
                    case '\\': sb.Append("\\\\"); break;
                    case (char)10: sb.Append("\\n"); break;
                    case (char)13: sb.Append("\\r"); break;
                    case (char)12: sb.Append("\\f"); break;
                    case (char)9: sb.Append("\\t"); break;
                    case (char)8: sb.Append("\\b"); break;
                    default:
                        if ((0 <= value[i] && value[i] <= 7) || (14 <= value[i] && value[i] <= 31) || value[i] == 11)
                        {
                            sb.Append("\\u");
                            sb.Append(((Int32)value[i]).ToString("x4"));
                        }
                        else
                        {
                            sb.Append(value[i]);
                        }
                        break;
                }
            }

            sb.Append('"');

            return (sb.ToString());
        }


        public static string ToString(object value)
        {
            if (value == null) return ("null");
            if (value is Boolean) return ((bool)value ? "true" : "false");
            if (value is Char) return (JSON.QuoteString(new string((char)value, 1)));
            if (value is String) return (JSON.QuoteString((String)value));
            if (value is Int32) return (((Int32)value).ToString(CultureInfo.InvariantCulture));
            if (value is Int64) return (((Int64)value).ToString(CultureInfo.InvariantCulture));

            if (value is DateTime)
            {
                var d = ((DateTime)value).ToUniversalTime();
                return d.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
                // return (String.Format("\"/Date({0:#})/\"", new TimeSpan(d.Ticks - (new DateTime(1970, 1, 1)).Ticks).TotalMilliseconds));

            }

            if (value is Decimal)
                value = Convert.ToDouble((Decimal)value);

            if (value is Double) {
                var lValue = (Double)value;

                if (Double.IsInfinity(lValue))
                    return ("null");

                return (lValue.ToString(NumberFormatInfo.InvariantInfo));
            }

            return (null);
        }
    }
}
