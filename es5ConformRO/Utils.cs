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
        public static void WriteResult(this XmlWriter w, string script, TimeSpan time)
        {
            WriteResult(w, script, null, time);
        }

        public static void WriteResult(this XmlWriter w, string script, string result, TimeSpan time)
        {
            w.WriteStartElement("test");
            w.WriteAttributeString("script", script);
            w.WriteAttributeString("time", time.ToString());
            w.WriteAttributeString("result", result == null ? "ok" : "fail");
            if (null != result) w.WriteString(result);
            w.WriteEndElement();
        }
    }
}
