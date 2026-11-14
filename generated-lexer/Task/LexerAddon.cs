using System;
using System.IO;
using SimpleScanner;
using ScannerHelper;
using System.Collections.Generic;

namespace GeneratedLexer
{

    public class LexerAddon
    {
        public Scanner myScanner;
        private string inputText;
        public int lineCount
        {
            get { return myScanner.lineCount; }
        }
        public int idCount
        {
            get { return myScanner.idCount; }
        }
        public int funcCount
        {
            get { return myScanner.funcCount; }
        }
        public int kwCount
        {
            get { return myScanner.kwCount; }
        }
        public int numCount
        {
            get { return myScanner.numCount; }
        }
        public int stringCount
        {
            get { return myScanner.stringCount; }
        }
        public int cellRefCount
        {
            get { return myScanner.cellRefCount; }
        }
        public int colRefCount
        {
            get { return myScanner.colRefCount; }
        }
        public int rowRefCount
        {
            get { return myScanner.rowRefCount; }
        }
        public int errorCount
        {
            get { return myScanner.errorCount; }
        }
        public int sumInt
        {
            get { return myScanner.sumInt; }
        }
        public bool lexerError
        {
            get { return myScanner.lexerError; }
        }

        public LexerAddon(string programText)
        {
            inputText = programText;
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(programText);
            writer.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            myScanner = new Scanner(ms);
        }

        public void Lex()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            int tok = 0;
            do
            {
                tok = myScanner.yylex();

                if (tok == (int)Tok.EOF)
                {
                    break;
                }
            } while (true);
        }
    }
}