/*
 * Author: Drake Olson
 * Class: Compiler Construction
 * Instructor: Dr. Hamer
 * Date: 1/30/18
 * Description: The Lexer will go through a C-- file and tokenize its contents.
 *              After it will display them to the screen.
 */

using System;
using System.IO;
using System.Linq;

namespace Compiler
{
    public class Lexer
    {
        public string[] lines;
        private int currentPosition = 0;
        private bool stringLiteral = false;
        private bool isNegative = false;

        /// <summary>
        /// This constructor will see if there is a file existing and if so open and read all lines into an array
        /// </summary>
        /// <param name="filename"></param>
        public Lexer(string filename)
        {
            Setup();
            if (File.Exists(filename)) {
                lines = File.ReadAllLines(filename);
            }
            else {
                Console.WriteLine($"{filename} does not exist or not in the current directory.");
            }
        }
        /// <summary>
        /// GetNextToken will start he process of tokenizing each line and be an entry point in the lexer after creation
        /// </summary>
        public void GetNextToken()
        {
            if(Globals.LineNumber >= lines.Length)
            {
                Globals.Lexeme = String.Empty;
                Globals.Token = Globals.Symbol.eoft;
                Globals.Attribute = String.Empty;
                return;
            }
            if (String.IsNullOrWhiteSpace(lines[Globals.LineNumber]))
            {
                Globals.LineNumber++;
            }
            while (Globals.currentChar == ' ' || Globals.currentChar == '\t')
            {
                GetNextChar();
            }
            if (Globals.LineNumber < lines.Length)
            {
                Globals.Lexeme = String.Empty;
                Globals.Token = Globals.Symbol.unknownT;
                Globals.Attribute = String.Empty;
                if(currentPosition >= lines[Globals.LineNumber].Length){
                    currentPosition = 0;
                    Globals.LineNumber++;
                }
                if (currentPosition == 0 && Globals.LineNumber < lines.Length)
                {
                    Globals.currentChar = lines[Globals.LineNumber][currentPosition];
                    while (Globals.currentChar == ' ' || Globals.currentChar == '\t')
                    {
                        GetNextChar();
                    }
                }
                ProcessToken();
            }
            else
            {
                Globals.Token = Globals.Symbol.eoft;
                Globals.Lexeme = String.Empty;
                Globals.Attribute = String.Empty;
            }
        }

