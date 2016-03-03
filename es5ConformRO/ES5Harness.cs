using RemObjects.Script.EcmaScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace es5Conform
{
    public class ES5Harness
    {
        string fName;
        string fDescription;
        EcmaScriptObject fTest;
        EcmaScriptObject fPrecondition;
        GlobalObject fRoot;

        public void log(params object[] values)
        {
            Console.WriteLine(String.Join("", values));
        }

        public void registerTest(EcmaScriptObject o)
        {
            fRoot = o.Root;
            fName = Utilities.GetObjAsString(o.Get(null, 0, "path"), fRoot.ExecutionContext);
            fDescription = Utilities.GetObjAsString(o.Get(null, 0, "description"), fRoot.ExecutionContext);
            fTest = o.Get(null, 0, "test") as EcmaScriptObject;
            fPrecondition = o.Get(null, 0, "precondition") as EcmaScriptObject;
        }

        public void run()
        { 
            object lRes;
            if (fPrecondition != null)
            {
                lRes = fPrecondition.CallEx(fRoot.ExecutionContext, fRoot);
                if (!Utilities.GetObjAsBoolean(lRes, fRoot.ExecutionContext))
                {
                    throw new ApplicationException("Precondition for " + fName + " failed. Description: " + fDescription);
                }
            }

            lRes = fTest.CallEx(fRoot.ExecutionContext, fRoot);
            if (!Utilities.GetObjAsBoolean(lRes, fRoot.ExecutionContext))
            {
                throw new ApplicationException("Testcase " + fName + " failed. Description: " + fDescription);
            }
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

        public ES5Harness() { }
    }
}
