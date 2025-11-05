using PZKS_Lab1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZKS_Lab1.Extensions
{
    public static class CharExtensions
    {
        public static CharType GetCharType(this char c)
        {
            if (char.IsLetter(c))
                return CharType.LETTER;

            if (char.IsDigit(c))
                return CharType.DIGIT;

            return c switch
            {
                '+' or '-' => CharType.PLUS_MINUS,
                '*' or '/' => CharType.MUL_DIV,
                '(' => CharType.OPEN_BRACKET,
                ')' => CharType.CLOSE_BRACKET,
                '.' => CharType.DOT,
                _ => CharType.UNKNOWN,
            };
        }
    }
}
