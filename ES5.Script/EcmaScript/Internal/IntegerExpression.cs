using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class IntegerExpression : LiteralExpression
    {
        Int64 fValue;

        public IntegerExpression(PositionPair aPositionPair, Int64 aValue) : 
            base(aPositionPair) 
        {
            fValue = aValue;
        }

        public Int64 Value
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
                return ElementType.IntegerExpression;
            }
        }
    }
}
