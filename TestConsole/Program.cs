using ES5.Script;
//using RemObjects.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {

        static void CallTest(string script, bool expectedResult)
        {
            script = @"
function testFunction1(cc) {
    var o = "+script+@";
    cc.propBoolean = new Boolean(o);
}

function testFunction2(cc) {
    var o = "+script+@";
    cc.propBoolean = o;
}
";

            var lConsole = new ScriptTestConsole();
            using (var engine = new EcmaScriptComponent())
            {
                engine.Include("test", script);

                //engine.RunFunction("testFunction1", lConsole);

                //if (expectedResult != lConsole.propBoolean)
                //{
                //    throw new ApplicationException(string.Format("1.Script is \"{0}\" is not equal to {1}", script, expectedResult));
                //}

                engine.RunFunction("testFunction2", lConsole);
                if (expectedResult != lConsole.propBoolean)
                {
                    throw new ApplicationException(string.Format("2.Script is \"{0}\" is not equal to {1}", script, expectedResult));
                }
            }
        }

        static void Main(string[] args)
        {
            try
            {
                CallTest("{}", true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}
