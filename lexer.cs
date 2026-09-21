#nullable enable

using System.Runtime.CompilerServices;
using System.IO;
using System.Text;

using System.Collections.Generic;
using static System.Console;
using System.Linq;

namespace Kiogas;

public class Lexer
{
    public readonly Dictionary<string, TokenType> toktypes;
    private int errors;

    public enum TokenType
    {
        // Generic compiler stuff
        STR_LIT,
        LIT,
        NUM_LIT,
        FLOAT_LIT,

        // Symbols
        LESS_THAN,
        GREATER_THAN,
        MINUS,
        EQUALS,

        ARROW_LEFT,
        ARROW_RIGHT,

        DARROW_LEFT,
        DARROW_RIGHT,
        COLON,

        // Types
        BOOL,
        INT,
        FLT,
        STR,
        UINT,
        BYTE,
        ARR,
        OBJ,

        SHORT,
        USHOT,
        SBYTE,
        LONG,
        ULONG
    }

    public readonly struct Token
    {
        public readonly TokenType type;
        public readonly Val       value;

        public readonly struct Val
        {
            public readonly int?    i;
            public readonly string? s;
            public readonly float?  f;
            public readonly uint?   u;
            public readonly byte?   by;
            public readonly bool?   b;
            public readonly short?  sh;
            public readonly ushort? ush;
            public readonly sbyte?  sby;
            public readonly long?   l;
            public readonly ulong?  ul;

            public Val
            (
                int?    i   = null, 
                string? s   = null,
                float?  f   = null,
                uint?   u   = null,
                byte?   by  = null,
                bool?   b   = null,
                short?  sh  = null,
                ushort? ush = null,
                sbyte?  sby = null,
                long?   l   = null,
                ulong?  ul  = null
            )
            {
                this.i  = i;
                this.s  = s;
                this.f  = f;
                this.u  = u;
                this.by = by;
                this.b  = b;

                this.sh  = sh;
                this.ush = ush;
                this.sby = sby;
                this.l   = l;
                this.ul  = ul;
            }
        }

        public Token
        (
            TokenType type, 
            int?      i   = null, 
            string?   s   = null,
            float?    f   = null,
            uint?     u   = null,
            byte?     by  = null,
            bool?     b   = null,
            short?    sh  = null,
            ushort?   ush = null,
            sbyte?    sby = null,
            long?     l   = null,
            ulong?    ul  = null
        )
        {
            this.type = type;
            value = new(i, s, f, u, by, b, sh, ush, sby, l, ul);
        }

        public void Print(Dictionary<string, TokenType> toktypes)
        {
            TokenType t = type;
            Write("type: ");
            if (t == TokenType.STR_LIT) Write("<str lit>");
            if (t == TokenType.NUM_LIT) Write("<num lit>");
            if (t == TokenType.FLOAT_LIT) Write("<float lit>");
            if (t == TokenType.LIT) Write("<lit>");

            Write(toktypes.FirstOrDefault(x => x.Value == t).Key + "\n");

            if (value.s != null) WriteLine($"Str: \"{value.s}\"");
            if (value.b.HasValue) WriteLine($"Bool: {value.b}");
            if (value.by.HasValue) WriteLine($"Byte: {value.by}");
            if (value.f.HasValue) WriteLine($"Float: {value.f}");
            if (value.i.HasValue) WriteLine($"Int: {value.i}");
            if (value.u.HasValue) WriteLine($"Uint: {value.u}");
            if (value.sh.HasValue) WriteLine($"Short: {value.sh}");
            if (value.ush.HasValue) WriteLine($"Ushort: {value.ush}");
            if (value.sby.HasValue) WriteLine($"Sbyte: {value.sby}");
            if (value.l.HasValue) WriteLine($"Long: {value.l}");
            if (value.ul.HasValue) WriteLine($"Ulong: {value.ul}");
        }
    }

    public Lexer()
    {
        toktypes = new Dictionary<string, TokenType>
        {
            // Types
            ["bool"] = TokenType.BOOL,
            ["int"]  = TokenType.INT,
            ["flt"]  = TokenType.FLT,
            ["str"]  = TokenType.STR,
            ["uint"] = TokenType.UINT,
            ["byte"] = TokenType.BYTE,
            ["arr"]  = TokenType.ARR,
            ["obj"]  = TokenType.OBJ,

            ["short"] = TokenType.SHORT,
            ["ushot"] = TokenType.USHOT,
            ["sbyte"] = TokenType.SBYTE,
            ["long"]  = TokenType.LONG,
            ["ulong"] = TokenType.ULONG,

            // Symbols
            ["->"]    = TokenType.ARROW_RIGHT,
            ["<-"]    = TokenType.ARROW_LEFT,
            ["==>"]   = TokenType.DARROW_RIGHT,
            ["<=="]   = TokenType.DARROW_LEFT,

            ["<"]     = TokenType.LESS_THAN,
            [">"]     = TokenType.GREATER_THAN,
            ["-"]     = TokenType.MINUS,
            ["="]     = TokenType.EQUALS,
            [":"]     = TokenType.COLON
        };

        errors = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsEof(char c) => c == '\0';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsSymbol(char c) => c == '-' || c == '=' || c == '<' || c == '>' || c == ':';
    
    public List<Token> Read(string fpath)
    {
        string ctx = File.ReadAllText(fpath);
        ctx += '\0';

        int i = 0;
        char c = ctx[i];

        StringBuilder buffer = new();
        List<Token> v = new();

        while (!IsEof(c))
        {
            if (char.IsLetter(c))
            {
                while (char.IsLetter(c))
                {
                    buffer.Append(c);

                    i++;
                    c = ctx[i];
                }

                TokenType t;
                if (toktypes.TryGetValue(buffer.ToString(), out t))
                {
                    // It's a type
                    Token tok = new(t);
                    v.Add(tok);

                    buffer.Clear();
                }
                else
                {
                    // It's obviously a literal then
                    Token tok = new(TokenType.LIT, s: buffer.ToString());
                    v.Add(tok);

                    buffer.Clear();
                }
            }
            else if (char.IsDigit(c))
            {
                // TODO
            }
            else if (char.IsWhiteSpace(c))
            {
                i++;
                c = ctx[i];
            }
            else if (IsSymbol(c))
            {
                while (IsSymbol(c))
                {
                    buffer.Append(c);

                    i++;
                    c = ctx[i];
                }

                TokenType t;
                if (toktypes.TryGetValue(buffer.ToString(), out t))
                {
                    Token tok = new(t);

                    v.Add(tok);
                    buffer.Clear();
                }
                else
                {
                    WriteLine($"lexer.unknown: Unknown symbol \"{buffer}\"");
                    errors++;
                }
                
                // TODO
            }
            else
            {
                WriteLine($"lexer.unknown: Unknown character {c}");
                errors++;

                i++;
                c = ctx[i];
            }
        }

        return v;
    }
}