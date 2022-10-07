using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    /// <summary>
    /// Contains functionality for evaluating infix expressions
    /// </summary>
    public static class Evaluator
    {
        public delegate int Lookup(String v);
        private static Stack<int> valS;
        private static Stack<string> operS;

        /// <summary>
        /// Evaluates an infix expression
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="variableEvaluator"></param>
        /// <returns></returns>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            if (exp == "")
            {
                throw new ArgumentException("empty expression");
            }

            //create tokens
            string[] tokens = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            valS = new Stack<int>();
            operS = new Stack<string>();

            //check the token
            foreach (string i in tokens)
            {
                string t = i.Trim();
                if (t == "")
                {   // do nothing if the token is whitespace
                }
                else if (IsInt(t))
                {
                    int token = int.Parse(t);
                    EvaluateNumericToken(token);
                }
                else if (IsVar(t))
                {
                    int token = variableEvaluator(t);
                    EvaluateNumericToken(token);
                }
                else if (IsOper(t, "+", "-"))
                {
                    if (operS.IsOnTop("+", "-"))
                    {
                        if (valS.Count >= 2)
                        {
                            valS.PushResult(valS.Pop(), operS.Pop(), valS.Pop());
                        }
                    }
                    operS.Push(t);
                }
                else if (IsOper(t, "*", "/"))
                {
                    operS.Push(t);
                }
                else if (t == "(")
                {
                    operS.Push(t);
                }
                else if (t == ")")
                {
                    if (operS.IsOnTop("+", "-"))
                    {
                        valS.PushResult(valS.Pop(), operS.Pop(), valS.Pop());
                    }

                    if (operS.Count == 0)
                    {
                        throw new ArgumentException("Expected '(' but was empty");
                    }
                    if (operS.Peek() != "(")
                    {
                        throw new ArgumentException("Expected '(' but found '" + operS.Peek() + "'");
                    }
                    operS.Pop();

                    if (operS.IsOnTop("*", "/"))
                    {
                        valS.PushResult(valS.Pop(), operS.Pop(), valS.Pop());
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid token: \"" + t + "\"");
                }
            }

            //last token has been processed
            if (operS.Count != 0)
            {
                if (valS.Count != 2)
                {
                    throw new ArgumentException("trailing operator");
                }
                valS.PushResult(valS.Pop(), operS.Pop(), valS.Pop());
            }

            // This code qualifies using double operators or unary negatives expressions as an error
            if (operS.Count != 0)
            {
                throw new ArgumentException("Using double operators or unary negatives are illegal: \"" + exp + "\"");
            }

            //return
            return valS.Pop();
        }

        /// <summary>
        /// Evaluates a numeric token.
        /// </summary>
        /// <param name="token"></param>
        private static void EvaluateNumericToken(int token)
        {
            if (operS.IsOnTop("*", "/"))
            {
                if (valS.Count == 0)
                {
                    throw new ArgumentException("Value stack is empty");
                }

                valS.PushResult(token, operS.Pop(), valS.Pop());
            }
            else
            {
                valS.Push(token);
            }
        }

        /// <summary>
        /// Checks if the token is a variable
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static bool IsVar(string t)
        {
            Regex varSyntax = new Regex("^[a-zA-Z]+[0-9]+$");
            return varSyntax.IsMatch(t);
        }

        /// <summary>
        /// Checks if the token is an integer
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static bool IsInt(string t)
        {
            return int.TryParse(t, out _);
        }

        /// <summary>
        /// Checks if the token is one of the argued operators
        /// </summary>
        /// <param name="t"></param>
        /// <param name="oper1"></param>
        /// <param name="oper2"></param>
        /// <returns></returns>
        private static bool IsOper(string t, string oper1, string oper2)
        {
            return t == oper1 || t == oper2;
        }

    }

    /// <summary>
    /// Extensions for the Stack data structure for my PS1 class
    /// </summary>
    public static class PS1StackExtensions
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

        /// <summary>
        /// Pushes the result of the second integer operated on by the first onto this stack of integers
        /// </summary>
        /// <param name="s"></param>
        /// <param name="x"></param>
        /// <param name="oper"></param>
        /// <param name="y"></param>
        public static void PushResult(this Stack<int> s, int y, string oper, int x)
        {
            if (oper == "*")
            {
                s.Push(x * y);
            }
            else if (oper == "/")
            {
                if (y == 0)
                {
                    throw new ArgumentException("division by 0");
                }

                s.Push(x / y);
            }
            else if (oper == "+")
            {
                s.Push(x + y);
            }
            else if (oper == "-")
            {
                s.Push(x - y);
            }
        }
    }
}