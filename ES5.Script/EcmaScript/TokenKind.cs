using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript
{
    /// <summary>contains all tokens the tokenizersupports</summary>
    public enum TokenKind
    {
        /// <summary>
        /// end of file
        /// </summary>
        EOF,
        /// <summary>An enter</summary>
        LineFeed,
        /// <summary>
        /// tab, space, enter
        /// </summary>
        Whitespace,
        /// <summary>Linecomment</summary>
        LineComment,
        // internal token, will never be returned
        /// <summary>
        /// comments
        /// </summary>
        MultilineComment,
        /// <summary>A comment without any enter in it</summary>
        Comment,
        // internal token, will never be returned
        /// <summary>
        /// error token
        /// </summary>

        Error,
        /// <summary>
        /// identifier
        /// </summary>

        Identifier,
        /// <summary>
        /// hex number // 0x123abcdefABCDEF
        /// </summary>
        HexNumber,

        /// <summary>
        /// regular number // 0123456789
        /// </summary>
        Number,

        /// <summary>
        /// a String
        /// </summary>
        String,
        SingleQuoteString,

        /// <summary>
        /// Float // 0.2394834
        /// </summary>
        Float,

        /// <summary>
        /// colon
        /// </summary>
        Colon,

        /// <summary>
        /// comma
        /// </summary>
        Comma,

        /// <summary>
        /// semicolon
        /// </summary>    
        Semicolon,

        /// <summary>
        /// dot
        /// </summary>
        Dot,

        /// <summary>
        /// equal
        /// </summary>
        Equal,

        /// <summary>
        /// not equal
        /// </summary>
        NotEqual,

        /// <summary>
        /// assign (=)
        /// </summary>
        Assign,

        /// <summary>
        /// less
        /// </summary>
        Less,

        /// <summary>
        /// greater
        /// </summary>
        Greater,

        /// <summary>
        /// less or equal
        /// </summary>

        LessOrEqual,

        /// <summary>
        /// greater or equal
        /// </summary>
        GreaterOrEqual,

        /// <summary>
        /// star *
        /// </summary>
        Multiply,

        /// <summary>
        /// curly open
        /// </summary>
        CurlyOpen,

        /// <summary>
        /// curly close
        /// </summary>
        CurlyClose,

        /// <summary>
        /// opening parenthesis
        /// </summary>
        OpeningParenthesis,

        /// <summary>
        /// closing parenthesis
        /// </summary>
        ClosingParenthesis,

        /// <summary>
        /// opening bracket
        /// </summary>
        OpeningBracket,

        /// <summary>
        /// closing bracket
        /// </summary>
        ClosingBracket,

        /// <summary>
        /// a divide operator /
        /// </summary>
        Divide,

        /// <summary>
        /// a modulus % sign
        /// </summary>
        Modulus,

        /// <summary>
        /// an and sign &amp;
        /// </summary>
        And,

        /// <summary>
        /// an or sign
        /// </summary>
        Or,

        /// <summary>
        /// &amp;&amp; logical and
        /// </summary>
        DoubleAnd,

        /// <summary>
        /// || logical or
        /// </summary>
        DoubleOr,

        /// <summary>
        /// a caret ^ (xor)
        /// </summary>
        Xor,

        /// <summary>
        /// ^^ logical xor
        /// </summary>
        DoubleXor,

        /// <summary>
        /// Tilde ~ (not)
        /// </summary>
        BitwiseNot,

        /// <summary>
        /// plus +
        /// </summary>
        Plus,

        /// <summary>
        /// minus -
        /// </summary>
        Minus,

        /// <summary>
        /// logical not !
        /// </summary>
        Not,


        /// <summary> &lt;&lt; </summary>
        ShiftLeft, // <<
                   /// <summary>&gt;&gt;</summary>
        ShiftRightSigned, // >>
                          /// <summary>&gt;&gt;&gt;</summary>
        ShiftRightUnsigned, // >>>
                            /// <summary>++</summary>
        Increment, // ++
                   /// <summary>--</summary>
        Decrement, // --
                   /// <summary>===</summary>
        StrictEqual, // ===
                     /// <summary>!==</summary>
        StrictNotEqual, // !==
                        /// <summary>?</summary>
        ConditionalOperator, // ?
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



        // Following are all Keywords
        K_break,
        K_do,
        K_instanceof,
        K_typeof,
        K_case,
        K_else,
        K_new,
        K_var,
        K_catch,
        K_finally,
        K_return,
        K_void,
        K_continue,
        K_for,
        K_switch,
        K_while,
        K_debugger,
        K_function,
        K_this,
        K_with,
        K_default,
        K_if,
        K_throw,
        K_delete,
        K_in,
        K_try,
        K_null,
        K_true,
        K_false,
        K_get,
        K_set
    }
}