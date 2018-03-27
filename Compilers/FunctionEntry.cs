/*
 * Author: Drake Olson
 * Class: Compiler Construction
 * Instructor: Dr. Hamer
 * Date: 3/17/18
 * Description: This file holds members for a function entry into the symbol table
 */

using System;
using System.Collections.Generic;

namespace Compiler
{
    public class FunctionEntry : TableEntry
    {
        public int SizeOfLocal = 0;
        public int NumberOfParameters = 0;
        public VariableType ReturnType = 0;
        public LinkedList<VariableType> ParameterList = null;

        public override void printEntry()
        {
            Console.Write($"Class: Function Lexeme: {lexeme,-15} Token Type:{tokenType.ToString(),-10} Depth: {depth,-3} Size Params Local: {SizeOfLocal,-3} Num of Params: {NumberOfParameters,-2} Return Type:{ReturnType.ToString(),-5}");
            foreach(var node in ParameterList)
            {
                Console.Write($" {node.ToString()},");
            }
            Console.WriteLine();
        }
    }
}
