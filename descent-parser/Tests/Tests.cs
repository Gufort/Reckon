using System;
using System.IO;
using NUnit.Framework;
using SimpleParser;
using SimpleLexer;

[assembly: Property("Task", "DESCENT_PARSER_reckon")]

namespace Tests
{
    public class ParserHelper
    {
        public static bool ParseSuccess(string text)
        {
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            Parser p = new Parser(l);
            p.Parse();
            return l.LexKind == Tok.EOF;
        }

        public static void ParseThrows(string text)
        {
            TextReader inputReader = new StringReader(text);
            Lexer l = new Lexer(inputReader);
            Parser p = new Parser(l);
            p.Parse();
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "ParserTests")]
    public class TestDefinitions
    {
        [Test, Property("ScorePercentage", 33)]
        public void TestCellRefDef()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("A1 := 42"));
        }

        [Test, Property("ScorePercentage", 33)]
        public void TestNamedCellDef()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("total := 100"));
        }

        [Test, Property("ScorePercentage", 34)]
        public void TestNamedFormulaDef()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("tax(rate, amount) := amount * rate"));
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "ParserTests")]
    public class TestExpressions
    {
        [Test, Property("ScorePercentage", 20)]
        public void TestSimpleArith()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("A1 := 42"));
        }

        [Test, Property("ScorePercentage", 20)]
        public void TestBinOp()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("A1 := 10 + 20"));
        }

        [Test, Property("ScorePercentage", 20)]
        public void TestPrecedence()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("A1 := 10 + 20 * 3"));
        }

        [Test, Property("ScorePercentage", 20)]
        public void TestParens()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("A1 := (A2 + A3) * 2"));
        }

        [Test, Property("ScorePercentage", 20)]
        public void TestUnaryMinus()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("A1 := -42"));
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "ParserTests")]
    public class TestComparisonsAndConcat
    {
        [Test, Property("ScorePercentage", 33)]
        public void TestEquality()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("A1 := price = 100"));
        }

        [Test, Property("ScorePercentage", 33)]
        public void TestComparison()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("A1 := price <> 100"));
        }

        [Test, Property("ScorePercentage", 34)]
        public void TestConcat()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("A1 := \"hello\" & \"world\""));
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "ParserTests")]
    public class TestRangesAndSheets
    {
        [Test, Property("ScorePercentage", 25)]
        public void TestSimpleRange()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("total := SUM(A1:A10)"));
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestRangeArg()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("A1 := SUM(B1:B5) + 1"));
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestSheetRef()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("A1 := Sheet1!B1 + 1"));
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestColumnRef()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("A1 := SUM(@A)"));
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "ParserTests")]
    public class TestCalls
    {
        [Test, Property("ScorePercentage", 25)]
        public void TestFuncCall()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("A1 := SUM(A1, A2, A3)"));
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestFuncCallNoArgs()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("A1 := PI()"));
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestNamedFormulaCall()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("total := price + tax(0.2, price)"));
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestFuncInExpr()
        {
            Assert.IsTrue(ParserHelper.ParseSuccess("A1 := IF(A1 > 0, 1, -1)"));
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "ParserTests")]
    public class TestInvalidPrograms
    {
        [Test, Property("ScorePercentage", 25)]
        public void TestMissingAssign()
        {
            Assert.Throws<ParserException>(() => ParserHelper.ParseThrows("A1 42"));
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestMissingExpr()
        {
            Assert.Throws<ParserException>(() => ParserHelper.ParseThrows("A1 :="));
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestBadDefinition()
        {
            Assert.Throws<ParserException>(() => ParserHelper.ParseThrows("42 := 10"));
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestUnclosedParen()
        {
            Assert.Throws<ParserException>(() => ParserHelper.ParseThrows("A1 := SUM(A1, A2"));
        }
    }

    [TestFixture, Property("Score", 1)]
    [Property("Job", "ParserTests")]
    public class TestFullProgram
    {
        [Test, Property("ScorePercentage", 50)]
        public void TestWorkbook()
        {
            string program = @"price := 100
tax(rate, amount) := amount * rate
total := price + tax(0.2, price)
A1 := Sheet1!B1 + SUM(A1:A10)
result := IF(A1 > 0, TRUE, #DIV/0!)";
            Assert.IsTrue(ParserHelper.ParseSuccess(program));
        }

        [Test, Property("ScorePercentage", 50)]
        public void TestMultipleDefs()
        {
            string program = @"A1 := 10
A2 := 20
A3 := A1 + A2
B1 := $A$1 + SUM(A1:A2)
total := price + tax(0.2, price)";
            Assert.IsTrue(ParserHelper.ParseSuccess(program));
        }
    }
}