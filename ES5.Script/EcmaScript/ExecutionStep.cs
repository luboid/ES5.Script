using ES5.Script.EcmaScript.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public sealed class ExecutionStep
    {
        public ExpressionElement Expression{get;set;}
        public int Step { get; set; }

        public ExecutionStep(ExpressionElement expression, int step)
        {
            Expression = expression;
            Step = step;
        }

        public ExecutionStep(ExpressionElement expression):this(expression,0)
        {}

        public ExecutionStep NextStep()
        {
            ++Step;
            return this;
        }
    }
}
