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
                engine.Include("test", @"

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

");
                engine.RunFunction("test");
            }
        }

        static void Main(string[] args)
        {
            try
            {
                CallTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}
