using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ScannerHelper;
using SimpleScanner;
using System.IO;

namespace GeneratedLexer
{

    public class LexerException : System.Exception
    {
        public LexerException(string msg)
            : base(msg)
        {
        }

    }

    public static class LexerMain
    {
        public static void Lex(string[] args)
        {
            int cnt_id = 0;
            int min_id_len = Int32.MaxValue, max_id_len = 0; 
            double avg_id_len = 0;

            int sum_int = 0; 
            double sum_d = 0;

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            var fname = @"..\..\..\a.txt";
            Console.WriteLine(File.ReadAllText(fname));
            Console.WriteLine("-------------------------");

            Scanner scanner = new Scanner(new FileStream(fname, FileMode.Open));

            int tok = 0;
            do
            {
                tok = scanner.yylex();

                if (tok == (int)Tok.EOF)
                {
                    Console.WriteLine();
                    Console.WriteLine("number of id: {0:D}", cnt_id);
                    Console.WriteLine("average length of the id: {0:N}", avg_id_len / cnt_id);
                    Console.WriteLine("min length of the id: {0:D}", min_id_len);
                    Console.WriteLine("min length of the id: {0:D}", max_id_len);

                    Console.WriteLine();
                    Console.WriteLine("sum of int: {0:D}", sum_int);
                    Console.WriteLine("sum of double: {0:N}", sum_d);

                    Console.WriteLine();

                    break;
                }

                Console.WriteLine(scanner.TokToString((Tok)tok));
            } while (true);

            Console.ReadKey();
        }

    }


    public class Program
    {
        public static void Main(string[] args)
        {
            LexerMain.Lex(args);

        }
    }
}