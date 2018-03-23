/*
 * Author: Drake Olson
 * Class: Compiler Construction
 * Instructor: Dr. Hamer
 * Date: 1/30/18
 * Description: This file holds members for an entry into the symbol table
 */

using System;
namespace Compiler
{
    public class IntegerConstantEntry : ConstantEntry
    {
        public int value= 0;

        public override void printEntry()
        {
            Console.WriteLine($"{lexeme,-20} {tokenType.ToString(),-10} {depth,-3}");
        }
    }
}
