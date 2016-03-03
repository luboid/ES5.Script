using System;
using System.Text;
using ES5.Script;
using ES5.Script.EcmaScript.Objects;
using Xunit;

namespace Test
{
    public class NetInterop
    {
        [Fact(Skip = "temporarily")]
        public void UnefinedToObject_Equals_Undefined()
        {
            using (var engine = new EcmaScriptComponent())
            {
                engine.Include("test", @"
            function testFunction(cc) {
                    cc.writeObject(undefined);
                }
                ");

                var lConsole = new ScriptTestConsole();

                engine.RunFunction("testFunction", lConsole);


                Assert.Equal<Object>(Undefined.Instance, lConsole.propObject);
            }
        }

        [Fact(Skip = "temporarily")]
        public void Test_Date_Counstructor()
        {
            using (var engine = new EcmaScriptComponent())
            {
                engine.Include("test", @"
            function testFunction(cc) {
                    cc.propDate = new Date(2013,1,17,1,2,3,4);
                }
                ");

                var lConsole = new ScriptTestConsole();

                engine.RunFunction("testFunction", lConsole);


                Assert.Equal<Object>(new DateTime(2013, 2, 17, 1, 2, 3, 4), lConsole.propDate);
            }
        }

        [Fact(Skip = "temporarily")]
        public void ExceptionMessage_Is_Not_Empty()
        {
            using (var engine = new EcmaScriptComponent())
            {
                engine.Include("test", @"
          function testFunction(cc) {
                try
                {
                    cc.throwException();
                }
                catch (e)
                {
                    cc.writeString(e.message);//is empty???, when .Net exception no message property exists, may be is need native .Net exception
                }
            }
            ");

                var lConsole = new ScriptTestConsole();
                try
                {
                    engine.RunFunction("testFunction", lConsole);
                }
                catch { }

                Assert.Equal<String>("Test Message||", lConsole.GetStringBuffer());
            }
        }

        [Fact(Skip = "temporarily")]
        public void Double_valueOf_DoesntFail()
        {
            var lConsole = new ScriptTestConsole();
            using (var engine = new EcmaScriptComponent())
            {

                engine.Include("test",
            @"
function testFunction(cc)
    {
        cc.propDouble = 1.2;
        cc.propDouble = cc.propDouble.valueOf() + 0.1;
    }
");
                engine.RunFunction("testFunction", lConsole);

                Assert.Equal<Double>(1.3, lConsole.propDouble);
            }
        }

        [Fact(Skip = "temporarily")]
        public void UndefinedToDouble_Equals_NaN()
        {
            var lConsole = new ScriptTestConsole();
            using (var engine = new EcmaScriptComponent())
            {

                engine.Include("test",
            @"
function testFunction(cc) {
    cc.propDouble = undefined;
}
");
                engine.RunFunction("testFunction", lConsole);

                Assert.True(Double.IsNaN(lConsole.propDouble));
            }
        }

        [Fact(Skip = "temporarily")]
        public void NullToDouble_Equals_0()
        {
            var lConsole = new ScriptTestConsole();
            using (var engine = new EcmaScriptComponent())
            {

                engine.Include("test",
            @"
function testFunction(cc) {
    cc.propDouble = null;
}
");
                engine.RunFunction("testFunction", lConsole);

                Assert.Equal<Double>(0, lConsole.propDouble);
            }
        }

        [Theory(Skip = "temporarily")]
        [InlineData("{}", true)]
        [InlineData("0", false)]
        [InlineData("-0", false)]
        [InlineData("1", true)]
        [InlineData("-1", true)]
        [InlineData("-0.1", true)]
        [InlineData("null", false)]
        [InlineData("''", false)]
        [InlineData("'true'", true)]
        [InlineData("'false'", true)]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("'AAAA'", true)]
        [InlineData("undefined", false)]
        [InlineData("Number.NaN", false)]
        public void JsObjectsAreConvertedToBooleanProperly(string script, bool expectedResult)
        {
            // If the Boolean object has no initial value, or if the passed value is one of the following:
            // 0,-0,null,'',false,undefined,NaN
            // the object is set to false. For any other value it is set to true (even with the string 'false')!

            var lConsole = new ScriptTestConsole();
            using (var engine = new EcmaScriptComponent())
            {

                engine.Include("test", @"
function testFunction1(cc) {
    var o = " + script + @";
    cc.propBoolean = new Boolean(o);
}

function testFunction2(cc) {
    var o = " + script + @";
    cc.propBoolean = o;
}
");

                engine.RunFunction("testFunction1", lConsole);

                Assert.Equal<Boolean>(expectedResult, lConsole.propBoolean);

                engine.RunFunction("testFunction2", lConsole);
                Assert.Equal<Boolean>(expectedResult, lConsole.propBoolean);
            }
        }

        [Fact(Skip = "temporarily")]
        public void BooleanValuesAreConvertedToStringProperly()
        {
            using (var engine = new EcmaScriptComponent())
            {
                engine.Include("test",
            @"
function testFunction(cc)
    {
        cc.propString = true;// lead to 'True'
        cc.writeString(cc.propString);
        cc.writeString(true);// lead to 'True'

        cc.propString = new Boolean(true);// lead to 'true'
        cc.writeString(cc.propString);
        cc.writeString(new Boolean(true));// lead to dirfferent presentation in string format 'True' when up one is 'true'

        cc.propString = false;// lead to 'False'
        cc.writeString(cc.propString);
        cc.writeString(false);// lead to 'False'

        cc.propString = new Boolean(false);// lead to 'false'
        cc.writeString(cc.propString);
        cc.writeString(new Boolean(false));// lead to dirfferent presentation in string format 'False' when up one is 'false'
    }
");
                var lConsole = new ScriptTestConsole();
                engine.RunFunction("testFunction", lConsole);

                Assert.Equal<String>("true||true||true||true||false||false||false||false||", lConsole.GetStringBuffer());
            }
        }

        [Fact(Skip = "temporarily")]
        public void _ToString_IsCalledWhenScriptCalls_toString_()
        {
            using (var engine = new EcmaScriptComponent())
            {
                engine.Include("test",
            @"
            function testFunction(cc)
    {
        // Result should be the same!
        var ts0 = cc.toString();
        var ts1 = cc.ToString();
        cc.writeString(ts0);
        cc.writeString(ts1);
    }
");
                var lConsole = new ScriptTestConsole();
                engine.RunFunction("testFunction", lConsole);

                Assert.Equal<String>("Custom .ToString call result||Custom .ToString call result||", lConsole.GetStringBuffer());
            }
        }

        [Fact(Skip = "temporarily")]
        public void DatePropertyAcceptsNumber()
        {
            var dat = new DateTime(2013, 1, 17, 17, 18, 0, 0);
            var i = GlobalObject.DateTimeToUnix(dat.ToUniversalTime());

            using (var engine = new EcmaScriptComponent())
            {
                engine.Include("test", @"
            function testFunction(cc)
    {
        cc.propDate = new Number(" + i + @".0);//Add TimeZone
    }
");
                var lConsole = new ScriptTestConsole();
                engine.RunFunction("testFunction", lConsole);

                Assert.Equal<DateTime>(dat, lConsole.propDate);
            }
        }

        [Fact(Skip = "temporarily")]
        public void DateConstructorCreatesValidDateForDatesPriorTo1970()
        {
            var dat = new DateTime(1968, 3, 4, 1, 1, 1, 1);
            using (var engine = new EcmaScriptComponent())
            {
                engine.Include("test", @"
            function testFunction(cc)
    {
        cc.propDate = new Date(1968, 2, 4, 1, 1, 1, 1);
    }
");
                var lConsole = new ScriptTestConsole();
                engine.RunFunction("testFunction", lConsole);

                Assert.Equal<DateTime>(dat, lConsole.propDate);
            }
        }

        [Fact(Skip = "temporarily")]
        public void DateConstructorCreatesValidDate()
        {
            var dat = new DateTime(2013, 2, 17, 1, 2, 3, 4);
            using (var engine = new EcmaScriptComponent())
            {
                engine.Include("test", @"
            function testFunction(cc)
    {
        cc.propDate = new Date(2013,1,17,1,2,3,4);
    }
");
                var lConsole = new ScriptTestConsole();
                engine.RunFunction("testFunction", lConsole);

                Assert.Equal<DateTime>(dat, lConsole.propDate);
            }
        }
    }
}