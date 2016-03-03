using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class FunctionExpression : ExpressionElement
    {
        FunctionDeclarationElement fFunction;

        public FunctionExpression(PositionPair aPositionPair, FunctionDeclarationElement aFunction)
            : base(aPositionPair)
        {
            fFunction = aFunction;
        }

        public FunctionDeclarationElement Function { get { return fFunction; } }
        public override ElementType Type { get { return ElementType.FunctionExpression; } }
    }
}
