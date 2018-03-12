using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    class VariableEntry : TableEntry
    {
        public VariableType variableType;
        public int Offset;
        public int size;

        public override void printEntry()
        {
            Console.WriteLine($"{lexeme} {tokenType} {depth} {Offset} {size}");
        }
    }
}
