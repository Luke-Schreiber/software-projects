using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using SpreadsheetUtilities;
using System.Linq;
using Newtonsoft.Json;

namespace SpreadsheetTests
{
    [TestClass]
    public class SpreadsheetTests
    {
        [TestMethod]
        public void CreateSpreadsheetTest()
        {
            bool isValid(string s)
            {
                return true;
            }
            string Normalize(string s)
            {
                return s.ToUpper();
            }
            string sheet = "{\"cells\":{\"A1\":{\"stringForm\":\"5\"},\"B3\":{\"stringForm\":\"=A1+2\"}},\"Version\":\"default\"}";
            File.WriteAllText("save.txt", sheet);
            AbstractSpreadsheet s1 = new Spreadsheet();
            AbstractSpreadsheet s2 = new Spreadsheet(isValid, Normalize, "1");
            AbstractSpreadsheet s3 = new Spreadsheet("save.txt", isValid, Normalize, "default");
            Assert.AreEqual(s2.IsValid, isValid);
            Assert.AreEqual(s2.IsValid, isValid);
            Assert.AreEqual(s3.IsValid, isValid);
            Assert.AreEqual(s3.IsValid, isValid);
            Assert.AreEqual("1", s2.Version);
            Assert.AreEqual("default", s3.Version);
        }
        [TestMethod]
        public void SetContentsOfCellTest()
        {
            Spreadsheet s1 = new Spreadsheet();
            s1.SetContentsOfCell("a1", "=b1+c1-f1");
            s1.SetContentsOfCell("b1", "=d1*2");
            s1.SetContentsOfCell("d1", "=e1/2");
            s1.SetContentsOfCell("e1", "=c1 + 1");
            s1.SetContentsOfCell("f1", "=e1 + 2");
            List<string> deps = s1.SetContentsOfCell("c1", "3.0").ToList<string>();

            List<string> expect = new List<string>() { "a1", "b1", "d1", "e1", "f1", "c1" };

            foreach (string dep in expect)
                Assert.IsTrue(deps.Contains(dep));
            Assert.IsTrue(deps.Count == 6);

        }
            [TestMethod]
        public void GetCellContentsTest()
        {
            Spreadsheet s1 = new Spreadsheet();
            s1.SetContentsOfCell("a1", "=b1+c1");
            s1.SetContentsOfCell("c1", "2.0");
            s1.SetContentsOfCell("c1", "3.0");
            s1.SetContentsOfCell("b1", "nottext");
            s1.SetContentsOfCell("b1", "text");
            Assert.AreEqual("b1+c1", s1.GetCellContents("a1").ToString());
            Assert.AreEqual(3.0, s1.GetCellContents("c1"));
            Assert.AreEqual("text", s1.GetCellContents("b1"));
            Assert.AreEqual("", s1.GetCellContents("e1"));
        }
        [TestMethod]
        public void GetNamesOfNonEmptyCellsTest()
        {
            Spreadsheet s1 = new Spreadsheet();
            Assert.IsTrue(s1.GetNamesOfAllNonemptyCells().ToArray().Length == 0);
            Spreadsheet s2 = new Spreadsheet();
            s2.SetContentsOfCell("f1", "");
            s1.SetContentsOfCell("a1", "=b1+c1-f1");
            s1.SetContentsOfCell("b1", "=d1*2");
            s1.SetContentsOfCell("d1", "=e1/2");
            s1.SetContentsOfCell("e1", "text");
            s1.SetContentsOfCell("f1", "0");

            string[] expected = { "a1", "b1", "d1", "e1", "f1" };
            string[] actual = s1.GetNamesOfAllNonemptyCells().ToArray();
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }

