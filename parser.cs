using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils;
using static System.Console;
using Kiogas;

    /*private bool hasERRORS = false;
    private bool inArr = false; // currently inside of an array
    private bool inObj = false; // currently inside of an object
    private ushort errorCOUNT = 0;*/
public class Parser
{
    private const Lexer L = new Lexer();
    private List<Token> tokens;
    private int pos = 0;
    private Dictionary<string, Data> data = new();
    private List<string> names = new();
    private string path; 

    private TokenType peek() => tokens[pos+1];
    private TokenType curr() => tokens[pos];
    private Token move() {
        Token t = tokens[pos];
        if (pos < tokens.Count - 1) pos++;
        return t;
    }
    private bool check(TokenType tt) => peek() == tt;
    private bool Match(TokenType tt) {
        if (check(ty)) {move(); return true; }
        return false;
    }
    private Token exp(TokenType tt) {
        if (check(tt)) return move();
        throw new Exception($"Expected {tt}, but found {curr().type} at {curr().line}:{curr().column}");
    }
    public Parser(string path) { this.path = path; this.tokens = Read(this.path); this.ctx = ReadWithNewlines(this.path); }

    public void parsePRIM(TokenType tt, TokenType lit, string type) {
        // blueprint: Type whitespace COLON whitespace* LIT
        Token t = curr();
        if (check(tt)) {
            //while (!check(TokenType.COLON)) t = move();
            if (check(TokenType.EOF)) printERR("expected a literal, got <EOF> (End Of File)", 
                this.path,
                curr().line,
                curr().col,
                23
            );
            if (Match(TokenType.COLON))  
            else {
                printERR($"expected ':', got {t}", this.path, curr().line, curr().col, 22);
                return;
            }
            if (!check(lit)) printERR($"expected value of type {lit}, got {type}.", 
                this.path,
                curr().line,
                curr().col,
                21
            );
        }
    }
    public void parseBOOL() {
        Token t = curr();
        while(!check(TokenType.EOL))
        if (check(TokenType.BOOL)) {
            if (check(TokenType))
            if (check(TokenType.COLON)) t = move();
            else {
                printERR($"expected ':', got {t}", this.path, this.line, this.col, 22); 
                return;
            }
            if (check(TokenType.LIT))
        }
    }
}
