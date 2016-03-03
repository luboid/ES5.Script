using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public static class DynamicMethods
    {
        //tools to write IL to assembly to be able we to check genarated IL Code
        //static AssemblyBuilder assemblyBuilder;
        //static Stack<TypeBuilder> builder = new Stack<TypeBuilder>();
        //static AssemblyName assemblyName;
        //static ModuleBuilder moduleBuilder;

        //static DynamicMethods()
        //{
        //    assemblyName = new AssemblyName("JavaScript");
        //    assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave, @"c:\\pub\\");
        //    moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, assemblyName.Name + ".dll");
        //}

        //public static MethodBuilder CreateMethod(string name, Type returnValue, Type[] args)
        //{
        //    builder.Push(moduleBuilder.DefineType("DynamicMethods_" + Guid.NewGuid().ToString("N"), TypeAttributes.Public));
        //    return builder.Peek().DefineMethod(name, MethodAttributes.Public | MethodAttributes.Static, returnValue, args);
        //}

        //public static T CreateDeleagte<T>()
        //    where T : class
        //{
        //    var t = builder.Pop().CreateType();
        //    return Delegate.CreateDelegate(typeof(T), t.GetMethods()[0]) as T;
        //}

        //public static void Save()
        //{
        //    assemblyBuilder.Save(assemblyName.Name + ".dll");
        //}
    }
}
