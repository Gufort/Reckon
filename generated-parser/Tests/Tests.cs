using System;
using NUnit.Framework;
using SimpleParser;
using SimpleScanner;

[assembly: Property("Task", "GENERATED_PARSER_reckon")]

namespace Tests
{
    [TestFixture, Property("Score", 8)]
    [Property("Job", "TestParser")]
    public class TestParser
    {
        private bool Parse(string text)
        {
            Scanner scanner = new Scanner();
            scanner.SetSource(text, 0);

            Parser parser = new Parser(scanner);

            return parser.Parse();
        }

        [Test, Property("ScorePercentage", 25)]
        public void TestSimpleDefinitions()
        {
            Assert.True(Parse(@"A1 := 10
A2 := 20
A3 := A1 + A2"));

            Assert.True(Parse(@"total := 100
rate := 0.2
result := total * rate"));

            Assert.True(Parse(@"tax(rate, amount) := amount * rate
price := 100
total := price + tax(0.2, price)"));
        }

        [Test, Property("ScorePercentage", 20)]
        public void TestExpressionsAndPrecedence()
        {
            Assert.True(Parse(@"A1 := 1 + 2 * 3
A2 := (1 + 2) * 3
A3 := 10 / 2 - 3
A4 := 2 ^ 3 ^ 2
A5 := -5 + +3"));

            Assert.True(Parse(@"A1 := 10 > 5
A2 := 3 < 4
A3 := 5 = 5
A4 := 5 <> 6
A5 := 5 <= 5
A6 := 5 >= 5"));

            Assert.True(Parse(@"A1 := ""Hello"" & "" "" & ""World"""));
        }

        [Test, Property("ScorePercentage", 20)]
        public void TestRangesAndSheetRefs()
        {
            Assert.True(Parse(@"result := SUM(A1:A10)
avg := AVG(B2:B20)"));

            Assert.True(Parse(@"A1 := Sheet1!B2
val := Data!C5"));
        }

        [Test, Property("ScorePercentage", 20)]
        public void TestCallsAndHigherOrder()
        {
            Assert.True(Parse(@"A1 := SUM(1, 2, 3)
A2 := IF(A1 > 0, ""pos"", ""neg"")
A3 := SQRT(100)"));

            Assert.True(Parse(@"A1 := MAP(A1:A5, x -> x * 2)
A2 := FILTER(A1:A5, x -> x > 0)
A3 := REDUCE(A1:A5, 0, (acc, x) -> acc + x)
A4 := SORT(A1:A5)
A5 := UNIQUE(A1:A5)"));

            Assert.True(Parse(@"A1 := LAMBDA(x, y, x + y)"));
        }

        [Test, Property("ScorePercentage", 15)]
        public void TestErrorLiteralsAndLiterals()
        {
            Assert.True(Parse(@"A1 := #DIV/0!
A2 := #N/A
A3 := #NAME?
A4 := #REF!
A5 := #VALUE!
A6 := #NUM!
A7 := #NULL!
A8 := 42
A9 := ""string""
A10 := true
A11 := false
A12 := $A$1
A13 := @A
A14 := @1"));
        }
    }
}