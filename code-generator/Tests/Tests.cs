using System;
using System.Collections.Generic;
using NUnit.Framework;
using SimpleParser;
using SimpleScanner;
using SimpleLang.Visitors;
using ProgramTree;

[assembly: Property("Task", "CODE_GENERATOR_reckon")]

namespace Tests
{
    [TestFixture, Property("Score", 7)]
    [Property("Job", "TestGenCode")]
    public class TestGenCode
    {
        [Test]
        public void TestStubShouldFail()
        {
            string text = "A1 := 10";
            Scanner scanner = new Scanner();
            scanner.SetSource(text, 0);
            Parser parser = new Parser(scanner);
            Assert.IsTrue(parser.Parse());
            var code = new GenCodeVisitor();
            code.VisitProgramNode((ProgramNode)parser.root);
            Assert.Fail("Stub should throw NotImplementedException");
        }
    }
}