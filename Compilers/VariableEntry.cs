/*
 * Author: Drake Olson
 * Class: Compiler Construction
 * Instructor: Dr. Hamer
 * Date: 1/30/18
 * Description: This file holds members for entry into the symbol table
 */

using System;

namespace Compiler
{
    public class VariableEntry : TableEntry
    {
        public VariableType variableType;
        public int Offset;
        public int size;
        public int BPOffset;

        public override void printEntry()
        {
            Console.WriteLine($"Class: Variable Lexeme: {lexeme,-15} Token Type:{tokenType.ToString(),-10} Depth: {depth,-3} Variable Type: {variableType.ToString(),5} Offset: {Offset,-3} Size Of: {size,-3}");

        }
        public string getBPValue()
        {
            if(BPOffset > 0)
            {
                return $"_BP+{BPOffset}";
            }
            else
            {
                return $"_BP{BPOffset}";
            }
        }
    }
}
