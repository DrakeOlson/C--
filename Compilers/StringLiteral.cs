using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class StringLiteral : TableEntry
    {

        public string assemblyLiteral;
        public string tempVarName;

        public StringLiteral()
        {
            assemblyLiteral = $"{Globals.Attribute},\"$\"";
        }

        public override void printEntry()
        {
            
        }
    }
}
