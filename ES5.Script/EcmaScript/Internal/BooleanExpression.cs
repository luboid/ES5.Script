using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class BooleanExpression : LiteralExpression
    {
        Boolean fValue;

        public BooleanExpression(PositionPair aPositionPair, Boolean aValue) : 
            base(aPositionPair) 
        {
            fValue = aValue;
        }

        public bool Value
        {
            get
            {
                return fValue;
            }
        }

        public override object ObjectValue
        {
            get
            {
                return fValue;
            }
        }

        public override ElementType Type
        {
            get
            {
                return ElementType.BooleanExpression;
            }
        }
    }
}
