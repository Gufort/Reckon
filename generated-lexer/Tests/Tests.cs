using System;
using NUnit.Framework;
using GeneratedLexer;
using System.Linq;

[assembly: Property("Task", "GENERATED_LEXER_reckon")]

namespace Tests
{
    [TestFixture, Property("Score", 1)]
    [Property("Job", "TestReckonIdCount")]
    public class TestReckonIdCount
    {
        [Test, Property("ScorePercentage", 50)]
        public void TestSimpleNames()
        {
            LexerAddon lexer = new LexerAddon(@"total price tax rate");
            lexer.Lex();
            Assert.AreEqual(4, lexer.idCount);
        }

        [Test, Property("ScorePercentage", 50)]
        public void TestNamesAndFuncs()
        {
            LexerAddon lexer = new LexerAddon(@"total SUM price IF rate");
            lexer.Lex();
            Assert.AreEqual(3, lexer.idCount);
            Assert.AreEqual(2, lexer.funcCount);
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "TestReckonNumbers")]
    public class TestReckonNumbers
    {
        [Test, Property("ScorePercentage", 50)]
        public void TestSumInt()
        {
            LexerAddon lexer = new LexerAddon(@"30 5 128");
            lexer.Lex();
            Assert.AreEqual(3, lexer.numCount);
            Assert.AreEqual(163, lexer.sumInt);
        }

        [Test, Property("ScorePercentage", 50)]
        public void TestRealNumbers()
        {
            LexerAddon lexer = new LexerAddon(@"3.14 2.5 0.5");
            lexer.Lex();
            Assert.AreEqual(3, lexer.numCount);
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "TestReckonKeywords")]
    public class TestReckonKeywords
    {
        [Test, Property("ScorePercentage", 50)]
        public void TestBoolKeywords()
        {
            LexerAddon lexer = new LexerAddon(@"TRUE FALSE");
            lexer.Lex();
            Assert.AreEqual(2, lexer.kwCount);
        }

        [Test, Property("ScorePercentage", 50)]
        public void TestErrorLiterals()
        {
            LexerAddon lexer = new LexerAddon(@"#DIV/0! #N/A #VALUE! #REF! #NAME? #NUM! #NULL!");
            lexer.Lex();
            Assert.AreEqual(7, lexer.errorCount);
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "TestReckonRefs")]
    public class TestReckonRefs
    {
        [Test, Property("ScorePercentage", 34)]
        public void TestCellRefs()
        {
            LexerAddon lexer = new LexerAddon(@"A1 $A$1 A$1 $A1 AA100");
            lexer.Lex();
            Assert.AreEqual(5, lexer.cellRefCount);
        }

        [Test, Property("ScorePercentage", 33)]
        public void TestColRefs()
        {
            LexerAddon lexer = new LexerAddon(@"@A @AB @Z");
            lexer.Lex();
            Assert.AreEqual(3, lexer.colRefCount);
        }

        [Test, Property("ScorePercentage", 33)]
        public void TestRowRefs()
        {
            LexerAddon lexer = new LexerAddon(@"@1 @42 @100");
            lexer.Lex();
            Assert.AreEqual(3, lexer.rowRefCount);
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "TestReckonStrings")]
    public class TestReckonStrings
    {
        [Test, Property("ScorePercentage", 50)]
        public void TestSimpleStrings()
        {
            LexerAddon lexer = new LexerAddon(@"""hello"" ""world""");
            lexer.Lex();
            Assert.AreEqual(2, lexer.stringCount);
        }

        [Test, Property("ScorePercentage", 50)]
        public void TestEscapedStrings()
        {
            LexerAddon lexer = new LexerAddon(@"""he said \""hi\""""");
            lexer.Lex();
            Assert.AreEqual(1, lexer.stringCount);
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "TestReckonComments")]
    public class TestReckonComments
    {
        [Test, Property("ScorePercentage", 50)]
        public void TestLineComment()
        {
            LexerAddon lexer = new LexerAddon(@"A1 := 10 -- this is a comment
A2 := 20");
            lexer.Lex();
            Assert.AreEqual(2, lexer.cellRefCount);
            Assert.AreEqual(2, lexer.numCount);
            Assert.AreEqual(2, lexer.lineCount);
        }

        [Test, Property("ScorePercentage", 50)]
        public void TestBlockComment()
        {
            LexerAddon lexer = new LexerAddon(@"A1 := 10 {- block
comment -} A2 := 20");
            lexer.Lex();
            Assert.AreEqual(2, lexer.cellRefCount);
            Assert.AreEqual(2, lexer.numCount);
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "TestReckonOperators")]
    public class TestReckonOperators
    {
        [Test, Property("ScorePercentage", 34)]
        public void TestMulticharOperators()
        {
            LexerAddon lexer = new LexerAddon(@":= -> <= <> >=");
            lexer.Lex();
        }

        [Test, Property("ScorePercentage", 33)]
        public void TestComparisons()
        {
            LexerAddon lexer = new LexerAddon(@"< > = <= >=");
            lexer.Lex();
        }

        [Test, Property("ScorePercentage", 33)]
        public void TestArithmetic()
        {
            LexerAddon lexer = new LexerAddon(@"+ - * / ^ &");
            lexer.Lex();
        }
    }
}