using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public class EcmaScriptException : ScriptParsingException
    {
        public EcmaScriptException(string aFilename, PositionPair aPosition, EcmaScriptErrorKind anError, string aMsg = "") :
            base(aFilename, aPosition, anError, aMsg)
        { }
    }
}
