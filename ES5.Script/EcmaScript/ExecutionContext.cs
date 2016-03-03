using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public class ExecutionContext
    {

        public bool Strict
        {
            get;
            set;
        }

        public EnvironmentRecord LexicalScope
        {
            get;
            set;
        }

        public EnvironmentRecord VariableScope
        {
            get;
            set;
        }

        public GlobalObject Global
        {
            get
            {
                return LexicalScope.Global;
            }
        }

        public ExecutionContext() : this(null, false) { }

        public ExecutionContext(EnvironmentRecord aScope, bool aStrict)
        {
            LexicalScope = aScope;
            VariableScope = aScope;
            Strict = aStrict;
        }

        public IDebugSink GetDebugSink()
        {
            return Global.Debug;
        }

        public void StoreParameter(object[] args, int index, string name, bool aStrict)
        {
            var lVal = index < args.Length ? args[index] : Undefined.Instance;
            if (!VariableScope.HasBinding(name))
            {
                VariableScope.CreateMutableBinding(name, false);
                VariableScope.SetMutableBinding(name, lVal, aStrict);
            }
        }

        public ExecutionContext With(object aVal)
        {
            return new ExecutionContext(new ObjectEnvironmentRecord(LexicalScope, Utilities.ToObject(this, aVal), true), false);
        }


        public static ExecutionContext Catch(object value, ExecutionContext context, string name)
        {
            var lResult = new ExecutionContext(new DeclarativeEnvironmentRecord(context.LexicalScope, context.Global), context.Strict);
            lResult.LexicalScope.CreateMutableBinding(name, false);

            if ((value is EcmaScriptObjectWrapper) && (((EcmaScriptObjectWrapper)value).Value is Exception))
                lResult.LexicalScope.SetMutableBinding(name, lResult.Global.ErrorCtor(lResult, null, ((Exception)((EcmaScriptObjectWrapper)value).Value).Message), false);
            else
                lResult.LexicalScope.SetMutableBinding(name, value, false);

            return lResult;
        }

        public static readonly MethodInfo method_SetStrict = typeof(ExecutionContext).GetMethod("set_Strict");
        public static readonly MethodInfo Method_Catch = typeof(ExecutionContext).GetMethod("Catch");
        public static readonly MethodInfo Method_With = typeof(ExecutionContext).GetMethod("With");
        public static readonly ConstructorInfo ConstructorInfo = typeof(ExecutionContext).GetConstructor(new[] { typeof(EnvironmentRecord), typeof(Boolean) });
        public static readonly MethodInfo Method_GetDebugSink = typeof(ExecutionContext).GetMethod("GetDebugSink");
        public static readonly MethodInfo Method_get_LexicalScope = typeof(ExecutionContext).GetMethod("get_LexicalScope");
        public static readonly MethodInfo Method_get_VariableScope = typeof(ExecutionContext).GetMethod("get_VariableScope");
        public static readonly MethodInfo Method_get_Global = typeof(ExecutionContext).GetMethod("get_Global");
        public static readonly MethodInfo Method_StoreParameter = typeof(ExecutionContext).GetMethod("StoreParameter");

    }
}