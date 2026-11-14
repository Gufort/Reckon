using System;
using NUnit.Framework;
using System.IO;
using Reckon;
using System.Collections.Generic;
using System.Linq;

[assembly: Property("Task", "SIMPLE_LEXER_reckon")]

namespace Tests
{
    public class LexemList : List<KeyValuePair<Tok, string>>
    {
        public void Add(Tok key, string value)
        {
            var element = new KeyValuePair<Tok, string>(key, value);
            this.Add(element);
        }
    }

    public class LexerHelper
    {
        public static List<KeyValuePair<Tok, string>> getLexerOutput(Lexer l)
        {
            List<KeyValuePair<Tok, string>> result = new List<KeyValuePair<Tok, string>>();
            do
            {
                result.Add(new KeyValuePair<Tok, string>(l.LexKind, l.LexText));
                l.NextLexem();
            } while (l.LexKind != Tok.EOF);
            return result;
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "TestReckonLexer")]
    public class TestReckonLexer
    {
        [Test, Property("ScorePercentage", 25)]
        public void TestNumbers()
        {
            string text = "42 3.14 0";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(3, lexems.Count);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.NUMBER, "42"},
                {Tok.NUMBER, "3.14"},
                {Tok.NUMBER, "0"},
            }.ToList(), lexems);
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestNames()
        {
            string text = "total price tax_rate _value";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(4, lexems.Count);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.NAME, "total"},
                {Tok.NAME, "price"},
                {Tok.NAME, "tax_rate"},
                {Tok.NAME, "_value"},
            }.ToList(), lexems);
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestBooleans()
        {
            string text = "TRUE FALSE";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(2, lexems.Count);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.BOOL, "TRUE"},
                {Tok.BOOL, "FALSE"},
            }.ToList(), lexems);
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestFuncNameVsCellRef()
        {
            string text = "SUM A1 IF AA100 total";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(5, lexems.Count);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.FUNCNAME, "SUM"},
                {Tok.CELLREF, "A1"},
                {Tok.FUNCNAME, "IF"},
                {Tok.CELLREF, "AA100"},
                {Tok.NAME, "total"},
            }.ToList(), lexems);
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "TestReckonOps")]
    public class TestReckonOps
    {
        [Test, Property("ScorePercentage", 50)]
        public void TestBasicOps()
        {
            string text = "+ - * / ^ & := ( ) , : !";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(12, lexems.Count);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.PLUS, "+"},
                {Tok.MINUS, "-"},
                {Tok.MULT, "*"},
                {Tok.DIV, "/"},
                {Tok.POW, "^"},
                {Tok.CONCAT, "&"},
                {Tok.ASSIGN, ":="},
                {Tok.LPAREN, "("},
                {Tok.RPAREN, ")"},
                {Tok.COMMA, ","},
                {Tok.COLON, ":"},
                {Tok.BANG, "!"},
            }.ToList(), lexems);
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestComparisons()
        {
            string text = "= <> < > <= >=";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(6, lexems.Count);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.EQ, "="},
                {Tok.NEQ, "<>"},
                {Tok.LT, "<"},
                {Tok.GT, ">"},
                {Tok.LEQ, "<="},
                {Tok.GEQ, ">="},
            }.ToList(), lexems);
        }

        [Test, Property("ScorePercentage", 25)]
