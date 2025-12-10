using BuildingBlocks.ExpressionTree.Nodes;
using BuildingBlocks.Services.ExpressionAnalyzer;
using BuildingBlocks.Services.ParallelTreeBuilder;
using PZKS_Lab5.Models;
using PZKS_Lab5.Services;

namespace PZKS_Lab5
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var testExpressions = new List<string>
            {
                "a+b+c+d+e+f+g+h",
                "a-b-c-d-e-f-g-h",
                "a/b/c/d/e/f/g/h",
                "a*(b-4) - 2*b*c - c*d - a*c*d/e/f/g - g-h-i-j",
            };

            foreach (var expression in testExpressions)
            {
                Console.WriteLine($"--- TEST: \"{expression}\" ---");
                Console.WriteLine(new string('-', 50));

                var results = ExpressionAnalyzer.Analyze(expression);

                if (results.Errors.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Errors found!");
                    foreach (var error in results.Errors)
                    {
                        Console.WriteLine(error);
                    }
                    Console.ResetColor();
                }
                else
                {
                    var builder = new ParallelTreeBuilder();

                    ExpressionNode root = builder.BuildParallelTree(results.Tokens);

                    Console.WriteLine($"Optimized expression:{root.ToInfixString()}");
                    Console.WriteLine(root);
                    Console.WriteLine("\n--- VLIW Scheduling (5 Processors) ---");

                    var scheduler = new VliwScheduler(new VliwConfig());
                    scheduler.Process(root);

                    Console.ReadLine();
                }

                Console.WriteLine(new string('-', 50));
                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
