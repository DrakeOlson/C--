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
            Console.WriteLine($"{lexeme,-20} {tokenType.ToString(),-10} {depth,-3}");
        }
    }
}
