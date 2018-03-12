using System;
namespace Compiler
{
    public class RealConstantEntry : ConstantEntry
    {
        float value;
        public override void printEntry()
        {
            Console.WriteLine($"{lexeme} {tokenType} {depth} {offset} {value}");
        }
    }
}
