using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public abstract class TableEntry
    {
        public enum VariableType { charType, intType, floatType }
        public enum EntryType { constEntry, varEntry, functionType }

        public Globals.Symbol tokenType;
        public string lexeme;
        public int depth;

        public abstract void printEntry();
    }
}
