using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class ThrowStatement : Statement
    {
        ExpressionElement fExpression;

        public ExpressionElement ExpressionElement { get { return fExpression; } }
        public override ElementType Type { get { return ElementType.ThrowStatement; } }

        public ThrowStatement(PositionPair aPositionPair, ExpressionElement anExpression)
            : base(aPositionPair)
        {
            fExpression = anExpression;
        }
    }
}
