// Tests implemented by Ronald Foster for CS 3500, TODO: date

using SpreadsheetUtilities;

namespace FormulaTests
{
    /// <summary>
    /// Test class for Formula.cs
    /// </summary>
    [TestClass]
    public class FormulaTests
    {
        [TestMethod]
        public void NormalizedVars_GetVariables()
        {
            Formula f = new Formula("5 + x + X * y / Zd", s => s.ToUpper(), s => true);
            string vars = "";
            foreach (string v in f.GetVariables())
            {
                vars += "'" + v + "' ";
            }
            Assert.AreEqual("'X' 'Y' 'ZD' ", vars);
        }

        [TestMethod]
        public void NoNormalizer_GetVariables()
        {
            Formula f = new Formula("5 + x + X + y/ Zd");
            string vars = "";
            foreach (string v in f.GetVariables())
            {
                vars += "'" + v + "' ";
            }
            Assert.AreEqual("'x' 'X' 'y' 'Zd' ", vars);
        }

        [TestMethod]
        public void NoVars_GetVariables()
        {
            Formula f = new Formula("5 + 5");
            Assert.AreEqual(0, f.GetVariables().Count());
        }

        [TestMethod]
        public void NormalizedVars_ToString()
        {
            Formula f = new Formula("5 + x + X + y / Zd", s => s.ToUpper(), s => true);
            Assert.AreEqual("5+X+X+Y/ZD", f.ToString());
        }

        [TestMethod]
        public void NoNormalizer_ToString()
        {
            Formula f = new Formula("5 + x + X + y / Zd");
            Assert.AreEqual("5+x+X+y/Zd", f.ToString());
        }

        [TestMethod]
        public void NullObject_Equals()
        {
            Formula f = new Formula("5 + 5");
            Assert.IsFalse(f.Equals(null));
        }

        [TestMethod]
        public void ObjectIsNotAForumla_Equals()
        {
            Formula f = new Formula("5 + 5");
            Assert.IsFalse(f.Equals(new Random()));
        }

        [TestMethod]
        public void UnequalFormualsNormalizedIntoBeingEqual_Equals()
        {
            Formula f1 = new Formula("5 + XY + YZ");
            Formula f2 = new Formula("5 + xy + Yz", s => s.ToUpper(), s => true);
            Assert.IsTrue(f1.Equals(f2));
            Assert.IsFalse(f1 != f2);
            Assert.IsTrue(f1 == f2);
        }

        [TestMethod]
        public void UnequalFormulasNormalizedAndStillUnequal_Equals()
        {
            Formula f1 = new Formula("5 + Xy");
            Formula f2 = new Formula("5 + x", s => s.ToUpper(), s => true);
            Assert.IsFalse(f1.Equals(f2));
            Assert.IsTrue(f1 != f2);
            Assert.IsFalse(f1 == f2);
        }

        [TestMethod]
        public void UnequalUnormalizedFormulas_Equals()
        {

            Formula f1 = new Formula("5 + X");
            Formula f2 = new Formula("5 + x");
            Assert.IsFalse(f1.Equals(f2));
            Assert.IsFalse(f1 == f2);
            Assert.IsTrue(f1 != f2);
        }

        [TestMethod]
        public void EqualUnormalizedFormuals_Equals()
        {
            Formula f1 = new Formula("5 + x");
            Formula f2 = new Formula("5 + x");
            Assert.IsTrue(f1.Equals(f2));
            Assert.IsTrue(f1 == f2);
            Assert.IsFalse(f1 != f2);
        }

        // ---------------Evaluate Tests---------------
        [TestMethod]
        public void ValidLookup_Evaluate()
        {
            Formula f = new Formula("5 + x");
            Assert.AreEqual(7.0, f.Evaluate(s => 2));
        }

        [TestMethod]
        public void DivisionByZeroThroughLookup_Evaluate()
        {
            Formula f = new Formula("5 / x");
            Assert.IsInstanceOfType(f.Evaluate(s => 0), typeof(FormulaError));
        }