            Assert.IsTrue(s2.GetNamesOfAllNonemptyCells().ToArray().Length == 0);
        }
        [TestMethod]
        public void SetContentsThrowsExceptionTest()
        {
            Spreadsheet s1 = new Spreadsheet();

            Assert.ThrowsException<InvalidNameException>(() => s1.SetContentsOfCell("1a", "=b1+c1-f1"));
            Assert.ThrowsException<InvalidNameException>(() => s1.SetContentsOfCell("a??", "=d1*2"));
            Assert.ThrowsException<InvalidNameException>(() => s1.SetContentsOfCell("1a", "=b1+c1-f1"));
            Assert.ThrowsException<InvalidNameException>(() => s1.SetContentsOfCell("9d9", "3.0"));
            Assert.ThrowsException<InvalidNameException>(() => s1.SetContentsOfCell(".", "text"));
            Assert.ThrowsException<InvalidNameException>(() => s1.SetContentsOfCell("", "text"));
            Assert.ThrowsException<InvalidNameException>(() => s1.GetCellContents(""));
            Assert.ThrowsException<CircularException>(() => s1.SetContentsOfCell("a1", "=a1"));
        }
        [TestMethod]
        public void GetCellValueTest()
        {
            Spreadsheet s1 = new Spreadsheet();
            s1.SetContentsOfCell("a1", "=a2+a3");
            s1.SetContentsOfCell("a2", "2");
            s1.SetContentsOfCell("a2", "=a3+3");
            s1.SetContentsOfCell("a3", "20");
            s1.SetContentsOfCell("a4", "=a1+a5");
            s1.SetContentsOfCell("a5", "=a2+1");
            s1.SetContentsOfCell("a6", "text");
            s1.SetContentsOfCell("a7", "=b5");

            Assert.AreEqual(43.0, s1.GetCellValue("a1"));
            Assert.AreEqual(23.0, s1.GetCellValue("a2"));
            Assert.AreEqual(20.0, s1.GetCellValue("a3"));
            Assert.AreEqual(67.0, s1.GetCellValue("a4"));
            Assert.AreEqual(24.0, s1.GetCellValue("a5"));
            Assert.AreEqual("text", s1.GetCellValue("a6"));
            Assert.AreEqual("", s1.GetCellValue("a8"));
            Assert.IsTrue(s1.GetCellValue("a7") is FormulaError);
        }
        [TestMethod]
        public void GetCellValueExceptionTest()
        {
            bool isValid(string s)
            {
                if (s == "A1")
                    return true;
                else return false;
            }
            string Normalize(string s)
            {
                return s.ToUpper();
            }

            AbstractSpreadsheet s1 = new Spreadsheet();
            AbstractSpreadsheet s2 = new Spreadsheet(isValid, Normalize, "default");
            s2.SetContentsOfCell("a1", "text");
            Assert.ThrowsException<InvalidNameException>(() => s1.GetCellValue("7a"));
            Assert.ThrowsException<InvalidNameException>(() => s2.GetCellValue("a2"));
            Assert.ThrowsException<InvalidNameException>(() => s2.GetCellValue(""));
        }
            [TestMethod]
        public void GetCellValueFormulaErrorTest()
        {
            Spreadsheet s1 = new Spreadsheet();
            s1.SetContentsOfCell("a1", "=a2");
            Assert.IsTrue(s1.GetCellValue("a1") is FormulaError);
            s1.SetContentsOfCell("a2", "=a3");
            Assert.IsTrue(s1.GetCellValue("a1") is FormulaError);
            s1.SetContentsOfCell("a3", "");
            Assert.IsTrue(s1.GetCellValue("a1") is FormulaError);
        }
        [TestMethod]
        public void JsonTest()
        {
            // Save this spreadsheet as test.txt
            Spreadsheet s1 = new Spreadsheet();
            s1.SetContentsOfCell("A1", "5");
            s1.SetContentsOfCell("B3", "=A1+2");
            s1.Save("test.txt");

            // Save this complex spreadsheet as complex.txt
            Spreadsheet com = new Spreadsheet(s => true, s => s, "version");
            com.SetContentsOfCell("A1", "5.0");
            com.SetContentsOfCell("A2", "6.0");
            com.SetContentsOfCell("B3", "=A1+2");
            com.SetContentsOfCell("b4", "=B3+2");
            com.SetContentsOfCell("b5", "text");
            com.Save("complex.txt");

            // Write Json string to file.
            string sheet = "{\"cells\":{\"A1\":{\"stringForm\":\"5\"},\"B3\":{\"stringForm\":\"=A1+2\"}},\"Version\":\"default\"}";
            File.WriteAllText("save.txt", sheet);

            // Create two spreadsheets, save s2 as test2.txt, and create a new spreadsheet from test2.txt.
            AbstractSpreadsheet ss = new Spreadsheet("save.txt", s => true, s => s, "default");
            AbstractSpreadsheet s2 = new Spreadsheet("test.txt", s => true, s => s, "default");
            s2.Save("test2.txt");
            AbstractSpreadsheet s3 = new Spreadsheet("test2.txt", s => true, s => s, "default");

            // Check all the values of ss.
            IEnumerator<string> ssStrings = ss.GetNamesOfAllNonemptyCells().GetEnumerator();
            ssStrings.MoveNext();
            Assert.AreEqual("A1", ssStrings.Current);
            Assert.AreEqual(5, (double)ss.GetCellContents("A1"));
            ssStrings.MoveNext();
            Assert.AreEqual("B3", ssStrings.Current);
            Assert.IsTrue(ss.GetCellContents("B3").ToString() == "A1+2");

            // Check all the values of s2.
            IEnumerator<string> s2Strings = s2.GetNamesOfAllNonemptyCells().GetEnumerator();
            s2Strings.MoveNext();
            Assert.AreEqual("A1", s2Strings.Current);
            Assert.AreEqual(5, (double)s2.GetCellContents("A1"));
            s2Strings.MoveNext();
            Assert.AreEqual("B3", s2Strings.Current);
            Assert.IsTrue(s2.GetCellContents("B3").ToString() == "A1+2");

            // Assert that s2 == s3 and complex.txt was saved correctly.
            string s2Serialized = JsonConvert.SerializeObject(s2);
            string s3Serialized = JsonConvert.SerializeObject(s3);
            Assert.AreEqual(s3Serialized, s2Serialized);
            Assert.AreEqual(File.ReadAllText("complex.txt"), JsonConvert.SerializeObject(com));
        }
        [TestMethod]
        public void JsonExceptionTest()
        {
            Spreadsheet s1 = new Spreadsheet();
            string incorrectJson = "{ ! }";
            string incorrectJson2 = "{\"cells\":{\"1A\":{\"stringForm\":\"5\"},\"B3\":{\"stringForm\":\"=A1+2\"}},\"Version\":\"default\"}";
            string incorrectJson3 = "{\"cells\":{\"A1\":{\"stringForm\":\"5\"},\"B3\":{\"stringForm\":\"=B3\"}},\"Version\":\"default\"}";
            string incorrectJson4 = "{\"cells\":{\"A1\":{\"stringForm\":\"5\"},\"B3\":{\"stringForm\":\"=A1!\"}},\"Version\":\"default\"}";

            File.WriteAllText("wrong.txt", incorrectJson);
            File.WriteAllText("wrong2.txt", incorrectJson2);
            File.WriteAllText("wrong3.txt", incorrectJson3);
            File.WriteAllText("wrong4.txt", incorrectJson4);
            Assert.ThrowsException<SpreadsheetReadWriteException>(() => new Spreadsheet("wrong.txt", s => true, s => s, "default"));
            Assert.ThrowsException<SpreadsheetReadWriteException>(() => new Spreadsheet("wrong2.txt", s => true, s => s, "wrong"));
            Assert.ThrowsException<SpreadsheetReadWriteException>(() => new Spreadsheet("wrong2.txt", s => true, s => s, "default"));
            Assert.ThrowsException<SpreadsheetReadWriteException>(() => new Spreadsheet("wrong3.txt", s => true, s => s, "default"));
            Assert.ThrowsException<SpreadsheetReadWriteException>(() => new Spreadsheet("wrong4.txt", s => true, s => s, "default"));
        }

