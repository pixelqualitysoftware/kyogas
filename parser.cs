using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils;
using static System.Console;
using Kiogas;

public class Parser
{
    private const Lexer L = new Lexer();
    private List<Token> tokens;
    private int pos = 0;
    private Dictionary<string, Data> data = new();
    private List<string> names = new();
    private string path; 
    private string ctx;
    public Parser(string path) { this.path = path; this.tokens = Read(this.path); this.ctx = ReadWithNewlines(this.path); }

    public void parsePRIM(TokenType tt, TokenType lit, string type) {
        // blueprint: Type whitespace COLON whitespace* LIT
        Token t = curr();
        if (check(tt)) {
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
            // if (check())
        }
    }
    public void parseBOOL() {
        Token t = curr();
        while(!check(TokenType.EOL))
        if (check(TokenType.BOOL)) {
            if (check(TokenType))
            if (check(TokenType.COLON)) t = move();
            else {
                printERR($"expected ':', got {t}", 
                    this.path, 
                    curr().line, 
                    curr().col, 
                    22); 
                return;
            }
            t.Value.b = move().Value; // I have zero clue if this works 
        }
    }
}
