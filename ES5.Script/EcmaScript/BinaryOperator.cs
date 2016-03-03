using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public enum BinaryOperator
    {
        Equal,
        NotEqual,
        Assign,
        Less,
        Greater,
        LessOrEqual,
        GreaterOrEqual,
        Multiply,
        Divide,
        Modulus,
        And,
        Or,
        DoubleAnd,
        DoubleOr,
        Xor,
        DoubleXor,
        BitwiseNot,
        Plus,
        Minus,
        ShiftLeft, // <<
        ShiftRightSigned, // >>
        ShiftRightUnsigned, // >>>
        StrictEqual, // ===
        StrictNotEqual, // !==
        PlusAssign, // +=
        MinusAssign,// -=
        MultiplyAssign, // *=
        ModulusAssign, // %=
        ShiftLeftAssign, // <<=
        ShiftRightSignedAssign,// >>=
        ShiftRightUnsignedAssign, // >>>=
        AndAssign, // &=
        OrAssign, // |=
        XorAssign, // ^=
        DivideAssign, // /=
        In,
        InstanceOf
    }
}