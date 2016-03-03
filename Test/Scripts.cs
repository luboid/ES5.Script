using ES5.Script;
using ES5.Script.EcmaScript;
using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class Scripts : ScriptTest
    {
        void AssertResult(string s0, string s1)
        {
            Assert.Equal(s0.Replace("\r\n", "\n").Trim(new char[] { (char)13, (char)9, (char)32, (char)10 }),
                s1.Replace("\r\n", "\n").Trim(new char[] { (char)13, (char)9, (char)32, (char)10 }));
        }

        [Fact(Skip = "temporarily")]
        public void EvalTest()
        {
            ExecuteJS(
  @"var i = 12;
writeln('t1: '+i);
eval('i = 10;');
writeln('t2: '+i);
        function test()
        {
            eval('var i = 15;');
            writeln('t3: ' + i);
        }
writeln('t4: '+i);
test();
writeln('t5: '+i);
");
            var lExpected = @"t1: 12
t2: 10
t4: 10
t3: 15
t5: 10
";
            AssertResult(lExpected, fResult);
        }

        [Fact(Skip = "temporarily")]
        public void PropertyTest()
        {
            ExecuteJS(@"
var myObj = function() {

            var value = 0;

            return {
                getValue: function() {
                    return value;
                }, 
        setValue: function(v) {
                    value = v;
                }
            }

        }(); 
writeln(myObj.getValue());
 myObj.setValue('test');
 writeln(myObj.getValue());
");
            var lExpected = @"0
test
";

            AssertResult(lExpected, fResult);
        }

        [Fact(Skip = "temporarily")]
        public void SimpleEvalTest()
        {
            var lScript = @"
  eval('var x = 1');
        y = 2;
  x + y;";
            Assert.Equal((Object)3, ExecuteJS(lScript));
        }

        [Fact(Skip = "temporarily")]
        public void FunctionTest()
        {
            var lScript = @"
var test = function(x) { return 'testthis' + x; }
writeln(test(15));
writeln('test');                              
";
            ExecuteJS(lScript);
            var lExpected = @"testthis15
test
";
            Assert.Equal(lExpected, fResult);
        }

        [Fact(Skip = "temporarily")]
        public void SimpleFunctionTest()
        {
            var lScript = @"
function test(x) { return 'testthis' + x; }
writeln(test(15));
writeln('test');                              
";
            ExecuteJS(lScript);
            var lExpected = @"testthis15
test
";
            Assert.Equal(lExpected, fResult);
        }

        [Fact(Skip = "I don't now waht is doing this test")]
        public void RegexData()
        {
            ExecuteJS(@"
function twitterCallback2(twitters) {
	var statusHTML = [];
	for (var i=0; i<twitters.length; i++){
		var username = twitters[i].user.screen_name;
		var status = twitters[i].text.replace(/((https?|s?ftp|ssh)\:\/\/[^""\s\<\>]*[^.,;'"">\:\s\<\>\)\]\!])/g, function(url) {
			return '<a href=""'+url+'"">'+url+'</a>';
		}).replace(/\B@([_a-z0-9]+)/ig, function(reply) {
			return  reply.charAt(0)+'<a href=""http://www.twitter.com/'+reply.substring(1)+'"">'+reply.substring(1)+'</a>';
		});
		statusHTML.push('<li><span>'+status+'</span> <a style=""font-size:85%"" href=""http://twitter.com/'+username+'/statuses/'+twitters[i].id+'"">'+relative_time(twitters[i].created_at)+'</a></li>');
	}
	document.getElementById('twitter_update_list_'+username).innerHTML = statusHTML.join('');
}
 
function relative_time(time_value) {
  var values = time_value.split("" "");
  time_value = values[1] + "" "" + values[2] + "", "" + values[5] + "" "" + values[3];
  var parsed_date = Date.parse(time_value);
  var relative_to = (arguments.length > 1) ? arguments[1] : new Date();
  var delta = parseInt((relative_to.getTime() - parsed_date) / 1000);
  delta = delta + (relative_to.getTimezoneOffset() * 60);
 
  if (delta < 60) {
    return 'less than a minute ago';
  } else if(delta < 120) {
    return 'about a minute ago';
  } else if(delta < (60*60)) {
    return (parseInt(delta / 60)).toString() + ' minutes ago';
  } else if(delta < (120*60)) {
    return 'about an hour ago';
  } else if(delta < (24*60*60)) {
    return 'about ' + (parseInt(delta / 3600)).toString() + ' hours ago';
  } else if(delta < (48*60*60)) {
    return '1 day ago';
  } else {
    return (parseInt(delta / 86400)).toString() + ' days ago';
  }
}
");
        }

        [Fact(Skip = "temporarily")]
        public void AndOrElse()
        {
            ExecuteJS(@"var x = ""test"";
writeln('string');
writeln(x || ""test2"");
writeln(x && ""test2"");
        x = false;
writeln('bool');
writeln(x || ""test2"");
writeln(x && ""test2"");
        x = null;
writeln('null');
writeln(x || ""test2"");
writeln(x && ""test2"");

        x = undefined;
writeln('undefined');
writeln(x || ""test2"");
writeln(x && ""test2"");");
            var lExpected = @"string
test
test2
bool
test2
false
null
test2
null
undefined
test2
undefined";
            AssertResult(lExpected, fResult);
        }

        [Fact(Skip = "temporarily")]
        public void MinMax()
        {
            ExecuteJS(
        @"var x = 1;
       var y = 2;
var b = Math.max(x,y);
var a = Math.min(x, y);
var c = a + ""--"" + b;
       writeln(c);
      ");
            var lExpected = "1--2" + Environment.NewLine;

            Assert.Equal(lExpected, fResult);
        }

        [Fact(Skip = "temporarily")]
        public void TestProto()
        {
            ExecuteJS(@" function Test(x) { this.x = x; }
Test.prototype.hello = function() { writeln('x = ' + this.x); };
        var t = new Test(42);
        t.hello();");
            var lExpected = "x = 42";
            AssertResult(lExpected, fResult);
        }

        [Fact(Skip = "temporarily")]
        public void Arguments()
        {
            ExecuteJS(@"var f = function(a,b) {
  writeln(arguments);
    };

f();");
            var lExpected = "[object Arguments]";
            AssertResult(lExpected, fResult);
        }

        [Fact(Skip = "temporarily")]
        public void UnicodeIdentifier()
        {
            ExecuteJS(@"var abc = 15;
  writeln('a\u0062c');
  writeln('\u0061bc');
  ");

            var lExpected = @"abc
abc";
            AssertResult(lExpected, fResult);
        }

        [Fact(Skip = "temporarily")]
        public void Error()
        {
            ExecuteJS(@"try {
  n = 15;
        n = n / 0;
  writeln(n.toString());
 eval('(');
    } catch(n){
   writeln(n);
   writeln(typeof(n));
   writeln(n instanceof Error);
   writeln(n instanceof SyntaxError);
   writeln(n.message);
}
var x = new Error('test');
writeln(x);
writeln(x.message);
writeln(Error.prototype.name);
writeln(Error.prototype.message);
writeln(Error.prototype.toString());");
            var lExpected = @"Infinity
SyntaxError: <eval>(1:2): Syntax error
object
true
true
<eval>(1:2): Syntax error
Error: test
test
Error

Error
";
            AssertResult(lExpected, fResult);
        }

        [Fact(Skip = "temporarily")]
        public void UnknownCall1()
        {
            try
            {
                ExecuteJS(@"
    var x = { };
        x.name();
  ");
                Assert.False(true, "Should not be here");
            }
            catch (ScriptRuntimeException e) when (e.ToString() == "TypeError: Object [object Object] has no method \"name\"")
            { }
        }

        [Fact(Skip = "temporarily")]
        public void UnknownCall2()
        {
            try
            {
                ExecuteJS(@"
    var x = { };
        x.name = {};
    x.name();
  ");
                Assert.False(true, "Should not be here");
            }
            catch (ScriptRuntimeException e) when (e.ToString() == @"TypeError: Property ""name"" of object [object Object] is not callable")
            { }
        }

        [Theory(Skip = "temporarily")]
        [InlineData("var x = new JSType(); writeln(x.A);", "JSType", "42")]
        [InlineData("var x = new JSType(); writeln(x.B);", "JSType", "Foo")]
        [InlineData("writeln(JSType.Foo());", "JSType", "Bar")]
        [InlineData("var x = new SimpleCLRType(); writeln(x.A);", "", "42")]
        [InlineData("writeln(lda.Bar());", "", "LDA.CALL CALLED")]
        public void ExposeType(string aScript, string aTypeName, string aExpectedResult)
        {
            using (var lScriptEngine = new EcmaScriptComponent())
            {
                lScriptEngine.Debug = false;
                lScriptEngine.RunInThread = false;

                lScriptEngine.Source = aScript;
                var fResult = String.Empty;

                ScriptDelegate lWriteLn = (object[] args) =>
                {
                    foreach (var el in args)
                        fResult = fResult + Utilities.GetObjAsString(el, lScriptEngine.GlobalObject.ExecutionContext);
                    return null;
                };
                lScriptEngine.Globals.SetVariable("writeln", lWriteLn);

                lScriptEngine.ExposeType(typeof(SimpleCLRType), aTypeName);
                lScriptEngine.Globals.SetVariable("lda", new EcmaScriptObjectWrapper(new SimpleCLRType(), typeof(SimpleCLRType), lScriptEngine.GlobalObject));
                lScriptEngine.Run();

                Assert.Equal(aExpectedResult, fResult);
            }
        }

        [Fact(Skip = "temporarily")]
        public void RunFunction_Exception_IsNotLost()
        {
            using (var lScriptEngine = new EcmaScriptComponent())
            {
                lScriptEngine.Debug = false;
                lScriptEngine.RunInThread = false;

                var lWasExceptionRaised = false;
                try
                {
                    lScriptEngine.RunFunction("eval", "throw new Error('test error...');");
                }
                catch
                {
                    lWasExceptionRaised = true;
                }

                Assert.True(lWasExceptionRaised, "Exception was not raised in the .NET code");
                Assert.NotNull(lScriptEngine.RunException);
            }
        }

        [Fact(Skip = "temporarily")]
        public void StingSlice()
        {
            // Samples were taken from the https://developer.mozilla.org/en-US/docs/JavaScript/Reference/Global_Objects/String/slice specification
            ExecuteJS(
  @"var str1 = ""1234567890ABCDEFGH"";
writeln(str1.slice(4, -2));
writeln(str1.slice(-3));
writeln(str1.slice(-3, -1));
writeln(str1.slice(0, -1));
");

            var lExpected = @"567890ABCDEF
FGH
FG
1234567890ABCDEFG
";

            Assert.Equal(lExpected, fResult);
        }

        [Fact(Skip = "temporarily")]
        public void StringSubstring()
        {
            // Samples were taken from the https://developer.mozilla.org/en-US/docs/JavaScript/Reference/Global_Objects/String/substring specification
            ExecuteJS(
  @"var anyString = ""1234567"";

writeln(anyString.substring(0,3));
writeln(anyString.substring(3,0));

writeln(anyString.substring(4,7));
writeln(anyString.substring(7,4));

writeln(anyString.substring(0,6));

writeln(anyString.substring(0,7));
writeln(anyString.substring(0,10))
");

            var lExpected = @"123
123
567
567
123456
1234567
1234567
";

            Assert.Equal(lExpected, fResult);
        }

        [Fact(Skip = "temporarily")]
        public void StirngSubStr()
        {
            // Samples were taken from the https://developer.mozilla.org/en-US/docs/JavaScript/Reference/Global_Objects/String/substr specification
            ExecuteJS(@"var str = ""1234567890"";
writeln(str.substr(1,2));
writeln(str.substr(-3,2));
writeln(str.substr(-3));
writeln(str.substr(1));
writeln(str.substr(-20,2));
writeln(str.substr(20,2));
");

            var lExpected = @"23
89
890
234567890
12

";

            Assert.Equal(lExpected, fResult);
        }

    }
}