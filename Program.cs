using static System.Console;
using System;
using System.IO;
using Kiogas;
using System.Collections.Generic;

class Program
{
    static void Main(string[] argv)
    {
        if (argv.Length < 1)
        {
            WriteLine("parser.huh: uh... what am i supposed to parse? please provide a .kyo file as an argument.");
            return;
        }

        Parser parser = new Parser();
        string file = "";
        // Kind of redundant, but the check above may not catch this
        try
        {
            file = argv[0];
        }
        catch (IndexOutOfRangeException)
        {
            WriteLine("parser.huh: uh... what am i supposed to parse? please provide a .kyo file as an argument.");
            return;
        }

        if (File.Exists(file))
        {
            // parser.parse(file);
            Lexer lexer = new Lexer();
            List<Lexer.Token> toks = lexer.Read(file);

            foreach (var tok in toks)
            {
                tok.Print(lexer.toktypes);
                WriteLine("");
            }
        }
        else
        {
            throw new Exception("external.fileSys: The file you passed does not exist in this context.");
        }

        if (!file.Contains("."))
        {
            WriteLine("external.fileSys.extension.missing: No file extension found");
            return;
        }
        else if (!file.EndsWith(".kyo"))
        {
            string[] temp = file.Split(".");
            WriteLine($"external.fileSys.extension.incorrect: Expected .kyo extension, got .{temp[1]}");
            return;
        }
    }
}
