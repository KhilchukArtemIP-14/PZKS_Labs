using BuildingBlocks.Services;
using PZKS_Lab2.Nodes;
using PZKS_Lab2.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PZKS_Lab2
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
                "a+(b+c+d+(e+f)+g)+h",
                "a-((b-c-d)-(e-f)-g)-h",
                "5040/8/7/6/5/4/3/2",
                "10-9-8-7-6-5-4-3-2-1",
                "64-(32-16)-8-(4-2-1)",
                "-(-i)/1.0 + 0 - 0*k*h + 2 - 4.8/2 + 1*e/2",
                "a*2/0 + b/(b+b*0-1*b) - 1/(c*2*4.76*(1-2+1))",
                "a + cos(b+c)",
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

                    Console.WriteLine("Resulting tree:");
                    Console.WriteLine(root);
                }

                Console.WriteLine(new string('-', 50));
                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}