using BuildingBlocks.ExpressionTree.Nodes;
using BuildingBlocks.Models;
using PZKS_Lab5.Models;
using System.Collections.Generic;

namespace PZKS_Lab5.Services
{
    public static class VliwGraphBuilder
    {
        public static VliwTaskGraph Build(ExpressionNode root)
        {
            var allTasks = new List<VliwTask>();
            int opIdCounter = 1;

            var rootTask = VisitRecursive(root, allTasks, ref opIdCounter);

            return new VliwTaskGraph(rootTask, allTasks);
        }

        private static VliwTask VisitRecursive(ExpressionNode node, List<VliwTask> allTasks, ref int opIdCounter)
        {
            VliwTask leftTask = null;
            VliwTask rightTask = null;

            if (node.Left != null)
                leftTask = VisitRecursive(node.Left, allTasks, ref opIdCounter);

            if (node.Right != null)
                rightTask = VisitRecursive(node.Right, allTasks, ref opIdCounter);

            var task = new VliwTask
            {
                Name = node.Value,
                Type = node.Type
            };

            if (node.Type == TokenType.OP || node.Type == TokenType.FN)
            {
                task.Id = opIdCounter++;
                task.OperationDuration = GetOpDuration(node.Value);

                if (leftTask != null)
                {
                    task.Parents.Add(leftTask);
                    leftTask.Children.Add(task);
                }
                if (rightTask != null)
                {
                    task.Parents.Add(rightTask);
                    rightTask.Children.Add(task);
                }
            }
            else
            {
                task.Id = 0;
                task.OperationDuration = 0;
            }

            allTasks.Add(task);
            return task;
        }

        private static int GetOpDuration(string op) => op switch
        {
            "+" or "-" => 1,
            "*" => 2,
            "/" => 4,
            _ => 1
        };
    }
}