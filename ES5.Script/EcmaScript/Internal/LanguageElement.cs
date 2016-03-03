using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public abstract class LanguageElement
    {
        PositionPair fPositionPair;

        public PositionPair PositionPair { get { return fPositionPair; } }
        public abstract ElementType Type { get; }

        public LanguageElement(PositionPair aPositionPair)
        {
            fPositionPair = aPositionPair;
        }
    }
}
