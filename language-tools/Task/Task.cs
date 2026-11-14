using System;
using System.IO;
using System.Text;
using SimpleScanner;
using SimpleParser;
using SimpleLang.Visitors;
using ProgramTree;

namespace SimpleCompiler
{
    public class SimpleCompilerMain
    {
        public static void Main()
        {
            string FileName = @"..\..\..\a.txt";
            try
            {
                string Text = File.ReadAllText(FileName);

                Scanner scanner = new Scanner();
                scanner.SetSource(Text, 0);

                Parser parser = new Parser(scanner);

                var b = parser.Parse();
                if (!b)
                    Console.WriteLine("Error");
                else
                {
                    Console.WriteLine("Parsing successful");

                    var pp = new PrettyPrintVisitor();
                    parser.root.Visit(pp);
                    Console.WriteLine(pp.Text);
                    Console.WriteLine("-------------------------------");

                    var deps = new DependencyExtractor();
                    parser.root.Visit(deps);
                    Console.WriteLine("Dependencies:");
                    foreach (var kv in deps.Dependencies)
                        Console.WriteLine("  {0} depends on [{1}]", kv.Key, string.Join(", ", kv.Value));
                    Console.WriteLine("-------------------------------");

                    var checker = new ReferenceChecker();
                    checker.Analyze((ProgramNode)parser.root);
                    if (checker.HasUndefinedReferences())
                        Console.WriteLine("Undefined refs: {0}", string.Join(", ", checker.UndefinedReferences));
                    else
                        Console.WriteLine("All references defined");
                    Console.WriteLine("-------------------------------");

                    var detector = new CycleDetector();
                    detector.Analyze((ProgramNode)parser.root);
                    if (detector.HasCycles())
                        Console.WriteLine("Cycles detected: {0}", detector.Cycles.Count);
                    else
                        Console.WriteLine("No cycles detected");

                    Console.WriteLine("-------------------------------");
                    var tool = new Task.ReckonTool();
                    Console.WriteLine("=== Cycle Detector Tool ===");
                    Console.WriteLine(tool.Process(Text));
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File {0} not found", FileName);
            }
            catch (LexException e)
            {
                Console.WriteLine("Lexical error: " + e.Message);
            }
            catch (SyntaxException e)
            {
                Console.WriteLine("Syntax error: " + e.Message);
            }

            Console.ReadLine();
        }
    }
}