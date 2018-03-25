/*
 * Author: Drake Olson
 * Class: Compiler Construction
 * Instructor: Dr. Hamer
 * Date: 1/30/18
 * Description: The Parser will check each token to see if it fits in the C-- Grammar
 */

using System;
using System.Collections.Generic;

namespace Compiler
{
    public class Parser
    {
        private Lexer l = null;
        private SymbolTable symbolTable = new SymbolTable();
        private int overallDepth = 1;
        private int overallOffset = 0;
        private enum Offset {character = 1,integer = 2,real = 4};
        private bool isParameter = false;
        private bool inFunction = false;
        private int parameterOffset = 0;
        private int localOffset = 0;
        private int currentTypeSize = 0;
        private TableEntry.VariableType currentVarType;
        private string currentLexeme = "";
        private string functionLexeme = "";
        private TableEntry.VariableType returnType;
        private int numberOfLocalParameters = 0;
        private LinkedList<FunctionEntry.VariableType> listOfLocalParam = new LinkedList<TableEntry.VariableType>();
        /// <summary>
        /// Default Constructor to create a Parser Object
        /// </summary>
        /// <param name="fileName"></param>
        public Parser(string fileName)
        {
            l = new Lexer(fileName);
            l.GetNextToken();
        }

        /// <summary>
        /// Run starts the parser. It is supposed to run after priming the parser which is done in the constructor.
        /// </summary>
        public void Run()
        {
            Prog();
            if(Globals.Token != Globals.Symbol.eoft)
            {
                Console.WriteLine($"Error: Line {Globals.LineNumber+1}: Reached end of file token with remaining tokens left over");
            }
        }

        /// <summary>
        /// Prog() handels the grammar PROG	->	TYPE idt REST PROG | lambda | const idt = num ; PROG
        /// </summary>
        private void Prog()
        {
            if (Globals.Token == Globals.Symbol.voidT || Globals.Token == Globals.Symbol.intT || Globals.Token == Globals.Symbol.floatT || Globals.Token == Globals.Symbol.charT)
            {
                //Functions
                Type();
                currentLexeme = Globals.Lexeme;
                functionLexeme = Globals.Lexeme;
                if(Globals.Token == Globals.Symbol.intT)
                {
                    returnType = TableEntry.VariableType.intType;
                }
                else if(Globals.Token == Globals.Symbol.floatT)
                {
                    returnType = TableEntry.VariableType.floatType;
                }
                Match(Globals.Symbol.idT);
                Rest();
                //End of a function
                Prog();
            }
            else if(Globals.Lexeme == "const")
            {
                Match(Globals.Symbol.constT);
                currentLexeme = Globals.Lexeme;
                Match(Globals.Symbol.idT);
                Match(Globals.Symbol.assignopT);
                //Global Variable Scope
                if(Globals.Value != null || Globals.ValueReal != null)
                {
                    if (Globals.Value != null)
                    {
                        IntegerConstantEntry entry = new IntegerConstantEntry()
                        {
                            lexeme = currentLexeme,
                            offset = overallOffset,
                            depth = overallDepth,
                            tokenType = Globals.Token,
                            value = Globals.Value.GetValueOrDefault()
                        };
                        symbolTable.insert(entry);
                        overallOffset += (int)Offset.integer;
                    }
                    else if (Globals.ValueReal != null)
                    {
                        RealConstantEntry entry = new RealConstantEntry()
                        {
                            lexeme = currentLexeme,
                            offset = overallOffset,
                            depth = overallDepth,
                            tokenType = Globals.Token,
                            value = Globals.Value.GetValueOrDefault()
                        };
                        symbolTable.insert(entry);
                        overallOffset += (int)Offset.real;
                    }
                    Match(Globals.Symbol.semicolonT);
                    Prog();
                }
                else
                {
                    Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: No right side of assignment. Expecting a numerical value after the equals sign.");
                }
            }
            else
            {
                //Lambda
            }
        }

