using BuildingBlocks.Models;
using PZKS_Lab2.Extensions;
using PZKS_Lab2.Nodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PZKS_Lab2.Services
{
    public class ParallelTreeBuilder
    {
        public ExpressionNode BuildParallelTree(List<Token> tokens)
        {
            ExpressionNode sequentialRoot = BuildSequentialTree(tokens, 0, tokens.Count - 1);

            ExpressionNode parallelRoot = OptimizeTree(sequentialRoot);

            return parallelRoot;
        }

        private ExpressionNode BuildSequentialTree(List<Token> tokens, int start, int end)
        {
            if (start == end) return new ExpressionNode(tokens[start]);

            if (tokens[start].TokenType == TokenType.OB && tokens[end].TokenType == TokenType.CB)
            {
                int balance = 0;
                bool isFullExpression = true;
                for (int i = start + 1; i < end; i++)
                {
                    if (tokens[i].TokenType == TokenType.OB) balance++;
                    if (tokens[i].TokenType == TokenType.CB) balance--;
                    if (balance < 0) { isFullExpression = false; break; }
                }
                if (isFullExpression && balance == 0)
                {
                    return BuildSequentialTree(tokens, start + 1, end - 1);
                }
            }

            int splitIndex = -1;
            int minPrecedence = 100;
            int bracketLevel = 0;
            for (int i = end; i >= start; i--)
            {
                if (tokens[i].TokenType == TokenType.CB) bracketLevel++;
                else if (tokens[i].TokenType == TokenType.OB) bracketLevel--;

                if (bracketLevel == 0 && tokens[i].TokenType == TokenType.OP)
                {
                    // must be non-unary
                    if (i == start) continue;

                    int currentPrecedence = tokens[i].GetPrecedence();

                    if (currentPrecedence < minPrecedence)
                    {
                        minPrecedence = currentPrecedence;
                        splitIndex = i;
                    }
                }
            }

            if (splitIndex != -1)
            {
                var node = new ExpressionNode(tokens[splitIndex]);
                node.Left = BuildSequentialTree(tokens, start, splitIndex - 1);
                node.Right = BuildSequentialTree(tokens, splitIndex + 1, end);
                return node;
            }

            // if haven't fallen through, then it's unary
            if (tokens[start].TokenType == TokenType.OP && (tokens[start].Value == "+" || tokens[start].Value == "-"))
            {
                var zeroNode = new ExpressionNode(new Token { Value = "0", TokenType = TokenType.CONST });
                var opNode = new ExpressionNode(tokens[start]);
                var rightNode = BuildSequentialTree(tokens, start + 1, end);

                opNode.Left = zeroNode;
                opNode.Right = rightNode;

                Console.WriteLine($"Unary minus enhanced with 0: ({tokens[start].Value}{NodeToExpressionString(rightNode)}) -> {NodeToExpressionString(opNode)}");

                return opNode;
            }

            if (tokens[start].TokenType == TokenType.FN && tokens[end].TokenType == TokenType.CB)
            {
                var funcNode = new ExpressionNode(tokens[start]);
                funcNode.Left = BuildSequentialTree(tokens, start + 2, end - 1);
                return funcNode;
            }

            return null;
        }

        private ExpressionNode OptimizeTree(ExpressionNode node)
        {
            if (node == null || (node.Type != TokenType.OP && node.Type != TokenType.FN))
            {
                return node;
            }

            node.Left = OptimizeTree(node.Left);
            node.Right = OptimizeTree(node.Right);

            // 2 + 4 + a -> 6 + a
            if (node.Type == TokenType.OP &&
                node.Left != null && node.Left.Type == TokenType.CONST &&
                node.Right != null && node.Right.Type == TokenType.CONST)
            {
                if (double.TryParse(node.Left.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double leftVal) &&
                  double.TryParse(node.Right.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double rightVal))
                {
                    string oldExpr = NodeToExpressionString(node);
                    double result = 0;
                    switch (node.Value)
                    {
                        case "+": result = leftVal + rightVal; break;
                        case "-": result = leftVal - rightVal; break;
                        case "*": result = leftVal * rightVal; break;
                        case "/":
                            if (rightVal == 0)
                            {
                                Console.WriteLine($"Numeric evaluation (Division by zero): {oldExpr} -> 0");
                                return new ExpressionNode(new Token { Value = "0", TokenType = TokenType.CONST });
                            }
                            result = leftVal / rightVal;
                            break;
                        default:
                            return node;
                    }
                    var newNode = new ExpressionNode(new Token
                    {
                        Value = result.ToString(CultureInfo.InvariantCulture),
                        TokenType = TokenType.CONST
                    });
                    Console.WriteLine($"Numeric evaluation: {oldExpr} -> {newNode.Value}");
                    return newNode;
                }
            }

            // 0 * a, a * 0 -> 0
            // 1 * a, a * 1 -> a
            if (node.Value == "*")
            {
                if ((node.Right != null && node.Right.IsZero()) || (node.Left != null && node.Left.IsZero()))
                {
                    Console.WriteLine($"Multiplication by zero: {NodeToExpressionString(node)} -> 0");
                    return new ExpressionNode(new Token { Value = "0", TokenType = TokenType.CONST });
                }
                if (node.Right != null && node.Right.IsOne())
                {
                    Console.WriteLine($"Multiplication by one: {NodeToExpressionString(node)} -> {NodeToExpressionString(node.Left)}");
                    return node.Left;
                }
                if (node.Left != null && node.Left.IsOne())
                {
                    Console.WriteLine($"Multiplication by one: {NodeToExpressionString(node)} -> {NodeToExpressionString(node.Right)}");
                    return node.Right;
                }
            }

            // 0 / a, a / 0 -> 0
            // a / 1 -> a
            if (node.Value == "/")
            {
                if (node.Right != null && node.Right.IsZero())
                {
                    Console.WriteLine($"Division by zero: {NodeToExpressionString(node)} -> 0");
                    return new ExpressionNode(new Token { Value = "0", TokenType = TokenType.CONST });
                }
                if (node.Right != null && node.Right.IsOne())
                {
                    Console.WriteLine($"Division by one: {NodeToExpressionString(node)} -> {NodeToExpressionString(node.Left)}");
                    return node.Left;
                }
                if (node.Left != null && node.Left.IsZero())
                {
                    Console.WriteLine($"Division by zero: {NodeToExpressionString(node)} -> 0");
                    return new ExpressionNode(new Token { Value = "0", TokenType = TokenType.CONST });
                }
            }

            // 0 + a, a + 0 -> a
            if (node.Value == "+")
            {
                if (node.Right != null && node.Right.IsZero())
                {
                    Console.WriteLine($"Addition with zero: {NodeToExpressionString(node)} -> {NodeToExpressionString(node.Left)}");
                    return node.Left;
                }
                if (node.Left != null && node.Left.IsZero())
                {
                    Console.WriteLine($"Addition with zero: {NodeToExpressionString(node)} -> {NodeToExpressionString(node.Right)}");
                    return node.Right;
                }
            }

            // a - 0 -> a
            // a - (-b) -> a + b
            if (node.Value == "-")
            {
                if (node.Right != null && node.Right.IsZero())
                {
                    Console.WriteLine($"Subtraction of zero: {NodeToExpressionString(node)} -> {NodeToExpressionString(node.Left)}");
                    return node.Left;
                }
                if (node.Right != null && node.Right.Value == "-" && node.Right.Left.IsZero())
                {
                    string oldExpr = NodeToExpressionString(node);
                    node.Value = "+";
                    node.Right = node.Right.Right;
                    Console.WriteLine($"Double minus elimination: {oldExpr} -> {NodeToExpressionString(node)}");
                    return OptimizeTree(node);
                }
            }

            // a - a -> 0
            // a / a -> 1
            if (node.Left != null && node.Right != null &&
                node.Left.Type == TokenType.VAR && node.Right.Type == TokenType.VAR &&
                node.Left.Value == node.Right.Value)
            {
                if (node.Value == "-")
                {
                    Console.WriteLine($"Symbolic subtraction: {NodeToExpressionString(node)} -> 0");
                    return new ExpressionNode(new Token { Value = "0", TokenType = TokenType.CONST });
                }

                if (node.Value == "/")
                {
                    Console.WriteLine($"Symbolic division: {NodeToExpressionString(node)} -> 1");
                    return new ExpressionNode(new Token { Value = "1", TokenType = TokenType.CONST });
                }
            }

            // a - b - c -> a - (b + c)
            if (node.Value == "-" && node.Left != null && node.Left.Value == "-")
            {
                string oldExpr = NodeToExpressionString(node);
                var child = node.Left;
                var newSubTree = new ExpressionNode(new Token { Value = "+", TokenType = TokenType.OP });
                newSubTree.Left = child.Right;
                newSubTree.Right = node.Right;
                node.Left = child.Left;
                node.Right = OptimizeTree(newSubTree);
                Console.WriteLine($"Subtraction parallelization: {oldExpr} -> {NodeToExpressionString(node)}");
            }

            // a / b / c -> a / (b * c)
            if (node.Value == "/" && node.Left != null && node.Left.Value == "/")
            {
                string oldExpr = NodeToExpressionString(node);
                var child = node.Left;
                var newSubTree = new ExpressionNode(new Token { Value = "*", TokenType = TokenType.OP });
                newSubTree.Left = child.Right;
                newSubTree.Right = node.Right;
                node.Left = child.Left;
                node.Right = OptimizeTree(newSubTree);
                Console.WriteLine($"Division parallelization: {oldExpr} -> {NodeToExpressionString(node)}");
            }

            // minimize operations for multiple * and +
            if (node.Value == "+" || node.Value == "*")
            {
                var operands = new List<ExpressionNode>();
                CollectOperands(node, node.Value, operands);

                if (operands.Count > 2)
                {
                    string oldExpr = NodeToExpressionString(node);
                    var constOperands = operands.Where(p => p.Type == TokenType.CONST).ToList();
                    var varOperands = operands.Where(p => p.Type != TokenType.CONST).ToList();

                    if (constOperands.Count > 1)
                    {
                        double accumulator = (node.Value == "+") ? 0.0 : 1.0;
                        var op = (node.Value == "+") ?
                            (Func<double, double, double>)((a, b) => a + b) :
                            ((a, b) => a * b);

                        foreach (var constNode in constOperands)
                        {
                            double.TryParse(constNode.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double val);
                            accumulator = op(accumulator, val);
                        }

                        varOperands.Add(new ExpressionNode(new Token
                        {
                            Value = accumulator.ToString(CultureInfo.InvariantCulture),
                            TokenType = TokenType.CONST
                        }));
                    }
                    else if (constOperands.Count == 1)
                    {
                        varOperands.Add(constOperands[0]);
                    }

                    var opToken = new Token { Value = node.Value, TokenType = TokenType.OP };
                    var newNode = BuildBalancedFromList(varOperands, opToken);

                    return newNode;
                }
            }

            return node;
        }

        private string NodeToExpressionString(ExpressionNode node)
        {
            if (node == null) return "null";
            if (node.Type == TokenType.VAR || node.Type == TokenType.CONST)
            {
                return node.Value;
            }
            if (node.Type == TokenType.FN)
            {
                return $"{node.Value}({NodeToExpressionString(node.Left)})";
            }
            if (node.Type == TokenType.OP)
            {
                return $"({NodeToExpressionString(node.Left)} {node.Value} {NodeToExpressionString(node.Right)})";
            }
            return node.Value;
        }

        private void CollectOperands(ExpressionNode node, string operatorValue, List<ExpressionNode> operands)
        {
            if (node != null && node.Value == operatorValue && node.Type == TokenType.OP)
            {
                CollectOperands(node.Left, operatorValue, operands);
                CollectOperands(node.Right, operatorValue, operands);
            }
            else if (node != null)
            {
                operands.Add(node);
            }
        }

        private ExpressionNode BuildBalancedFromList(List<ExpressionNode> nodes, Token opToken)
        {
            if (nodes.Count == 0)
            {
                return new ExpressionNode(new Token { Value = (opToken.Value == "+") ? "0" : "1", TokenType = TokenType.CONST });
            }

            while (nodes.Count > 1)
            {
                var newNodes = new List<ExpressionNode>();
                for (int i = 0; i < nodes.Count; i += 2)
                {
                    if (i + 1 < nodes.Count)
                    {
                        var newNode = new ExpressionNode(opToken)
                        {
                            Left = nodes[i],
                            Right = nodes[i + 1]
                        };
                        newNodes.Add(newNode);
                    }
                    else
                    {
                        newNodes.Add(nodes[i]);
                    }
                }
                nodes = newNodes;
            }
            return nodes[0];
        }
    }
}