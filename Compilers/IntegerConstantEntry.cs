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
        public int value;

        public override void printEntry()
        {
            Console.WriteLine($"Class: Int Constant Lexeme: {lexeme,-11} Token Type:{tokenType.ToString(),-10} Depth: {depth,-3} Offset: {offset,-3} Value: {value,-5}");
        }
    }
}
