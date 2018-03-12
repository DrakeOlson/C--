using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    class SymbolTable
    {
        const int tableSize = 211;
        Dictionary<int,LinkedList<TableEntry>> table;

        public SymbolTable()
        {
            table = new Dictionary<int, LinkedList<TableEntry>>(tableSize);
        }

        public void insert(string lexeme, Globals.Symbol symbol, int depth)
        {
            TableEntry found = lookup(lexeme);
            if(found == null)
            {
                LinkedList<TableEntry> lexemeList = new LinkedList<TableEntry>();
                lexemeList.AddFirst(new VariableEntry()
                {
                    lexeme = lexeme,
                    tokenType = symbol,
                    depth = depth,
                    variableType = TableEntry.VariableType.intType,
                    Offset = 0,
                    size = 4
                });
                table.Add(lexeme.GetHashCode() % tableSize,lexemeList);
            }
            else if(found != null && found.depth != depth)
            {
                table[hash(lexeme)].AddFirst(new VariableEntry()
                {
                    lexeme = lexeme,
                    tokenType = symbol,
                    depth = depth,
                    variableType = TableEntry.VariableType.intType,
                    Offset = 0,
                    size = 4                    
                });
            }
        }
        public TableEntry lookup(string lexeme)
        {
            LinkedList<TableEntry> foundList = table[hash(lexeme)];

            foreach(TableEntry t in foundList)
            {
                if(t.lexeme == lexeme)
                {
                    return t;
                }
            }

            return null;
        }

        public void deleteDepth(int depth)
        {
            foreach(LinkedList<TableEntry> list in table.Values)
            {
                foreach(TableEntry val in list)
                {
                    if(val.depth == depth)
                    {
                        list.Remove(val);
                    }
                }
            }
        }

        public void writeTable(int depth)
        {
            foreach (LinkedList<TableEntry> list in table.Values)
            {
                foreach (TableEntry val in list)
                {
                    val.printEntry();
                }
            }
        }

        private int hash(string lexeme)
        {
            return lexeme.GetHashCode() % tableSize;
        }
    }
}
