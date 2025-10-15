using PZKS_Lab1.Extensions;
using PZKS_Lab1.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PZKS_Lab1.Services
{
    public static class ExpressionAnalyzer
    {
        private static readonly Router _router = new Router();


        public static List<string> Analyze(string inputExpression)
        {
            var errors = new List<string>();
            var tokens = new List<(string Type, string Value)>();

            if (string.IsNullOrWhiteSpace(inputExpression))
            {
                errors.Add("Empty expression passed.");
                return errors;
            }

            var prevState = State.START;
            var bracketBalance = 0;
            var currentToken = new StringBuilder();

            for (int i = 0; i < inputExpression.Length; i++)
            {
                char currentChar = inputExpression[i];

                if (char.IsWhiteSpace(currentChar))
                {
                    continue;
                }

                if (currentChar == '(') bracketBalance++;
                else if (currentChar == ')') bracketBalance--;

                if (bracketBalance < 0)
                {
                    AddError(errors, inputExpression, i, "Obsolete closing bracket");
                    bracketBalance = 0;
                    continue;
                }

                var currState = _router.GetNextState(prevState, currentChar);

                bool isTokenBoundary = IsTokenBoundary(prevState, currState, currentChar);

                if (currentToken.Length > 0 && isTokenBoundary)
                {
                    if (prevState == State.VARIABLE_OR_FUNCTION)
                    {
                        string tokenType = (currState == State.FUNCTION_OPENING_BRACKET) ? "FN" : "VAR";
                        tokens.Add((tokenType, currentToken.ToString()));
                    }
                    else
                    {
                        FinalizeToken(tokens, currentToken.ToString(), prevState);
                    }
                    currentToken.Clear();
                }

                if (currState != State.ERR)
                {
                    prevState = currState;
                    currentToken.Append(currentChar);

                    if (currentToken.Length > 0 && isTokenBoundary)
                    {
                        FinalizeToken(tokens, currentToken.ToString(), prevState);
                        currentToken.Clear();
                    }
                }
                else
                {
                    string expected = GetExpectedChars(prevState);
                    AddError(errors, inputExpression, i, $"Unexpected symbol '{currentChar}'. Expecting: {expected}");
                }
            }

            if (currentToken.Length > 0)
            {
                FinalizeToken(tokens, currentToken.ToString(), prevState);
            }

            if (bracketBalance > 0)
            {
                AddError(errors, inputExpression, inputExpression.Length - 1, "Not enough closing brackets.");
            }

            var validEndStates = new[] { State.VARIABLE_OR_FUNCTION, State.CONSTANT, State.CONSTANT_DOT_CONSTANT, State.CLOSING_BRACKET };
            if (!validEndStates.Contains(prevState))
            {
                AddError(errors, inputExpression, inputExpression.Length - 1, $"Unfinished expression. Expecting characters: {GetExpectedChars(prevState)}");
            }

            if (errors.Any())
            {
                var result = new List<string> { "Errors detected!" };
                result.AddRange(errors);
                return result;
            }

            return new List<string> { "Tokens: " + string.Join(" ", tokens.Select(t => $"{t.Type}({t.Value})")) };
        }


        private static bool IsTokenBoundary(State current, State next, char c)
        {
            if (c == '(' || c == ')') return true;
            if (current != State.OPERATION && (next == State.OPERATION || next == State.ERR)) return true;
            return false;
        }

        private static void FinalizeToken(List<(string, string)> tokens, string tokenValue, State tokenState)
        {
            string tokenType = "UNKNOWN";
            switch (tokenState)
            {
                case State.VARIABLE_OR_FUNCTION:
                    tokenType = "VAR";
                    break;
                case State.CONSTANT:
                case State.CONSTANT_DOT_CONSTANT:
                    tokenType = "CONST";
                    break;
                case State.OPERATION:
                    tokenType = "OP";
                    break;
                case State.OPENING_BRACKET:
                case State.FUNCTION_OPENING_BRACKET:
                    tokenType = "OB";
                    break;
                case State.CLOSING_BRACKET:
                    tokenType = "CB";
                    break;
            }
            tokens.Add((tokenType, tokenValue));
        }

        private static void AddError(List<string> errors, string originalInput, int index, string message)
        {
            string pointer = new string(' ', index) + "^";
            errors.Add($"{originalInput}\n{pointer}\n{errors.Count + 1}. {message}");
        }

        private static string GetExpectedChars(State state)
        {
            return state switch
            {
                State.START => "+, -, 0..9, a..z, A..Z, (",
                State.OPERATION => "0..9, a..z, A..Z, (",
                State.VARIABLE_OR_FUNCTION => "+, -, *, /, ), (",
                State.CONSTANT => "0..9, +, -, *, /, ), .",
                State.CONSTANT_DOT_CONSTANT => "0..9,+, -, *, /, )",
                State.CLOSING_BRACKET => "+, -, *, /, )",
                State.OPENING_BRACKET => "+, -,0..9, a..z, A..Z,(",
                _ => "Correct one"
            };
        }
    }
}