public void TestArrowVsMinus()
        {
            string text = "-> - ->->-";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(5, lexems.Count);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.ARROW, "->"},
                {Tok.MINUS, "-"},
                {Tok.ARROW, "->"},
                {Tok.ARROW, "->"},
                {Tok.MINUS, "-"},
            }.ToList(), lexems);
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "TestReckonComments")]
    public class TestReckonComments
    {
        [Test, Property("ScorePercentage", 25)]
        public void TestLineComment()
        {
            string text = "A1 := 10 -- this is a comment\nB1 := 20";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.CELLREF, "A1"},
                {Tok.ASSIGN, ":="},
                {Tok.NUMBER, "10"},
                {Tok.CELLREF, "B1"},
                {Tok.ASSIGN, ":="},
                {Tok.NUMBER, "20"},
            }.ToList(), lexems);
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestLineCommentToFileEnd()
        {
            string text = "A1 := 10 -- comment without newline";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(3, lexems.Count);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.CELLREF, "A1"},
                {Tok.ASSIGN, ":="},
                {Tok.NUMBER, "10"},
            }.ToList(), lexems);
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestBlockComment()
        {
            string text = "A1 := {- comment -} 42";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(3, lexems.Count);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.CELLREF, "A1"},
                {Tok.ASSIGN, ":="},
                {Tok.NUMBER, "42"},
            }.ToList(), lexems);
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestNestedBlockComment()
        {
            string text = "A1 := {- outer {- inner -} end -} 99";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(3, lexems.Count);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.CELLREF, "A1"},
                {Tok.ASSIGN, ":="},
                {Tok.NUMBER, "99"},
            }.ToList(), lexems);
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "TestReckonStringsErrors")]
    public class TestReckonStringsErrors
    {
        [Test, Property("ScorePercentage", 25)]
        public void TestStringLiterals()
        {
            string text = "\"hello\" \"world\"";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(2, lexems.Count);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.STRING, "\"hello\""},
                {Tok.STRING, "\"world\""},
            }.ToList(), lexems);
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestStringEscapes()
        {
            string text = "\"hello\\\"world\" \"a\\nb\"";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(2, lexems.Count);
            Assert.AreEqual(Tok.STRING, lexems[0].Key);
            Assert.AreEqual("\"hello\\\"world\"", lexems[0].Value);
            Assert.AreEqual("\"a\\nb\"", lexems[1].Value);
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestErrorLiterals()
        {
            string text = "#DIV/0! #N/A #VALUE! #REF! #NAME? #NUM! #NULL!";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(7, lexems.Count);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.ERROR, "#DIV/0!"},
                {Tok.ERROR, "#N/A"},
                {Tok.ERROR, "#VALUE!"},
                {Tok.ERROR, "#REF!"},
                {Tok.ERROR, "#NAME?"},
                {Tok.ERROR, "#NUM!"},
                {Tok.ERROR, "#NULL!"},
            }.ToList(), lexems);
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestUnterminatedString()
        {
            string text = "\"hello";
            TextReader inputReader = new StringReader(text);
            Assert.Throws<LexerException>(() =>
            {
                Lexer l = new Lexer(inputReader);
                var lexems = LexerHelper.getLexerOutput(l);
            });
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "TestReckonRefs")]
    public class TestReckonRefs
    {
        [Test, Property("ScorePercentage", 25)]
        public void TestCellRefVariants()
        {
            string text = "A1 $A$1 A$1 $A1 AA123";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(5, lexems.Count);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.CELLREF, "A1"},
                {Tok.CELLREF, "$A$1"},
                {Tok.CELLREF, "A$1"},
                {Tok.CELLREF, "$A1"},
                {Tok.CELLREF, "AA123"},
            }.ToList(), lexems);
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestColRef()
        {
            string text = "@A @AB @Z";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(3, lexems.Count);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.COLREF, "@A"},
                {Tok.COLREF, "@AB"},
                {Tok.COLREF, "@Z"},
            }.ToList(), lexems);
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestRowRef()
        {
            string text = "@1 @42 @100";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            Assert.AreEqual(3, lexems.Count);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.ROWREF, "@1"},
                {Tok.ROWREF, "@42"},
                {Tok.ROWREF, "@100"},
            }.ToList(), lexems);
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestInvalidSymbol()
        {
            string text = "A1 @";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            Assert.Throws<LexerException>(() =>
            {
                var lexems = LexerHelper.getLexerOutput(l);
            });
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "TestReckonProgram")]
    public class TestReckonProgram
    {
        [Test, Property("ScorePercentage", 100)]
        public void TestFullProgram()
        {
            string text = @"A1 := 10
A2 := $A$1 + SUM(A1:A2) -- cell sum
{- block {- nested -} comment -}
result := IF(A1 > 0, ""ok"", #DIV/0!)";
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            var lexems = LexerHelper.getLexerOutput(l);
            CollectionAssert.AreEqual(new LexemList()
            {
                {Tok.CELLREF, "A1"},
                {Tok.ASSIGN, ":="},
                {Tok.NUMBER, "10"},
                {Tok.CELLREF, "A2"},
                {Tok.ASSIGN, ":="},
                {Tok.CELLREF, "$A$1"},
                {Tok.PLUS, "+"},
                {Tok.FUNCNAME, "SUM"},
                {Tok.LPAREN, "("},
                {Tok.CELLREF, "A1"},
                {Tok.COLON, ":"},
                {Tok.CELLREF, "A2"},
                {Tok.RPAREN, ")"},
                {Tok.NAME, "result"},
                {Tok.ASSIGN, ":="},
                {Tok.FUNCNAME, "IF"},
                {Tok.LPAREN, "("},
                {Tok.CELLREF, "A1"},
                {Tok.GT, ">"},
                {Tok.NUMBER, "0"},
                {Tok.COMMA, ","},
                {Tok.STRING, "\"ok\""},
                {Tok.COMMA, ","},
                {Tok.ERROR, "#DIV/0!"},
                {Tok.RPAREN, ")"},
            }.ToList(), lexems);
        }
    }
}