// Andrew Fryzel 

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static Formulas.TokenType;

namespace Formulas
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  Provides a means to evaluate Formulas.  Formulas can be composed of
    /// non-negative floating-point numbers, variables, left and right parentheses, and
    /// the four binary operator symbols +, -, *, and /.  (The unary operators + and -
    /// are not allowed.)
    /// </summary> 
    public class Formula
    {
        // List of original tokens after being run through GetToken
        private List<Token> tokens;

        // Make new list normalizedtokens that will hold normalized variables and be use in the formula
        private List<Token> normalizedTokens;

        // Holds variables to used with the GetVariables method
        private HashSet<String> variables;

        /// <summary>
        /// Single parameter constructor that passes along the input to the three parameter constructor
        /// </summary>
        /// <param name="formula"></param>
        public Formula(String formula) :
            this(formula, s => s, s => true)

        {

        }

        /// <summary>
        /// Creates a Formula from a string that consists of a standard infix expression composed
        /// from non-negative floating-point numbers (using C#-like syntax for double/int literals), 
        /// variable symbols (a letter followed by zero or more letters and/or digits), left and right
        /// parentheses, and the four binary operator symbols +, -, *, and /.  White space is
        /// permitted between tokens, but is not required.
        /// If the formula is syntacticaly invalid, throws a FormulaFormatException with an 
        /// explanatory Message.
        /// </summary>
        public Formula(String formula, Func<string, string> normalizer, Func<String, bool> validator)
        {
            // String that holds the normalized variable
            String normal;
            variables = new HashSet<String>();

            if (formula == null || normalizer == null || validator == null)
            {
                throw new ArgumentNullException("Cannot use null values");
            }

            // GetTokens
            tokens = new List<Token>(GetTokens(formula));

            normalizedTokens = new List<Token>();

            // There can be no invalid tokens.  (Valid tokens are described in the Formula class. and code is provided to detect them.)
            foreach (var t in tokens)
            {
                if (t.type == TokenType.Invalid)
                {
                    throw new FormulaFormatException("There can be no invalid tokens");
                }
                // Iterate through the variables, validity checks
                if (t.type == TokenType.Var)
                {
                    normal = normalizer(t.text);
                    Regex r = new Regex(@"[a-zA-Z][0-9a-zA-Z]*");

                    if (!r.IsMatch(normal))

                    {
                        throw new FormulaFormatException("There can be no invalid tokens");
                    }

                    if (!validator(normal))
                    {
                        throw new FormulaFormatException("There can be no invalid tokens");
                    }


                    // Instead of tokens use normalized tokens
                    else
                    {
                        // Create a new token to store the normalized variable to be added to normalizedTokens
                        Token normalized = new Token(normal, TokenType.Var);
                        normalizedTokens.Add(normalized);
                        // Add variables to the variables list
                        variables.Add(normal);
                    }
                }

                // Add everything that isn't a variable
                else
                {
                    normalizedTokens.Add(t);
                }
            }

            // There must be at least one token.
            if (tokens.Count == 0)
            {
                throw new FormulaFormatException("There must be at least one token");
            }

            // When reading tokens from left to right, at no point should the number of closing parentheses 
            // seen so far be greater than the number of opening parentheses seen so far.
            int opening = 0;
            int closing = 0;
            // The total number of opening parentheses must equal the total number of closing parentheses.
            foreach (var t in normalizedTokens)
            {
                //normal = normalizer(t.tType);

                if (t.type == (TokenType.LParen))
                {
                    opening++;

                }
                else if (t.type == (TokenType.RParen))
                {
                    closing++;

                }
                if (closing > opening)
                {
                    throw new FormulaFormatException("When reading tokens from left to right, at no point should the number of closing parentheses seen so far be greater than the number of opening parentheses seen so far.");
                }
            }
            if (opening != closing)
            {
                throw new FormulaFormatException("The total number of opening parentheses must equal the total number of closing parentheses.");
            }

            // The first token of a formula must be a number, a variable, or an opening parenthesis.
            if (!(tokens[0].type == TokenType.Number || tokens[0].type == TokenType.Var || tokens[0].type == TokenType.LParen))
            {
                throw new FormulaFormatException("The first token of a formula must be a number, a variable, or an opening parenthesis.");
            }

            // The last token of a formula must be a number, a variable, or a closing parenthesis.
            int end = tokens.Count - 1;
            if (!(tokens[end].type == TokenType.Number || tokens[end].type == TokenType.RParen || tokens[end].type == TokenType.Var))
            {
                throw new FormulaFormatException("The last token of a formula must be a number, a variable, or a closing parenthesis.");
            }

            // Any token that immediately follows an opening parenthesis or an operator must be either a number, a variable, or an opening parenthesis.
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].type == TokenType.LParen || tokens[i].type == TokenType.Oper)
                {
                    int j = i + 1;

                    if (j < tokens.Count)
                    {
                        if (!(tokens[j].type == TokenType.Number || tokens[j].type == TokenType.Var || tokens[j].type == TokenType.LParen))
                        {
                            throw new FormulaFormatException("Any token that immediately follows an opening parenthesis or an operator must be either a number, a variable, or an opening parenthesis");
                        }
                    }
                }
            }

            //Any token that immediately follows a number, a variable, or a closing parenthesis must be either an operator or a closing parenthesis.
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].type == TokenType.Number || tokens[i].type == TokenType.RParen || tokens[i].type == TokenType.Var)
                {
                    int j = i + 1;
                    if (j < tokens.Count)
                    {
                        if (!(tokens[j].type == TokenType.RParen || tokens[j].type == TokenType.Oper))
                        {
                            throw new FormulaFormatException("Any token that immediately follows a number, a variable, or a closing parenthesis must be either an operator or a closing parenthesis.");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Evaluates this Formula, using the Lookup delegate to determine the values of variables.  (The
        /// delegate takes a variable name as a parameter and returns its value (if it has one) or throws
        /// an UndefinedVariableException (otherwise).  Uses the standard precedence rules when doing the evaluation.
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, its value is returned.  Otherwise, throws a FormulaEvaluationException  
        /// with an explanatory Message.
        /// </summary>
        public double Evaluate(Lookup lookup)

        {
            Stack<char> operators = new Stack<char>();
            Stack<double> value = new Stack<double>();

            foreach (var t in normalizedTokens)
            {
                // t is a double 
                if (t.type == TokenType.Number)
                {
                    double number = double.Parse(t.text); ;

                    if (operators.Count > 0)
                    {
                        if (operators.Peek() == '*' || operators.Peek() == '/')
                        {
                            double pop = value.Pop();
                            char oper = operators.Pop();

                            double ans = 0;

                            if (oper.Equals('*'))
                            {
                                ans = number * pop;
                                value.Push(ans);

                            }
                            else if (oper.Equals('/'))
                            {
                                if (number == 0)
                                {
                                    throw new FormulaEvaluationException("Cannot divide by zero");
                                }

                                ans = pop / number;
                                value.Push(ans);
                            }
                        }
                        else
                        {
                            value.Push(number);
                        }
                    }
                    else
                    {
                        value.Push(number);
                    }
                }

                // t is a variable
                else if (t.type == TokenType.Var)
                {
                    double lookupVal = 0;

                    try
                    {
                        lookupVal = lookup(t.text);
                    }
                    catch
                    {
                        throw new FormulaEvaluationException("The given string is unmapped to a value");
                    }

                    if (operators.Count > 0)
                    {
                        if (operators.Peek() == '*' || operators.Peek() == '/')
                        {
                            double pop = value.Pop();
                            char oper = operators.Pop();

                            double ans = 0;

                            if (oper.Equals('*'))
                            {
                                ans = lookupVal * pop;
                                value.Push(ans);

                            }

                            else if (oper.Equals('/'))
                            {
                                if (lookupVal == 0)
                                {
                                    throw new FormulaEvaluationException("Cannot divide by zero");
                                }
                                ans = pop / lookupVal;
                                value.Push(ans);
                            }
                        }

                        else
                        {
                            value.Push(lookupVal);
                        }
                    }

                    else
                    {
                        value.Push(lookupVal);
                    }
                }

                // t is + or -
                else if (t.text.Equals("+") || t.text.Equals("-"))
                {
                    if (operators.Count > 0)
                    {
                        char peekChar = operators.Peek();
                        double ans;

                        if (peekChar.Equals('+') || peekChar.Equals('-'))
                        {
                            double popped1 = value.Pop();
                            double popped2 = value.Pop();
                            double popChar = operators.Pop();

                            if (popChar.Equals('+'))
                            {
                                ans = popped1 + popped2;
                                value.Push(ans);
                            }

                            else if (popChar.Equals('-'))
                            {
                                ans = popped2 - popped1;
                                value.Push(ans);
                                operators.Push('-');
                            }
                        }
                    }

                    if (t.text.Equals("+"))
                    {
                        operators.Push('+');
                    }

                    else if (t.text.Equals("-"))
                    {
                        operators.Push('-');
                    }
                }

                // t is * or /
                else if (t.text.Equals("*") || t.text.Equals("/"))
                {
                    if (t.text.Equals("*"))
                    {
                        operators.Push('*');
                    }

                    else if (t.text.Equals("/"))
                    {
                        operators.Push('/');
                    }
                }

                // t is (
                else if (t.type == TokenType.LParen)
                {
                    operators.Push('(');
                }

                // t is )
                else if (t.type == TokenType.RParen)
                {
                    double opPeek = operators.Peek();

                    if (opPeek.Equals('+') || opPeek.Equals('-'))
                    {
                        double popVal1 = value.Pop();
                        double popVal2 = value.Pop();
                        char popOp = operators.Pop();
                        double ans;

                        if (popOp.Equals('+'))
                        {
                            ans = popVal1 + popVal2;
                            value.Push(ans);
                        }

                        else if (popOp.Equals('-'))
                        {
                            ans = popVal2 - popVal1;
                            value.Push(ans);
                        }
                    }

                    char popLeft = operators.Pop();

                    if (operators.Count > 0)
                    {
                        opPeek = operators.Peek();

                        if (opPeek.Equals('*') || opPeek.Equals('/'))
                        {
                            double popVal1 = value.Pop();
                            double popVal2 = value.Pop();
                            char popOp = operators.Pop();
                            double ans;

                            if (popOp.Equals('*'))
                            {
                                ans = popVal1 * popVal2;
                                value.Push(ans);
                            }

                            else if (popOp.Equals('/'))
                            {
                                if (popVal1 == 0)
                                {
                                    throw new FormulaEvaluationException("Cannot divide by zero");
                                }

                                ans = popVal2 / popVal1;
                                value.Push(ans);
                            }
                        }
                    }
                }
            }
            // end of tokens, do last two things
            if (operators.Count == 0)
            {
                double popVal = value.Pop();
                return popVal;
            }
            else if (operators.Count != 0)
            {
                double popVal1 = value.Pop();
                double popVal2 = value.Pop();
                char opPop = operators.Pop();

                if (opPop.Equals('+'))
                {
                    return popVal1 + popVal2;
                }

                else if (opPop.Equals('-'))
                {
                    return popVal2 - popVal1;
                }
            }
            //Shouldnt ever get here
            return 0;
        }

        /// <summary>
        /// Given a formula, enumerates the tokens that compose it.  Each token is described by
        /// the token's text and TokenType.  There are no empty tokens, and no
        /// token contains white space.
        /// </summary>
        private static IEnumerable<Token> GetTokens(String formula)
        {
            // Patterns for individual tokens.
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]*";

            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: e[\+-]?\d+)?";
            String spacePattern = @"\s+";

            String tokenPattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5}) | (.)",
                                            spacePattern, lpPattern, rpPattern, opPattern, varPattern, doublePattern);

            // Create a Regex for matching tokens.  Notice the second parameter to Split says 
            // to ignore embedded white space in the pattern.
            Regex r = new Regex(tokenPattern, RegexOptions.IgnorePatternWhitespace);

            // Look for the first match
            Match match = r.Match(formula);

            // Start enumerating tokens
            while (match.Success)
            {
                // Ignore spaces
                if (!match.Groups[1].Success)
                {
                    // Holds the token's type
                    TokenType type;

                    if (match.Groups[2].Success)
                    {
                        type = LParen;
                    }
                    else if (match.Groups[3].Success)
                    {
                        type = RParen;
                    }
                    else if (match.Groups[4].Success)
                    {
                        type = Oper;
                    }
                    else if (match.Groups[5].Success)
                    {
                        type = Var;
                    }
                    else if (match.Groups[6].Success)
                    {
                        type = Number;
                    }
                    else if (match.Groups[7].Success)
                    {
                        type = Invalid;
                    }
                    else
                    {

                        throw new InvalidOperationException("Regular exception failed in GetTokens");
                    }

                    Token token = new Token(match.Value, type);

                    yield return token;

                }

                // Look for the next match
                match = match.NextMatch();
            }
        }

        /// <summary>
        /// Return an ISet<string> that contains each distinct variable, 
        /// (in normalized form), that appears in the Formula
        /// </summary>
        /// <returns></returns>
        public ISet<string> GetVariables()
        {
            return variables;
        }

        /// <summary>
        /// Returns a string version of the Formula (in normalized form)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String ans = "";

            foreach (Token t in normalizedTokens)
            {
                ans = ans + t.text;
            }

            return ans;
        }
    }

    /// <summary>
    /// Identifies the type of a token.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Left parenthesis
        /// </summary>
        LParen,

        /// <summary>
        /// Right parenthesis
        /// </summary>
        RParen,

        /// <summary>
        /// Operator symbol
        /// </summary>
        Oper,

        /// <summary>
        /// Variable
        /// </summary>
        Var,

        /// <summary>
        /// Double literal
        /// </summary>
        Number,

        /// <summary>
        /// Invalid token
        /// </summary>
        Invalid
    };

    /// <summary>
    /// A Lookup method is one that maps some strings to double values.  Given a string,
    /// such a function can either return a double (meaning that the string maps to the
    /// double) or throw an UndefinedVariableException (meaning that the string is unmapped 
    /// to a value. 
    /// </summary>
    public delegate double Lookup(string var);

    /// <summary>
    /// A Normalizer converts variables into a canonical form
    /// </summary>
    public delegate string Normalizer(string s);

    /// <summary>
    ///  A Validator imposes extra restrictions on the validity of a variable, 
    ///  beyond the ones already built into the Formula definition
    /// </summary>
    public delegate bool Validator(string s);

    /// <summary>
    /// Used to report that a Lookup delegate is unable to determine the value
    /// of a variable.
    /// </summary>
    [Serializable]
    public class UndefinedVariableException : Exception
    {
        /// <summary>
        /// Constructs an UndefinedVariableException containing whose message is the
        /// undefined variable.
        /// </summary>
        /// <param name="variable"></param>
        public UndefinedVariableException(String variable)
            : base(variable)
        {
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the parameter to the Formula constructor.
    /// </summary>
    [Serializable]
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message) : base(message)
        {
        }
    }

    /// <summary>
    /// Used to report errors that occur when evaluating a Formula.
    /// </summary>
    [Serializable]
    public class FormulaEvaluationException : Exception
    {
        /// <summary>
        /// Constructs a FormulaEvaluationException containing the explanatory message.
        /// </summary>
        public FormulaEvaluationException(String message) : base(message)
        {
        }
    }
    /// <summary>
    /// Defines a token which is described by the token's text and TokenType.  
    /// </summary>
    public struct Token
    {
        public string text { get; set; }
        public TokenType type { get; set; }

        /// <summary>
        /// Initiailizes the text and type parameters
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        public Token(String text, TokenType type)
        {
            this.text = text;
            this.type = type;
        }
    }
}