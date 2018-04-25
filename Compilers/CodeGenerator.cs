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
