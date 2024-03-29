﻿using RemObjects.Script.EcmaScript;
using RemObjects.Script;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace es5Conform
{
    class Program
    {
        static string fE5Root;
        static string fTestRoot;
        static string fLibrary;


        static Program()
        {
            var lPath = new Uri(typeof(Program).Assembly.EscapedCodeBase).LocalPath;

            fE5Root = Path.GetFullPath(Path.Combine(Path.Combine(Path.Combine(Path.GetDirectoryName(lPath),
                "..\\..\\..\\.."), "TestScripts"), "es5conform"));

            fTestRoot = Path.Combine(fE5Root, "TestCases");

            fLibrary = File.ReadAllText(Path.Combine(fTestRoot, "lib.js"));
        }

        static string RunTest(string script)
        {
            var lScriptFilename = Path.Combine(fTestRoot, script);
            var lScriptContent = File.ReadAllText(lScriptFilename, Encoding.UTF8).Replace((char)65533, ' ');
            using (var se = new EcmaScriptComponent())
            {
                var es5Harness = new ES5Harness(); // Real lib allways return null { Context = se.RootContext };
                se.RunInThread = false;
                se.Debug = false;
                se.Globals.SetVariable("ES5Harness", es5Harness);
                se.Include("lib.js", fLibrary);
                se.Include(script, lScriptContent);
                try
                {
                    es5Harness.run();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            return null;
        }

        public static IEnumerable<string> EnumerateTests(string aScan)
        {
            foreach (var f in Directory.EnumerateFiles(aScan).OrderBy(a => a))
            {
                if (f.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase) && !f.EndsWith("lib.js", StringComparison.InvariantCultureIgnoreCase))
                    yield return f.Substring(fTestRoot.Length + 1).ToLower();
            }

            foreach (var dir in Directory.EnumerateDirectories(aScan).OrderBy(a => a))
            {
                if (!dir.EndsWith(".svn", StringComparison.InvariantCultureIgnoreCase))
                    foreach (var i in EnumerateTests(dir))
                        yield return i;
            }
        }


        static void Main(string[] args)
        {
            DateTime d0;
            var testResult = true;
            string result; var c = 0; var o = 0; var f = 0; var settings = new XmlWriterSettings { NewLineOnAttributes = true, Indent = true };
            using (var fail = XmlWriter.Create(Path.GetFullPath("fail.xml"), settings))
            using (var success = XmlWriter.Create(Path.GetFullPath("success.xml"), settings))
            {
                fail.WriteStartElement("tests");
                success.WriteStartElement("tests");
                try
                {
                    foreach (var test in EnumerateTests(fTestRoot))
                    {
                        ++c;
                        Console.Write("{0}", test);
                        d0 = DateTime.Now;
                        try
                        {
                            result = RunTest(test);
                            Console.WriteLine(":{1}:{2}", test, DateTime.Now.Subtract(d0), result ?? "OK");
                            if (result == null)
                            {
                                ++o;
                                success.WriteResult(test, null, DateTime.Now.Subtract(d0), testResult);
                            }
                            else
                            {
                                ++f;
                                fail.WriteResult(test, result, DateTime.Now.Subtract(d0), testResult);
                            }
                        }
                        catch (Exception ex)
                        {
                            ++f;
                            Console.WriteLine("{1}:{2}", DateTime.Now.Subtract(d0), "break");
                            fail.WriteResult(test, ex.ToString(), DateTime.Now.Subtract(d0), testResult);
                        }
                    }
                }
                finally
                {
                    success.WriteEndElement();
                    fail.WriteEndElement();
                    success.Flush();
                    fail.Flush();
                }
                Console.WriteLine("all:{0}, ok:{1}, fail:{2}, perecnt OK:{3}, perecnt FAIL:{4}", c, o, f, ((double)o / c) * 100, ((double)f / c) * 100);
                Console.ReadLine();
            }
        }
    }
}
