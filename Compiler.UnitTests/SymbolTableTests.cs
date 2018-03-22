using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Compiler.UnitTests
{
    [TestClass]
    public class SymbolTableTests
    {
        //Tests Lookup and insert and hash
        [TestMethod]
        public void Lookup_Insert_Hash()
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

            VariableEntry comparedEntry = new VariableEntry() { lexeme = "blue", tokenType = Globals.Symbol.idT, depth = 5 };
            TableEntry entry = testTable.lookup("blue");

            Assert.AreEqual(comparedEntry.lexeme, entry.lexeme);
            Assert.AreEqual(comparedEntry.tokenType, entry.tokenType);
            Assert.AreEqual(comparedEntry.depth, entry.depth);
        }

        //Tests Delete depth
        [TestMethod]
        public void Insert_Delete()
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
                foreach (TableEntry val in list)
                {
                    Assert.IsFalse(val.depth == 5);
                }
            }
        }
        [TestMethod]
        public void Delete_With_Empty_Table()
        {
            SymbolTable testTable = new SymbolTable();
            testTable.deleteDepth(3);
            Assert.IsTrue(testTable.table.Count == 0);
        }
        [TestMethod]
        public void LookUp_With_Item_Inserted()
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

            VariableEntry actual = new VariableEntry()
            {
                lexeme = "blue",
                tokenType = Globals.Symbol.idT,
                depth = 4,
                size = 4,
                variableType = TableEntry.VariableType.intType,
                Offset = 0
            };
            VariableEntry entry = testTable.lookup("blue") as VariableEntry;
            Assert.AreEqual(actual.Offset, entry.Offset);
            Assert.AreEqual(actual.tokenType, entry.tokenType);
            Assert.AreEqual(actual.depth, entry.depth);
            Assert.AreEqual(actual.variableType, entry.variableType);
            Assert.AreEqual(actual.size, entry.size);
            Assert.AreEqual(actual.lexeme, entry.lexeme);
        }
        [TestMethod]
        public void Lookup_Blank_Table()
        {
            SymbolTable testTable = new SymbolTable();
            VariableEntry entry = testTable.lookup("blue") as VariableEntry;
            Assert.AreEqual(entry, null);
        }
    }
}
