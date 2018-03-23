/*
 * Author: Drake Olson
 * Class: Compiler Construction
 * Instructor: Dr. Hamer
 * Date: 1/30/18
 * Description: This Class contains members for entry of a symbol table for the parser to hold symbols
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler
{
    public class SymbolTable
    {
        private const int tableSize = 211;
        public Dictionary<int,LinkedList<TableEntry>> table;
        /// <summary>
        /// Default Constructor
        /// </summary>
        public SymbolTable()
        {
            table = new Dictionary<int, LinkedList<TableEntry>>(tableSize);
        }
        /// <summary>
        /// Takes a table entry,hashes and inserts it into the symbol table
        /// </summary>
        /// <param name="entry"></param>
        public void insert(TableEntry entry)
        {
            TableEntry found = lookup(entry.lexeme);
            if (found == null)
            {
                LinkedList<TableEntry> lexemeList = new LinkedList<TableEntry>();
                lexemeList.AddFirst(entry);
                table.Add(hash(entry.lexeme), lexemeList);
            }
            else if (found != null)
            {
                table[hash(entry.lexeme)].AddFirst(entry);
            }
        }
        /// <summary>
        /// Takes the base items of a Table Entry, hashes and insert it into the symbol table
        /// </summary>
        /// <param name="lexeme"></param>
        /// <param name="symbol"></param>
        /// <param name="depth"></param>
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
                table.Add(hash(lexeme),lexemeList);
            }
            else if(found != null)
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
        /// <summary>
        /// Find a lexeme in the symbol table and return the entry
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns></returns>
        public TableEntry lookup(string lexeme)
        {
            try
            {
                LinkedList<TableEntry> foundList = table[hash(lexeme)];

                foreach (TableEntry t in foundList)
                {
                    if (t.lexeme == lexeme)
                    {
                        return t;
                    }
                }
            }
            catch (KeyNotFoundException)
            {
                return null;
            }

            return null;
        }
        /// <summary>
        /// Given a depth delete all symbols in the symbol table with the given depth
        /// </summary>
        /// <param name="depth"></param>
        public void deleteDepth(int depth)
        {
            foreach(LinkedList<TableEntry> list in table.Values)
            {
                if(list != null && list.Count > 0)
                {
                    foreach (TableEntry val in list.ToList())
                    {
                        if (val.depth == depth)
                        {
                            list.Remove(val);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Write the table out to the screen, used for debugging
        /// </summary>
        /// <param name="depth"></param>
        public void writeTable(int depth)
        {
            Console.WriteLine(String.Format("{0,-20} {1,-10} {2,-3}", "Lexeme", "TokenType", "Depth"));
            Console.WriteLine("----------------------------------------");
            foreach (LinkedList<TableEntry> list in table.Values)
            {
                foreach (TableEntry val in list)
                {
                    if(val.depth == depth)
                    {
                        val.printEntry();
                    }
                }
            }
        }
        /// <summary>
        /// Given a string hash the string and return it with an int that will fit in the symbol table
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns></returns>
        private int hash(string lexeme)
        {
            return lexeme.GetHashCode() % tableSize;
        }
    }
}
