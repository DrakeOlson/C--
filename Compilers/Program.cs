﻿/*
 * Author: Drake Olson
 * Class: Compiler Construction
 * Instructor: Dr. Hamer
 * Date: 1/30/18
 * Description: Using techiniques of current compilers, this program uses the lexical
 *              analyzer to start off process for a subset of the C++ language C--. 
 *              This compiler will be built upon.
 */
using System;

namespace Compiler
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            SymbolTable testTable = new SymbolTable();
            if (args.Length == 0)
            {
                Console.WriteLine($"Usage:{args[0]} filename.");
                return;
            }
            else
            {
                Parser p = new Parser(args[0]);
                p.Run();
                if (Globals.Token == Globals.Symbol.eoft)
                {
                    Console.WriteLine("Successful Build");
                    CodeGenerator codeGenerator = new CodeGenerator(p.outputtedFileName,p.symbolTable);
                    codeGenerator.Run();
                }
            }
        }
    }
}
