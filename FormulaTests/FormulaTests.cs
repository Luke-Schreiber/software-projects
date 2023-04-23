
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;

namespace FormulaTests
{
    [TestClass]
    public class FormulaTests
    {
        [TestMethod]
        public void CorrectFormulaSyntaxTest()
        {

            new Formula("3+.4*.123098e001/(.123098E-001)-_aasdlfkj123");
            new Formula("zZ4+(56*(.2340+0.123E8-(A1_+3)))");
            new Formula("0/A___111111/.2e+1/098098.3E001*(5-a7)");
        }

        [TestMethod]
        public void IncorrectFormulaSyntaxTest()
        {
            static bool validate(string s)
            {
                return false;
            }
            Assert.ThrowsException<FormulaFormatException>(() => new Formula(""));
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("-5"));
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("5-"));
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("a5", s => s, validate));
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("5 5"));
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("a5 5"));
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("3+.4**.123098e001/(.123098E-001)-_aasdlfkj123"));
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("zZ4!+(56*(.2340+0.123E8-(A1_+3)))"));
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("(5-a7))"));
            Assert.ThrowsException<FormulaFormatException>(() => new Formula(")(5-a7)"));
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("((5-a7)"));
            Assert.ThrowsException<FormulaFormatException>(() => new Formula("7a + 4"));
        }

        [TestMethod]
        public void ToStringTest()
        {
            static string normalize(string s)
            {
                String result = s.ToUpper();
                return result;
            }
            Formula f1 = new Formula("a7 + bb70 + ABc30", normalize, s => true);
            Formula f2 = new Formula("a7 + bB70 + ABC30 + 6.0000", normalize, s => true);
            Formula f3 = new Formula(f2.ToString(), normalize, s => true);
            Formula f4 = new Formula("1e+6");
            Assert.AreEqual("A7+BB70+ABC30", f1.ToString());
            Assert.IsTrue(f2.Equals(f3));
            Assert.AreEqual("A7+BB70+ABC30+6", f2.ToString());
            Assert.AreEqual("1000000", f4.ToString());
        }

        [TestMethod]
        public void GetVariablesTest()
        {
            static string normalize(string s)
            {
                String result = s.ToUpper();
                return result;
            }
            Formula lower = new Formula("a7 + bb70 + ABc30", normalize, s => true);
            IEnumerator<string> enumerator = lower.GetVariables().GetEnumerator();
            enumerator.MoveNext();
            Assert.AreEqual("A7", enumerator.Current);
            enumerator.MoveNext();
            Assert.AreEqual("BB70", enumerator.Current);
            enumerator.MoveNext();
            Assert.AreEqual("ABC30", enumerator.Current);
        }

        [TestMethod]
        public void EqualsTest()
        {
            static string normalize(string s)
            {
                String result = s.ToUpper();
                return result;
            }
            static string normalize2(string s)
            {
                String result = s.ToLower();
                return result;
            }

            Formula f1 = new Formula("a7 + bb70 + ABc30 + 6.00", normalize, s => true);
            Formula f2 = new Formula("A7 + Bb70 + aBC30 + 6", normalize, s => true);
            Formula f3 = new Formula("a7 + bB70 + ABC30 + 6.0000", normalize2, s => true);
            Formula f4 = new Formula("a7 + bB70 + ABC30 + 6.0001", normalize, s => true);
            Formula f5 = new Formula("a7 + bB70 + 6 + ABC30", normalize2, s => true);
            Formula f6 = new Formula("a7 + bB70 + ABC30 + 6.0000");
            Formula f7 = new Formula("A7 + bB70 + ABC30 + 6.0000");
            Object? obj = null;

            String one = f1.ToString();
            String two = f2.ToString();
            String tthreo = f3.ToString();

            Assert.IsFalse(f7.Equals(f6));
            Assert.IsTrue(f1.Equals(f2));
            Assert.IsTrue(f1 == f2);
            Assert.IsFalse(f1 == f3);
            Assert.IsFalse(f2.Equals(f3));
            Assert.IsFalse(f3.Equals(f4));
            Assert.IsFalse(f3.Equals(obj));
            Assert.IsFalse(f4.Equals("string"));
            Assert.IsTrue(f4 != f1);
            Assert.IsTrue(f4 != f2);
            Assert.IsFalse(f1 == f6);

        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            static string normalize(string s)
            {
                String result = s.ToUpper();
                return result;
            }
            static string normalize2(string s)
            {
                String result = s.ToLower();
                return result;
            }
            Formula f1 = new Formula("a7 + bb70 + ABc30 + 6.00", normalize, s => true);
            Formula f2 = new Formula("A7 + Bb70 + aBC30 + 6", normalize, s => true);
            Formula f3 = new Formula("a7 + bB70 + ABC30 + 6.0000", normalize2, s => true);
            Formula f4 = new Formula("a7 + bB70 + ABC30 + 6.0001", normalize, s => true);
            Formula f5 = new Formula("a7 + bB70 + 6 + ABC30", normalize2, s => true);
            Formula f6 = new Formula("A7 + BB70 + ABC30 + 6.0000");

            Assert.IsTrue(f1.GetHashCode() == f2.GetHashCode());
            Assert.IsFalse(f1.GetHashCode() == f3.GetHashCode());
            Assert.IsTrue(f2.GetHashCode() == f6.GetHashCode());
            Assert.IsFalse(f2.GetHashCode() == f4.GetHashCode());
            Assert.IsFalse(f2.GetHashCode() == f5.GetHashCode());
        }

        [TestMethod]
        public void EvaluateTest()
        {
            static string normalize(string s)
            {
                String result = s.ToUpper();
                return result;
            }

            static double lookup(string s)
            {
                if (s.Equals("A7"))
                    return 5;
                if (s.Equals("BB70"))
                    return 10;
                if (s.Equals("ABC30"))
                    return 1;
                return 0;
            }
            Formula f1 = new Formula("a7 / bb70 * ABc30 + 6.00", normalize, s => true);
            Formula f2 = new Formula("A7 * Bb70 / aBC30 - 6", normalize, s => true);
            Formula f3 = new Formula("a7 * (bB70 / ABC30) + 6.0000 + 7", normalize, s => true);
            Formula f4 = new Formula("a7 + bB70 + ABC30 + 6.0001", normalize, s => true);
            Formula f5 = new Formula("a7 + bB70 + 6 + ABC30", normalize, s => true);
            Formula f6 = new Formula("A7-(BB70 -(ABC30 - 6.0000))");
            Formula f7 = new Formula("A7+ (5E6 -(ABC30 - 6.0000))");
            Formula f8 = new Formula(".5E+6 + 5e6 + 5e+6");


            Assert.AreEqual(f1.Evaluate(lookup), 6.5);
            Assert.AreEqual(f2.Evaluate(lookup), 44.0);
            Assert.AreEqual(f3.Evaluate(lookup), 63.0);
            Assert.AreEqual(f4.Evaluate(lookup), 22.0001);
            Assert.AreEqual(f5.Evaluate(lookup), 22.0);
            Assert.AreEqual(f6.Evaluate(lookup), -10.0);
            Assert.AreEqual(f7.Evaluate(lookup), 5000010.0);
            Assert.AreEqual(f8.Evaluate(lookup), 10500000.0);

        }
    }
}