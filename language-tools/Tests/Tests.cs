using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleParser;
using SimpleScanner;
using SimpleLang.Visitors;
using ProgramTree;

[assembly: Property("Task", "LANGUAGE_TOOLS_reckon")]

namespace Tests
{
    public class ParserTest
    {
        public static Parser Parse(string text)
        {
            Scanner scanner = new Scanner();
            scanner.SetSource(text, 0);
            Parser parser = new Parser(scanner);
            return parser;
        }
    }

    [TestFixture, Property("Score", 2)]
    [Property("Job", "TestCycleDetectorTool")]
    public class TestCycleDetectorTool : ParserTest
    {
        private string RunTool(string program)
        {
            var tool = new Task.ReckonTool();
            return tool.Process(program);
        }

        [Test, Property("ScorePercentage", 25)]
        public void NoCyclesTest()
        {
            string result = RunTool("A1 := 10\nA2 := 20\nA3 := A1 + A2");
            Assert.IsTrue(result.Contains("No cycles detected"));
        }

        [Test, Property("ScorePercentage", 25)]
        public void SimpleCycleTest()
        {
            string result = RunTool("A1 := B1\nB1 := A1");
            Assert.IsTrue(result.Contains("cycle detected"));
            Assert.IsTrue(result.Contains("A1"));
            Assert.IsTrue(result.Contains("B1"));
        }

        [Test, Property("ScorePercentage", 25)]
        public void ComplexCycleTest()
        {
            string result = RunTool("A1 := B1\nB1 := C1\nC1 := A1");
            Assert.IsTrue(result.Contains("cycle detected"));
            Assert.IsTrue(result.Contains("A1 -> B1 -> C1 -> A1"));
        }

        [Test, Property("ScorePercentage", 25)]
        public void NoErrorsTest()
        {
            string result = RunTool("A1 := 10\ntotal := A1 + 5");
            Assert.IsTrue(result.Contains("No cycles"));
            Assert.IsFalse(result.Contains("Error:"));
        }
    }

    [TestFixture, Property("Score", 2)]
    [Property("Job", "TestMultipleDefinitions")]
    public class TestMultipleDefinitions : ParserTest
    {
        [Test, Property("ScorePercentage", 50)]
        public void NamedCellTest()
        {
            Parser p = Parse("price := 100\ntax := price * 0.2\ntotal := price + tax");
            Assert.IsTrue(p.Parse());
            var deps = new DependencyExtractor();
            p.root.Visit(deps);
            Assert.IsTrue(deps.GetDependencies("total").Contains("price"));
            Assert.IsTrue(deps.GetDependencies("total").Contains("tax"));
        }

        [Test, Property("ScorePercentage", 50)]
        public void NamedFormulaTest()
        {
            Parser p = Parse("tax(rate, amount) := amount * rate\nprice := 100\ntotal := price + tax(0.2, price)");
            Assert.IsTrue(p.Parse());
            var pp = new PrettyPrintVisitor();
            p.root.Visit(pp);
            Assert.IsTrue(pp.Text.Contains("tax"));
            Assert.IsTrue(pp.Text.Contains("price"));
        }
    }

    [TestFixture, Property("Score", 2)]
    [Property("Job", "TestDependencyExtractor")]
    public class TestDependencyExtractor : ParserTest
    {
        [Test, Property("ScorePercentage", 50)]
        public void SimpleDepsTest()
        {
            Parser p = Parse("A1 := 10\nA2 := 20\nA3 := A1 + A2");
            Assert.IsTrue(p.Parse());
            var deps = new DependencyExtractor();
            p.root.Visit(deps);
            Assert.IsTrue(deps.Dependencies.ContainsKey("A3"));
            Assert.IsTrue(deps.GetDependencies("A3").Contains("A1"));
            Assert.IsTrue(deps.GetDependencies("A3").Contains("A2"));
        }

        [Test, Property("ScorePercentage", 50)]
        public void NoDepsTest()
        {
            Parser p = Parse("A1 := 42\ntotal := 100");
            Assert.IsTrue(p.Parse());
            var deps = new DependencyExtractor();
            p.root.Visit(deps);
            Assert.AreEqual(0, deps.GetDependencies("A1").Count);
            Assert.AreEqual(0, deps.GetDependencies("total").Count);
        }
    }

    [TestFixture, Property("Score", 1.5)]
    [Property("Job", "TestCycleDetector")]
    public class TestCycleDetector : ParserTest
    {
        [Test, Property("ScorePercentage", 50)]
        public void NoCycleTest()
        {
            Parser p = Parse("A1 := 10\nA2 := A1 + 1\nA3 := A2 + 1");
            Assert.IsTrue(p.Parse());
            var detector = new CycleDetector();
            detector.Analyze((ProgramNode)p.root);
            Assert.IsFalse(detector.HasCycles());
        }

        [Test, Property("ScorePercentage", 50)]
        public void CycleTest()
        {
            Parser p = Parse("A1 := B1\nB1 := A1");
            Assert.IsTrue(p.Parse());
            var detector = new CycleDetector();
            detector.Analyze((ProgramNode)p.root);
            Assert.IsTrue(detector.HasCycles());
        }
    }

    [TestFixture, Property("Score", 1.5)]
    [Property("Job", "TestReferenceChecker")]
    public class TestReferenceChecker : ParserTest
    {
        [Test, Property("ScorePercentage", 50)]
        public void AllDefinedTest()
        {
            Parser p = Parse("A1 := 10\nA2 := A1 + 1");
            Assert.IsTrue(p.Parse());
            var checker = new ReferenceChecker();
            checker.Analyze((ProgramNode)p.root);
            Assert.IsFalse(checker.HasUndefinedReferences());
        }

        [Test, Property("ScorePercentage", 50)]
        public void UndefinedRefTest()
        {
            Parser p = Parse("A1 := B1 + C1\ntotal := A1");
            Assert.IsTrue(p.Parse());
            var checker = new ReferenceChecker();
            checker.Analyze((ProgramNode)p.root);
            Assert.IsTrue(checker.HasUndefinedReferences());
            Assert.IsTrue(checker.UndefinedReferences.Contains("B1"));
            Assert.IsTrue(checker.UndefinedReferences.Contains("C1"));
        }
    }
}