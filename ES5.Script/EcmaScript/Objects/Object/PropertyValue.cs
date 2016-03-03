using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class PropertyValue
    {
        public PropertyValue(PropertyAttributes aAttributes, object aValue)
        {
            Value = aValue;
            Attributes = aAttributes | PropertyAttributes.HasValue;
        }

        public PropertyValue(PropertyAttributes aAttributes, EcmaScriptBaseFunctionObject aGet, EcmaScriptBaseFunctionObject aSet)
        {
            Attributes = aAttributes;
            Get = aGet;
            Set = aSet;
        }

        public EcmaScriptObject Owner { get; set; }
        public ulong Ticks { get; set; }
        public bool Deleted { get; set; }

        public PropertyAttributes Attributes { get; set; }
        public EcmaScriptBaseFunctionObject Get { get; set; }
        public EcmaScriptBaseFunctionObject Set { get; set; }
        public object Value { get; set; }

        public static PropertyValue Enumerable(object aValue)
        {
            return new PropertyValue(PropertyAttributes.Enumerable, aValue);
        }

        public static PropertyValue NotEnum(object aValue)
        {
            return new PropertyValue(PropertyAttributes.All & ~PropertyAttributes.Enumerable, aValue);
        }

        public static PropertyValue NotAllFlags(object aValue)
        {
            return new PropertyValue(PropertyAttributes.None, aValue);
        }

        public static PropertyValue NotDeleteAndReadOnly(object aValue)
        {
            return new PropertyValue(PropertyAttributes.All & ~PropertyAttributes.Writable & ~PropertyAttributes.Configurable, aValue);
        }

        public static PropertyValue Create(object aValue, PropertyAttributes properties = PropertyAttributes.None)
        {
            return new PropertyValue(properties, aValue);
        }
    }
}
