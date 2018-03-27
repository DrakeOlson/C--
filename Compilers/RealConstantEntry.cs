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
    public class RealConstantEntry : ConstantEntry
    {
        public float value;
        public override void printEntry()
        {
            Console.WriteLine($"Class: Float Constant Lexeme: {lexeme,-9} Token Type:{tokenType.ToString(),-10} Depth: {depth,-3} Offset: {offset,-3} Value Real: {value,-5}");
        }
    }
}
