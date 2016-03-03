using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ES5.Script.EcmaScript.Internal;

namespace ES5.Script.EcmaScript.Objects
{
    public partial class GlobalObject : EcmaScriptObject
    {
        static readonly long Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        public static long DateTimeToUnix(DateTime date)
        {
            return (date.Ticks - Epoch) / 10000;
        }


        public static DateTime UnixToDateTime(long date, DateTimeKind kind = DateTimeKind.Unspecified)
        {
            date = (date * TimeSpan.TicksPerMillisecond) + Epoch;
            if (date > DateTime.MaxValue.Ticks)
                date = DateTime.MaxValue.Ticks;
            else if (date < DateTime.MinValue.Ticks)
                date = DateTime.MinValue.Ticks;

            return new DateTime(date, kind);
        }

        public EcmaScriptObject CreateDate()
        {
            var result = Get("Date") as EcmaScriptObject;
            if (result != null) return result;

            result = new EcmaScriptDateObject(this, "Date", DateCall, 1) { Class = "Date" };
            Values.Add("Date", PropertyValue.NotEnum(result));
            result.Values.Add("now", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "now", DateNow, 0)));
            result.Values.Add("parse", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "parse", DateParse, 1)));
            result.Values.Add("UTC", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "UTC", DateUTC, 7)));

            DatePrototype = new EcmaScriptObject(this) { Class = "Date", Value = Double.NaN };
            DatePrototype.Values.Add("constructor", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "Date", DateCtor, 7) { Class = "Date" }));
            DatePrototype.Prototype = ObjectPrototype;
            result.Values["prototype"] = PropertyValue.NotAllFlags(DatePrototype);

            DatePrototype.Values.Add("toString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toString", DateToString, 0)));
            DatePrototype.Values.Add("toISOString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toISOString", DateToISOString, 0)));
            DatePrototype.Values.Add("toJSON", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toJSON", DateToJSON, 1)));
            DatePrototype.Values.Add("toUTCString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toUTCString", DateToUTCString, 0)));
            DatePrototype.Values.Add("toGMTString", PropertyValue.NotEnum(DatePrototype.Values["toUTCString"].Value));
            DatePrototype.Values.Add("toDateString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toDateString", DateToDateString, 0)));
            DatePrototype.Values.Add("toTimeString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toTimeString", DateToTimeString, 0)));
            DatePrototype.Values.Add("toLocaleString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toLocaleString", DateToLocaleString, 0)));
            DatePrototype.Values.Add("toLocaleDateString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toLocaleDateString", DateToLocaleDateString, 0)));
            DatePrototype.Values.Add("toLocaleTimeString", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "toLocaleTimeString", DateToLocaleTimeString, 0)));
            DatePrototype.Values.Add("valueOf", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "valueOf", DateValueOf, 0)));
            DatePrototype.Values.Add("getTime", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getTime", DateGetTime, 0)));
            DatePrototype.Values.Add("getFullYear", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getFullYear", DateGetFullYear, 0)));
            DatePrototype.Values.Add("getYear", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getYear", DateGetYear, 0)));
            DatePrototype.Values.Add("getUTCFullYear", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getUTCFullYear", DateGetUTCFullYear, 0)));
            DatePrototype.Values.Add("getMonth", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getMonth", DateGetMonth, 0)));
            DatePrototype.Values.Add("getUTCMonth", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getUTCMonth", DateGetUTCMonth, 0)));
            DatePrototype.Values.Add("getDate", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getDate", DateGetDate, 0)));
            DatePrototype.Values.Add("getUTCDate", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getUTCDate", DateGetUTCDate, 0)));
            DatePrototype.Values.Add("getDay", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getDay", DateGetDay, 0)));
            DatePrototype.Values.Add("getUTCDay", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getUTCDay", DateGetUTCDay, 0)));
            DatePrototype.Values.Add("getHours", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getHours", DateGetHours, 0)));
            DatePrototype.Values.Add("getUTCHours", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getUTCHours", DateGetUTCHours, 0)));
            DatePrototype.Values.Add("getMinutes", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getMinutes", DateGetMinutes, 0)));
            DatePrototype.Values.Add("getUTCMinutes", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getUTCMinutes", DateGetUTCMinutes, 0)));
            DatePrototype.Values.Add("getSeconds", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getSeconds", DateGetSeconds, 0)));
            DatePrototype.Values.Add("getUTCSeconds", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getUTCSeconds", DateGetUTCSeconds, 0)));
            DatePrototype.Values.Add("getMilliseconds", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getMilliseconds", DateGetMilliseconds, 0)));
            DatePrototype.Values.Add("getUTCMilliseconds", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getUTCMilliseconds", DateGetUTCMilliseconds, 0)));
            DatePrototype.Values.Add("getTimezoneOffset", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "getTimezoneOffset", DateGetTimezoneOffset, 0)));
            DatePrototype.Values.Add("setTime", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setTime", DateSetTime, 1)));
            DatePrototype.Values.Add("setMilliseconds", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setMilliseconds", DateSetMilliseconds, 1)));
            DatePrototype.Values.Add("setUTCMilliseconds", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setUTCMilliseconds", DateSetUTCMilliseconds, 1)));
            DatePrototype.Values.Add("setSeconds", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setSeconds", DateSetSeconds, 2)));
            DatePrototype.Values.Add("setUTCSeconds", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setUTCSeconds", DateSetUTCSeconds, 2)));
            DatePrototype.Values.Add("setMinutes", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setMinutes", DateSetMinutes, 3)));
            DatePrototype.Values.Add("setUTCMinutes", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setUTCMinutes", DateSetUTCMinutes, 3)));
            DatePrototype.Values.Add("setHours", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setHours", DateSetHours, 4)));
            DatePrototype.Values.Add("setUTCHours", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setUTCHours", DateSetUTCHours, 4)));
            DatePrototype.Values.Add("setDate", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setDate", DateSetDate, 1)));
            DatePrototype.Values.Add("setUTCDate", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setUTCDate", DateSetUTCDate, 1)));
            DatePrototype.Values.Add("setMonth", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setMonth", DateSetMonth, 2)));
            DatePrototype.Values.Add("setUTCMonth", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setUTCMonth", DateSetUTCMonth, 2)));
            DatePrototype.Values.Add("setYear", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setYear", DateSetFullYear, 1)));
            DatePrototype.Values.Add("setFullYear", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setFullYear", DateSetFullYear, 3)));
            DatePrototype.Values.Add("setUTCFullYear", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "setUTCFullYear", DateSetUTCFullYear, 1)));

            return result;
        }

        public object DateCall(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return DateCtor(null, Utilities.EmptyParams)?.ToString();
        }


        public object DateCtor(ExecutionContext caller, object aSelf, params object[] args)
        {
            var lValue = 0L;
            if (args.Length == 0) {
                lValue = DateTimeToUnix(DateTime.UtcNow);
            }
            else if (args.Length == 1) {
                if (args[0] is EcmaScriptObject)
                    args[0] = ((EcmaScriptObject)args[0]).Value;
                if (args[0] is String) {
                    return DateParse(caller, aSelf, args[0]);
                }
                else {
                    lValue = Utilities.GetArgAsInt64(args, 0, caller);
                }
            }
            else {
                var lYear = Utilities.GetArgAsInteger(args, 0, caller);
                var lMonth = Utilities.GetArgAsInteger(args, 1, caller); // Month is 0-based in JS
                var lDay = Utilities.GetArgAsInteger(args, 2, caller);
                var lHour = Utilities.GetArgAsInteger(args, 3, caller);
                var lMinute = Utilities.GetArgAsInteger(args, 4, caller);
                var lSec = Utilities.GetArgAsInteger(args, 5, caller);
                var lMSec = Utilities.GetArgAsInteger(args, 6, caller);

                lValue = DateTimeToUnix(new DateTime(lYear, 1, 1, lHour, lMinute, lSec, lMSec)
                    .AddMonths(lMonth)
                    .AddDays(lDay - 1)
                    .ToUniversalTime());
            }

            return new EcmaScriptObject(this, DatePrototype) { Class = "Date", Value = lValue };
        }


        public object DateParse(ExecutionContext caller, object aSelf, params object[] args)
        {
            try
            {
                var lValue = DateTime.Parse(Utilities.GetArgAsString(args, 0, caller), 
                    System.Globalization.DateTimeFormatInfo.InvariantInfo,
                    System.Globalization.DateTimeStyles.AssumeLocal)
                    .ToUniversalTime();

                return DateTimeToUnix(lValue);
            }
            catch
            {
                return Double.NaN;
            }
            //exit new EcmaScriptObject(this, DatePrototype, &Class = "Date", Value = lValue);
        }


        public object DateUTC(ExecutionContext caller, object aSelf, params object[] args)
        {
            var lYear = Utilities.GetArgAsInteger(args, 0, caller);
            var lMonth = Utilities.GetArgAsInteger(args, 1, caller);
            var lDay = Utilities.GetArgAsInteger(args, 2, caller);
            var lHour = Utilities.GetArgAsInteger(args, 3, caller);
            var lMinute = Utilities.GetArgAsInteger(args, 4, caller);
            var lSec = Utilities.GetArgAsInteger(args, 5, caller);
            var lMSec = Utilities.GetArgAsInteger(args, 6, caller);

            var lValue = new DateTime(lYear, 1, 1, lHour, lMinute, lSec, lMSec)
                .AddMonths(lMonth)
                .AddDays(lDay - 1);

            return new EcmaScriptObject(this, DatePrototype) { Class = "Date", Value = DateTimeToUnix(lValue) };
        }


        public object DateToString(ExecutionContext caller, object aSelf, params object[] args)
        {
            var lSelf = (aSelf as EcmaScriptObject)?.Value ?? aSelf;

            return UnixToDateTime(Utilities.GetObjAsInt64(lSelf, caller))
                .ToLocalTime()
                .ToString(System.Globalization.DateTimeFormatInfo.CurrentInfo);
        }


        public object DateToUTCString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller))
                .ToString("R", System.Globalization.DateTimeFormatInfo.InvariantInfo);
        }

        public object DateToDateString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller))
                .ToLocalTime()
                .ToString("d", System.Globalization.DateTimeFormatInfo.InvariantInfo);
        }


        public object DateToTimeString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller))
                .ToLocalTime()
                .ToString("T", System.Globalization.DateTimeFormatInfo.InvariantInfo);
        }

        public object DateToLocaleString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller))
                .ToLocalTime()
                .ToString(System.Globalization.DateTimeFormatInfo.CurrentInfo);
        }

        public object DateToLocaleDateString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller))
                .ToLocalTime()
                .ToString("d", System.Globalization.DateTimeFormatInfo.CurrentInfo);
        }

        public object DateToLocaleTimeString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller))
                .ToLocalTime()
                .ToString("T", System.Globalization.DateTimeFormatInfo.CurrentInfo);
        }


        public object DateValueOf(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = (aSelf as EcmaScriptObject)?.Value ?? aSelf;
            if (!((lValue is Double) || (lValue is Int64)))
                RaiseNativeError(NativeErrorType.TypeError, "Date.valueOf is not generic");

            return Convert.ToInt64(lValue);
        }


        public object DateGetTime(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return Utilities.GetObjAsInt64(aSelf, aCaller);
        }

        public object DateGetFullYear(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller))
                .ToLocalTime()
                .Year;
        }

        public object DateGetUTCFullYear(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller))
                .Year;
        }

        public object DateGetMonth(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller))
                .ToLocalTime()
                .Month - 1;
        }

        public object DateGetUTCMonth(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller))
                .Month - 1;
        }

        public object DateGetDate(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller))
                .ToLocalTime()
                .Day;
        }

        public object DateGetUTCDate(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).Day;
        }

        public object DateGetDay(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return (int)UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).ToLocalTime().DayOfWeek;
        }

        public object DateGetUTCDay(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return (int)UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).DayOfWeek;
        }

        public object DateGetHours(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).ToLocalTime().Hour;
        }

        public object DateGetUTCHours(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).Hour;
        }

        public object DateGetMinutes(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).ToLocalTime().Minute;
        }

        public object DateGetUTCMinutes(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).Minute;
        }

        public object DateGetSeconds(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).ToLocalTime().Second;
        }

        public object DateGetUTCSeconds(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).Second;
        }

        public object DateGetMilliseconds(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).ToLocalTime().Millisecond;
        }

        public object DateGetUTCMilliseconds(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).Millisecond;
        }

        public object DateGetTimezoneOffset(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller));
            return (lValue.ToUniversalTime() - lValue).TotalMinutes;
        }

        public object DateSetTime(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return (aSelf as EcmaScriptObject).Value = Utilities.GetArgAsInt64(args, 0, aCaller);
        }

        public object DateSetMilliseconds(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).ToLocalTime();
            lValue = new DateTime(lValue.Year, lValue.Month, lValue.Day, lValue.Hour, lValue.Minute, lValue.Second, 0)
                .AddMilliseconds(Utilities.GetArgAsInt64(args, 0, aCaller));

            return (aSelf as EcmaScriptObject).Value = DateTimeToUnix(lValue.ToUniversalTime());
        }

        public object DateSetUTCMilliseconds(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller));
            lValue = new DateTime(lValue.Year, lValue.Month, lValue.Day, lValue.Hour, lValue.Minute, lValue.Second, 0)
                .AddMilliseconds(Utilities.GetArgAsInt64(args, 0, aCaller));

            return (aSelf as EcmaScriptObject).Value = DateTimeToUnix(lValue);
        }

        public object DateSetSeconds(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).ToLocalTime();
            var lSeconds = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lMilliseconds = (Int32)(args.Length > 1 ? Utilities.GetArgAsInteger(args, 1, aCaller) : lValue.Millisecond);
            lValue = new DateTime(lValue.Year, lValue.Month, lValue.Day, lValue.Hour, lValue.Minute, 0, 0)
                .AddSeconds(lSeconds)
                .AddMilliseconds(lMilliseconds);

            return (aSelf as EcmaScriptObject).Value = DateTimeToUnix(lValue.ToUniversalTime());
        }

        public object DateSetUTCSeconds(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller));
            var lSeconds = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lMilliseconds = (Int32)(args.Length > 1 ? Utilities.GetArgAsInteger(args, 1, aCaller) : lValue.Millisecond);
            lValue = new DateTime(lValue.Year, lValue.Month, lValue.Day, lValue.Hour, lValue.Minute, 0, 0)
                .AddSeconds(lSeconds)
                .AddMilliseconds(lMilliseconds);

            return (aSelf as EcmaScriptObject).Value = DateTimeToUnix(lValue);
        }

        public object DateSetMinutes(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).ToLocalTime();

            var lMinutes = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lSeconds = (args.Length > 1 ? Utilities.GetArgAsInteger(args, 1, aCaller) : lValue.Second);
            var lMilliseconds = (args.Length > 2 ? Utilities.GetArgAsInteger(args, 2, aCaller) : lValue.Millisecond);
            lValue = new DateTime(lValue.Year, lValue.Month, lValue.Day, lValue.Hour, 0, 0, 0).AddMinutes(lMinutes).AddSeconds(lSeconds).AddMilliseconds(lMilliseconds);

            return (aSelf as EcmaScriptObject).Value = DateTimeToUnix(lValue.ToUniversalTime());
        }

        public object DateSetUTCMinutes(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller));

            var lMinutes = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lSeconds = (args.Length > 1 ? Utilities.GetArgAsInteger(args, 1, aCaller) : lValue.Second);
            var lMilliseconds = (args.Length > 2 ? Utilities.GetArgAsInteger(args, 2, aCaller) : lValue.Millisecond);
            lValue = new DateTime(lValue.Year, lValue.Month, lValue.Day, lValue.Hour, 0, 0, 0).AddMinutes(lMinutes).AddSeconds(lSeconds).AddMilliseconds(lMilliseconds);

            return (aSelf as EcmaScriptObject).Value = DateTimeToUnix(lValue);
        }

        public object DateSetHours(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).ToLocalTime();

            var lHours = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lMinutes = (args.Length > 1 ? Utilities.GetArgAsInteger(args, 1, aCaller) : lValue.Minute);
            var lSeconds = (args.Length > 2 ? Utilities.GetArgAsInteger(args, 2, aCaller) : lValue.Second);
            var lMilliseconds = (args.Length > 3 ? Utilities.GetArgAsInteger(args, 3, aCaller) : lValue.Millisecond);
            lValue = new DateTime(lValue.Year, lValue.Month, lValue.Day, 0, 0, 0, 0)
                .AddHours(lHours)
                .AddMinutes(lMinutes)
                .AddSeconds(lSeconds)
                .AddMilliseconds(lMilliseconds);

            return (aSelf as EcmaScriptObject).Value = DateTimeToUnix(lValue.ToUniversalTime());
        }

        public object DateSetUTCHours(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller));

            var lHours = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lMinutes = (args.Length > 1 ? Utilities.GetArgAsInteger(args, 1, aCaller) : lValue.Minute);
            var lSeconds = (args.Length > 2 ? Utilities.GetArgAsInteger(args, 2, aCaller) : lValue.Second);
            var lMilliseconds = (args.Length > 3 ? Utilities.GetArgAsInteger(args, 3, aCaller) : lValue.Millisecond);
            lValue = new DateTime(lValue.Year, lValue.Month, lValue.Day, 0, 0, 0, 0)
                .AddHours(lHours)
                .AddMinutes(lMinutes)
                .AddSeconds(lSeconds)
                .AddMilliseconds(lMilliseconds);

            return (aSelf as EcmaScriptObject).Value = DateTimeToUnix(lValue);
        }

        public object DateSetDate(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).ToLocalTime();

            lValue = new DateTime(lValue.Year, lValue.Month, 1, lValue.Hour, lValue.Minute, lValue.Second, lValue.Millisecond)
                .AddDays(Utilities.GetArgAsInteger(args, 0, aCaller) - 1);

            return (aSelf as EcmaScriptObject).Value = DateTimeToUnix(lValue.ToUniversalTime());
        }

        public object DateSetUTCDate(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller));

            lValue = new DateTime(lValue.Year, lValue.Month, 1, lValue.Hour, lValue.Minute, lValue.Second, lValue.Millisecond)
                .AddDays(Utilities.GetArgAsInteger(args, 0, aCaller) - 1);

            return (aSelf as EcmaScriptObject).Value = DateTimeToUnix(lValue);
        }

        public object DateSetMonth(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).ToLocalTime();

            var lMonth = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lDay = (args.Length > 1 ? Utilities.GetArgAsInteger(args, 1, aCaller) : lValue.Day);
            lValue = new DateTime(lValue.Year, 1, 1, lValue.Hour, lValue.Minute, lValue.Second, lValue.Millisecond)
                .AddMonths(lMonth).AddDays(lDay - 1);

            return (aSelf as EcmaScriptObject).Value = DateTimeToUnix(lValue.ToUniversalTime());
        }

        public object DateSetUTCMonth(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller));

            var lMonth = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lDay = (args.Length > 1 ? Utilities.GetArgAsInteger(args, 1, aCaller) : lValue.Day);
            lValue = new DateTime(lValue.Year, 1, 1, lValue.Hour, lValue.Minute, lValue.Second, lValue.Millisecond)
                .AddMonths(lMonth).AddDays(lDay - 1);

            return (aSelf as EcmaScriptObject).Value = DateTimeToUnix(lValue);
        }

        public object DateSetFullYear(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).ToLocalTime();

            var lYear = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lMonth = (args.Length > 1 ? Utilities.GetArgAsInteger(args, 1, aCaller) : lValue.Month - 1);
            var lDay = (args.Length > 2 ? Utilities.GetArgAsInteger(args, 2, aCaller) : lValue.Day);
            lValue = new DateTime(lYear, 1, 1, lValue.Hour, lValue.Minute, lValue.Second, lValue.Millisecond)
                .AddMonths(lMonth)
                .AddDays(lDay - 1);

            return (aSelf as EcmaScriptObject).Value = DateTimeToUnix(lValue.ToUniversalTime());
        }

        public object DateSetUTCFullYear(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller));

            var lYear = Utilities.GetArgAsInteger(args, 0, aCaller);
            var lMonth = (args.Length > 1 ? Utilities.GetArgAsInteger(args, 1, aCaller) : lValue.Month - 1);
            var lDay = (args.Length > 2 ? Utilities.GetArgAsInteger(args, 2, aCaller) : lValue.Day);
            lValue = new DateTime(lYear, 1, 1, lValue.Hour, lValue.Minute, lValue.Second, lValue.Millisecond)
                .AddMonths(lMonth)
                .AddDays(lDay - 1);

            return (aSelf as EcmaScriptObject).Value = DateTimeToUnix(lValue);
        }

        public object CreateDateObject(DateTime date)
        {
            return new EcmaScriptObject(this, DatePrototype) { Class = "Date", Value = DateTimeToUnix(date) };
        }


        public object DateNow(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return DateTimeToUnix(DateTime.UtcNow);
        }


        public object DateToISOString(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            return UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller))
                .ToString("s", System.Globalization.DateTimeFormatInfo.InvariantInfo);
        }

        public object DateToJSON(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = (EcmaScriptObject)Utilities.ToObject(aCaller, aSelf);

            if ((lValue.Value is Int32) || (lValue.Value is Int64) || (lValue.Value is Double))
                return (DateToISOString(aCaller, lValue));

            return null;
        }

        public object DateGetYear(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lValue = UnixToDateTime(Utilities.GetObjAsInt64(aSelf, aCaller)).ToLocalTime();
            return lValue.Year % 100;
        }
    }
}
