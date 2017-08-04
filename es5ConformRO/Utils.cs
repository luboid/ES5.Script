using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace es5Conform
{
    static class Utils
    {
        public static void WriteResult(this XmlWriter w, string script, TimeSpan time, bool testResult = true)
        {
            WriteResult(w, script, null, time, testResult);
        }

        public static void WriteResult(this XmlWriter w, string script, string result, TimeSpan time, bool testResult = true)
        {
            w.WriteStartElement("test");
            w.WriteAttributeString("script", script);
            if (testResult)
            {
                w.WriteAttributeString("time", time.ToString());
            }
            w.WriteAttributeString("result", result == null ? "ok" : "fail");
            if (testResult && null != result)
            {
                w.WriteString(result);
            }
            w.WriteEndElement();
        }
    }
}
