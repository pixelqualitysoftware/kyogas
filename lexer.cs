#nullable enable

using System.Runtime.CompilerServices;
using System.IO;
using System.Text;

using System.Collections.Generic;
using static System.Console;
using System.Linq;

using Kiogas;
using System;


public class Lexer
{
    public readonly Dictionary<string, TokenType> toktypes;
    private uint errors;

    public enum TokenType {
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

        public readonly uint      line;
        public readonly uint      column;

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
            uint      line,
            uint      column,

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
            this.line = line;
            this.column = column;
            value = new(i, s, f, u, by, b, sh, ush, sby, l, ul);
        }

        public void Print(Dictionary<string, TokenType> toktypes)
        {
            TokenType t = type;
            Write("Type: ");
            if (t == TokenType.STR_LIT) Write("<str lit>");
            if (t == TokenType.NUM_LIT) Write("<num lit>");
            if (t == TokenType.FLOAT_LIT) Write("<float lit>");
            if (t == TokenType.LIT) Write("<lit>");

            Write($" at {line}:{column}");

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

    public string ReadWithNewlines(string path)
    {
        string[] ctx = File.ReadAllLines(path);

        for (int i = 0; i < ctx.Length; i++)
        {
            ctx[i] += '\n';
        }

        StringBuilder str = new();
        foreach (string s in ctx)
        {
            str.Append(s);
        }
        str.Append('\0');

        return str.ToString();
    }
    
    public List<Token> Read(string fpath)
    {
        string ctx = ReadWithNewlines(fpath);

        int i = 0;
        char c = ctx[i];

        StringBuilder buffer = new();
        List<Token> v = new();

        uint line = 1;
        uint column = 1;

        while (!IsEof(c))
        {
            if (char.IsLetter(c))
            {
                uint pos = column;
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
                    Token tok = new(t, pos, column);
                    v.Add(tok);

                    buffer.Clear();
                }
                else
                {
                    // It's obviously a literal then
                    Token tok = new(TokenType.LIT, pos, column, s: buffer.ToString());
                    v.Add(tok);

                    buffer.Clear();
                }
            }
            else if (c == '\n')
            {
                i++;
                c = ctx[i];

                line++;
                column = 1;
            }
            else if (char.IsDigit(c))
            {
                uint pos = column;
                while (char.IsDigit(c) || c == '.')
                {
                    buffer.Append(c);

                    i++;
                    c = ctx[i];

                    column++;
                }

                if (buffer.ToString().Contains('.')) 
                {
                    float? f = null;
                    try
                    {
                        f = float.Parse(buffer.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        WriteLine($"{fpath}:{line}:{pos}: lexer.float.formatting: Float is not formatted correctly");
                        errors++;
                    }
                    catch (OverflowException)
                    {
                        WriteLine($"{fpath}:{line}:{pos}: lexer.float.overflow: Float is overflowing/underflowing");
                        errors++;
                    }
                    finally
                    {
                        if (f != null)
                        {
                            Token tok = new(TokenType.FLOAT_LIT, line, pos, f: f);
                            v.Add(tok);

                            buffer.Clear();
                        }
                    }
                }
                else
                {
                    int? num = null;
                    try
                    {
                        num = int.Parse(buffer.ToString());
                    }
                    catch (FormatException)
                    {
                        WriteLine($"{fpath}:{line}:{pos}: lexer.num.formatting: Number is not formatted correctly");
                        errors++;
                    }
                    catch (OverflowException)
                    {
                        WriteLine($"{fpath}:{line}:{pos}: lexer.num.overflow: Number is overflowing/underflowing");
                        errors++;
                    }
                    finally
                    {
                        if (num != null)
                        {
                            Token tok = new(TokenType.NUM_LIT, line, pos, i: num);
                            v.Add(tok);

                            buffer.Clear();
                        }
                    }
                }
            }
            else if (char.IsWhiteSpace(c))
            {
                i++;
                c = ctx[i];

                column++;
            }
            else if (c == '"')
            {
                uint pos = column;

                // Handle the string
                i++;
                c = ctx[i];

                column++;

                while (c != '"')
                {
                    buffer.Append(c);

                    i++;
                    c = ctx[i];

                    column++;
                }

                // Advance because we're on char "
                i++;
                c = ctx[i];
                column++;

                Token tok = new(TokenType.STR_LIT, line, pos, s: buffer.ToString());
                v.Add(tok);

                buffer.Clear();
            }
            else if (IsSymbol(c))
            {
                uint pos = column;

                while (IsSymbol(c))
                {
                    buffer.Append(c);

                    i++;
                    c = ctx[i];

                    column++;
                }

                TokenType t;
                if (toktypes.TryGetValue(buffer.ToString(), out t))
                {
                    Token tok = new(t, line, pos);

                    v.Add(tok);
                    buffer.Clear();
                }
                else
                {
                    WriteLine($"{fpath}:{line}:{pos}: lexer.unknown.symbol: Unknown symbol \"{buffer}\"");
                    errors++;
                }
            }
            else
            {
                WriteLine($"{fpath}:{line}:{column}: lexer.unknown.char: Unknown character {c}");
                errors++;

                i++;
                c = ctx[i];

                column++;
            }
        }

        return v;
    }
}