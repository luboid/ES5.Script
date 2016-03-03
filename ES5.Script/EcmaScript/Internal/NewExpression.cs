using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class NewExpression : CallExpression
    {
        public NewExpression(PositionPair aPositionPair, ExpressionElement aMember, params ExpressionElement[] aParameters)
            : base(aPositionPair, aMember, aParameters)
        { }

        public NewExpression(PositionPair aPositionPair, ExpressionElement aMember, IEnumerable<ExpressionElement> aParameters)
            : base(aPositionPair, aMember, aParameters)
        { }

        public NewExpression(PositionPair aPositionPair, ExpressionElement aMember, List<ExpressionElement> aParameters)
            : base(aPositionPair, aMember, aParameters)
        { }

        public override ElementType Type { get { return ElementType.NewExpression; } }
    }
}
