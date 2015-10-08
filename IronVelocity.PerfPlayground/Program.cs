﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronVelocity.PerfPlayground
{
    public class Program
    {
        public static void Main()
        {
            var generator = new TemplateCompilation()
            {
                ExecuteTemplate = false,
                Compile = false,
                SaveDlls = false,
            };
            generator.TemplateDirectories.Add("../../templates/");
            generator.BlockDirectives.Add("registerEndOfPageHtml");

            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var testCases = generator.CreateTemplateTestCases().ToList(); ;

            int passCount = 0;
            var executionTime = new TimeSpan();
            var stopwatch = new Stopwatch();
            foreach (var testcase in testCases)
            {
                try
                {
                    stopwatch.Restart();
                    generator.TemplateCompilationTests((string)testcase.Arguments[0], (string)testcase.Arguments[1]);
                    stopwatch.Stop();
                    executionTime += stopwatch.Elapsed;
                    passCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed: " + testcase.TestName);
                    var originalColour = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: " + ex);
                    Console.ForegroundColor = originalColour;
                }
            }
            Console.WriteLine($"Processed {passCount} / {testCases.Count} in {executionTime.TotalMilliseconds:f2}ms");
            var avgExecutionTime = (double)executionTime.TotalMilliseconds / passCount;
            Console.WriteLine($"Avg execution time: { avgExecutionTime:f3}ms");
        }
    }
}
