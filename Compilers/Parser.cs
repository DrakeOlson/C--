/*
 * Author: Drake Olson
 * Class: Compiler Construction
 * Instructor: Dr. Hamer
 * Date: 3/28/18
 * Description: The Parser will check each token to see if it fits in the C-- Grammar
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Compiler
{
    public class Parser
    {
        private Lexer l = null;
        public SymbolTable symbolTable = new SymbolTable();
        private int overallDepth = 0;
        private bool isFirstOverall = true;
        private int overallOffset = 0;
        private enum Offset {character = 1,integer = 2,real = 4};
        private bool isParameter = false;
        private bool inFunction = false;
        private int parameterOffset = 0;
        private int localOffset = 0;
        private int currentTypeSize = 0;
        private int sizeOfLocal = 0;
        private TableEntry.VariableType currentVarType;
        private string currentLexeme = "";
        private string functionLexeme = "";
        private TableEntry.VariableType returnType;
        private int numberOfLocalParameters = 0;
        private LinkedList<FunctionEntry.VariableType> listOfLocalParam = new LinkedList<TableEntry.VariableType>();
        private Stack<TableEntry> actualParameters = new Stack<TableEntry>();
        private Stack<object> passedParams = new Stack<object>();
        private int paramBasePointerCounter = 4;
        private int localBasePointerCounter = 2;
        private StringBuilder outputtedString = new StringBuilder();
        private int tempCounter = 1;
        private const string ax = "_AX";
        private bool unary = false;
        private int rightHandSide = 0;
        public string outputtedFileName;
        private string finalLeftHand;
        private string finalLeftHandLex;
        private bool inReturn = false;
        private bool singleRightHand = true;
        private string functionName;
        private int globalOffset = 2;
        private bool multiPart = false;
        private Stack<string> expressionStack = new Stack<string>();
        private Stack<string> temps = new Stack<string>();
        public List<string> literals = new List<string>();
        public bool hasSign = false;
        private int stringTempCounter = 0;

        /// <summary>
        /// Default Constructor to create a Parser Object
        /// </summary>
        /// <param name="fileName"></param>
        public Parser(string fileName)
        {
            l = new Lexer(fileName);
            outputtedFileName = fileName.Remove(fileName.IndexOf('.')) + ".tac";
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
            symbolTable.writeTable(overallDepth);
            if(File.ReadAllLines(outputtedFileName).Contains("Proc main") == false && File.ReadAllLines(outputtedFileName).Contains("Proc Main") == false)
            {
                File.Delete(outputtedFileName);
                Console.WriteLine("Error: No function named 'main'!");
            }
            else
            {
                WriteToFile("Call proc main");
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
                char lookahead = l.getNextChar();
                Match(Globals.Symbol.idT);
                if(lookahead != '(')
                {
                    isFirstOverall = true;
                    if(isFirstOverall && overallOffset == 0)
                    {
                        VariableEntry entry = new VariableEntry()
                        {
                            lexeme = currentLexeme,
                            depth = overallDepth,
                            size = currentTypeSize,
                            Offset = 0,
                            variableType = currentVarType,
                            BPOffset = -globalOffset
                        };
                        globalOffset +=2;
                        overallOffset += currentTypeSize;
                        insertSymbol(entry);
                        isFirstOverall = false;
                    }
                    else if(isFirstOverall && overallOffset >0)
                    {
                        VariableEntry entry = new VariableEntry()
                        {
                            lexeme = currentLexeme,
                            depth = overallDepth,
                            size = currentTypeSize,
                            Offset = overallOffset,
                            variableType = currentVarType,
                            BPOffset = -globalOffset
                        };
                        globalOffset += 2;
                        insertSymbol(entry);
                        overallOffset += currentTypeSize;
                    }

                    
                }
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
                        Match(Globals.Symbol.intT);
                        IntegerConstantEntry entry = new IntegerConstantEntry()
                        {
                            lexeme = currentLexeme,
                            offset = 0,
                            depth = overallDepth,
                            tokenType = Globals.Symbol.intT,
                            value = Globals.Value.GetValueOrDefault()
                        };
                        Globals.Value = null;
                        symbolTable.insert(entry);
                    }
                    else if (Globals.ValueReal != null)
                    {
                        Match(Globals.Symbol.floatT);
                        RealConstantEntry entry = new RealConstantEntry()
                        {
                            lexeme = currentLexeme,
                            offset = 0,
                            depth = overallDepth,
                            tokenType = Globals.Symbol.floatT,
                            value = Globals.ValueReal.GetValueOrDefault()
                        };
                        Globals.ValueReal = null;
                        symbolTable.insert(entry);
                        overallOffset += currentTypeSize;
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

                    if(isFirstOverall){
                        parameterOffset = 0;
                        isFirstOverall = false;
                    }
                    numberOfLocalParameters++;
                    listOfLocalParam.AddLast(TableEntry.VariableType.intType);
                }
                else if(inFunction)
                {
                    if(isFirstOverall){
                        localOffset = 0;
                        isFirstOverall = false;
                    }
                }
                else
                {
                    if (isFirstOverall && overallOffset == 0)
                    {
                        isFirstOverall = false;
                        overallOffset = 0;

                    }
                }
                currentTypeSize = 2;
                currentVarType = TableEntry.VariableType.intType;
            }
            else if(Globals.Token == Globals.Symbol.floatT)
            {
                Match(Globals.Symbol.floatT);
                if (isParameter)
                {
                    
                    numberOfLocalParameters++;
                    listOfLocalParam.AddLast(TableEntry.VariableType.floatType);
                    if (isFirstOverall)
                    {
                        parameterOffset = 0;
                        isFirstOverall = false;
                    }
                }
                else if (inFunction)
                {
                    if (isFirstOverall)
                    {
                        localOffset = 0;
                        isFirstOverall = false;
                    }
                }
                else
                {
                    if (isFirstOverall && overallOffset == 0)
                    {
                        isFirstOverall = false;
                        overallOffset = 0;

                    }
                }
                currentTypeSize = 4;
                currentVarType = TableEntry.VariableType.floatType;
            }
            else if(Globals.Token == Globals.Symbol.charT)
            {
                Match(Globals.Symbol.charT);
                if (isParameter)
                {

                    numberOfLocalParameters++;
                    listOfLocalParam.AddLast(TableEntry.VariableType.charType);
                    if (isFirstOverall)
                    {
                        parameterOffset = 0;
                        isFirstOverall = false;
                    }
                }
                else if (inFunction)
                {
                    if (isFirstOverall)
                    {
                        localOffset = 0;
                        isFirstOverall = false;
                    }
                }
                else
                {
                    if (isFirstOverall && overallOffset == 0)
                    {
                        isFirstOverall = false;
                        overallOffset = 0;

                    }
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
                WriteToFile($"Proc {functionLexeme}");
                returnType = currentVarType;
                Match(Globals.Symbol.lparenT);
                overallDepth++;
                ParameterList();
                Match(Globals.Symbol.rparenT);
                assignParamBPOffsets();
                paramBasePointerCounter = 4;
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
        /// Assigns the BP offset of parameters in a function
        /// </summary>
        private void assignParamBPOffsets()
        {
            while(actualParameters.Count != 0)
            {
                if(actualParameters.Count != 0)
                {
                    VariableEntry lookup = symbolTable.lookup(actualParameters.Pop().lexeme) as VariableEntry;
                    
                    lookup.BPOffset = paramBasePointerCounter;
                    paramBasePointerCounter += 2;

                }
            }
            actualParameters.Clear();
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
                currentLexeme = Globals.Lexeme;
                Match(Globals.Symbol.idT);
                parameterOffset = 0;
                VariableEntry entry = new VariableEntry()
                {
                    lexeme = currentLexeme,
                    depth = overallDepth,
                    Offset = parameterOffset,
                    size = currentTypeSize,
                    variableType = currentVarType
                };
                actualParameters.Push(entry);
                insertSymbol(entry);
                parameterOffset += currentTypeSize;
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
                isFirstOverall = false;
                Type();
                currentLexeme = Globals.Lexeme;
                Match(Globals.Symbol.idT);
                VariableEntry entry = new VariableEntry()
                {
                    lexeme = currentLexeme,
                    depth = overallDepth,
                    Offset = parameterOffset,
                    size = currentTypeSize,
                    variableType = currentVarType
                };
                actualParameters.Push(entry);
                insertSymbol(entry);
                parameterOffset += currentTypeSize;
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
            WriteToFile($"Endp {functionLexeme}");
            outputtedString.Clear();
            localBasePointerCounter = 2;
            paramBasePointerCounter = 4;
            FunctionEntry entry = new FunctionEntry()
            {
                lexeme = functionLexeme,
                depth = overallDepth - 1,
                tokenType = Globals.Symbol.idT,
                SizeOfLocal = sizeOfLocal,
                NumberOfParameters = numberOfLocalParameters,
                ParameterList = listOfLocalParam,
                ReturnType = returnType
            };
            isParameter = false;
            inFunction = false;
            numberOfLocalParameters = 0;
            localBasePointerCounter = 2;
            insertSymbol(entry);
            symbolTable.writeTable(overallDepth);
            symbolTable.deleteDepth(overallDepth);
            overallDepth--;
            parameterOffset = 0;
            localOffset = 0;
            sizeOfLocal = 0;
            listOfLocalParam.Clear();
        }
        /// <summary>
        /// Decl() handels the rule DECL	->	TYPE IDLIST | lambda | const idt = num ; DECL
        /// </summary>
        private void Decl()
        {
            if (Globals.Token == Globals.Symbol.voidT || Globals.Token == Globals.Symbol.intT || Globals.Token == Globals.Symbol.floatT || Globals.Token == Globals.Symbol.charT)
            {
                if(localOffset == 0)
                {
                    isFirstOverall = true;
                }
                if (inFunction && isFirstOverall == false && localOffset != 0)
                {
                    localOffset += currentTypeSize;
                    isFirstOverall = false;
                }
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
                        Match(Globals.Symbol.intT);
                        IntegerConstantEntry entry = new IntegerConstantEntry()
                        {
                            lexeme = currentLexeme,
                            offset = 0,
                            depth = overallDepth,
                            tokenType = Globals.Symbol.intT,
                            value = Globals.Value.GetValueOrDefault()
                        };
                        Globals.Value = null;
                        symbolTable.insert(entry);
                        overallOffset += (int)Offset.integer;

                    }
                    else if (Globals.ValueReal != null)
                    {
                        Match(Globals.Symbol.floatT);
                        RealConstantEntry entry = new RealConstantEntry()
                        {
                            lexeme = currentLexeme,
                            offset = 0,
                            depth = overallDepth,
                            tokenType = Globals.Symbol.floatT,
                            value = Globals.ValueReal.GetValueOrDefault()
                        };
                        Globals.ValueReal = null;
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
            currentLexeme = Globals.Lexeme;
            isFirstOverall = true;
            sizeOfLocal += currentTypeSize;
            if (!isFirstOverall)
            {
                localOffset += getOffset();
            }
            int templocal;
            if(inFunction)
            {
                templocal = -localBasePointerCounter;
                if (currentVarType == TableEntry.VariableType.intType)
                {
                    localBasePointerCounter += 2;
                }
                else if (currentVarType == TableEntry.VariableType.floatType)
                {
                    localBasePointerCounter += 4;
                }
            }
            else
            {
                templocal = -globalOffset;
                if (currentVarType == TableEntry.VariableType.intType)
                {
                    localBasePointerCounter += 2;
                }
                else if (currentVarType == TableEntry.VariableType.floatType)
                {
                    localBasePointerCounter += 4;
                }
            }
            VariableEntry entry = new VariableEntry()
            {
                lexeme = currentLexeme,
                depth = overallDepth,
                Offset = localOffset,
                size = currentTypeSize,
                variableType = currentVarType,
                BPOffset = templocal
            };
            insertSymbol(entry);
            isFirstOverall = false;
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
            if (Globals.Token == Globals.Symbol.commaT) {
                Match(Globals.Symbol.commaT);
                currentLexeme = Globals.Lexeme;
                Match(Globals.Symbol.idT);

                if (inFunction)
                {
                    sizeOfLocal += currentTypeSize;
                    if (!isFirstOverall)
                    {
                        localOffset += getOffset();
                    }

                    int templocal = -localBasePointerCounter;
                    if(currentVarType == TableEntry.VariableType.intType)
                    {
                        localBasePointerCounter += 2;
                    }
                    else if(currentVarType == TableEntry.VariableType.floatType)
                    {
                        localBasePointerCounter += 4;
                    }
                    VariableEntry entry = new VariableEntry()
                    {
                        lexeme = currentLexeme,
                        depth = overallDepth,
                        size = currentTypeSize,
                        Offset = localOffset,
                        variableType = currentVarType,
                        BPOffset = templocal
                    };
                    insertSymbol(entry);
                }
                else
                {
                    int templocal = -globalOffset;
                    globalOffset += 2;
                    VariableEntry entry = new VariableEntry()
                    {
                        lexeme = currentLexeme,
                        depth = overallDepth,
                        size = currentTypeSize,
                        Offset = overallOffset,
                        variableType = currentVarType,
                        BPOffset = templocal
                    };
                    insertSymbol(entry);
                    overallOffset += currentTypeSize;
                }
                IdTail();
            }
            else
            {
                //lambda    
            }
        }

        /// <summary>
        /// StatList() handels the grammar rule STAT_LIST -> Statement ; STAT_LIST|Lambda
        /// </summary>
        private void StatList()
        {
            if (Globals.Token == Globals.Symbol.idT)
            {
                Statement();
                Match(Globals.Symbol.semicolonT);
                if(Globals.Value != null)
                {
                    Globals.Value = null;
                }
                if(Globals.ValueReal != null)
                {
                    Globals.ValueReal = null;
                }
                StatList();
            }
            else
            {
                //Lambda
            }
        }
        /// <summary>
        /// Statement		-> 	AssignStat	|IOStat
        /// </summary>
        private void Statement()
        {
            if(Globals.Token == Globals.Symbol.idT)
            {
                AssignStat();
            }
            else
            {
                IOStat();
            }
        }
        /// <summary>
        /// Current just hold lambda. Probably going to be expanded upon later
        /// </summary>
        private void IOStat()
        {
            if(Globals.Token == Globals.Symbol.cinT)
            {
                In_Stat();
            }
            else if(Globals.Token == Globals.Symbol.coutT)
            {
                Out_Stat();
            }
            else
            {
                Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: Looking for IO Statement got Token: {Globals.Lexeme}");
            }

        }

        private void Out_Stat()
        {
            Match(Globals.Symbol.coutT);
            Match(Globals.Symbol.coutsymT);
           
            if(Globals.Token == Globals.Symbol.idT)
            {
                TableEntry lookup = symbolTable.lookup(Globals.Lexeme);
                    
                if (lookup == null)
                {
                    Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: {Globals.Lexeme} has not been declared.");
                }
                else if (lookup is ConstantEntry)
                {
                    Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: Can't change the value of a constant.");
                }
                else
                {
                    WriteToFile($"wri {((VariableEntry)lookup).getBPValue()}");
                }
            }
            else if(Globals.Token == Globals.Symbol.endlT)
            {
                Match(Globals.Symbol.endlT);
                WriteToFile("writeln");
            }
            else if(Globals.Token == Globals.Symbol.literalT)
            {
                StringLiteral entry = new StringLiteral()
                {
                    lexeme = Globals.Attribute,
                    depth = 1,
                    tokenType = Globals.Symbol.literalT,
                    tempVarName = getStringTemp()
                };
                symbolTable.insert(entry);
                WriteToFile($"wrs {entry.lexeme}");
                Match(Globals.Symbol.literalT);
            }
            Out_End();
        }

        private void Out_End()
        {
            if(Globals.Token == Globals.Symbol.coutsymT)
            {
                Match(Globals.Symbol.coutsymT);
                if (Globals.Token == Globals.Symbol.idT)
                {
                    TableEntry lookup = symbolTable.lookup(Globals.Lexeme);

                    if (lookup == null)
                    {
                        Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: {Globals.Lexeme} has not been declared.");
                    }
                    else if (lookup is ConstantEntry)
                    {
                        Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: Can't change the value of a constant.");
                    }
                    else
                    {
                        WriteToFile($"wri {((VariableEntry)lookup).getBPValue()}");
                    }
                }
                else if (Globals.Token == Globals.Symbol.endlT)
                {
                    Match(Globals.Symbol.endlT);
                    WriteToFile("wrln");
                    Out_End();
                }
                else if (Globals.Token == Globals.Symbol.literalT)
                {
                    StringLiteral entry = new StringLiteral()
                    {
                        lexeme = Globals.Attribute,
                        depth = 1,
                        tokenType = Globals.Symbol.literalT,
                        tempVarName = getStringTemp()
                    };
                    symbolTable.insert(entry);
                    WriteToFile($"wrs {entry.lexeme}");
                    Match(Globals.Symbol.literalT);
                    Out_End();
                }
            }
            
        }

        private void In_Stat()
        {
            Match(Globals.Symbol.cinT);
            Match(Globals.Symbol.cinsymT);
            TableEntry lookup = symbolTable.lookup(Globals.Lexeme);
            
            if(lookup == null)
            {
                Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: {Globals.Lexeme} has not been declared.");
            }
            else if(lookup is ConstantEntry)
            {
                Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: Can't change the value of a constant.");
            }
            else
            {
                Match(Globals.Symbol.idT);
                WriteToFile($"rdi {((VariableEntry)lookup).getBPValue()}");
                In_End();
            }
        }

        private void In_End()
        {
            if(Globals.Token == Globals.Symbol.cinsymT)
            {
                Match(Globals.Symbol.cinsymT);
                TableEntry lookup = symbolTable.lookup(Globals.Lexeme);

                if (lookup == null)
                {
                    Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: {Globals.Lexeme} has not been declared.");
                }
                else if (lookup is ConstantEntry)
                {
                    Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: Can't change the value of a constant.");
                }
                else
                {
                    Match(Globals.Symbol.idT);
                    WriteToFile($"rdi {((VariableEntry)lookup).getBPValue()}");
                    In_End();
                }
            }
        }

        /// <summary>
        /// Assignstat -> idt = Expr | idt = FuncCall
        /// </summary>
        private void AssignStat()
        {
            TableEntry leftHand = symbolTable.lookup(Globals.Lexeme);
            if(leftHand == null && Globals.Lexeme != "-")
            {
                Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: Token {Globals.Lexeme} hasn't been declared.");
                Environment.Exit(-1);
            }
            if(leftHand is ConstantEntry)
            {
                Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: Token {Globals.Lexeme} is a constant and can't be reassigned.");
                Environment.Exit(-1);
            }
            
            Match(Globals.Symbol.idT);
            outputtedString.Append("=");
            Match(Globals.Symbol.assignopT);
            TableEntry lookup = symbolTable.lookup(Globals.Lexeme);
            if ((lookup != null && (lookup is VariableEntry || lookup is ConstantEntry)) || (lookup == null && (Globals.Token == Globals.Symbol.floatT || Globals.Token == Globals.Symbol.intT)) || Globals.Lexeme == "-")
            {
                TableEntry idptr = null;
                TableEntry Eplace = null;
                idptr = leftHand;
                Expr(ref Eplace);
                if(Eplace is VariableEntry)
                {
                    VariableEntry lefthandptr = idptr as VariableEntry;
                    VariableEntry temp = Eplace as VariableEntry;
                    WriteToFile($"{lefthandptr.getBPValue()}={temp.getBPValue()}");
                }
                else
                {
                    VariableEntry lefthandptr = idptr as VariableEntry;
                    WriteToFile($"{lefthandptr.getBPValue()}={Eplace.lexeme}");
                }              
            }
            else if (lookup != null && lookup is FunctionEntry)
            {
                VariableEntry temp = leftHand as VariableEntry;
                FuncCall();
                WriteToFile($"{temp.getBPValue()} = {ax}");
            }
            else if (lookup == null)
            {
                Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: Token {Globals.Lexeme} hasn't been declared.");
                Environment.Exit(-1);
            }
            
        }

        private void FuncCall()
        {
            string functionName = Globals.Lexeme;
            Match(Globals.Symbol.idT);
            Match(Globals.Symbol.lparenT);
            Params();
            while(passedParams.Count != 0)
            {
                WriteToFile($"push {passedParams.Pop()}");
            }
            WriteToFile($"Call {functionName}");
            Match(Globals.Symbol.rparenT);
        }

        private void Params()
        {
            
            if(Globals.Token == Globals.Symbol.idT)
            {
                TableEntry lookup = symbolTable.lookup(Globals.Lexeme);
                if(lookup != null)
                {
                    if (lookup.depth == 1 || lookup is ConstantEntry)
                    {
                        if (lookup is IntegerConstantEntry)
                        {
                            IntegerConstantEntry entry = lookup as IntegerConstantEntry;
                            passedParams.Push(entry.value);
                        }
                        else if (lookup is RealConstantEntry)
                        {
                            RealConstantEntry entry = lookup as RealConstantEntry;
                            passedParams.Push(entry.value);
                        }
                        else if (lookup is VariableEntry)
                        {
                            VariableEntry entry = lookup as VariableEntry;
                            passedParams.Push(entry.getBPValue(overallDepth));
                        }

                    }
                    else
                    {
                        VariableEntry entry = lookup as VariableEntry;
                        passedParams.Push(entry.getBPValue(overallDepth));
                    }

                    Match(Globals.Symbol.idT);
                    ParamsTail();
                }
                else
                {
                    Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: Token {Globals.Lexeme} hasn't been declared.");
                    Environment.Exit(-1);
                }
            }
            else if(Globals.Token == Globals.Symbol.floatT || Globals.Token == Globals.Symbol.intT)
            {
                if(Globals.Token == Globals.Symbol.floatT)
                {
                    passedParams.Push(Globals.ValueReal);
                    Match(Globals.Symbol.floatT);
                }
                else
                {
                    passedParams.Push(Globals.Value);
                    Match(Globals.Symbol.intT);
                }
                ParamsTail();
            }
            else
            {
                
                //Lambda
            }

            passedParams = reverseParams(passedParams);
        }

        private Stack<object> reverseParams(Stack<object> passedParams)
        {
            Stack<object> temp = new Stack<object>();

            while(passedParams.Count != 0)
            {
                temp.Push(passedParams.Pop());
            }
            return temp;
        }

        private void ParamsTail()
        {
            if(Globals.Token == Globals.Symbol.commaT)
            {
                Match(Globals.Symbol.commaT);
                if (Globals.Token == Globals.Symbol.idT)
                {
                    TableEntry lookup = symbolTable.lookup(Globals.Lexeme);
                    if(lookup != null)
                    {
                        if (lookup.depth == 1 || lookup is ConstantEntry)
                        {
                            if (lookup is IntegerConstantEntry)
                            {
                                IntegerConstantEntry entry = lookup as IntegerConstantEntry;
                                passedParams.Push(entry.value);
                            }
                            else if (lookup is RealConstantEntry)
                            {
                                RealConstantEntry entry = lookup as RealConstantEntry;
                                passedParams.Push(entry.value);
                            }
                            else if (lookup is VariableEntry)
                            {
                                VariableEntry entry = lookup as VariableEntry;
                                passedParams.Push(entry.getBPValue(overallDepth));
                            }

                        }
                        else
                        {
                            VariableEntry entry = lookup as VariableEntry;
                            passedParams.Push(entry.getBPValue(overallDepth));
                        }
                        Match(Globals.Symbol.idT);
                        ParamsTail();
                    }
                    else
                    {
                        Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: Token {Globals.Lexeme} hasn't been declared.");
                        Environment.Exit(-1);
                    }
                }
                else if (Globals.Token == Globals.Symbol.floatT || Globals.Token == Globals.Symbol.intT)
                {
                    if (Globals.Token == Globals.Symbol.floatT)
                    {
                        passedParams.Push(Globals.ValueReal);
                        Match(Globals.Symbol.floatT);
                    }
                    else
                    {
                        passedParams.Push(Globals.Value);
                        Match(Globals.Symbol.intT);
                    }
                    ParamsTail();
                }
            }
            else
            {
                passedParams.Reverse();
                //lambda
            }

        }

        /// <summary>
        /// Grammar Rule: AssignStat		->	idt  =  Expr 
        /// </summary>
        private void Expr(ref TableEntry Eplace)
        {
            Realtion(ref Eplace);
        }
        /// <summary>
        /// Grammar Rule Realtion		->	SimpleExpr
        /// </summary>
        private void Realtion(ref TableEntry Eplace)
        {
            SimpleExpr(ref Eplace);
        }
        /// <summary>
        /// Grammar Rule SimpleExpr		->	SignOp Term MoreTerm
        /// </summary>
        private void SimpleExpr(ref TableEntry Eplace)
        {
            TableEntry Tplace = null;
            SignOp(ref Tplace);
            Term(ref Tplace);
            MoreTerm(ref Tplace);
            Eplace = Tplace;
        }

        private void MoreTerm(ref TableEntry Rplace)
        {
            TableEntry Tplace = null;
            TableEntry tmpptr = null;
            string output;
            if (Globals.Lexeme == "+" || Globals.Lexeme == "-" || Globals.Lexeme == "||")
            {
                getNextTemp(ref tmpptr);
                if (Rplace is ConstantEntry)
                {
                    output = $"{tmpptr.lexeme}={Rplace.lexeme}{Globals.Lexeme}";
                }
                else
                {
                    VariableEntry temp = Rplace as VariableEntry;
                    output = $"{tmpptr.lexeme}={temp.getBPValue()}{Globals.Lexeme}";
                }
                Match(Globals.Symbol.addopT);
                Term(ref Tplace);
                if (Tplace is ConstantEntry)
                {
                    if (Tplace is IntegerConstantEntry)
                    {
                        IntegerConstantEntry tempT = Tplace as IntegerConstantEntry;
                        output += $"{tempT.value}";
                    }
                    else
                    {
                        RealConstantEntry tempT = Tplace as RealConstantEntry;
                        output += $"{tempT.value}";
                    }
                }
                else
                {
                    VariableEntry temp = Tplace as VariableEntry;
                    output += $"{temp.getBPValue()}";
                }
                Rplace = tmpptr;
                WriteToFile(output);
                MoreTerm(ref Rplace);
            }
            else
            {
                //lambda
            }
        }

        /// <summary>
        /// Grammar Rule Term			->	Factor  MoreFactor
        /// </summary>
        private void Term(ref TableEntry Tplace)
        {
            Factor(ref Tplace);
            MoreFactor(ref Tplace);
        }
        /// <summary>
        /// Mulop Factor MoreFactor | lambda
        /// </summary>
        private void MoreFactor(ref TableEntry Rplace)
        {
            TableEntry Tplace = null;
            TableEntry tmpptr = null;
            string output;
            if(Globals.Lexeme == "*" || Globals.Lexeme == "/" || Globals.Lexeme == "&&")
            {
                getNextTemp(ref tmpptr);
                if (Rplace is ConstantEntry)
                {
                    output = $"{tmpptr.lexeme}={Rplace.lexeme}{Globals.Lexeme}";
                }
                else
                {
                    VariableEntry temp = Rplace as VariableEntry;
                    output = $"{tmpptr.lexeme}={temp.getBPValue()}{Globals.Lexeme}";
                }
                Match(Globals.Symbol.mulopT);
                Factor(ref Tplace);
                if (Tplace is ConstantEntry)
                {
                    if(Tplace is IntegerConstantEntry)
                    {
                        IntegerConstantEntry tempT = Tplace as IntegerConstantEntry;
                        output += $"{tempT.value}";
                    }
                    else
                    {
                        RealConstantEntry tempT = Tplace as RealConstantEntry;
                        output += $"{tempT.value}";
                    }
                }
                else
                {
                    VariableEntry temp = Tplace as VariableEntry;
                    output += $"{temp.getBPValue()}";
                }
                Rplace = tmpptr;
                WriteToFile(output);
                MoreFactor(ref Rplace);
            }
            else
            {
                
                //lambda
            }
        }

        /// <summary>
        /// Grammar Rule ->	id | num|( Expr )
        /// </summary>
        private void Factor(ref TableEntry Tplace)
        {
            if (Globals.Token == Globals.Symbol.idT)
            {
                TableEntry tempEntry = null;
                if (hasSign)
                {
                    tempEntry = Tplace;
                }
                else
                {
                    tempEntry = symbolTable.lookup(Globals.Lexeme);
                }
                if (tempEntry == null)
                {
                    Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: Token {Globals.Lexeme} hasn't been declared.");
                    Environment.Exit(-1);
                }
                else
                {
                    Tplace = tempEntry;
                    hasSign = false;
                    Match(Globals.Symbol.idT);
                }
            }
            else if (Globals.Value != null)
            {
                IntegerConstantEntry localTemp = new IntegerConstantEntry()
                {
                    lexeme = getNextTemp(false),
                    depth = overallDepth,
                    tokenType = Globals.Symbol.idT,
                    value = Globals.Value.GetValueOrDefault()
                };
                WriteToFile($"{localTemp.lexeme}={Globals.Value.GetValueOrDefault()}");
                symbolTable.insert(localTemp);
                Tplace = localTemp;
                Match(Globals.Symbol.intT);
            }
            else if(Globals.ValueReal != null)
            {
                RealConstantEntry localTemp = new RealConstantEntry()
                {
                    lexeme = getNextTemp(false),
                    depth = overallDepth,
                    tokenType = Globals.Symbol.idT,
                    value = Globals.ValueReal.GetValueOrDefault()
                };
                symbolTable.insert(localTemp);
                WriteToFile($"{localTemp.lexeme}={Globals.ValueReal.GetValueOrDefault()}");
                Tplace = localTemp;
                Match(Globals.Symbol.floatT);
            }
            else if(Globals.Token == Globals.Symbol.lparenT)
            {
                Match(Globals.Symbol.lparenT);
                Expr(ref Tplace);
                Match(Globals.Symbol.rparenT);
            }
            else
            {
                Console.WriteLine($"Error: Line {Globals.LineNumber + 1}: Expression doesn't start with a decalred variable, number, or '('.");
                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// SignOp		->	! | - | Empty
        /// </summary>
        private void SignOp(ref TableEntry Tplace)
        {  
            if (Globals.Lexeme == "-")
            {
                hasSign = true;
                if (Char.IsLetter(l.getNextChar())){
                    Match(Globals.Symbol.addopT);
                    getNextTemp(ref Tplace);
                    TableEntry temp = symbolTable.lookup(Globals.Lexeme);
                    if(temp is VariableEntry)
                    {
                        VariableEntry var = temp as VariableEntry;
                        WriteToFile($"{Tplace.lexeme} = -{var.getBPValue()}");
                    }
                    else if(temp is ConstantEntry)
                    {
                        if(temp is IntegerConstantEntry)
                        {
                            IntegerConstantEntry var = temp as IntegerConstantEntry;
                            int tempValue = -var.value;
                            WriteToFile($"{Tplace.lexeme} = {-tempValue}");
                        }
                        else if(temp is RealConstantEntry)
                        {
                            RealConstantEntry var = temp as RealConstantEntry;
                            float tempValue = -var.value;
                            WriteToFile($"{Tplace.lexeme} = {-tempValue}");
                        }
                    }
                }
            }
            else if (Globals.Token == Globals.Symbol.notT)
            {
                Match(Globals.Symbol.notT);
            }
            else
            {
                //lambda
            }
        }

        /// <summary>
        /// RetList() handels the grammar rule RET_LIST -> Lambda
        /// </summary>
        private void RetList()
        {
            TableEntry Eplace = null;
            Match(Globals.Symbol.returnT);
            Expr(ref Eplace);
            Match(Globals.Symbol.semicolonT);
            WriteToFile($"{ax}={Eplace.lexeme}");
            outputtedString.Clear();
            inReturn = false;
            multiPart = false;
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
                Console.WriteLine($"Error: Line {Globals.LineNumber}: Expecting {token.ToString()}. Received {Globals.Token.ToString()}");
                Environment.Exit(-1);
            }
        }
        private void insertSymbol(TableEntry entry)
        {
            TableEntry lookup = symbolTable.lookup(entry.lexeme);
            if(lookup == null)
            {
                symbolTable.insert(entry);
            }
            else
            {
                if(lookup.depth == entry.depth)
                {
                    Console.WriteLine($"Error: Line {Globals.LineNumber}: Can't insert {entry.lexeme} at depth {entry.depth} because one already exists at that depth.");
                    Console.Write("Press any enter to continue...");
                    Console.ReadLine();
                    Environment.Exit(-1);
                }
                else
                {
                    symbolTable.insert(entry);
                }
            }
        }

        private string getNextTemp(bool insert)
        {
            string returned = "_BP-" + localBasePointerCounter;
            if (insert)
            {
                VariableEntry entry = new VariableEntry
                {
                    lexeme = returned,
                    variableType = TableEntry.VariableType.tempType,
                    depth = overallDepth,
                    size = 2,
                    Offset = localOffset,
                    BPOffset = -localBasePointerCounter
                };
                symbolTable.insert(entry);
                WriteToFile($"{entry.lexeme} = {Globals.Lexeme}");
            }
            localOffset += 2;
            sizeOfLocal += 2;
            localBasePointerCounter += 2;
            return returned;
        }
        private void getNextTemp(ref TableEntry entry)
        {
            string returned = "_BP-" + localBasePointerCounter;
            VariableEntry temp = new VariableEntry
            {
                lexeme = returned,
                variableType = TableEntry.VariableType.tempType,
                depth = overallDepth,
                size = 2,
                Offset = localOffset,
                BPOffset = -localBasePointerCounter
            };
            symbolTable.insert(temp);
            localOffset += 2;
            localBasePointerCounter += 2;
            entry = temp;
            sizeOfLocal += 2;
        }

        private void WriteToFile(string line)
        {
            using(StreamWriter output = new StreamWriter(outputtedFileName, true))
            {
                output.WriteLine(line);
            }
        }
        private string getStringTemp()
        {
            string returned = $"_S{stringTempCounter}";
            stringTempCounter++;
            return returned;
        }
        /// <summary>
        /// Gets the offset of the current type
        /// </summary>
        /// <returns></returns>
        private int getOffset()
        {
            if(currentVarType == TableEntry.VariableType.charType)
            {
                return 1;    
            }
            else if(currentVarType == TableEntry.VariableType.floatType){
                return 4;
            }
            else
            {
                return 2;
            }
        }
    }
}