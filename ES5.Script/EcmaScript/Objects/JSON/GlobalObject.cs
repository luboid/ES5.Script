using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace ES5.Script.EcmaScript.Objects
{
    public partial class GlobalObject : EcmaScriptObject
    {
        public EcmaScriptObject CreateJSON()
        {
            var result = Get("JSON") as EcmaScriptObject;
            if (result != null)
                return result;

            result = new EcmaScriptObject(this, ObjectPrototype) { Class = "JSON" };
            Values.Add("JSON", PropertyValue.NotEnum(result));
            result.Values.Add("parse", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "parse", JSONParse, 2, false)));
            result.Values.Add("stringify", PropertyValue.NotEnum(new EcmaScriptFunctionObject(this, "stringify", JSONStringify, 3, false)));

            result.Prototype = ObjectPrototype;
            return result;
        }


        public object JSONParse(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lTok = new Tokenizer();
            lTok.JSON = true;
            lTok.Error += (Tokenizer caller, TokenizerErrorKind kind, string parameter) =>
                          {
                              RaiseNativeError(NativeErrorType.SyntaxError, "Error in json data at " + caller.Row + ":" + caller.Col);
                          };

            lTok.SetData(Utilities.GetArgAsString(args, 0, aCaller), "");

            var result = JSONParse(lTok);
            if (lTok.Token != TokenKind.EOF)
                RaiseNativeError(NativeErrorType.SyntaxError, "EOF expected " + lTok.Row + ":" + lTok.Col);

            var lReviver = Utilities.GetArg(args, 1) as EcmaScriptBaseFunctionObject;
            if (lReviver != null)
            {
                var lWork = new EcmaScriptObject(this);
                lWork.DefineOwnProperty("", new PropertyValue(PropertyAttributes.None, result), false);
                result = Walk(aCaller, lWork, lReviver, "", lWork);
            }
            return result;
        }


        public object JSONParse(Tokenizer aTok)
        {
            object result = null;
            switch (aTok.Token)
            {
                case TokenKind.K_null:
                    {
                        result = null;
                        aTok.Next();
                    }
                    break;
                case TokenKind.K_true:
                    {
                        result = true;
                        aTok.Next();
                    }
                    break;
                case TokenKind.K_false:
                    {
                        result = false;
                        aTok.Next();
                    }
                    break;
                case TokenKind.OpeningBracket:
                    {
                        aTok.Next();
                        EcmaScriptArrayObject o;
                        result = o = new EcmaScriptArrayObject(this, 0);
                        if (aTok.Token == TokenKind.ClosingBracket)
                        {
                            aTok.Next();
                            break;
                        }

                        while (true)
                        {
                            o.AddValue(JSONParse(aTok));
                            if (aTok.Token == TokenKind.ClosingBracket)
                            {
                                aTok.Next();
                                return o;
                            }

                            if (aTok.Token != TokenKind.Comma)
                                RaiseNativeError(NativeErrorType.SyntaxError, "Comma expected at " + aTok.Row + ":" + aTok.Col);

                            aTok.Next();
                        }
                    }
                case TokenKind.CurlyOpen:
                    {
                        aTok.Next();
                        EcmaScriptObject o;
                        result = o = new EcmaScriptObject(this);
                        if (aTok.Token == TokenKind.CurlyClose)
                        {
                            aTok.Next();
                            return o;
                        }

                        while (true)
                        {
                            if (aTok.Token != TokenKind.String)
                                RaiseNativeError(NativeErrorType.SyntaxError, "String expected at " + aTok.Row + ":" + aTok.Col);

                            var lKey = Tokenizer.DecodeString(aTok.TokenStr);
                            aTok.Next();

                            if (aTok.Token != TokenKind.Colon)
                                RaiseNativeError(NativeErrorType.SyntaxError, "Colon expected at " + aTok.Row + ":" + aTok.Col);
                            aTok.Next();

                            var lValue = JSONParse(aTok);
                            o.AddValue(lKey, lValue);
                            if (aTok.Token == TokenKind.CurlyClose)
                            {
                                aTok.Next();
                                return o;
                            }

                            if (aTok.Token != TokenKind.Comma)
                                RaiseNativeError(NativeErrorType.SyntaxError, "Comma expected at " + aTok.Row + ":" + aTok.Col);

                            aTok.Next();
                        }
                    }
                case TokenKind.String:
                    {
                        result = Tokenizer.DecodeString(aTok.TokenStr);
                        aTok.Next();
                    }
                    break;
                case TokenKind.Float:
                    {
                        double d;
                        if (!Double.TryParse(aTok.TokenStr, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out d))
                            RaiseNativeError(NativeErrorType.SyntaxError, "Number expected at " + aTok.Row + ":" + aTok.Col);

                        aTok.Next();
                        result = d;
                    }
                    break;
                case TokenKind.Number:
                    {
                        var i = 0;
                        if (!Int32.TryParse(aTok.TokenStr, out i))
                        {
                            var i6 = 0L;
                            if (Int64.TryParse(aTok.TokenStr, out i6))
                            {
                                aTok.Next();
                                return (i6);
                            }

                            RaiseNativeError(NativeErrorType.SyntaxError, "Number expected at " + aTok.Row + ":" + aTok.Col);
                        }

                        aTok.Next();
                        result = i;
                    }
                    break;
                default:
                    RaiseNativeError(NativeErrorType.SyntaxError, "JSON object expected at " + aTok.Row + ":" + aTok.Col); break
                   ;
            }
            return result;
        }


        public object JSONStringify(ExecutionContext aCaller, object aSelf, params object[] args)
        {
            var lStack = new List<Object>();
            var lIndent = String.Empty;
            var lGap = String.Empty;
            var lReplacerFunction = Utilities.GetArg(args, 1) as EcmaScriptBaseFunctionObject;
            List<String> lPropList = null;

            if (lReplacerFunction == null)
            {
                var lItem = Utilities.GetArg(args, 1) as EcmaScriptArrayObject;
                lPropList = new List<String>();
                if (lItem != null)
                {
                    for (uint i = 0, l = lItem.Length; i < l; i++)
                    {
                        var lEl = lItem.Get(i.ToString(), aCaller, 0);
                        if ((lEl == null) || (lEl == Undefined.Instance))
                            continue;

                        lPropList.Add(lEl.ToString());
                    }
                }
            }

            var lSpace = Utilities.GetArg(args, 2);
            var e = lSpace as EcmaScriptObject;
            if (e != null)
            {
                if (e.Class == "Number")
                    lGap = new String(' ', Utilities.GetObjAsInteger(lSpace, aCaller));
                else if (e.Class == "String")
                    lGap = Utilities.GetObjAsString(lSpace, aCaller);
            }
            else if (lSpace is String)
                lGap = (String)lSpace;
            else if ((lSpace is Int32) || (lSpace is Double))
                lGap = new String(' ', Utilities.GetObjAsInteger(lSpace, aCaller));

            if (lGap.Length > 10)
                lGap = lGap.Substring(0, 10);

            var lWork = new EcmaScriptObject(this);
            lWork.DefineOwnProperty("", new PropertyValue(PropertyAttributes.All, Utilities.GetArg(args, 0)), false);

            return (this.JSONStr(aCaller, lStack, lGap, lIndent, lReplacerFunction, lPropList, lWork, ""));
        }


        public object Walk(ExecutionContext aCaller, EcmaScriptObject aRoot, EcmaScriptBaseFunctionObject aReviver, string aName, EcmaScriptObject aCurrent)
        {
            var lItem = aCurrent.Get(aName, aCaller, 2);
            var lEc = lItem as EcmaScriptObject;
            if (lEc != null)
            {
                var lArr = lEc as EcmaScriptArrayObject;
                if (lArr != null)
                {
                    var i = 0;
                    var lLen = (Int32)lArr.Length;
                    while (i < lLen)
                    {
                        var lNewVal = Walk(aCaller, aRoot, aReviver, i.ToString(), lArr);
                        if (lNewVal == Undefined.Instance)
                        {
                            lArr.Delete(i.ToString(), false);
                        }
                        else
                        {
                            lArr.Put(i.ToString(), lNewVal, aCaller);
                            ++i;
                        }
                    }
                }
                else
                {
                    foreach (var el in System.Linq.Enumerable.ToArray(lEc.Values.Keys))
                    { // copy it first
                        var lNewVal = Walk(aCaller, aRoot, aReviver, el, lArr);
                        if (lNewVal == Undefined.Instance)
                            lEc.Delete(el, false);
                        else
                            lEc.DefineOwnProperty(el, new PropertyValue(PropertyAttributes.None, lNewVal), false);
                    }
                }
            }
            return (aReviver.CallEx(aCaller, aRoot, aName, lItem));
        }


        string JSONStr(ExecutionContext aExecutionContext, List<Object> aStack, string aGap, string aIndent,
                           EcmaScriptBaseFunctionObject aReplacerFunction,
                           List<String> aProplist, EcmaScriptObject aWork,
                           string aValue)
        {
            var Value = aWork.Get(aValue, aExecutionContext);

            var lObj = Value as EcmaScriptObject;
            if (lObj != null)
            {
                var lCall = lObj.Get("toJSON", aExecutionContext) as EcmaScriptBaseFunctionObject;
                if (lCall != null)
                {
                    Value = lCall.CallEx(aExecutionContext, lObj, aValue);
                    if (Value is string)
                    {
                        return Value as string;
                    }
                }
            }

            if (aReplacerFunction != null)
                Value = aReplacerFunction.CallEx(aExecutionContext, aWork, aValue, Value);

            lObj = Value as EcmaScriptObject;
            if (lObj != null)
            {
                if (lObj.Class == "Number")
                    Value = lObj.Value;
                else if (lObj.Class == "String")
                    Value = lObj.Value;
                else if (lObj.Class == "Boolean")
                    Value = lObj.Value;
            }

            var lResult = JSON.ToString(Value);
            if (lResult != null)
                return (lResult);

            if (lObj == null)
                return (null);

            if ((lObj is EcmaScriptBaseFunctionObject) && !(lObj is EcmaScriptObjectWrapper))
                return (null);

            var wrapper = lObj as EcmaScriptObjectWrapper;

            if (null != wrapper && typeof(Delegate).IsAssignableFrom(wrapper.Value.GetType().BaseType))
                return ("function");

            if (aStack.Contains(lObj))
                RaiseNativeError(NativeErrorType.TypeError, "Recursive JSON structure");

            aStack.Add(lObj);
            var lWork = new StringBuilder();
            if (lObj.Class == "Array")
            {
                var arr = lObj as EcmaScriptArrayObject;
                if (arr.Length == 0)
                    lWork.Append("[]");

                aIndent = aIndent + aGap;

                for (uint i = 0, l = arr.Length; i < l; i++)
                {
                    var el = JSONStr(aExecutionContext, aStack, aGap, aIndent, aReplacerFunction, aProplist, lObj, i.ToString());
                    if (el == null) el = "null";
                    if (i == 0)
                    {
                        if (aGap == "")
                            lWork.Append("[");
                        else
                        {
                            lWork.Append("[\n");
                            lWork.Append(aIndent);
                        }
                    }
                    lWork.Append(el);
                    if (i == arr.Length - 1)
                    {
                        if (aGap == "")
                        {
                            lWork.Append("]");
                        }
                        else
                        {
                            lWork.Append("\n");
                            lWork.Append(aIndent.Substring(0, aIndent.Length - aGap.Length));
                            lWork.Append("]");
                        }
                    }
                    else
                    {
                        if (aGap == "") lWork.Append(",");
                        else
                        {
                            lWork.Append(",\n");
                            lWork.Append(aIndent);
                        }
                    }
                }
            }
            else
            {
                aIndent = aIndent + aGap;
                var k = aProplist?.ToArray();
                if (k == null || k.Length == 0)
                {
                    if (lObj is EcmaScriptObjectWrapper)
                        k = ((EcmaScriptObjectWrapper)lObj).GetOwnNames().ToArray();
                    else
                        k = lObj.Values.Where(a => (PropertyAttributes.Enumerable & a.Value.Attributes) != 0)
                            .Select(a => a.Key)
                            .ToArray();
                }

                var lItems = new List<String>();
                if (null != k)
                {
                    foreach (var el in k)
                    {
                        var lVal = JSONStr(aExecutionContext, aStack, aGap, aIndent, aReplacerFunction, aProplist, lObj, el);
                        if (lVal != null)
                        {
                            if (aGap == "")
                                lItems.Add(JSON.QuoteString(el) + ":" + lVal);
                            else
                                lItems.Add(JSON.QuoteString(el) + ": " + lVal);
                        }
                    }
                }

                if (lItems.Count == 0)
                    lWork.Append("{}");
                else
                {
                    if (aGap == "")
                    {
                        lWork.Append("{");
                        for (int i = 0, l = lItems.Count; i < l; i++)
                        {
                            if (i != 0) lWork.Append(",");
                            lWork.Append(lItems[i]);
                        }
                        lWork.Append("}");
                    }
                    else
                    {
                        lWork.Append("{\n");
                        lWork.Append(aIndent);
                        for (int i = 0, l = lItems.Count; i < l; i++)
                        {
                            if (i != 0)
                            {
                                lWork.Append(",\n");
                                lWork.Append(aIndent);
                            }
                            lWork.Append(lItems[i]);
                        }
                        lWork.Append("\n");
                        lWork.Append(aIndent.Substring(0, aIndent.Length - aGap.Length));
                        lWork.Append("}");
                    }
                }
            }
            aStack.Remove(lObj);

            return (lWork.ToString());
        }
    }
}