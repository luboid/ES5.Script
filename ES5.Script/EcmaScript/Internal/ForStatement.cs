using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class ForStatement : IterationStatement
    {
        Statement fBody;
        ExpressionElement fIncrement;
        ExpressionElement fComparison;
        ExpressionElement fInitializer;
        List<VariableDeclaration> fInitializers;

        public ForStatement(PositionPair aPositionPair, ExpressionElement aInitializer, ExpressionElement aComparison, ExpressionElement aIncrement, Statement aBody)
        : base(aPositionPair)
        {
            fBody = aBody;
            fComparison = aComparison;
            fIncrement = aIncrement;
            fInitializer = aInitializer;
        }

        public ForStatement(PositionPair aPositionPair, VariableDeclaration[] aInitializers, ExpressionElement aComparison, ExpressionElement aIncrement, Statement aBody)
            : base(aPositionPair)
        {
            fBody = aBody;
            fComparison = aComparison;
            fIncrement = aIncrement;
            fInitializers = new List<VariableDeclaration>(aInitializers);
        }

        public ForStatement(PositionPair aPositionPair, IEnumerable<VariableDeclaration> aInitializers, ExpressionElement aComparison, ExpressionElement aIncrement, Statement aBody) :
            base(aPositionPair)
        {
            fBody = aBody;
            fComparison = aComparison;
            fIncrement = aIncrement;
            fInitializers = new List<VariableDeclaration>(aInitializers);
        }

        public ForStatement(PositionPair aPositionPair, List<VariableDeclaration> aInitializers, ExpressionElement aComparison, ExpressionElement aIncrement, Statement aBody) :
            base(aPositionPair)
        {
            fBody = aBody;
            fComparison = aComparison;
            fIncrement = aIncrement;
            fInitializers = aInitializers;
        }

        public List<VariableDeclaration> Initializers
        {
            get
            {
                return fInitializers;
            }
        }

        public ExpressionElement Initializer
        {
            get
            {
                return fInitializer;
            }
        }

        public ExpressionElement Comparison
        {
            get
            {
                return fComparison;
            }
        }

        public ExpressionElement Increment
        {
            get
            {
                return fIncrement;
            }
        }

        public Statement Body
        {
            get
            {
                return fBody;
            }
        }

        public override ElementType Type { get { return ElementType.ForStatement; } }
    }
}