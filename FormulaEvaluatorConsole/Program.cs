// See https://aka.ms/new-console-template for more information
using FormulaEvaluator;

// Found real number.
static int variableLookup(String t)
{
    return 5;
}
// Found no number.
static int variableLookupException(String t)
{
    throw new ArgumentException();
}


// (No errors expected)
// Use of addition, subtraction, multiplication, division, parenthesis, and variables
Console.WriteLine(Evaluator.Evaluate("1+2+3+4+5", variableLookup));
Console.WriteLine(Evaluator.Evaluate("1-2-3+4+5", variableLookup));
Console.WriteLine(Evaluator.Evaluate("1+2*3*4+5", variableLookup));
Console.WriteLine(Evaluator.Evaluate("1*2/3*4/5", variableLookup));
Console.WriteLine(Evaluator.Evaluate("1+(2+3)/4+5", variableLookup));
Console.WriteLine(Evaluator.Evaluate("(1-(2/3)*4)+5", variableLookup));
Console.WriteLine(Evaluator.Evaluate("100/(3/(4+5*(6+2)/3)+2)", variableLookup));
Console.WriteLine(Evaluator.Evaluate("4+aAzZ00006", variableLookup));
Console.WriteLine(Evaluator.Evaluate("4/zzz1", variableLookup));
Console.WriteLine(Evaluator.Evaluate("a5/2+6", variableLookup));


// (Errors expected)
// Empty strings, different combinations of wrong inputs, and one use of throwing an exception with our delegate.
try
{
    Console.WriteLine(Evaluator.Evaluate("", variableLookup));
}
catch (ArgumentException e)
{
    Console.WriteLine(e.Message);
}
try
{
    Console.WriteLine(Evaluator.Evaluate(" ", variableLookup));
}
catch (ArgumentException e)
{
    Console.WriteLine(e.Message);
}
try
{
    Console.WriteLine(Evaluator.Evaluate("a7", variableLookupException));
}
catch (ArgumentException e)
{
    Console.WriteLine(e.Message);
}
try
{
    Console.WriteLine(Evaluator.Evaluate("//", variableLookup));
}
catch (ArgumentException e)
{
    Console.WriteLine(e.Message);
}
try
{
    Console.WriteLine(Evaluator.Evaluate("(3+4", variableLookup));
}
catch (ArgumentException e)
{
    Console.WriteLine(e.Message);
}
try
{
    Console.WriteLine(Evaluator.Evaluate("(3+4)*", variableLookup));
}
catch (ArgumentException e)
{
    Console.WriteLine(e.Message);
}
try
{
    Console.WriteLine(Evaluator.Evaluate("()", variableLookup));
}
catch (ArgumentException e)
{
    Console.WriteLine(e.Message);
}
try
{
    Console.WriteLine(Evaluator.Evaluate("(8))", variableLookup));
}
catch (ArgumentException e)
{
    Console.WriteLine(e.Message);
}
try
{
    Console.WriteLine(Evaluator.Evaluate("-5+2", variableLookup));
}
catch (ArgumentException e)
{
    Console.WriteLine(e.Message);
}

