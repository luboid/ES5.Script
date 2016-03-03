using ES5.Script;
using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public abstract class ScriptTest
    {
        protected delegate object ScriptDelegate(params object[] args);

        protected string fResult;


        protected object ExecuteJS(string script)
        {
            using (var lEngine = new EcmaScriptComponent())
            {
                lEngine.Debug = false;
                lEngine.RunInThread = false;

                lEngine.Source = script;

                fResult = String.Empty;

                ScriptDelegate lWriteLn = (object[] args) =>
                   {
                       foreach (var el in args)
                           fResult = fResult + Utilities.GetObjAsString(el, lEngine.GlobalObject.ExecutionContext) + Environment.NewLine;

                       return (object)null;
                   };

                lEngine.Globals.SetVariable("writeln", lWriteLn);
                lEngine.Run();

                return lEngine.RunResult;
            }
        }
    }
}