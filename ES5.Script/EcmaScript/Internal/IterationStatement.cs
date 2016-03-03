using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public abstract class IterationStatement : Statement
    {
        public Label? Break { get; set; }
        public Label? Continue { get; set; }

        public IterationStatement(PositionPair aPositionPair) : base(aPositionPair) { }
    }
}
