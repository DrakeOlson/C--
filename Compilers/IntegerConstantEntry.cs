using System;
namespace Compiler
{
    public class IntegerConstantEntry : ConstantEntry
    {
        int value;

        public override void printEntry()
        {
            Console.WriteLine($"{lexeme} {tokenType} {depth} {offset} {value}");
        }
    }
}
