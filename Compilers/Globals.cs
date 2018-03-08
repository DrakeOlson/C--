/*
 * Author: Drake Olson
 * Class: Compiler Construction
 * Instructor: Dr. Hamer
 * Date: 1/30/18
 * Description: This file holds global variables that will be used in the compiler.
 */

using System;
using System.Collections.Generic;
namespace Compiler
{
    public static class Globals
    {
        public enum Symbol { unknownT, idT, ifT, elseT, whileT, floatT, intT, charT, breakT, continueT, voidT,
                             lparenT, rparenT, lbraceT, rbraceT, lbracketT, rbracketT, commaT, periodT,
                             semicolonT, singlequoteT, assignopT, addopT, mulopT, relopT, literalT, eoft}; 
        public static Dictionary<string,Symbol> SymbolDictionary = new Dictionary<string, Symbol>();
        public static Symbol Token = Globals.Symbol.unknownT;
        public static string Lexeme = String.Empty;
        public static string Attribute = String.Empty;
        public static char? currentChar = null;
        public static int LineNumber = 0;
        public static int? Value = null;
        public static float? ValueReal = null;
    }
}
