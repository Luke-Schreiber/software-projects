using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    /// <summary>
    /// This class has a public method 'Evaluate' which evaluates a standard infix expression
    /// given by the user, and a public delegate method so the user can define how variables are defined.
    /// </summary>
    public static class Evaluator
    {
        public delegate int Lookup(String v);

        /// <summary>
        /// Takes a proper infix expression as a string, evaluates it and returns an int as the answer.
        /// Throws an argument exception if the infix expression is not formatted correctly.
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="variableEvaluator"></param>
        /// <returns> Returns an int </returns>
        /// <exception cref="ArgumentException"></exception>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            // Takes string from user and creates an array of substrings (tokens).
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            // Create operator stack and value stack, and final value to be returned.
            int finalValue = 0;
            Stack opStack = new Stack();
            Stack<int> valStack = new Stack<int>();

            // While loop that iterates through every token and gradually solving the expression.                       
            int index = 0;
            while (index < substrings.Length)
            {
                // Define the token as a string without whitespace, and as a 'current Value' int.
                string token = (substrings[index]).Trim();
                int currentVal = 0;

                // If the token is + or - perform these tasks.
                if (token.Equals("+") || token.Equals("-"))
                {
                    // Check if operator stack is empty.
                    if (opStack.Count == 0);

                    else
                        // Check if the previous values need to be added or subtracted.
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
                        throw new ArgumentException();

                    else
                        // Check if the previous values need to be added or subtracted.
                        addOrSub();

                    // Pop the '('
                    
                    if (opStack.Count == 0)
                        throw new ArgumentException();

                    else if (!opStack.Peek().Equals("("))
                            throw new ArgumentException();
                    opStack.Pop();
                    

                    // If the operator stack is empty, skip all
                    if (opStack.Count == 0);

                    else if (opStack.Peek().Equals("*"))
                    {
                        // Push the last two popped numbers multiplied.
                        if (valStack.Count < 2)
                            throw new ArgumentException();
                        valStack.Push(valStack.Pop() * valStack.Pop());
                        opStack.Pop();
                    }
                    else if (opStack.Peek().Equals("/"))
                    {
                        // Push the last two popped numbers divided.
                        if (valStack.Count < 2)
                            throw new ArgumentException();
                        int divisor = valStack.Pop();
                        int dividend = valStack.Pop();
                        if (dividend == 0)
                            throw new ArgumentException();

                        valStack.Push(dividend / divisor);
                        opStack.Pop();
                    }
                }

                // If the token is either an integer or a variable, check the last operator and push it to the stack.
                else if (int.TryParse(token, out currentVal) || Regex.IsMatch(token, "^[a-zA-Z]+[0-9]+$"))
                {

                    if (Regex.IsMatch(token, "^[a-zA-Z]+[0-9]+$"))
                        try
                        {
                            currentVal = variableEvaluator(token);
                        }
                        catch (ArgumentException e)
                        {
                            throw new ArgumentException();
                        }
                        

                    if (opStack.Count == 0)
                        valStack.Push(currentVal);
                    
                    else if (opStack.Peek().Equals("*"))
                    {
                        // Push the last two popped numbers multiplied.
                        if (valStack.Count == 0)
                            throw new ArgumentException();
                        valStack.Push(valStack.Pop() * currentVal);
                        opStack.Pop();
                    }
                    else if (opStack.Peek().Equals("/"))
                    {
                        // Push the last two popped numbers divided.
                        if (currentVal == 0 || valStack.Count == 0)
                            throw new ArgumentException();
                        valStack.Push(valStack.Pop() / currentVal);
                        opStack.Pop();
                    }
                    else
                        valStack.Push(currentVal);
                }

                // If the operator is anything but an empty string now, throw an exception.
                else if (token != "")
                    throw new ArgumentException();

                index++;
            }

            // After the while loop, if the operator stack is empty, the final value must be in the value stack.
            if (opStack.Count == 0)
            {
                // If there is more than one value left, throw an exception.
                if (valStack.Count == 1)
                    finalValue = valStack.Pop();
                else
                    throw new ArgumentException();
            }

            // If the operator stack is not empty, there are two more values in the stack that must be added/sub.
            else
            {
                // If two values aren't in the stack, there is an error.
                if (valStack.Count != 2 || opStack.Count != 1)
                    throw new ArgumentException();

                else if (opStack.Peek().Equals("+"))
                {
                    // Push the last two popped numbers added.
                    finalValue = valStack.Pop() + valStack.Pop();
                }
                else if (opStack.Peek().Equals("-"))
                {
                    // Push the last two popped numbers subtracted.
                    int subtrahend = valStack.Pop();
                    int minuend = valStack.Pop();
                    finalValue = (minuend - subtrahend);
                }
                else
                    throw new ArgumentException();
            }

            /// <summary>
            /// Checks if the previous two values need to be added or subtracted.
            /// </summary>
            void addOrSub()
            {
                if (opStack.Peek().Equals("+"))
                {
                    if (valStack.Count < 2)
                        throw new ArgumentException();
                    // Push the last two popped numbers added.
                    valStack.Push(valStack.Pop() + valStack.Pop());
                    opStack.Pop();
                }
                else if (opStack.Peek().Equals("-"))
                {
                    if (valStack.Count < 2)
                        throw new ArgumentException();
                    // Push the last two popped numbers subtracted.
                    int subtrahend = valStack.Pop();
                    int minuend = valStack.Pop();
                    valStack.Push(minuend - subtrahend);
                    opStack.Pop();
                }
            }

            // Return final value to caller.
            return finalValue;
        }
    }
    
}