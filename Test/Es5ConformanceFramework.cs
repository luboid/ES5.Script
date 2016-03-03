using ES5.Script.EcmaScript;
using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class Es5ConformanceFramework
    {
        string fName;
        string fDescription;
        EcmaScriptObject fTest;
        EcmaScriptObject fPrecondition;
        GlobalObject fRoot;

        public void registerTest(EcmaScriptObject o)
        {
            fRoot = o.Root;
            fName = Utilities.GetObjAsString(o.Get("path"), fRoot.ExecutionContext);
            fDescription = Utilities.GetObjAsString(o.Get("description"), fRoot.ExecutionContext);
            fTest = o.Get("test") as EcmaScriptObject;
            fPrecondition = o.Get("precondition") as EcmaScriptObject;
        }

        public void run()
        { 
            object lRes;
            if (fPrecondition != null)
            {
                lRes = fPrecondition.CallEx(fRoot.ExecutionContext, fRoot);
                Xunit.Assert.True(
                    Utilities.GetObjAsBoolean(lRes, fRoot.ExecutionContext), "Precondition for " + 
                    fName + " failed. Description: " +
                    fDescription);
            }

            lRes = fTest.CallEx(fRoot.ExecutionContext, fRoot);
            Xunit.Assert.True(
                Utilities.GetObjAsBoolean(lRes, fRoot.ExecutionContext), "Testcase " + 
                fName + " failed. Description: " + 
                fDescription);

            //lName.ToString();
            //test: A function that performs the actual test. The test harness will call this function to execute the test.  
            // It must return true if the test passes. Any other return value indicates a failed test. 
            // The test function is called as a method with its this value set to its defining test case object.
            //precondition: (Optional) A function that is called before the test function to see if it is ok to run the test.  
            // If all preconditions necessary to run the test are established it should return true. If it returns false, 
            // the test will not be run.  The precondition function is called as a method with its this value set to its 
            // defining test case object. This property is optional. If it is not present, the test function will always be 
            // executed.
        }

        public Es5ConformanceFramework() { }
    }
}
