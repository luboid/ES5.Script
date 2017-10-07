using ES5.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {

        static void CallTest()
        {
            using (var engine = new EcmaScriptComponent())
            {
                // not woriking 
                var test = @"
function test() {
    try {
        var o = JSON.stringify;
        var d = delete JSON.stringify;
        if (d === true && JSON.stringify === undefined) {
            return true;
        }
    } finally {
        JSON.stringify = o;
    }
}
";
/*
                var test = @"
function test() {
  var ok = false;
  try {
      var o = JSON.stringify;
      var d = delete JSON.stringify;
      if (d === true && JSON.stringify === undefined) {
        ok = true;
      }
  } finally {
    JSON.stringify = o;
  }
  return ok;
}
";
*/
                engine.Include("test", test);
                engine.RunFunction("test");
            }
        }

        static void Main(string[] args)
        {
            try
            {
                CallTest();
                Console.WriteLine("Ok.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("Press Enter to exit ...");
            Console.ReadLine();
        }
    }
}