        /// <summary>
        /// ProcessToken will determine which type of token is currently being worked on and call the appropriate function.
        /// </summary>
        private void ProcessToken()
        {
            Globals.Lexeme += Globals.currentChar;
            GetNextChar();
            if (Char.IsLetter(Globals.Lexeme[0]))
            {
                ProcessWordToken();
            }
            else if (Char.IsNumber(Globals.Lexeme[0]) || (Globals.Lexeme[0] == '-' &&  Char.IsNumber(Globals.currentChar.GetValueOrDefault()))  || (Globals.Lexeme == "." && Char.IsNumber(lines[Globals.LineNumber][currentPosition])))
            {
                ProcessNumberToken();
            }
            else if (Globals.Lexeme[0] == '=' || Globals.Lexeme[0] == '!' || Globals.Lexeme[0] == '<' || Globals.Lexeme[0] == '>' || Globals.Lexeme[0] == '&' || Globals.Lexeme[0] == '|')
            {
                if (Globals.currentChar == '=' || Globals.currentChar == '|' || Globals.currentChar == '&' || Globals.currentChar == '>' || Globals.currentChar == '<')
                {
                    ProcessDoubleToken();
                }
                else
                {
                    ProcessSingleToken();
                }
            }
            else if(Globals.Lexeme[0] == '/' && Globals.currentChar == '/')
            {
                Globals.LineNumber++;
                currentPosition = 0;
                if(Globals.LineNumber < lines.Length)
                {
                    GetNextToken();
                }
                else
                {
                    Globals.Token = Globals.Symbol.eoft;
                    Globals.Lexeme = String.Empty;
                    Globals.Attribute = String.Empty;
                    return;
                }
            }
            else if(Globals.Lexeme[0] == '/' && Globals.currentChar == '*'){
                ProcessMultiLineComment();
                if (Globals.LineNumber < lines.Length)
                {
                    GetNextToken();
                }
                else
                {
                    Globals.Token = Globals.Symbol.eoft;
                    Globals.Lexeme = String.Empty;
                    Globals.Attribute = String.Empty;
                    return;
                }
            }
            else if(Globals.Lexeme.Equals("\""))
            {
                stringLiteral = true;
                ProcessStringLiteral();
            }
            else
            {
                ProcessSingleToken();
            }
        }
        /// <summary>
        /// ProcessDoubleToken will determine which relational operator is being used and set the Token value in Globals.
        /// </summary>
        private void ProcessDoubleToken()
        {
            Globals.Lexeme += Globals.currentChar;
            GetNextChar();
            switch (Globals.Lexeme)
            {
                case "==":
                case "!=":
                case "<=":
                case ">=":
                    {
                        Globals.Token = Globals.Symbol.relopT;
                        break;
                    }
                case "||":
                    {
                        Globals.Token = Globals.Symbol.addopT;
                        break;
                    }
                case "&&":
                    {
                        Globals.Token = Globals.Symbol.mulopT;
                        break;
                    }
                case ">>":
                    {
                        Globals.Token = Globals.Symbol.cinT;
                        break;
                    }
                case "<<":
                    {
                        Globals.Token = Globals.Symbol.coutT;
                        break;
                    }
            }
        }
        /// <summary>
        /// ProcesssingleToken will process any single character that is not a number or letter or underscore and attempt to match it to a reserved token/word
        /// </summary>
        private void ProcessSingleToken()
        {
            Globals.SymbolDictionary.TryGetValue(Globals.Lexeme, out Globals.Token);
        }
        /// <summary>
        /// ProcessStringLiteral will handel the process and stroing of a string literal.
        /// </summary>
        private void ProcessStringLiteral()
        {
            int currentLine = Globals.LineNumber;
            while (stringLiteral == true && currentLine == Globals.LineNumber)
            {
                Globals.Lexeme += Globals.currentChar;
                GetNextChar();
                if (Globals.currentChar == 34)
                {
                    Globals.Lexeme += "\"";
                    GetNextChar();
                    stringLiteral = false;
                    Globals.Attribute = Globals.Lexeme.Substring(1, Globals.Lexeme.Length - 2);
                    Globals.Token = Globals.Symbol.literalT;
                }
            }
        }
        /// <summary>
        /// ProcessMultiLineComment will treat multi-line comments as whitespace and go to the positon of the end of that comment block
        /// </summary>
        private void ProcessMultiLineComment()
        {
            while(Globals.LineNumber <= lines.Length){
                if (lines[Globals.LineNumber].Contains("*/"))
                {
                    currentPosition = lines[Globals.LineNumber].IndexOf("*/", StringComparison.Ordinal) + 2;
                    if(currentPosition >= lines[Globals.LineNumber].Length)
                    {
                        GetNextChar();
                    }
                    return;
                }
                else
                {
                    Globals.LineNumber++;
                }
            }
                
        }
        /// <summary>
        /// Will process any token that starts with a character. This will determine if its an identifier, reserved word, or literal.
        /// </summary>
        private void ProcessWordToken()
        {
            string value = String.Empty;
            while (Globals.currentChar != null && (Char.IsLetterOrDigit(lines[Globals.LineNumber][currentPosition]) || lines[Globals.LineNumber][currentPosition] == '_'))
            {
                if(Globals.Lexeme.Length > 27)
                {
                    while(Char.IsLetterOrDigit(lines[Globals.LineNumber][currentPosition]) || lines[Globals.LineNumber][currentPosition] == '_')
                    {
                        currentPosition++;

                        if(currentPosition < lines[Globals.LineNumber].Length)
                        {
                            Globals.Attribute += lines[Globals.LineNumber][currentPosition];
                        }
                        else{
                            break;
                        }
                    }
                    Console.WriteLine($"Error: Too long of a identifier (27 Characters). (Line:{Globals.LineNumber + 1})");
                    return;
                }
                else
                {
                    Globals.Lexeme += Globals.currentChar;
                    Globals.Attribute = Globals.Lexeme;
                    GetNextChar();
                }
            }
            if (Globals.SymbolDictionary.ContainsKey(Globals.Lexeme))
            {
                Globals.SymbolDictionary.TryGetValue(Globals.Lexeme, out Globals.Token);
                Globals.Attribute = String.Empty;
            }
            else if (stringLiteral == true)
            {
                while(currentPosition <= lines[Globals.LineNumber].Length && stringLiteral == true)
                {
                    Globals.Lexeme += Globals.currentChar;
                    GetNextChar();
                    if (Globals.currentChar.Equals("\"")){
                        stringLiteral = false;
                    }
                }
                Globals.Token = Globals.Symbol.literalT;
                Globals.Attribute = Globals.Lexeme;
            }
            else
            {
                Globals.Token = Globals.Symbol.idT;
                Globals.Attribute = String.Empty;
            }
        }
        /// <summary>
        /// ProcessNumberToken will tokenize all valid numbers either real or decimal.
        /// </summary>
        private void ProcessNumberToken()
        {
            bool decimalFlag = false;
            
            while (Globals.currentChar != null && (Char.IsDigit(lines[Globals.LineNumber][currentPosition]) || lines[Globals.LineNumber][currentPosition] == '.'))
            {
                Globals.Lexeme += Globals.currentChar;
                GetNextChar();
                if (Globals.Lexeme.EndsWith(".") && (currentPosition == lines[Globals.LineNumber].Length || !Char.IsNumber(lines[Globals.LineNumber][currentPosition])))
                {
                    Globals.Lexeme += Globals.currentChar;
                    Console.WriteLine($"Warning: Not a valid real number. (Line: {Globals.LineNumber})");
                    Globals.Token = Globals.Symbol.unknownT;
                    Globals.Attribute = String.Empty;
                    return;
                }
                else if ((Globals.Lexeme.EndsWith(".") && decimalFlag == false) || (Globals.Lexeme.StartsWith(".") && Globals.Lexeme.Count(c => c == '.') == 1))
                {
                    decimalFlag = true;
                }
                else if (Globals.Lexeme.Count(c => c == '.') > 1 && decimalFlag == true)
                {
                    Console.WriteLine($"Warning: Not a valid real number. (Line: {Globals.LineNumber})");
                    Globals.Token = Globals.Symbol.unknownT;
                    while (Globals.currentChar != null && (Char.IsDigit(lines[Globals.LineNumber][currentPosition]) || lines[Globals.LineNumber][currentPosition] == '.'))
                    {
                        Globals.Lexeme += Globals.currentChar;
                        GetNextChar();
                    }
                        return;
                }

            }
           if(decimalFlag == false)
            {
                Globals.Token = Globals.Symbol.intT;
                Globals.Value = Int32.Parse(Globals.Lexeme);
                Globals.Attribute = Globals.Value.ToString();
            }
            else
            {
                Globals.Token = Globals.Symbol.floatT;
                Globals.ValueReal = float.Parse(Globals.Lexeme);
                Globals.Attribute = Globals.ValueReal.ToString();
            }
        }
        /// <summary>
        /// GetNextChar put the next character to be see in the currentChar globals.
        /// </summary>
        private void GetNextChar()
        {
            currentPosition++;
            if (currentPosition >= lines[Globals.LineNumber].Length)
            {
                Globals.currentChar = null;
                Globals.LineNumber++;
                currentPosition = 0;
                return;
            }
            else
            {
                Globals.currentChar = lines[Globals.LineNumber][currentPosition];
            }

        }
        /// <summary>
        /// DisplayToken displays a token to the screen.
        /// </summary>
        public void DisplayToken(){
            Console.WriteLine($"{Globals.Lexeme,-30} {Globals.Token.ToString(),-13} {Globals.Attribute}");
        }

