using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public abstract class EnvironmentRecord
    {
        readonly EnvironmentRecord fPrevious;

        public EnvironmentRecord Previous { get { return fPrevious; } }
        public abstract GlobalObject Global { get; internal set; }
        public abstract bool IsDeclarative { get; }


        public EnvironmentRecord(EnvironmentRecord aPrev)
        {
            fPrevious = aPrev;
        }

        public abstract bool HasBinding(string aName);
        public abstract void CreateMutableBinding(string aName, bool aDeleteAfter); 
        public abstract void SetMutableBinding(string aName, object aValue, bool aStrict);
        public abstract object GetBindingValue(string aName, bool aStrict);
        public abstract bool DeleteBinding(string aName);
        public abstract object ImplicitThisValue();
        public abstract IEnumerable<string> Names();


        public void CreateMutableBindingNoFail(string aName, bool aDeleteAfter)
        {
            if (!HasBinding(aName))
                CreateMutableBinding(aName, aDeleteAfter);
        }

        public static void CreateAndSetMutableBindingNoFail(object aVal, string aName, EnvironmentRecord ex, bool aImmutable, bool aDeleteAfter)
        {
            if (aImmutable)
            {
                var lDec = ex as DeclarativeEnvironmentRecord;
                if (lDec != null)
                {
                    lDec.CreateImmutableBinding(aName);
                    lDec.InitializeImmutableBinding(aName, aVal);
                    return;
                }
            }
            ex.CreateMutableBinding(aName, aDeleteAfter);
            ex.SetMutableBinding(aName, aVal, false);
        }

        public static Reference GetIdentifier(EnvironmentRecord aLex, string aName, bool aStrict)
        {
            while (aLex != null)
            {
                if (aLex.HasBinding(aName))
                {
                    return new Reference(aLex, aName, aStrict ? 1 : 0);
                }
                aLex = aLex.Previous;
            }

            if (aLex == null)
                return new Reference(Undefined.Instance, aName, aStrict ? 1 : 0);

            return null;
        }

        public static readonly MethodInfo Method_GetIdentifier = typeof(EnvironmentRecord).GetMethod("GetIdentifier");
        public static readonly MethodInfo Method_CreateAndSetMutableBindingNoFail = typeof(EnvironmentRecord).GetMethod("CreateAndSetMutableBindingNoFail");
        public static readonly MethodInfo Method_CreateMutableBindingNoFail = typeof(EnvironmentRecord).GetMethod("CreateMutableBindingNoFail");
        public static readonly MethodInfo Method_SetMutableBinding = typeof(EnvironmentRecord).GetMethod("SetMutableBinding");
        public static readonly MethodInfo Method_HasBinding = typeof(EnvironmentRecord).GetMethod("HasBinding");

    }
}