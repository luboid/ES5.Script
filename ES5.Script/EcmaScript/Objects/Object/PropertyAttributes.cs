using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    [Flags]
    public enum PropertyAttributes
    {
        All = 1 + 2 + 4,
        HasValue = 8,
        Writable = 1,
        Enumerable = 2,
        Configurable = 4,
        None = 0
    }
}
