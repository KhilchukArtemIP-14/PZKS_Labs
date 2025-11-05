using PZKS_Lab1.Extensions;
using PZKS_Lab1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZKS_Lab1.Services
{
    public class Router
    {
        public State GetNextState(State currentState, char input)
        {
            CharType type = input.GetCharType();

            switch (currentState)
            {
                case State.START:
                    return type switch
                    {
                        CharType.LETTER => State.VARIABLE_OR_FUNCTION,
                        CharType.DIGIT => State.CONSTANT,
                        CharType.PLUS_MINUS => State.OPERATION,
                        CharType.OPEN_BRACKET => State.OPENING_BRACKET,
                        _ => State.ERR 
                    };

                case State.OPERATION:
                    return type switch
                    {
                        CharType.LETTER => State.VARIABLE_OR_FUNCTION,
                        CharType.DIGIT => State.CONSTANT,
                        CharType.OPEN_BRACKET => State.OPENING_BRACKET,
                        _ => State.ERR
                    };

                case State.VARIABLE_OR_FUNCTION:
                    return type switch
                    {
                        CharType.LETTER => State.VARIABLE_OR_FUNCTION,
                        CharType.PLUS_MINUS or CharType.MUL_DIV => State.OPERATION,
                        CharType.OPEN_BRACKET => State.FUNCTION_OPENING_BRACKET,
                        CharType.CLOSE_BRACKET => State.CLOSING_BRACKET,
                        CharType.DIGIT => State.VARIABLE_OR_FUNCTION,
                        _ => State.ERR
                    };

                case State.CONSTANT:
                    return type switch
                    {
                        CharType.DIGIT => State.CONSTANT,
                        CharType.DOT => State.CONSTANT_DOT,
                        CharType.PLUS_MINUS or CharType.MUL_DIV => State.OPERATION,
                        CharType.CLOSE_BRACKET => State.CLOSING_BRACKET,
                        _ => State.ERR
                    };

                case State.OPENING_BRACKET:
                    return type switch
                    {
                        CharType.LETTER => State.VARIABLE_OR_FUNCTION,
                        CharType.DIGIT => State.CONSTANT,
                        CharType.PLUS_MINUS => State.OPERATION,
                        CharType.OPEN_BRACKET => State.OPENING_BRACKET,
                        _ => State.ERR
                    };
                case State.FUNCTION_OPENING_BRACKET:
                    return type switch
                    {
                        CharType.LETTER => State.VARIABLE_OR_FUNCTION,
                        CharType.DIGIT => State.CONSTANT,
                        CharType.PLUS_MINUS => State.OPERATION,
                        CharType.OPEN_BRACKET => State.OPENING_BRACKET,
                        CharType.CLOSE_BRACKET => State.CLOSING_BRACKET,
                        _ => State.ERR
                    };
                case State.CONSTANT_DOT:
                    return type switch
                    {
                        CharType.DIGIT => State.CONSTANT_DOT_CONSTANT,
                        _ => State.ERR
                    };

                case State.CONSTANT_DOT_CONSTANT:
                    return type switch
                    {
                        CharType.DIGIT => State.CONSTANT_DOT_CONSTANT,
                        CharType.PLUS_MINUS or CharType.MUL_DIV => State.OPERATION,
                        CharType.CLOSE_BRACKET => State.CLOSING_BRACKET,
                        _ => State.ERR
                    };

                case State.CLOSING_BRACKET:
                    return type switch
                    {
                        CharType.PLUS_MINUS or CharType.MUL_DIV => State.OPERATION,
                        CharType.CLOSE_BRACKET => State.CLOSING_BRACKET,
                        _ => State.ERR
                    };

                case State.END:
                    return State.END;

                default:
                    return State.ERR;
            }
        }
    }
}
