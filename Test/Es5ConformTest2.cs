using RemObjects.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class Es5ConformTest2
    {
        static string fE5Root;
        static string fTestRoot;
        static string fLibrary;
        public static Es5ConformTestData Es5ConformTestData; 


        static Es5ConformTest2()
        {
            var lPath = new Uri(typeof(Es5ConformTest).Assembly.EscapedCodeBase).LocalPath;

            fE5Root = Path.GetFullPath(Path.Combine(Path.Combine(Path.Combine(Path.GetDirectoryName(lPath), 
                "..\\..\\.."), "TestScripts"), "es5conform"));

            fTestRoot = Path.Combine(fE5Root, "TestCases");

            fLibrary = File.ReadAllText(Path.Combine(fTestRoot, "lib.js"));
            Es5ConformTestData = new Es5ConformTestData(fTestRoot);
        }

        [Theory]
        [MemberData("Es5ConformTestData")]
        public void Test(string script)
        {
            script = "chapter15\\15.2\\15.2.3\\15.2.3.3\\15.2.3.3-4-6.js";
            var lScriptFilename = Path.Combine(fTestRoot, script);
            using (var se = new EcmaScriptComponent())
            {
                var es5Harness = new Es5ConformanceFramework2 { Context = se.RootContext };
                se.RunInThread = false;
                se.Debug = false;
                se.Globals.SetVariable("ES5Harness", es5Harness);
                se.Include("lib.js", fLibrary);
                se.Include(script, File.ReadAllText(lScriptFilename, Encoding.UTF8).Replace((char)65533, ' '));
                es5Harness.run();
            }
        }
    }
}
