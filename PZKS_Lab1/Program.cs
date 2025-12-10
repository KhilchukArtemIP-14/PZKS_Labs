using PZKS_Lab1.Services;
using System;
using System.Collections.Generic;

namespace PZKS_Lab1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var testExpressions = new List<string>
            {
                // === Valid Expressions ===
                /*"3.15 + 4545.313",
                "3 + 5",
                "x * (y + 2)",
                "sin(x) + cos(y)",
                "2 * (3 + 4) / 5",
                "sqrt(a) - log(b)",
                "(x + y) * (z - w)",
                "3 + 5 * (2 - 4)",
                "x / (y - z) * a",
                "abs(-5) + round(4.67)",
                "log(10) + sqrt(16)",
                "a+b*(c*cos(t-a*x)-d*sin(t+a*x)/(4.81*k-q*t))/(d*cos(t+a*y/f(5.616*x-t))+c*sin(t-a*y*(u-v*1)))",
                "- (3 + 5)",


                // === Invalid Expressions - starts with invalid characters ===
                "* 3 + 5",
                "/ 2 - 1",
                ") x + 5",

                // === Invalid Expressions - ends with invalid characters ===
                "3 + 5 *",
                "(x - y /",
                "sqrt(16) +",
                "(a + b -",

                // === Invalid Expressions - mismatched brackets ===
                "3 + (4 - 5",
                "(2 + 3)) * 7",
                "(x * (y + z)",
                "((a - b) + c",
                "log(10 + sqrt(25)) + abs(x",

                // === Invalid Expressions - empty brackets ===
                "x * () + 5",
                "sin() + cos(y)",
                "(3 + ) * 4",
                "log( + b)",

                // === Stress Tests xp ===
                "1 /a*b**c + m)*a*b + a*c - a*smn(j*k/m + m",
                "-cos(-&t))/(*(*f)(127.0.0.1, \"/dev/null\", (t==0)?4more_errors:b^2) - .5",
                "//(*0)- an*0p(a+b)-1.000.5//6(*f(-b, 1.8-0*(2-6) %1 + (++a)/(6x^2+4x-1) + d/dt*(smn(at+q)/(4cos(at)-ht^2)",
                "-(-5x((int*)exp())/t - 3.14.15k/(2x^2-5x-1)*y - A[N*(i++)+j]",
                "-(-exp(3et/4.0.2, 2i-1)/L + )((void*)*f()) + ((i++) + (++i/(i--))/k//) + 6.000.500.5",
                "**f(*k, -p+1, ))2.1.1 + 1.8q((-5x ++ i)",
                "/.1(2x^2-5x+7)-(-i)+ (j++)/0 - )(*f)(2, 7-x, )/q + send(-(2x+7)/A[j, i], 127.0.0.1 ) + )/",
                "*101*1#(t-q)(t+q)//dt - (int*)f(8t, -(k/h)A[i+6.]), exp(), ))(t-k*8.00.1/.0",*/

                //===//
                "-3+12*c*d/e-d*f/cd*(a+2.2*4)",
                "-(b+c)+func1((1a*baa+1bj_ko2*(j-e))",
                "-a+b2*0-nm",
                "g2*(b-17.3)))+(6-cos(5)))",
                "-(215.01+312,2)b)+(1c",
                "bj_k.o2"
            };

            foreach (var expression in testExpressions)
            {
                Console.WriteLine($"--- Analyzing Expression: \"{expression}\" ---");
                var results = ExpressionAnalyzer.Analyze(expression);
                foreach (var line in results)
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine(new string('-', 50));
                Console.WriteLine();
            }
        }
    }
}