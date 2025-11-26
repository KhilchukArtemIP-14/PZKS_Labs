using BuildingBlocks.ExpressionTree.Nodes;
using BuildingBlocks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZKS_Lab3.Services
{
    public class DistributiveTransformer
    {
        public static List<string> GetDistributiveForms(ExpressionNode root)
        {
            var queue = new Queue<ExpressionNode>();

            var visitedForms = new HashSet<string>();

            var results = new List<string>();

            var startTree = root.Clone();
            queue.Enqueue(startTree);

            string startStr = startTree.ToInfixString();
            visitedForms.Add(startStr);
            results.Add(startStr);

            while (queue.Count > 0)
            {
                var currentTree = queue.Dequeue();

                var possibleTargetPaths = FindAllExpandableNodes(currentTree, "");

                foreach (var path in possibleTargetPaths)
                {
                    var nextStateTree = currentTree.Clone();

                    var targetNode = GetNodeByPath(nextStateTree, path);

                    if (targetNode != null)
                    {
                        ApplyDistributiveToNode(targetNode);

                        string newForm = nextStateTree.ToInfixString();
                        if (!visitedForms.Contains(newForm))
                        {
                            //keeping hashset and list to preserve order of traversed states, instead of just hashset
                            visitedForms.Add(newForm);
                            results.Add(newForm);
                            queue.Enqueue(nextStateTree);
                        }
                    }
                }
            }

            return results;
        }

        private static List<string> FindAllExpandableNodes(ExpressionNode node, string currentPath)
        {
            var paths = new List<string>();
            if (node == null || node.Type != TokenType.OP) return paths;

            bool canExpand = false;
            if (node.Value == "*")
            {
                // (A+B)*C, C*(A+B)
                canExpand = IsAddOrSub(node.Left) || IsAddOrSub(node.Right);
            }
            else if (node.Value == "/")
            {
                // (A+B)/C
                canExpand = IsAddOrSub(node.Left);
            }

            if (canExpand)
            {
                paths.Add(currentPath);
            }

            paths.AddRange(FindAllExpandableNodes(node.Left, currentPath + "L"));
            paths.AddRange(FindAllExpandableNodes(node.Right, currentPath + "R"));

            return paths;
        }


        private static ExpressionNode GetNodeByPath(ExpressionNode root, string path)
        {
            var current = root;
            foreach (char direction in path)
            {
                if (current == null) return null;
                if (direction == 'L') current = current.Left;
                else if (direction == 'R') current = current.Right;
            }
            return current;
        }

        private static void ApplyDistributiveToNode(ExpressionNode node)
        {
            bool isRightMultiplier = false;

            if (node.Value == "*")
            {
                // (B+C) * A
                isRightMultiplier = IsAddOrSub(node.Left) && !IsAddOrSub(node.Right);
            }
            else if (node.Value == "/")
            {
                // (B+C) / A
                isRightMultiplier = IsAddOrSub(node.Left);
            }

            var groupNode = isRightMultiplier ? node.Left : node.Right;
            var multiplierNode = isRightMultiplier ? node.Right : node.Left;

            ApplyDistributive(node, groupNode, multiplierNode, isRightMultiplier);
        }

        private static void ApplyDistributive(ExpressionNode parentNode, ExpressionNode groupNode, ExpressionNode multiplierNode, bool isRightMultiplier)
        {
            string operation = parentNode.Value;
            string innerOperation = groupNode.Value;

            var term1 = groupNode.Left;
            var term2 = groupNode.Right;
            var factor = multiplierNode;

            parentNode.Value = innerOperation;

            var newLeft = new ExpressionNode(new Token { Value = operation, TokenType = TokenType.OP });
            var newRight = new ExpressionNode(new Token { Value = operation, TokenType = TokenType.OP });

            if (isRightMultiplier)
            {
                newLeft.Left = term1; newLeft.Right = factor.Clone();
                newRight.Left = term2; newRight.Right = factor.Clone();
            }
            else
            {
                newLeft.Left = factor.Clone(); newLeft.Right = term1;
                newRight.Left = factor.Clone(); newRight.Right = term2;
            }

            parentNode.Left = newLeft;
            parentNode.Right = newRight;
        }

        private static bool IsAddOrSub(ExpressionNode node)
        {
            return node != null && node.Type == TokenType.OP && (node.Value == "+" || node.Value == "-");
        }
    }
}
