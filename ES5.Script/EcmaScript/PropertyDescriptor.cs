using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ES5.Script.EcmaScript
{
    public class PropertyDescriptor
    {
        public bool? Enumerable { get; set; }
        public bool? Configurable { get; set; }
        public bool? Writable { get; set; }
        public bool? HasValue { get; set; }
        public object Value { get; set; }
    }
}
