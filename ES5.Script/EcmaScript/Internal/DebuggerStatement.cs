using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class DebuggerStatement : Statement
    {
        public DebuggerStatement(PositionPair aPositionPair) : base(aPositionPair) { }
        public override ElementType Type { get { return ElementType.DebuggerStatement; } }
    }
}
