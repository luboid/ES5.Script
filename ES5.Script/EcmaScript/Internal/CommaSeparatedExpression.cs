using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class CommaSeparatedExpression : ExpressionElement
    {
        List<ExpressionElement> fParameters;

        public CommaSeparatedExpression(PositionPair aPositionPair, params ExpressionElement[] aParameters)
            : base(aPositionPair)
        {
            fParameters = new List<ExpressionElement>(aParameters);
        }

        public CommaSeparatedExpression(PositionPair aPositionPair, IEnumerable<ExpressionElement> aParameters)
            : base(aPositionPair)
        {
            fParameters = new List<ExpressionElement>(aParameters);
        }

        public CommaSeparatedExpression(PositionPair aPositionPair, List<ExpressionElement> aParameters)
            : base(aPositionPair)
        {
            fParameters = aParameters;
        }

        public List<ExpressionElement> Parameters { get { return fParameters; } }
        public override ElementType Type { get { return ElementType.CommaSeparatedExpression; } }

    }
}
