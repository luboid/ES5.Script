using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class ExpressionStatement : Statement
    {
        ExpressionElement fExpression;

        public ExpressionStatement(PositionPair aPositionPair, ExpressionElement anExpression)
            : base(aPositionPair)
        {
            fExpression = anExpression;
        }

        public ExpressionElement ExpressionElement { get { return fExpression; } }
        public override ElementType Type { get { return ElementType.ExpressionStatement; } }

    }
}
