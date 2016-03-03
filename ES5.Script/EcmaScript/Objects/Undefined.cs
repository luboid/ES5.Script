using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Objects
{
    public class Undefined
    {
        static Undefined fInstance = new Undefined();

        private Undefined() { }

        public static readonly System.Reflection.MethodInfo Method_Instance = typeof(Undefined).GetMethod("get_Instance");

        public static Undefined Instance
        {
            get
            {
                return fInstance;
            }
        }

        public override string ToString()
        {
            return "undefined";
        }
    }
}