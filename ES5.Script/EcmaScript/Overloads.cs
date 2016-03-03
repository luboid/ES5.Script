using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public class Overloads
    {
        public Overloads(object aInstance, List<MethodBase> aItems)
        {
            Instance = aInstance;
            Items = aItems;
        }

        public object Instance { get; set; }
        public List<MethodBase> Items { get; set; }
    }
}
