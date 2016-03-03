using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public abstract class PropertyBaseExpression : ExpressionElement
    {
        public PropertyBaseExpression(PositionPair aPositionPair) : base(aPositionPair) { }
        public abstract object ObjectValue { get; }
    }
}
