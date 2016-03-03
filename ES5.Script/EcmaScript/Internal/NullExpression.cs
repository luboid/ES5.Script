using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class NullExpression : LiteralExpression
    {
        public NullExpression(PositionPair aPositionPair) : base(aPositionPair) { }

        public override object ObjectValue
        {
            get
            {
                return null;
            }
        }

        public override ElementType Type
        {
            get
            {
                return ElementType.NullExpression;
            }
        }
    }
}