        /// <summary>
        /// Setup sets up the dictionary for lookup of reserved words/tokens with a symbol type.
        /// </summary>
        private void Setup()
        {
            Globals.SymbolDictionary.Add("if", Globals.Symbol.ifT);
            Globals.SymbolDictionary.Add("else", Globals.Symbol.elseT);
            Globals.SymbolDictionary.Add("while", Globals.Symbol.whileT);
            Globals.SymbolDictionary.Add("float", Globals.Symbol.floatT);
            Globals.SymbolDictionary.Add("int", Globals.Symbol.intT);
            Globals.SymbolDictionary.Add("char", Globals.Symbol.charT);
            Globals.SymbolDictionary.Add("break", Globals.Symbol.breakT);
            Globals.SymbolDictionary.Add("continue", Globals.Symbol.continueT);
            Globals.SymbolDictionary.Add("void", Globals.Symbol.voidT);
            Globals.SymbolDictionary.Add("const", Globals.Symbol.constT);
            Globals.SymbolDictionary.Add("return", Globals.Symbol.returnT);
            Globals.SymbolDictionary.Add("cout", Globals.Symbol.coutT);
            Globals.SymbolDictionary.Add("cin", Globals.Symbol.cinT);
            Globals.SymbolDictionary.Add("(", Globals.Symbol.lparenT);
            Globals.SymbolDictionary.Add(")", Globals.Symbol.rparenT);
            Globals.SymbolDictionary.Add("{", Globals.Symbol.lbraceT);
            Globals.SymbolDictionary.Add("}", Globals.Symbol.rbraceT);
            Globals.SymbolDictionary.Add("[", Globals.Symbol.lbracketT);
            Globals.SymbolDictionary.Add("]", Globals.Symbol.rbracketT);
            Globals.SymbolDictionary.Add(",", Globals.Symbol.commaT);
            Globals.SymbolDictionary.Add(".", Globals.Symbol.periodT);
            Globals.SymbolDictionary.Add(";", Globals.Symbol.semicolonT);
            Globals.SymbolDictionary.Add("\"", Globals.Symbol.singlequoteT);
            Globals.SymbolDictionary.Add("=", Globals.Symbol.assignopT);
            Globals.SymbolDictionary.Add("+", Globals.Symbol.addopT);
            Globals.SymbolDictionary.Add("-", Globals.Symbol.addopT);
            Globals.SymbolDictionary.Add("||", Globals.Symbol.addopT);
            Globals.SymbolDictionary.Add("*", Globals.Symbol.mulopT);
            Globals.SymbolDictionary.Add("/", Globals.Symbol.mulopT);
            Globals.SymbolDictionary.Add("&&", Globals.Symbol.mulopT);
            Globals.SymbolDictionary.Add("%", Globals.Symbol.mulopT);
            Globals.SymbolDictionary.Add("==", Globals.Symbol.relopT);
            Globals.SymbolDictionary.Add("!=", Globals.Symbol.relopT);
            Globals.SymbolDictionary.Add("<", Globals.Symbol.relopT);
            Globals.SymbolDictionary.Add("<=", Globals.Symbol.relopT);
            Globals.SymbolDictionary.Add(">=", Globals.Symbol.relopT);
            Globals.SymbolDictionary.Add(">", Globals.Symbol.relopT);
            Globals.SymbolDictionary.Add("!", Globals.Symbol.notT);
            Globals.SymbolDictionary.Add(">>", Globals.Symbol.cinsymT);
            Globals.SymbolDictionary.Add("<<", Globals.Symbol.coutsymT);
            Globals.SymbolDictionary.Add("endl", Globals.Symbol.endlT);
        }

        /// <summary>
        /// Peaks at the next char. 
        /// </summary>
        /// <returns></returns>
        public char getNextChar(){
            int tempCurrentPosition = currentPosition;
            int tempLineNumber = Globals.LineNumber;
            if(tempCurrentPosition >= lines[tempLineNumber].Length){
                tempLineNumber++;
                tempCurrentPosition = 0;
            }
            while(String.IsNullOrWhiteSpace(lines[tempLineNumber][tempCurrentPosition].ToString()))
            {
                tempCurrentPosition++;
            }
            return lines[tempLineNumber][tempCurrentPosition];
        }
    }
}
