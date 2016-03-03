using RemObjects.Script.EcmaScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class Es5ConformanceFramework2
    {
        string fName;
        string fDescription;
        object fTest;
        object fPrecondition;
        GlobalObject fRoot;

        public ExecutionContext Context { get; set; }

        public void registerTest(EcmaScriptObject o)
        {
            fName = Utilities.GetObjAsString(o.Get(null, 0, "path"), Context);
            fDescription = Utilities.GetObjAsString(o.Get(null, 0, "description"), Context);
            fTest = o.Get(null, 0, "test");
            fPrecondition = o.Get(null, 0, "precondition");
            fRoot = o.Root;
        }

        public void run()
        { 
            object lRes;
            if ((fPrecondition != null) && (fPrecondition != Undefined.Instance))
            {
                lRes = ((EcmaScriptObject)fPrecondition).CallEx(Context, fRoot);
                Xunit.Assert.True(
                    Utilities.GetObjAsBoolean(lRes, Context), "Precondition for " + 
                    fName + " failed. Description: " +
                    fDescription);
            }

            lRes = ((EcmaScriptObject)fTest).Call(Context, fRoot);
            Xunit.Assert.True(
                Utilities.GetObjAsBoolean(lRes, Context), "Testcase " + 
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

        public Es5ConformanceFramework2() { }
    }
}
