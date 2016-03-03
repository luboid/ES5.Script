using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class WithStatement : Statement
    {
        Statement fBody;
        ExpressionElement fExpression;

        public WithStatement(PositionPair aPositionPair, ExpressionElement aExpression, Statement aBody)
            : base(aPositionPair)
        {
            fExpression = aExpression;
            fBody = aBody;
        }

        public ExpressionElement ExpressionElement { get { return fExpression; } }
        public Statement Body { get { return fBody; } }
        public override ElementType Type { get { return ElementType.WithStatement; } }

    }
}