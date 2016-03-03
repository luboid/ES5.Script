using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script
{
    public class SyntaxErrorException : Exception
    {
        public SyntaxErrorException(string aSource, string aMessage, PositionPair aSpan, int anErrorCode)
            : base(String.Format("{0}({1}, {2}): {4} {3}",
                aSource, aSpan.StartRow, aSpan.StartCol, aMessage,
                "error"))
        {
            Source = aSource;
            Span = aSpan;
            ErrorCode = anErrorCode;
            Message = aMessage;
        }

        public new string Message { get; private set; }
#if SILVERLIGHT
        public string Source { get; private set; }
#else
        public new string Source { get; private set; }
#endif
        public string SourceFilename { get { return Span.File; } }
        public PositionPair Span { get; private set; }
        public int ErrorCode{ get; private set; }


    public override string ToString()
        {
            return base.Message;
        }

    }
}
