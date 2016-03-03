using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class ArrayLiteralExpression : ExpressionElement
    {
        List<ExpressionElement> fItems;

        public ArrayLiteralExpression(PositionPair aPositionPair, params ExpressionElement[] aItems)
            : base(aPositionPair)
        {
            fItems = new List<ExpressionElement>(aItems);
        }

        public ArrayLiteralExpression(PositionPair aPositionPair, IEnumerable<ExpressionElement> aItems)
            : base(aPositionPair)
        {
            fItems = new List<ExpressionElement>(aItems);
        }

        public ArrayLiteralExpression(PositionPair aPositionPair, List<ExpressionElement> aItems)
            : base(aPositionPair)
        {
            fItems = aItems;
        }

        public List<ExpressionElement> Items { get { return fItems; } }
        public override ElementType Type { get { return ElementType.ArrayLiteralExpression; } }
    }
}
