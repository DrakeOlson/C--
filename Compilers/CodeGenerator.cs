using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Compiler
{
    class CodeGenerator
    {
        private string outputtedFileName;
        private string[] lines;
        private SymbolTable symbolTable;

        public CodeGenerator(string outputtedFileName, SymbolTable symbolTable)
        {
            lines = File.ReadAllLines(outputtedFileName);
            this.outputtedFileName = outputtedFileName.Remove(outputtedFileName.IndexOf('.')) + ".asm";
            this.symbolTable = symbolTable;
        }

        public void Run()
        {
            WriteToFile(".MODEL SMALL");
            WriteToFile(".STACK 100H");
            WriteToFile(".DATA");
            WriteGlobals();
            WriteToFile(".code");
            WriteToFile("include io.asm");
            ProcessTACFile();
        }

        private void ProcessTACFile()
        {
            foreach(string line in lines)
            {
                
                if(!line.Contains("+") && !line.Contains("=-") && !line.Contains("*") && !line.Contains("/") && !line.Contains("-")) // a=b
                {
                    int indexOfEquals = line.IndexOf('=');
                    string lefthandSide = line.Substring(0, indexOfEquals);
                    lefthandSide = StripUnderScore(lefthandSide);
                    string rightHandSide = line.Substring(indexOfEquals + 1);
                    rightHandSide = StripUnderScore(rightHandSide);
                    WriteToFile($"mov ax,{rightHandSide}");
                    WriteToFile($"mov {lefthandSide},ax");
                }
                else if(line.Contains("=-") && !line.Contains("+") && !line.Contains("*") && !line.Contains("/") && !line.Contains("-")) //a=-b
                {
                    int indexOfEquals = line.IndexOf("=-");
                    string lefthandSide = line.Substring(0, indexOfEquals);
                    lefthandSide = StripUnderScore(lefthandSide);
                    string rightHandSide = line.Substring(indexOfEquals + 1);
                    rightHandSide = StripUnderScore(rightHandSide);
                    WriteToFile($"mov ax, -{rightHandSide}");
                    WriteToFile($"mov {lefthandSide},ax");
                }
                else if (line.Contains("+") && !line.Contains("=-") && !line.Contains("*") && !line.Contains("/") && !line.Contains("-")) //a= b + c
                {
                    int indexOfEquals = line.IndexOf("=-");
                    string lefthandSide = line.Substring(0, indexOfEquals);
                    lefthandSide = StripUnderScore(lefthandSide);
                    string rightHandSide = line.Substring(indexOfEquals + 1);
                    int leftOperandIndex = rightHandSide.IndexOf('+');
                    string leftOperand = rightHandSide.Substring(0, leftOperandIndex);
                    leftOperand = StripUnderScore(leftOperand);
                    string rightOperand = rightHandSide.Substring(leftOperandIndex + 1);
                    rightOperand = StripUnderScore(rightOperand);
                    WriteToFile($"add {leftOperand},{rightOperand}");
                    WriteToFile($"mov {lefthandSide},ax");
                }
                else if (line.Contains("*") && !line.Contains("+") && !line.Contains("=-") && !line.Contains("/") && !line.Contains("-")) //a= b * c
                {
                    int indexOfEquals = line.IndexOf("=-");
                    string lefthandSide = line.Substring(0, indexOfEquals);
                    lefthandSide = StripUnderScore(lefthandSide);
                    string rightHandSide = line.Substring(indexOfEquals + 1);
                    int leftOperandIndex = rightHandSide.IndexOf('+');
                    string leftOperand = rightHandSide.Substring(0, leftOperandIndex);
                    leftOperand = StripUnderScore(leftOperand);
                    string rightOperand = rightHandSide.Substring(leftOperandIndex + 1);
                    rightOperand = StripUnderScore(rightOperand);
                    WriteToFile($"mov ax,{leftOperand}");
                    WriteToFile($"mov bx,{rightOperand}");
                    WriteToFile($"imul bx");
                    WriteToFile($"mov {lefthandSide},ax");
                }
            }
        }

        private string StripUnderScore(string lefthandSide)
        {
            if (lefthandSide.StartsWith("_"))
            {
                return $"[{lefthandSide.Substring(1)}]";
            }
            else
            {
                return lefthandSide;
            }
        }

        private void WriteGlobals()
        {
            foreach(LinkedList<TableEntry> list in symbolTable.table.Values)
            {
                foreach(var node in list)
                {
                    if (node is StringLiteral)
                    {
                        StringLiteral temp = node as StringLiteral;
                        WriteToFile($"{temp.tempVarName} DB \"{ temp.assemblyLiteral}\"");
                    }
                    else if (node.depth == 1 && node is VariableEntry)
                    {
                        VariableEntry temp = node as VariableEntry;
                        WriteToFile($"{temp.lexeme} DW ?");
                    }
                    else if(node.depth == 1 && node is IntegerConstantEntry)
                    {
                        IntegerConstantEntry temp = node as IntegerConstantEntry;
                        WriteToFile($"{temp.lexeme} DW {temp.value}");
                    }
                }
            }
        }

        private void WriteToFile(string line)
        {
            using (StreamWriter output = new StreamWriter(outputtedFileName, true))
            {
                output.WriteLine(line);
            }
        }
    }
}