        [TestMethod]
        public void ChagnedTest()
        {
            Spreadsheet s1 = new Spreadsheet();
            Assert.IsTrue(s1.Changed == false);
            s1.SetContentsOfCell("A1", "5");
            Assert.IsTrue(s1.Changed == true);
            s1.Save("test.txt");
            Assert.IsTrue(s1.Changed == false);

            // Check if changed is correct with the other two constructors.
            Spreadsheet s2 = new Spreadsheet(s => true, s => s, "default");
            Assert.IsTrue(s2.Changed == false);
            s2.SetContentsOfCell("A1", "5");
            Assert.IsTrue(s2.Changed == true);
            s2.Save("test.txt");
            Assert.IsTrue(s2.Changed == false);

            string sheet = "{\"cells\":{\"A1\":{\"stringForm\":\"5\"},\"B3\":{\"stringForm\":\"=A1+2\"}},\"Version\":\"default\"}";
            File.WriteAllText("save.txt", sheet);

            Spreadsheet s3 = new Spreadsheet("test.txt", s => true, s => s, "default");
            Assert.IsTrue(s3.Changed == false);
            s3.SetContentsOfCell("A1", "5");
            Assert.IsTrue(s3.Changed == true);
            s3.Save("test.txt");
            Assert.IsTrue(s3.Changed == false);
        }
        [TestMethod]
        public void StressTest()
        {
            Spreadsheet s = new Spreadsheet();
            for (int i = 0; i < 500; i++)
            {
                s.SetContentsOfCell("A1" + i, "=A1" + (i + 1) + " + 1");
            }
            s.SetContentsOfCell("A1500", "0");
            Assert.AreEqual(500.0, s.GetCellValue("A10"));
        }
    }
}