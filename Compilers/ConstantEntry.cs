
using System;
/// <summary>
/// Contains the abstract class for a Constant Entry. 
/// Children: Real,Integer
/// </summary>
namespace Compiler
{
    public class ConstantEntry : TableEntry
    {
        public int offset;
        public int BPOffset;
        public override void printEntry()
        {
            Console.WriteLine($"Class: Constant Lexeme: {lexeme,-15} Token Type:{tokenType.ToString(),-5} Depth: {depth,-3} Offset: {offset,5}");
        }
    }
}
