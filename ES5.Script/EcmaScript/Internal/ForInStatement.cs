using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class ForInStatement : IterationStatement
    {
        Statement fBody;
        VariableDeclaration fInitializer;
        ExpressionElement fLeftExpression;
        ExpressionElement fExpression;

        public ForInStatement(PositionPair aPositionPair, ExpressionElement aLeftExpression, ExpressionElement anExpression, Statement aBody)
            : base(aPositionPair)
        {
            fBody = aBody;
            fExpression = anExpression;
            fLeftExpression = aLeftExpression;
        }

        public ForInStatement(PositionPair aPositionPair, VariableDeclaration anInitializer, ExpressionElement anExpression, Statement aBody)
            : base(aPositionPair)
        {
            fBody = aBody;
            fExpression = anExpression;
            fInitializer = anInitializer;
        }

        public ExpressionElement LeftExpression { get { return fLeftExpression; } }
        public ExpressionElement ExpressionElement { get { return fExpression; } }

        public VariableDeclaration Initializer
        {
            get
            {
                return fInitializer;
            }
        }

        public Statement Body
        {
            get
            {
                return fBody;
            }
        }

        public override ElementType Type { get { return ElementType.ForInStatement; } }
    }
}