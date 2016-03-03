using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script
{
    public abstract class ParserMessage
    {
        Position fPosition;

        public ParserMessage(Position aPosition)
        {
            fPosition = aPosition;
        }

        public abstract bool IsError { get; }
        public abstract int Code { get; }

        public Position Position
        {
            get
            {
                return fPosition;
            }
        }

        public abstract string IntToString();

        public override string ToString()
        {
            return string.Format("{0}({1}:{2}): {3}", fPosition.Module, fPosition.Row, fPosition.Col, IntToString());
        }
    }
}