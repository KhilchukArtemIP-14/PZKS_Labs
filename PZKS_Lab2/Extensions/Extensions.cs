using BuildingBlocks.Models;
using PZKS_Lab2.Nodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZKS_Lab2.Extensions
{
    public static class Extensions
    {
        public static int GetPrecedence(this Token token)
        {
            if (token == null || token.TokenType != TokenType.OP) return 0;
            return token.Value switch
            {
                "+" or "-" => 1,
                "*" or "/" => 2,
                _ => 0,
            };
        }

        public static bool IsZero(this ExpressionNode node)
        {
            if (node == null || node.Type != TokenType.CONST)
                return false;

            return double.TryParse(node.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double val) && val == 0.0;
        }


        public static bool IsOne(this ExpressionNode node)
        {
            if (node == null || node.Type != TokenType.CONST)
                return false;

            return double.TryParse(node.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double val) && val == 1.0;
        }
    }
}
