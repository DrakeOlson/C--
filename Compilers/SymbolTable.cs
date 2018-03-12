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
            LinkedList <TableEntry> found = lookup(lexeme);
            if(found != null && found.First.Value.depth == depth)
            {
                Console.WriteLine($"Error: The symbol {lexeme} with type {symbol.ToString()} is already inserted with depth {depth}.");
            }
            else if(found == null)
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
            else if(found != null && found.First.Value.depth != depth)
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

        public LinkedList<TableEntry> lookup(string lexeme)
        {
            return table[hash(lexeme)];
        }

        public void deleteDepth(int depth)
        {
            foreach(LinkedList<TableEntry> list in table.Values)
            {
                foreach(TableEntry value in list)
                {
                    if(value.depth == depth)
                    {
                        list.Remove(value);
                    }
                }
            }
        }

        public void writeTable(int depth)
        {
            foreach (LinkedList<TableEntry> list in table.Values)
            {
                foreach (TableEntry value in list)
                {
                    
                }
            }
        }

        private int hash(string lexeme)
        {
            return lexeme.GetHashCode() % tableSize;
        }
    }
}
