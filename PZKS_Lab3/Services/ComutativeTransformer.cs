using BuildingBlocks.ExpressionTree.Nodes;
using BuildingBlocks.Extensions;
using BuildingBlocks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZKS_Lab3.Services
{
    public static class CommutativeTransformer
    {
        public static List<string> GetEquivalentForms(ExpressionNode root)
        {
            var uniqueForms = GenerateVariations(root);
            return uniqueForms.ToList();
        }

        private static HashSet<string> GenerateVariations(ExpressionNode node)
        {
            var results = new HashSet<string>();

            if (node == null) return results;

            if (node.Type == TokenType.VAR || node.Type == TokenType.CONST)
            {
                results.Add(node.Value);
                return results;
            }

            if (node.Type == TokenType.FN)
            {
                var argVariations = GenerateVariations(node.Left);
                foreach (var arg in argVariations)
                {
                    results.Add($"{node.Value}({arg})");
                }
                return results;
            }

            if (node.Type == TokenType.OP)
            {
                var leftOptions = GenerateVariations(node.Left);
                var rightOptions = GenerateVariations(node.Right);

                foreach (var left in leftOptions)
                {
                    foreach (var right in rightOptions)
                    {
                        string directForm = Combine(left, node.Value, right, node.Left, node.Right, false);
                        results.Add(directForm);

                        if (node.Value == "+" || node.Value == "*")
                        {
                            string swappedForm = Combine(right, node.Value, left, node.Right, node.Left, true);
                            results.Add(swappedForm);
                        }
                    }
                }
            }

            return results;
        }

        private static string Combine(string leftStr, string op, string rightStr, ExpressionNode leftNode, ExpressionNode rightNode, bool swapped)
        {
            int currentPrecedence = GetPrecedence(op);

            bool bracketsLeft = CheckBrackets(leftNode, currentPrecedence);
            bool bracketsRight = CheckBrackets(rightNode, currentPrecedence);

            string finalLeft = bracketsLeft ? $"({leftStr})" : leftStr;
            string finalRight = bracketsRight ? $"({rightStr})" : rightStr;

            return $"{finalLeft}{op}{finalRight}";
        }

        private static bool CheckBrackets(ExpressionNode child, int parentPrecedence)
        {
            if (child == null || child.Type != TokenType.OP) return false;
            int childPrecedence = new Token { Value = child.Value, TokenType = TokenType.OP }.GetPrecedence();
            return childPrecedence < parentPrecedence;
        }

        private static int GetPrecedence(string op)
        {
            return new Token { Value = op, TokenType = TokenType.OP }.GetPrecedence();
        }
    }
}
