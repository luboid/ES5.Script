using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class BinaryExpression : ExpressionElement
    {
        ExpressionElement fRightSide;
        ExpressionElement fLeftSide;
        BinaryOperator fOperator;

        public BinaryExpression(PositionPair aPositionPair, ExpressionElement aLeft, ExpressionElement aRight, BinaryOperator anOperator)
            : base(aPositionPair)
        {
            fLeftSide = aLeft;
            fRightSide = aRight;
            fOperator = anOperator;
        }

        public ExpressionElement LeftSide { get { return fLeftSide; } }
        public ExpressionElement RightSide { get { return fRightSide; } }
        public BinaryOperator Operator { get { return fOperator; } }
        public override ElementType Type { get { return ElementType.BinaryExpression; } }
    }
}