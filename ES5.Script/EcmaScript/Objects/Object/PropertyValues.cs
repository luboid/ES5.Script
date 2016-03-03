using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ES5.Script.EcmaScript.Objects
{
    internal class PropertyValues : Dictionary<string, PropertyValue>
    {
        EcmaScriptObject fOwner;
        public PropertyValues(EcmaScriptObject aOwner)
        {
            fOwner = aOwner;
        }

        new public void Add(string key, PropertyValue value)
        {
            value.Owner = fOwner;
            base.Add(key, value);
        }

        new public PropertyValue this[string name]
        {
            get
            {
                return base[name];
            }
            set
            {
                value.Owner = fOwner;
                base[name] = value;
            }
        }
    }
}
