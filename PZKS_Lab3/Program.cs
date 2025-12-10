using BuildingBlocks.ExpressionTree.Nodes;
using BuildingBlocks.Services.ExpressionAnalyzer;
using BuildingBlocks.Services.ParallelTreeBuilder;
using PZKS_Lab3.Services;
using System.Diagnostics.Metrics;

namespace PZKS_Lab_3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var commutativeLawTests = new List<string>
            {
                "a+b+c+d",
                "x*y*z*w",
                "a-b*c+k",
                "A - B*c - L*k*2 + D*t/d*y - H + Q*3 - J*(w-1)/r + P",
                "A-B*c-J*(d*t*j-u*t+c*r-1+w-k/q+m*(n-k*s+z*(y+u*p-y/r-5)+x+t/2))/r+P",
                "a + b*c + d + e*f*g + h*i + j*(k + L + m*(n-p*q+r) - s*t)",
                "sin(x) + cos(y) * 2",
                "a*(c+d+a*(b-c))"
            };

            foreach (var expression in commutativeLawTests)
            {
                Console.WriteLine($"--- TEST: \"{expression}\" ---");
                Console.WriteLine(new string('-', 50));

                var results = ExpressionAnalyzer.Analyze(expression);

                var builder = new ParallelTreeBuilder();

                ExpressionNode root = builder.BuildParallelTree(results.Tokens);

                Console.WriteLine($"Optimized expression:{root.ToInfixString()}");

                var forms = CommutativeTransformer.GetEquivalentForms(root);

                Console.WriteLine($"Equivalent Forms found: {forms.Count}");

                int counter = 1;
                if (forms.Count > 50) Console.WriteLine("Too many forms! Outputting first 50:");
                foreach (var form in forms.Take(50))
                {
                    Console.WriteLine($"Form {counter++}:   {form}");
                }
                Console.WriteLine(new string('=', 50));
                Console.WriteLine();
                Console.WriteLine();
            }

            var distributiveTests = new List<string>
            {
                "a*(b+c-1)*d",
                "(a-c)*(b-k+1)",
                "(1-d)/(a+b-2)/e",
                "a-b*(k-t)-(f-g)*(f*5.9-q)-(f+g)/(d+q-w)",
                "a-b*(k-t+(f-g)*(f*5.9-q)+(w-y*(m-1))/p)-(x-3)*(x+3)/(d+q-w)",
                "a*(c+d+a*(b-c))"
            };

            foreach (var expression in distributiveTests)
            {
                Console.WriteLine($"\n--- TEST: \"{expression}\" ---");

                var results = ExpressionAnalyzer.Analyze(expression);
                if (results.Errors.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    foreach (var error in results.Errors) Console.WriteLine(error);
                    Console.ResetColor();
                    continue;
                }

                var builder = new ParallelTreeBuilder();
                ExpressionNode root = builder.BuildParallelTree(results.Tokens);
                Console.WriteLine($"Optimized expression:{root.ToInfixString()}");

                var forms = DistributiveTransformer.GetDistributiveForms(root);

                Console.WriteLine($"Equivalent Forms found: {forms.Count}");
                if (forms.Count > 50) Console.WriteLine("Too many forms! Outputting first 50:");
                int counter = 1;
                foreach (var form in forms.Take(50))
                {
                    Console.WriteLine($"Form {counter++}:   {form}");
                }
                Console.WriteLine(new string('=', 50));
                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
