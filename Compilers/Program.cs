/*
 * Author: Drake Olson
 * Class: Compiler Construction
 * Instructor: Dr. Hamer
 * Date: 1/30/18
 * Description: Using techiniques of current compilers, this program uses the lexical
 *              analyzer to start off process for a subset of the C++ language C--. 
 *              This compiler will be built upon.
 */
using System;
using System.Collections.Generic;

namespace Compiler
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            SymbolTable testTable = new SymbolTable();
            testTable.insert("red", Globals.Symbol.idT, 1);
            testTable.insert("red", Globals.Symbol.idT, 1);
            testTable.insert("red", Globals.Symbol.idT, 2);
            testTable.insert("red", Globals.Symbol.idT, 3);
            testTable.insert("red", Globals.Symbol.idT, 4);
            testTable.insert("red", Globals.Symbol.idT, 5);

            testTable.insert("blue", Globals.Symbol.idT, 1);
            testTable.insert("blue", Globals.Symbol.idT, 1);
            testTable.insert("blue", Globals.Symbol.idT, 2);
            testTable.insert("blue", Globals.Symbol.idT, 3);
            testTable.insert("blue", Globals.Symbol.idT, 4);
            testTable.insert("blue", Globals.Symbol.idT, 5);

            testTable.deleteDepth(5);

            foreach (LinkedList<TableEntry> list in testTable.table.Values)
            {
                if(list != null || list.Count > 0)
                {
                    foreach (TableEntry val in list)
                    {
                        if (val.depth == 5)
                        {
                            Console.WriteLine("WRONG");
                        }
                    }
                }
            }
            Console.Read();
        }
    }
}
