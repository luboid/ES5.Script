using ES5.Script.EcmaScript.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptArgumentObject : EcmaScriptObject
    {
        ExecutionContext fExecutionScope;
        bool fStrict;
        string[] fNames;
        object[] fArgs;

        public new static readonly ConstructorInfo ConstructorInfo = typeof(EcmaScriptArgumentObject).GetConstructors()[0];
        public EcmaScriptObject Map { get; set; }

        public EcmaScriptArgumentObject(ExecutionContext ex, object[] aArgs, string[] aArgNames, EcmaScriptInternalFunctionObject aCaller, bool aStrict)
            : base(ex.Global, ex.Global.ObjectPrototype)
        {
            Class = "Arguments";
            fExecutionScope = ex;
            DefineOwnProperty("length", 
                new PropertyValue(PropertyAttributes.Configurable | PropertyAttributes.Writable, aArgs.Length));

            if (aStrict) {
                DefineOwnProperty("caller", new PropertyValue(PropertyAttributes.None, ex.Global.Thrower));
                DefineOwnProperty("callee", new PropertyValue(PropertyAttributes.None, ex.Global.Thrower));
            }
            else {
                DefineOwnProperty("callee", new PropertyValue(PropertyAttributes.Writable | PropertyAttributes.Configurable, aCaller));
            }

            fArgs = aArgs;
            fNames = aArgNames;
            fStrict = aStrict;
        }


        public override object Get(string aName, ExecutionContext aExecutionContext= null, int aFlags=0)
        {
            int lIndex = 0;
            if (!fStrict && Int32.TryParse(aName, out lIndex))
            {
                if (lIndex < Math.Min(fNames.Length, fArgs.Length))
                    return fExecutionScope.LexicalScope.GetBindingValue(fNames[lIndex], false);

                if (lIndex < fArgs.Length)
                    return fArgs[lIndex];
            }
            return base.Get(aName, aExecutionContext, aFlags);
        }


        public override PropertyValue GetOwnProperty(string name, bool getPropertyValue)
        {
            var lIndex = 0;
            if (!fStrict && Int32.TryParse(name, out lIndex) && (lIndex < Math.Min(fNames.Length, fArgs.Length)))
                return (new PropertyValue(PropertyAttributes.Writable | PropertyAttributes.Configurable, fExecutionScope.LexicalScope.GetBindingValue(fNames[lIndex], false)));

            return base.GetOwnProperty(name, getPropertyValue);
        }


        public override bool DefineOwnProperty(string aName, PropertyValue aValue, bool aThrow = true)
        {
            var lIndex = 0;
            if (!fStrict && Int32.TryParse(aName, out lIndex))
            {
                if (lIndex < Math.Min(fNames.Length, fArgs.Length))
                {
                    fExecutionScope.LexicalScope.SetMutableBinding(fNames[lIndex], aValue.Value, aThrow);
                    return true;
                }

                if ((lIndex < fArgs.Length) && (PropertyAttributes.HasValue & aValue.Attributes) != 0)
                {
                    fArgs[lIndex] = aValue.Value;
                    return true;
                }

                return true;
            }

            return base.DefineOwnProperty(aName, aValue, aThrow);
        }


        public override bool Delete(string aName, bool aThrow)
        {
            var n = 0;
            if (Int32.TryParse(aName, out n))
            {
                if (n < fArgs.Length)
                {
                    fArgs[n] = Undefined.Instance;
                    return true;
                }
            }
            return base.Delete(aName, aThrow);
        }

    }
}