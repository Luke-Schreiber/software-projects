// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!

// Change log:
// Last updated: 9/8, updated for non-nullable types

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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
        // Hold the formula tokens and its normalizer and validator.
        private IEnumerable<string> tokens = new List<string>();
        private Func<string, string> Normalizer;
        private Func<string, bool> Validator;
 
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
            // Get and hold the tokens, normalize and validate them, and hold the normalizer and validator.
            tokens = GetTokens(formula);
            tokens = isValidFormula(s => s, s => true, tokens);
            this.Normalizer = s => s;
            this.Validator = s => true;

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
            // Get and hold the tokens, normalize and validate them, and hold the normalizer and validator.
            tokens = GetTokens(formula);
            tokens = isValidFormula(normalize, isValid, tokens);
            this.Normalizer = normalize;
            this.Validator = isValid;

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
            
            // Takes string from user and creates an array of substrings (tokens).
            IEnumerator<string> enumerator = tokens.GetEnumerator();
            // Create operator stack and value stack, and final value to be returned.
            double finalValue = 0;
            Stack opStack = new Stack();
            Stack<double> valStack = new Stack<double>();

            // While loop that iterates through every token and gradually solving the expression.                       
            
            while (enumerator.MoveNext())
            {
                // Define the token as a string without whitespace, and as a 'current Value' double.
                string token = enumerator.Current;
                double currentVal = 0;

                // If the token is + or - perform these tasks.
                if (token.Equals("+") || token.Equals("-"))
                {
                    // Check if operator stack is empty.
                    if (opStack.Count != 0)
                        addOrSub();

                    // Regaurdless, push the + or - to the stack.
                    opStack.Push(token);
                }

                // If the token is * / or - simply push the token to the operator stack.
                else if (token.Equals("*") || token.Equals("/") || token.Equals("("))
                {
                    opStack.Push(token);
                }

                // If the token is ) perform these tasks.
                else if (token.Equals(")"))
                {
                    // If the operator stack is empty, throw error.
                    if (opStack.Count == 0)
                        return new FormulaError("Unexpected close parenthesis");

                    else
                        // Check if the previous values need to be added or subtracted.
                        addOrSub();

                    // Pop the '('

                    if (opStack.Count == 0)
                        return new FormulaError("Missing operator");

                    else if ((string?)(opStack.Peek()) != "(")
                        return new FormulaError("Unexpected open parenthesis");
                    opStack.Pop();


                    // If the operator stack is empty, skip all
                    if (opStack.Count != 0) {

                        if ((string?)(opStack.Peek()) == "*")
                        {
                            // Push the last two popped numbers multiplied.
                            if (valStack.Count < 2)
                                return new FormulaError("Not enough values to be multiplied");
                            valStack.Push(valStack.Pop() * valStack.Pop());
                            opStack.Pop();
                        }
                        else if ((string?)(opStack.Peek()) == "/")
                        {
                            // Push the last two popped numbers divided.
                            if (valStack.Count < 2)
                                return new FormulaError("Not enough values to be divided");
                            double divisor = valStack.Pop();
                            double dividend = valStack.Pop();
                            if (divisor == 0)
                                return new FormulaError("Divide by zero error");

                            valStack.Push(dividend / divisor);
                            opStack.Pop();
                        }
                    }
                }

                // If the token is either an integer or a variable, check the last operator and push it to the stack.
                else if (double.TryParse(token, out currentVal) || Regex.IsMatch(token, "^[a-zA-Z]+[0-9]+$"))
                {

                    if (Regex.IsMatch(token, "^[a-zA-Z]+[0-9]+$"))
                        try
                        {
                            currentVal = lookup(token);
                        }
                        catch (ArgumentException)
                        {
                            return new FormulaError("The variable cannot be found.");
                        }


                    if (opStack.Count == 0)
                        valStack.Push(currentVal);

                    else if ((string?)(opStack.Peek()) == "*")
                    {
                        // Push the last two popped numbers multiplied.
                        if (valStack.Count == 0)
                            return new FormulaError("Not enough values to be multiplied");
                        valStack.Push(valStack.Pop() * currentVal);
                        opStack.Pop();
                    }
                    else if ((string?)(opStack.Peek()) == "/")
                    {
                        // Push the last two popped numbers divided.
                        if (currentVal == 0 || valStack.Count == 0)
                            return new FormulaError("Not enough values to be divided");
                        valStack.Push(valStack.Pop() / currentVal);
                        opStack.Pop();
                    }
                    else
                        valStack.Push(currentVal);
                }

                // If the operator is anything but an empty string now, throw an exception.
                else if (token != "")
                    return new FormulaError("Invalid operator: " + token);

               
            }

            // After the while loop, if the operator stack is empty, the final value must be in the value stack.
            if (opStack.Count == 0)
            {
                // If there is more than one value left, throw an exception.
                if (valStack.Count == 1)
                    finalValue = valStack.Pop();
                else
                    return new FormulaError("Extra unneeded value");
            }

            // If the operator stack is not empty, there are two more values in the stack that must be added/sub.
            else
            {
                // If two values aren't in the stack, there is an error.
                if (valStack.Count != 2 || opStack.Count != 1)
                    return new FormulaError("Extra unneeded value");

                else if ((string?)(opStack.Peek()) == "+")
                {
                    // Push the last two popped numbers added.
                    finalValue = valStack.Pop() + valStack.Pop();
                }
                else if ((string?)(opStack.Peek()) == "-")
                {
                    // Push the last two popped numbers subtracted.
                    double subtrahend = valStack.Pop();
                    double minuend = valStack.Pop();
                    finalValue = (minuend - subtrahend);
                }
                else
                    return new FormulaError("Extra operator not needed");
            }

            /// <summary>
            /// Checks if the previous two values need to be added or subtracted.
            /// </summary>
            Object addOrSub()
            {
                if ((string?)(opStack.Peek()) == "+")
                {
                    if (valStack.Count < 2)
                        return new FormulaError("Not enough values to be added");
                    // Push the last two popped numbers added.
                    valStack.Push(valStack.Pop() + valStack.Pop());
                    opStack.Pop();
                }
                else if ((string?)(opStack.Peek()) == "-")
                {
                    if (valStack.Count < 2)
                        return new FormulaError("Not enough values to be subtracted");
                    // Push the last two popped numbers subtracted.
                    double subtrahend = valStack.Pop();
                    double minuend = valStack.Pop();
                    valStack.Push(minuend - subtrahend);
                    opStack.Pop();
                }
                return 0;
            }

            // Return final value to caller.
            return finalValue;
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
            // Loops through every token and returns only the variables.
            IEnumerator<string> enumerator = tokens.GetEnumerator();
            HashSet<String> variables = new HashSet<String>();
            while (enumerator.MoveNext())
            {
                if (Regex.IsMatch(enumerator.Current, "^[a-zA-z_][a-zA-z_\\d]*$"))
                    variables.Add(enumerator.Current);
            }
            IEnumerable<String> result = variables;
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
            
            string formula = "";
            IEnumerator<string> enumerator = tokens.GetEnumerator();

            // Loops through every token and adds it to the string, normalizing the numbers.
            while (enumerator.MoveNext())
            {
                if (Regex.IsMatch(enumerator.Current, "^\\d*\\.?\\d*([Ee][+\\-]?\\d+)?$"))
                    formula += Double.Parse(enumerator.Current).ToString();
                else formula += enumerator.Current;
            }

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
            // If the object is null or not a formula, return false.
            if ( !(obj is Formula))
                return false;

            // Otherwise, compare the strings and return the boolean.
            
            else return ((Formula)obj).ToString().Equals(this.ToString());
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            // Compare f1 to f2.
            return f1.Equals(f2);
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            // Compare f1 to f2 and return the opposite truth value.
            return !(f1.Equals(f2));
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            // Return the hashcode value of the string of this object.
            return this.ToString().GetHashCode();
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
        private static IEnumerable<string> isValidFormula(Func<string, string> normalize, Func<string, bool> isValid, IEnumerable<string> formula)
        {
            // Create new tokens to be returned.
            List<string> newTokens = new List<string>();

            // Hold the last token, open parenthesis count, and closed parenthesis count.
            string lastToken = "";
            int openParenCount = 0;
            int closedParenCount = 0;
            
            IEnumerator<string> enumerator = formula.GetEnumerator();

            // If there are no tokens, throw exception
            if (!formula.Any())
                throw new FormulaFormatException("The formula is empty");

            // Check if the first and last tokens are valid.
            if (!Regex.IsMatch(formula.First(), "^\\d*\\.?\\d*([Ee][+\\-]?\\d+)?$|^[a-zA-z_][a-zA-z_\\d]*$|\\("))
                throw new FormulaFormatException("The formula must start with a number, variable or open parenthesis");

            if (!Regex.IsMatch(formula.Last(), "^\\d*\\.?\\d*([Ee][+\\-]?\\d+)?$|^[a-zA-z_][a-zA-z_\\d]*$|\\)"))
                throw new FormulaFormatException("The formula must end with a number, variable or closed parenthesis");

            // Iterate through every token and check its validity with context of the token before it.
            while (enumerator.MoveNext())
            {
                string curr = enumerator.Current;

                // If curr is a variable, normalize it and check its validity.
                if (Regex.IsMatch(curr, "^[a-zA-z_][a-zA-z_\\d]*$"))
                {
                    curr = normalize(curr);
                    if (!isValid(curr))
                        throw new FormulaFormatException("Invalid variable: " + curr);
                }

                // Check if token is valid or empty.
                else if (!Regex.IsMatch(curr, "^[\\(\\)\\+\\-\\*\\/]$|^\\d*\\.?\\d*([Ee][+\\-]?\\d+)?$"))
                    throw new FormulaFormatException("Invalid character: " + curr);

                // Count the number of parenthesis.
                if (curr.Equals("("))
                    openParenCount++;
                if (curr.Equals(")"))
                    closedParenCount++;

                // Check balance of parenthesis.
                if (closedParenCount > openParenCount)
                    throw new FormulaFormatException("More closed parenthesis than open parenthesis");

                // If last token is open paren or operator, this token must be either a number, a variable, or an opening parenthesis.
                if (Regex.IsMatch(lastToken, "^[\\(\\+\\-\\*\\/]$") & !lastToken.Equals(""))
                    if (!(Regex.IsMatch(curr, "^\\d*\\.?\\d*([Ee][+\\-]?\\d+)?$|^[a-zA-z_][a-zA-z_\\d]*$|\\(")))
                        throw new FormulaFormatException(lastToken +
                        " must be followed by a number, a variable, or an opening parenthesis.");
                // If last token was a number, a variable, or an closed parenthesis this token must be a closed paren or operator.
                if (Regex.IsMatch(lastToken, "^\\d*\\.?\\d*([Ee][+\\-]?\\d+)?$|^[a-zA-z_][a-zA-z_\\d]*$|^\\)$") & !lastToken.Equals(""))
                    if (!(Regex.IsMatch(curr, "^[\\)\\+\\-\\*\\/]$")))
                        throw new FormulaFormatException(lastToken +
                        " must be followed by an operator or closing parenthesis.");

                

                // Swithc the last token to this current token, and add the current to the list.
                lastToken = curr;
                newTokens.Add(curr);
            }

            // Check balance of parenthesis.
            if (closedParenCount != openParenCount)
                throw new FormulaFormatException("Differing number of closed and open parenthesis");

            
            return newTokens;
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