        /// <summary>
        /// Type() handels the grammar rule TYPE ->	int | float | char | void
        /// </summary>
        private void Type()
        {
            if (Globals.Token == Globals.Symbol.voidT)
            {
                Match(Globals.Symbol.voidT);
            }
            else if(Globals.Token == Globals.Symbol.intT)
            {
                Match(Globals.Symbol.intT);
                if (isParameter)
                {
                    parameterOffset += (int)Offset.integer;
                    numberOfLocalParameters++;
                    listOfLocalParam.AddLast(TableEntry.VariableType.intType);
                }
                else if(inFunction)
                {
                    localOffset += (int)Offset.integer;
                }
                currentTypeSize = 2;
                currentVarType = TableEntry.VariableType.intType;
            }
            else if(Globals.Token == Globals.Symbol.floatT)
            {
                Match(Globals.Symbol.floatT);
                if (isParameter)
                {
                    parameterOffset += (int)Offset.real;
                    numberOfLocalParameters++;
                    listOfLocalParam.AddLast(TableEntry.VariableType.floatType);
                }
                else if (inFunction)
                {
                    localOffset += (int)Offset.real;
                }
                currentTypeSize = 4;
                currentVarType = TableEntry.VariableType.floatType;
            }
            else if(Globals.Token == Globals.Symbol.charT)
            {
                Match(Globals.Symbol.charT);
                if (isParameter)
                {
                    parameterOffset = (int)Offset.character;
                    numberOfLocalParameters++;
                    listOfLocalParam.AddLast(TableEntry.VariableType.charType);
                }
                else if (inFunction)
                {
                    localOffset = (int)Offset.character;
                }
                currentTypeSize = 1;
                currentVarType = TableEntry.VariableType.charType;
            }
        }
        /// <summary>
        /// Rest() handels the grammar rule REST	->	( PARAMLIST ) COMPOUND | IDTAIL ; PROG
        /// </summary>
        private void Rest()
        {
            if(Globals.Token == Globals.Symbol.lparenT)
            {
                //functions
                Match(Globals.Symbol.lparenT);
                overallDepth++;
                ParameterList();
                Match(Globals.Symbol.rparenT);
                parameterOffset = 0;
                Compound();
            }
            else
            {
                IdTail();
                Match(Globals.Symbol.semicolonT);
                Prog();
            }
        }
        /// <summary>
        /// ParameterList() handels the rule PARAMLIST	->	TYPE idt PARAMTAIL | lambda
        /// </summary>
        private void ParameterList()
        {
            if(Globals.Token == Globals.Symbol.voidT || Globals.Token == Globals.Symbol.intT || Globals.Token == Globals.Symbol.floatT || Globals.Token == Globals.Symbol.charT)
            {
                isParameter = true;
                Type();
                Match(Globals.Symbol.idT);
                parameterOffset = 0;
                TableEntry lookup = symbolTable.lookup(Globals.Lexeme);
                if (lookup.depth != overallDepth)
                {
                    VariableEntry entry = new VariableEntry()
                    {
                        lexeme = currentLexeme,
                        depth = overallDepth,
                        Offset = parameterOffset,
                        size = currentTypeSize,
                        variableType = currentVarType
                    };
                    symbolTable.insert(entry);
                }
                else
                {
                    Console.WriteLine($"Error: Line Number {Globals.LineNumber + 1}. The symbol {Globals.Lexeme} at that depth already exists.");
                    Environment.Exit(-1);
                }
                ParamTrail();
            }
            else{
                //Lambda
            }
        }
        /// <summary>
        /// ParamTrail() handels the rule PARAMTAIL	->	, TYPE idt PARAMTAIL | lamda
        /// </summary>
        private void ParamTrail(){
            if(Globals.Token == Globals.Symbol.commaT)
            {
                Match(Globals.Symbol.commaT);
                Type();
                Match(Globals.Symbol.idT);
                TableEntry lookup = symbolTable.lookup(Globals.Lexeme);
                if(lookup.depth != overallDepth)
                {
                    VariableEntry entry = new VariableEntry()
                    {
                        lexeme = currentLexeme,
                        depth = overallDepth,
                        Offset = parameterOffset,
                        size = currentTypeSize,
                        variableType = currentVarType
                    };
                    symbolTable.insert(entry);
                }
                else
                {
                    Console.WriteLine($"Error: Line Number {Globals.LineNumber + 1}. The symbol {Globals.Lexeme} at that depth already exists.");
                    Environment.Exit(-1);
                }
                ParamTrail();
            }
            else
            {
                //Lambda
            }
        }
        /// <summary>
        /// Compound() handels the rule COMPOUND	->	{ DECL STAT_LIST	RET_STAT }
        /// </summary>
        private void Compound()
        {
            isParameter = false;
            inFunction = true;
            Match(Globals.Symbol.lbraceT);
            Decl();
            StatList();
            RetList();
            Match(Globals.Symbol.rbraceT);
            FunctionEntry entry = new FunctionEntry()
            {
                lexeme = functionLexeme,
                depth = overallDepth,
                tokenType = Globals.Symbol.idT,
                SizeOfLocal = localOffset,
                NumberOfParameters = numberOfLocalParameters,
                ParameterList = listOfLocalParam
            };
            isParameter = false;
            inFunction = false;
            symbolTable.deleteDepth(overallDepth);
            overallDepth--;
            localOffset = 0;
        }
        /// <summary>
        /// Decl() handels the rule DECL	->	TYPE IDLIST | lambda | const idt = num ; DECL
        /// </summary>
        private void Decl()
        {
            if (Globals.Token == Globals.Symbol.voidT || Globals.Token == Globals.Symbol.intT || Globals.Token == Globals.Symbol.floatT || Globals.Token == Globals.Symbol.charT)
            {
                Type();
                IdList();
            }
            else if (Globals.Lexeme == "const")
            {
                Match(Globals.Symbol.constT);
                currentLexeme = Globals.Lexeme;
                Match(Globals.Symbol.idT);
                Match(Globals.Symbol.assignopT);
                //Global Variable Scope
                if (Globals.Value != null || Globals.ValueReal != null)
                {
                    if (Globals.Value != null)
                    {
                        IntegerConstantEntry entry = new IntegerConstantEntry()
                        {
                            lexeme = currentLexeme,
                            offset = overallOffset,
                            depth = overallDepth,
                            tokenType = Globals.Token,
                            value = Globals.Value.GetValueOrDefault()
                        };
                        symbolTable.insert(entry);
                        overallOffset += (int)Offset.integer;
                    }
                    else if (Globals.ValueReal != null)
                    {
                        RealConstantEntry entry = new RealConstantEntry()
                        {
                            lexeme = currentLexeme,
                            offset = overallOffset,
                            depth = overallDepth,
                            tokenType = Globals.Token,
                            value = Globals.Value.GetValueOrDefault()
                        };
                        symbolTable.insert(entry);
                        overallOffset += (int)Offset.real;
                    }
                    Match(Globals.Symbol.semicolonT);
                    Decl();
                }
                else
                {
                    Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: No right side of assignment. Expecting a numerical value after the equals sign.");
                }
            }
            else
            {
                //Lambda
            }
        }
        /// <summary>
        /// IdList() handels the rule IDLIST	->	idt IDTAIL ; DECL
        /// </summary>
        private void IdList()
        {
            Match(Globals.Symbol.idT);
            IdTail();
            Match(Globals.Symbol.semicolonT);
            Decl();
        }
        /// <summary>
        /// IdTail() handels the grammar rule IDTAIL	->	, idt IDTAIL | lambda
        /// </summary>
        private void IdTail()
        {
            if(Globals.Token == Globals.Symbol.commaT){
                Match(Globals.Symbol.commaT);
                Match(Globals.Symbol.idT);
                IdTail();
            }
            else
            {
                //lambda    
            }
        }

        /// <summary>
        /// StatList() handels the grammar rule STAT_LIST -> Lambda
        /// </summary>
        private void StatList()
        {
            //lambda
        }
        /// <summary>
        /// RetList() handels the grammar rule RET_LIST -> Lambda
        /// </summary>
        private void RetList()
        {
            //lambda
        }
        /// <summary>
        /// Match() Matches the current token to which is expected in the grammar
        /// </summary>
        /// <param name="token"></param>
        private void Match(Globals.Symbol token)
        {
            if(token == Globals.Token)
            {
                l.GetNextToken();
            }
            else{
                Console.WriteLine($"Error: Line {Globals.LineNumber+1}: Expecting {token.ToString()}. Received {Globals.Token.ToString()}");
                Environment.Exit(-1);
            }
        }
    }
}
