using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class ParameterDeclaration : SourceElement
    {
        string fName;

        public ParameterDeclaration(PositionPair aPositionPair, string aName):
            base(aPositionPair)
        {
            fName = aName;
        }

        public string Name
        {
            get
            {
                return fName;
            }
        }

        public override ElementType Type
        {
            get
            {
                return ElementType.ParameterDeclaration;
            }
        }
    }
}
