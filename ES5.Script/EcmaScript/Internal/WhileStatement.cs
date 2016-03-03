using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class WhileStatement : IterationStatement
    {
        ExpressionElement fExpression;
        Statement fBody;

        public WhileStatement(PositionPair aPositionPair, ExpressionElement anExpression, Statement aBody)
            : base(aPositionPair)
        {
            fBody = aBody;
            fExpression = anExpression;
        }

        public Statement Body { get { return fBody; } }
        public ExpressionElement ExpressionElement { get { return fExpression; } }
        public override ElementType Type { get { return ElementType.WhileStatement; } }
    }
}