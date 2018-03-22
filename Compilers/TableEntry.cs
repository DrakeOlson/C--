/*
 * Author: Drake Olson
 * Class: Compiler Construction
 * Instructor: Dr. Hamer
 * Date: 1/30/18
 * Description: This file holds base members for entry into the symbol table
 */

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
