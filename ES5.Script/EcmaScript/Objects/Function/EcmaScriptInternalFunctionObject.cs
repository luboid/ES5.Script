using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class EcmaScriptInternalFunctionObject : EcmaScriptBaseFunctionObject
    {
        string fOriginalBody;
        InternalFunctionDelegate fDelegate;

        public InternalFunctionDelegate Delegate { get { return fDelegate; } }
        public string OriginalBody { get { return fOriginalBody; } }

        public new static readonly ConstructorInfo ConstructorInfo = typeof(EcmaScriptInternalFunctionObject).GetConstructor(new[]
        {
            typeof(GlobalObject),
            typeof(EnvironmentRecord),
            typeof(String),
            typeof(InternalFunctionDelegate),
            typeof(int),
            typeof(String),
            typeof(Boolean)
        });

        public EcmaScriptInternalFunctionObject(GlobalObject aScope, EnvironmentRecord aScopeVar, string aOriginalName, InternalFunctionDelegate aDelegate, int aLength, string aOriginalBody, bool aStrict = false)
            : base(aScope, aScope.Root.FunctionPrototype)//new EcmaScriptObject(aScope, aScope.Root.FunctionPrototype)
        {
            Class = "Function";
            Scope = aScopeVar;
            var lProto = new EcmaScriptObject(aScope);
            lProto.DefineOwnProperty("constructor", new PropertyValue(PropertyAttributes.Writable | PropertyAttributes.Configurable, this));
            fOriginalName = aOriginalName;
            fDelegate = aDelegate;
            Values.Add("length", PropertyValue.NotAllFlags(aLength));
            DefineOwnProperty("prototype", new PropertyValue(PropertyAttributes.Writable, lProto));
            if (aStrict) {
                DefineOwnProperty("caller", new PropertyValue(PropertyAttributes.None, aScope.Thrower, aScope.Thrower));
                DefineOwnProperty("arguments", new PropertyValue(PropertyAttributes.None, aScope.Thrower, aScope.Thrower));
            }
            fOriginalBody = aOriginalBody;
        }

        public override object Call(ExecutionContext context, params object[] args)
        {
            return fDelegate(new ExecutionContext(Scope, false), this, args, this);
        }

        public override object CallEx(ExecutionContext context, object aSelf, params object[] args)
        {
            return fDelegate(new ExecutionContext(Scope, false), aSelf, args, this);
        }

        public override object Construct(ExecutionContext context, params object[] args)
        {
            var lRes = new EcmaScriptObject(Root);
            lRes.Prototype = ((EcmaScriptObject)Get("prototype", context)) ?? Root.ObjectPrototype;

            var result = fDelegate(context, lRes, args, this);
            if (!(result is EcmaScriptObject)) result = lRes;
            return result;
        }
    }
}