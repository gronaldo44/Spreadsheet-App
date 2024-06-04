// Implemented by Ronald Foster for CS 3500 in September 2022

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Markup;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax (without unary preceeding '-' or '+'); 
    /// variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        private string formula;

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {
        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            this.formula = "";
            int rightParensCount = 0;
            int leftParensCount = 0;
            bool expectedNum = true;    // the first token should be either a number or a '('
            foreach (string t in GetTokens(formula))
            {
                double num;
                bool isNum = Double.TryParse(t, out num);
                if (IsVar(t))
                {
                    if (!expectedNum)
                    {
                        throw new FormulaFormatException("Expected an operator but found: " + t);
                    }
                    string normalizedToken = normalize(t);
                    if (!IsVar(normalizedToken))
                    {
                        throw new FormulaFormatException("Normalizer is invalidating variables");
                    }
                    if (!isValid(normalizedToken))
                    {
                        throw new FormulaFormatException("'" + normalizedToken + "' is not a valid variable.");
                    }
                    this.formula += normalizedToken;
                    expectedNum = false;    // the next token should be either a ')', '+', '-', '*', or '/'
                }
                else if (isNum)
                {
                    if (!expectedNum)
                    {
                        throw new FormulaFormatException("Expected an operator but found: " + t);
                    }
                    this.formula += num.ToString();
                    expectedNum = false;    // the next token should be either a ')', '+', '-', '*', or '/'
                }
                else if (t == "(")
                {
                    if (!expectedNum)
                    {
                        throw new FormulaFormatException("Expected an operator but found: " + t);
                    }
                    this.formula += t;
                    leftParensCount++;
                    expectedNum = true;     // the next token should be either a number or '('
                }
                else if (t == ")")
                {
                    if (expectedNum)
                    {
                        throw new FormulaFormatException("Expected a number or '(' but found: " + t);
                    }
                    this.formula += t;
                    rightParensCount++;
                    expectedNum = false;    // the next token should be either a ')', '+', '-', '*', or '/'
                }
                else if (t == "+" || t == "-" || t == "*" || t == "/")
                {
                    if (expectedNum)
                    {
                        throw new FormulaFormatException("Expected number or '(' but found: " + t);
                    }
                    this.formula += t;
                    expectedNum = true;     // the next token should be either a number or '('
                }
                else
                {   
                    throw new FormulaFormatException("Invalid token: " + t);
                }
            }
            // Final checks
            if (this.formula.Length == 0)
            {
                throw new FormulaFormatException("There must be at least one token");
            }
            if (leftParensCount != rightParensCount)
            {
                throw new FormulaFormatException("There must be an equal amount of left and right parentheses");
            }
            if (expectedNum)
            {
                throw new FormulaFormatException("Invalid ending token: " + this.formula.Last());
            }
        }

        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup) 
        {
            // create tokens
            Stack<string> vals = new();
            Stack<string> opers = new();
            string[] tokens = Regex.Split(formula, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            // check the tokens
            foreach (string t in tokens)
            {
                double tokenNum;
                bool isNum = Double.TryParse(t, out tokenNum);
                if (isNum)
                {
                    object resultant = EvaluateNumericToken(tokenNum, opers, vals);
                    if (!PushedResult(resultant, vals))
                    {
                        return resultant;   // division by zero occurred
                    }
                }
                else if (IsVar(t))
                {
                    try
                    {
                        object resultant = EvaluateNumericToken(lookup(t), opers, vals);
                        if (!PushedResult(resultant, vals))
                        {
                            return resultant;   // divison by zero occurred
                        }
                    }
                    catch (Exception e)
                    {
                        return new FormulaError("Exception caught when looking up var '" + t + "'\nError: " + e.Message);
                    }
                }
                else if (IsOper(t, "+", "-"))
                {
                    if (opers.IsOnTop("+", "-"))
                    {
                        if (vals.Count >= 2)
                        {
                            object resultant = Operate(Double.Parse(vals.Pop()), opers.Pop(), Double.Parse(vals.Pop()));
                            vals.Push((string)resultant);
                        }
                    }
                    opers.Push(t);
                }
                else if (IsOper(t, "*", "/"))
                {
                    opers.Push(t);
                }
                else if (t == "(")
                {
                    opers.Push(t);
                }
                else if (t == ")")
                {
                    if (opers.IsOnTop("+", "-"))
                    {
                        object resultant = Operate(Double.Parse(vals.Pop()), opers.Pop(), Double.Parse(vals.Pop()));
                        vals.Push((string)resultant);
                    }
                    opers.Pop();
                    if (opers.IsOnTop("*", "/"))
                    {
                        object resultant = Operate(Double.Parse(vals.Pop()), opers.Pop(), Double.Parse((vals.Pop())));
                        if (!PushedResult(resultant, vals))
                        {
                            return resultant;   // division by zero occurred
                        }
                    }
                }
            }

            // last token has been evaluated
            if (opers.Count != 0)
            {
                object resultant = Operate(Double.Parse(vals.Pop()), opers.Pop(), Double.Parse((vals.Pop())));
                PushedResult(resultant, vals);
            }
            return Double.Parse(vals.Pop());
        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            HashSet<string> result = new();
            foreach (string token in GetTokens(formula))
            {
                if (IsVar(token))
                {
                    result.Add(token);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            return formula;
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is Formula))
            {
                return false;
            }
            Formula f = (Formula)obj;   // this is the only way I could figure out how to use Formula.ToString()
            return f.GetHashCode() == this.GetHashCode();
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            return f1.Equals(f2);
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            return !f1.Equals(f2);
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }

        }

        /// <summary>
        /// Pushes the result if possible and returns true; otherwise, returns false
        /// </summary>
        /// <param name="resultant"></param>
        /// <param name="vals"></param>
        /// <returns></returns>
        private bool PushedResult(object resultant, Stack<string> vals)
        {
            if (resultant.GetType() == typeof(string))
            {
                vals.Push((string)resultant);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Is this token a variable?
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool IsVar(string t)
        {
            string varPattern = "^[a-zA-Z_][a-zA-Z0-9_]*$";
            return Regex.IsMatch(t, varPattern);
        }

        /// <summary>
        /// Evaluates a numeric token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="opers"></param>
        /// <param name="vals"></param>
        /// <returns></returns>
        private object EvaluateNumericToken(double t, Stack<string> opers, Stack<string> vals)
        {
            if (opers.IsOnTop("*", "/"))
            {
                return Operate(t, opers.Pop(), Double.Parse(vals.Pop()));
            }
            else
            {
                return t.ToString();
            }
        }

        /// <summary>
        /// Returns the result of the second integer operated on by the first onto the values stack
        /// 
        /// The result is a FormulaError if a divsion by zero occurs
        /// </summary>
        /// <param name="s"></param>
        /// <param name="x"></param>
        /// <param name="oper"></param>
        /// <param name="y"></param>
        private object Operate(double y, string oper, double x)
        {
            if (oper == "*")
            {
                return (x * y).ToString();
            }
            else if (oper == "/")
            {
                if (y == 0)
                {
                    return new FormulaError("Division by zero");
                }

                return (x / y).ToString();
            }
            else if (oper == "+")
            {
                return (x + y).ToString();
            }
            else // if (oper == "-")
            {
                return (x - y).ToString();
            }


        }

        /// <summary>
        /// Checks if the token is one of the argued operators
        /// </summary>
        /// <param name="t"></param>
        /// <param name="oper1"></param>
        /// <param name="oper2"></param>
        /// <returns></returns>
        private bool IsOper(string t, string oper1, string oper2)
        {
            return t == oper1 || t == oper2;
        }
    }

    /// <summary>
    /// Extensions for the Stack data structure for my Formula class
    /// </summary>
    internal static class FormulaStackExtensions
    {
        /// <summary>
        /// Checks if either of these values are on top of this stack of strings
        /// </summary>
        /// <param name="s"></param>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool IsOnTop(this Stack<string> s, string val1, string val2)
        {
            if (s.Count == 0)
            {
                return false;
            }
            string top = s.Peek();
            return top == val1 || top == val2;
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}
