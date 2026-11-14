using System;
using System.IO;
using SimpleScanner;
using SimpleParser;
using SimpleLang.Visitors;

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
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File {0} not found", FileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e);
            }

            Console.ReadLine();
        }
    }
}