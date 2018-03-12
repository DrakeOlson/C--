using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class FunctionEntry : TableEntry
    {
        int SizeOfLocal;
        int NumberOfParameters;
        VariableType ReturnType;
        LinkedList<VariableType> ParameterList;

        public override void printEntry()
        {
            Console.WriteLine($"{lexeme} {tokenType} {depth} {SizeOfLocal} {NumberOfParameters} {ReturnType} {ParameterList}");
        }
    }
}
