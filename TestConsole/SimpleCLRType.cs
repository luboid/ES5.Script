using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestConsole
{
    public class SimpleCLRType
    {
        public int A { get; set; }
        public string B { get; set; }

        public SimpleCLRType()
        {
            A = 42;
            B = "Foo";
        }

        public static string Foo()
        {
            return "Bar";
        }

        public string Bar()
        {
            return "LDA.CALL CALLED";
        }
    }
}
