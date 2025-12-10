using System;
using System.Linq;
using System.Collections.Generic;
using PZKS_Lab5.Models;

namespace PZKS_Lab5.Services
{
    public static class VliwVisualizer
    {
        public static void PrintAll(VliwTaskGraph graph, VliwConfig config, VliwMemoryBank[] banks)
        {
            Console.WriteLine("\n=== Scheduled Task Tree ===");
            PrintTaskTree(graph.Root, "", true);

            PrintTable(graph);
            PrintProcessorGantt(graph, config);
            PrintBankGantt(graph, config, banks);
            PrintMetrics(graph, config);
        }

        private static void PrintTaskTree(VliwTask node, string indent, bool isLast)
        {
            if (node == null || node.IsLeaf) return;
            var marker = isLast ? "└──" : "├──";
            Console.WriteLine($"{indent}{marker}({node.Name}) [ID:{node.Id}]");
            indent += isLast ? "    " : "│   ";
            var opParents = node.Parents.Where(p => !p.IsLeaf).ToList();
            for (int i = 0; i < opParents.Count; i++) PrintTaskTree(opParents[i], indent, i == opParents.Count - 1);
        }

        private static void PrintTable(VliwTaskGraph graph)
        {
            Console.WriteLine("\n=== Detailed Operation Schedule ===");
            Console.WriteLine($"{"Task",-6} {"Op",-4} {"Proc",-4} {"Read",-8} {"Calc",-8} {"Write",-8} {"End",-4} {"Bank",-4}");
            Console.WriteLine(new string('-', 60));

            foreach (var t in graph.Operations.OrderBy(x => x.StartTime).ThenBy(x => x.Id))
                Console.WriteLine($"{t.Id,-6} {t.Name,-4} P{t.ProcessorId + 1,-4} {t.ReadStartTime,-8} {t.CalcStartTime,-8} {t.WriteStartTime,-8} {t.EndTime,-4} B{t.ResultBankId + 1,-4}");
        }

        private static void PrintProcessorGantt(VliwTaskGraph graph, VliwConfig config)
        {
            Console.WriteLine("\n=== Processors Gantt (VLIW Sync) ===");
            var ops = graph.Operations;
            if (ops.Count == 0) return;

            int maxTime = ops.Max(t => t.EndTime);
            PrintTimelineHeader(maxTime);

            for (int p = 0; p < config.NumProcessors; p++)
            {
                Console.Write($"P{p + 1}:  ");
                for (int t = 0; t < maxTime; t++)
                {
                    string symbol = ".  ";
                    var task = ops.FirstOrDefault(x => x.ProcessorId == p && t >= x.ReadStartTime && t < x.EndTime);
                    if (task != null)
                    {
                        if (t >= task.ReadStartTime && t < task.CalcStartTime) symbol = "R  ";
                        else if (t >= task.CalcStartTime && t < task.WriteStartTime) symbol = "C  ";
                        else if (t >= task.WriteStartTime && t < task.EndTime) symbol = "W  ";
                    }
                    Console.Write(symbol);
                }
                Console.WriteLine();
            }
        }

        private static void PrintBankGantt(VliwTaskGraph graph, VliwConfig config, VliwMemoryBank[] banks)
        {
            Console.WriteLine("\n=== Memory Banks Gantt ===");
            if (graph.Operations.Count == 0) return;

            int maxTime = graph.Operations.Max(t => t.EndTime);
            PrintTimelineHeader(maxTime);

            for (int b = 0; b < config.NumBanks; b++)
            {
                Console.Write($"B{b + 1}:  ");
                for (int t = 0; t < maxTime; t++)
                {
                    string symbol = ".  ";
                    var eventString = banks[b].GetEventString(t);
                    if (eventString.Contains("W")) symbol = "W  ";
                    else if (eventString.Contains("R")) symbol = "R  ";
                    Console.Write(symbol);
                }
                Console.WriteLine();
            }
        }

        private static void PrintMetrics(VliwTaskGraph graph, VliwConfig config)
        {
            var ops = graph.Operations;
            if (ops.Count == 0) return;

            int parallelTime = ops.Max(t => t.EndTime);
            int sequentialTime = 0;

            foreach (var task in ops)
            {
                int readTime = task.Parents.All(p => p.IsLeaf) ? config.TimeReadLeaf : 0;
                sequentialTime += readTime + task.OperationDuration + config.TimeWrite;
            }

            double speedup = (double)sequentialTime / parallelTime;
            double efficiency = speedup / config.NumProcessors;

            Console.WriteLine("\n=== Metrics ===");
            Console.WriteLine($"Parallel Time (Tp):   {parallelTime}");
            Console.WriteLine($"Sequential Time (Ts): {sequentialTime}");
            Console.WriteLine($"Speedup (Ky):         {speedup:F3}");
            Console.WriteLine($"Efficiency (e):       {efficiency:F3}");
        }

        private static void PrintTimelineHeader(int maxTime)
        {
            Console.Write("T:   ");
            for (int i = 1; i <= maxTime; i++) Console.Write($"{i,-3}");
            Console.WriteLine("\n" + new string('-', 5 + maxTime * 3));
        }
    }
}