        [TestMethod]
        public void SingleNumber_Evaluate()
        {
            Formula f = new Formula("5");
            Assert.AreEqual(5.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void SingleVariable_Evaluate()
        {
            Formula f = new Formula("a2");
            Assert.AreEqual(5.0, f.Evaluate(s => 5));
        }

        [TestMethod]
        public void Addition_Evaluate()
        {
            Formula f = new Formula("5 + 3");
            Assert.AreEqual(8.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void Subtraction_Evaluate()
        {
            Formula f = new Formula("18-10");
            Assert.AreEqual(8.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void Multiplication_Evaluate()
        {
            Formula f = new Formula("2*4");
            Assert.AreEqual(8.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void Division_Evaluate()
        {
            Formula f = new Formula("15 /2 ");
            Assert.AreEqual(7.5, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void ArithmeticWithVariable_Evaluate()
        {
            Formula f = new Formula("2 +_1");
            Assert.AreEqual(6.0, f.Evaluate(s => 4));
        }

        [TestMethod]
        public void UnkownVariable_Evaluate()
        {
            Formula f = new Formula("2 + X1");
            Assert.IsInstanceOfType(f.Evaluate(s => { throw new ArgumentException("Unkown Variable"); }), typeof(FormulaError));
        }

        [TestMethod]
        public void LeftToRight_Evaluate()
        {
            Formula f = new Formula("2 * 6 + 3");
            Assert.AreEqual(15.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void OrderOfOperations_Evaluate()
        {
            Formula f = new Formula("2+6*3");
            Assert.AreEqual(20.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void ParenthesesTimes_Evalute()
        {
            Formula f = new Formula("(2+6)*3");
            Assert.AreEqual(24.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void PlusParentheses_Evaluate()
        {
            Formula f = new Formula("2+(3+5)");
            Assert.AreEqual(10.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void PlusComplex_Evaluate()
        {
            Formula f = new Formula("2+(3+5*9)");
            Assert.AreEqual(50.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void OperatorAfterParens_Evaluate()
        {
            Formula f = new Formula("(1*1)-2/2");
            Assert.AreEqual(0.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void ComplexTimesParentheses_Evaluate()
        {
            Formula f = new Formula("2+3*(3+5)");
            Assert.AreEqual(26.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void ComplexAndParentheses_Evaluate()
        {
            Formula f = new Formula("2+3*5+(3+4*8)*5+2");
            Assert.AreEqual(194.0, f.Evaluate(s => 0));
        }

        [TestMethod]
        public void DivideByZero_Evaluate()
        {
            Formula f = new Formula("5 / 0");
            Assert.IsInstanceOfType(f.Evaluate(s => 0), typeof(FormulaError));
        }

        [TestMethod]
        public void DivisionByZeroFromParentheses_Evaluate()
        {
            Formula f = new Formula("2 / (5 - 5)");
            Assert.IsInstanceOfType(f.Evaluate(s => 0), typeof(FormulaError));
        }

        [TestMethod]
        public void ComplexMultiVar_Evaluate()
        {
            Formula f = new Formula("y1*3-8/2+4*(8-9*2)/1*x7");
            Assert.AreEqual(-32.0, f.Evaluate(s => (s == "x7") ? 1 : 4));
        }

        [TestMethod]
        public void ComplexNestedParensRight_Evaluate()
        {
            Formula f = new Formula("x1+(x2+(x3+(x4+(x5+x6))))");
            Assert.AreEqual(6.0, f.Evaluate(s => 1));
        }

        [TestMethod]
        public void ComplexNestedParensLef_Evaluate()
        {
            Formula f = new Formula("((((x1+x2)+x3)+x4)+x5)+x6");
            Assert.AreEqual(12.0, f.Evaluate(s => 2));
        }

        [TestMethod]
        public void RepeatedVar_Evaluate()
        {
            Formula f = new Formula("a4-a4*a4/a4");
            Assert.AreEqual(0.0, f.Evaluate(s => 3));
        }

        // ---------------Constructor Tests---------------
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void SingleOperator_Constructor()
        {
            Formula f = new Formula("+");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void ExtraOperator_Constructor()
        {
            Formula f = new Formula("2+5+");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void ExtraRightParentheses_Constructor()
        {
            Formula f = new Formula("2+5*7)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void NormalizerReturnsIllegalVariable_Constructor()
        {
            Formula f = new Formula("5 + _5", s => "$", s => true);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void IsValidReturnsFalse_Constructor()
        {
            Formula f = new Formula("5 + _5", s => s, s => false);
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void InvalidVariable_Constructor()
        {
            Formula f = new Formula("5 + _5", s => s, s =>
            {
                foreach (char c in s)
                {
                    if (c == '_')
                    {
                        return false;
                    }
                }
                return true;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void RightParensNoOperator_Constructor()
        {
            Formula f = new Formula("5+7+(5)8");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Empty_Constructor()
        {
            Formula f = new Formula("");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void LeftParensWithoutRighParens_Constructor()
        {
            Formula f = new Formula("(8*7-5");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void LeftParensNoVar_Constructor()
        {
            Formula f = new Formula("5+7(+5)+8");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void VarWhereUnexpected_Constructor()
        {
            Formula f = new Formula("5+(a7 - 5)a7 - 1");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void RightParensWhereUnexpected_Constructor()
        {
            Formula f = new Formula("5+(7 -)5");
        }

        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void InvalidToken_Constructor()
        {
            Formula f = new Formula("5 + $");
        }
    }
}