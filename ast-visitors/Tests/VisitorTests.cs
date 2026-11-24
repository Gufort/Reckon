using System.Collections.Generic;
using NUnit.Framework;
using SimpleLang.Visitors;
using ProgramTree;

namespace Tests
{
    [TestFixture]
    public class TestAutoVisitor : ParserTest
    {
        private class CountingVisitor : AutoVisitor
        {
            public int Programs;
            public int Definitions;
            public int BinOps;
            public int Numbers;

            public override void VisitProgramNode(ProgramNode n) { Programs++; base.VisitProgramNode(n); }
            public override void VisitCellDefNode(CellDefNode n) { Definitions++; base.VisitCellDefNode(n); }
            public override void VisitBinOpNode(BinOpNode n) { BinOps++; base.VisitBinOpNode(n); }
            public override void VisitNumberNode(NumberNode n) { Numbers++; }
        }

        [Test]
        public void VisitsWholeTreeTest()
        {
            var p = Parse("A1 := (2 + 3) * 4");
            Assert.IsTrue(p.Parse());

            var visitor = new CountingVisitor();
            p.root.Visit(visitor);

            Assert.AreEqual(1, visitor.Programs);
            Assert.AreEqual(1, visitor.Definitions);
            Assert.AreEqual(2, visitor.BinOps);
            Assert.AreEqual(3, visitor.Numbers);
        }
    }

    [TestFixture]
    public class TestDependencyExtractorAdvanced : ParserTest
    {
        [Test]
        public void NamedFormulaParametersAreNotDependenciesTest()
        {
            var p = Parse("tax(rate, amount) := amount * rate\nprice := 100\ntotal := price + tax(0.2, price)");
            Assert.IsTrue(p.Parse());

            var deps = new DependencyExtractor();
            p.root.Visit(deps);

            CollectionAssert.Contains(deps.GetDependencies("total"), "price");
            CollectionAssert.Contains(deps.GetDependencies("total"), "tax");
            Assert.AreEqual(0, deps.GetDependencies("tax").Count);
            Assert.AreEqual(0, deps.GetDependencies("price").Count);
        }

        [Test]
        public void RangeDependenciesAreExpandedTest()
        {
            var p = Parse("A1 := 1\nB1 := 2\nA2 := 3\nB2 := 4\nC1 := A1:B2");
            Assert.IsTrue(p.Parse());

            var deps = new DependencyExtractor();
            p.root.Visit(deps);

            CollectionAssert.AreEquivalent(new[] { "A1", "A2", "B1", "B2" }, deps.GetDependencies("C1"));
        }

        [Test]
        public void AbsoluteAddressIsNormalizedTest()
        {
            var p = Parse("A1 := 1\nB1 := $A$1 + 2");
            Assert.IsTrue(p.Parse());

            var deps = new DependencyExtractor();
            p.root.Visit(deps);

            CollectionAssert.Contains(deps.GetDependencies("B1"), "A1");
            Assert.AreEqual(0, deps.GetDependencies("unknown").Count);
        }

        [Test]
        public void SingleParameterFormulaTest()
        {
            var p = Parse("f(x) := x + A1\nA1 := 1\nB1 := f(A1)");
            Assert.IsTrue(p.Parse());

            var deps = new DependencyExtractor();
            p.root.Visit(deps);

            CollectionAssert.AreEquivalent(new[] { "A1" }, deps.GetDependencies("f"));
            CollectionAssert.AreEquivalent(new[] { "A1", "f" }, deps.GetDependencies("B1"));
        }
    }

    [TestFixture]
    public class TestCycleDetectorAdvanced : ParserTest
    {
        [Test]
        public void ThreeNodeCycleTest()
        {
            var p = Parse("A1 := B1 + 1\nB1 := C1 + 1\nC1 := A1 + 1");
            Assert.IsTrue(p.Parse());

            var detector = new CycleDetector();
            detector.Analyze((ProgramNode)p.root);

            Assert.IsTrue(detector.HasCycles());
            Assert.AreEqual(1, detector.Cycles.Count);
            CollectionAssert.AreEquivalent(new[] { "A1", "B1", "C1" }, detector.Cycles[0]);
        }

        [Test]
        public void SelfCycleTest()
        {
            var p = Parse("A1 := A1 + 1");
            Assert.IsTrue(p.Parse());

            var detector = new CycleDetector();
            detector.Analyze((ProgramNode)p.root);

            Assert.IsTrue(detector.HasCycles());
        }

        [Test]
        public void AnalyzeResetsPreviousResultTest()
        {
            var cyclic = Parse("A1 := B1\nB1 := A1");
            Assert.IsTrue(cyclic.Parse());

            var detector = new CycleDetector();
            detector.Analyze((ProgramNode)cyclic.root);
            Assert.IsTrue(detector.HasCycles());

            var acyclic = Parse("A1 := 1\nB1 := A1");
            Assert.IsTrue(acyclic.Parse());

            detector.Analyze((ProgramNode)acyclic.root);
            Assert.IsFalse(detector.HasCycles());
        }
    }

    [TestFixture]
    public class TestReferenceCheckerAdvanced : ParserTest
    {
        [Test]
        public void NamedFormulaParametersAreNotReferencesTest()
        {
            var p = Parse("tax(rate, amount) := amount * rate\nA1 := tax(0.2, 100)");
            Assert.IsTrue(p.Parse());

            var checker = new ReferenceChecker();
            checker.Analyze((ProgramNode)p.root);

            Assert.IsFalse(checker.HasUndefinedReferences());
        }

        [Test]
        public void LambdaParametersAreNotReferencesTest()
        {
            var p = Parse("f := x -> x + A1\nA1 := 1");
            Assert.IsTrue(p.Parse());

            var checker = new ReferenceChecker();
            checker.Analyze((ProgramNode)p.root);

            Assert.IsFalse(checker.HasUndefinedReferences());
        }

        [Test]
        public void UndefinedFunctionCallTest()
        {
            var p = Parse("A1 := calc(10)");
            Assert.IsTrue(p.Parse());

            var checker = new ReferenceChecker();
            checker.Analyze((ProgramNode)p.root);

            Assert.IsTrue(checker.HasUndefinedReferences());
            CollectionAssert.Contains(checker.UndefinedReferences, "calc");
        }

        [Test]
        public void BuiltinFunctionIsNotReferenceTest()
        {
            var p = Parse("values := 1\nA1 := MAP(values)");
            Assert.IsTrue(p.Parse());

            var checker = new ReferenceChecker();
            checker.Analyze((ProgramNode)p.root);

            Assert.IsFalse(checker.HasUndefinedReferences());
        }
    }

    [TestFixture]
    public class TestConstantFolderAdvanced : ParserTest
    {
        [Test]
        public void FoldsArithmeticTest()
        {
            var p = Parse("A1 := (2 + 3) * 4");
            Assert.IsTrue(p.Parse());

            p.root.Visit(new ConstantFolder());

            var number = ((ProgramNode)p.root).Defs[0].Rhs as NumberNode;
            Assert.IsNotNull(number);
            Assert.AreEqual(20.0, number.Value, 1e-9);
        }

        [Test]
        public void FoldsUnaryMinusTest()
        {
            var p = Parse("A1 := -5 + 2");
            Assert.IsTrue(p.Parse());

            p.root.Visit(new ConstantFolder());

            var number = ((ProgramNode)p.root).Defs[0].Rhs as NumberNode;
            Assert.IsNotNull(number);
            Assert.AreEqual(-3.0, number.Value, 1e-9);
        }

        [Test]
        public void FoldsNestedAndPowerTest()
        {
            var p = Parse("A1 := 2 ^ (1 + 2)");
            Assert.IsTrue(p.Parse());

            p.root.Visit(new ConstantFolder());

            var number = ((ProgramNode)p.root).Defs[0].Rhs as NumberNode;
            Assert.IsNotNull(number);
            Assert.AreEqual(8.0, number.Value, 1e-9);
        }

        [Test]
        public void KeepsNonConstantAndDivisionByZeroTest()
        {
            var p = Parse("A1 := A2 + 1\nB1 := 1 / 0");
            Assert.IsTrue(p.Parse());

            p.root.Visit(new ConstantFolder());

            var defs = ((ProgramNode)p.root).Defs;
            Assert.IsInstanceOf<BinOpNode>(defs[0].Rhs);
            Assert.IsInstanceOf<BinOpNode>(defs[1].Rhs);
        }

        [Test]
        public void FoldReturnsConstantFlagTest()
        {
            ExprNode node = new BinOpNode(new NumberNode(2), new NumberNode(3), "+");

            Assert.IsTrue(new ConstantFolder().Fold(ref node));

            var number = node as NumberNode;
            Assert.IsNotNull(number);
            Assert.AreEqual(5.0, number.Value, 1e-9);
        }
    }

    [TestFixture]
    public class TestPrettyPrintAdvanced : ParserTest
    {
        [Test]
        public void PrintsBinaryExpressionWithParenthesesTest()
        {
            var p = Parse("A1 := (2 + 3) * 4");
            Assert.IsTrue(p.Parse());

            var pp = new PrettyPrintVisitor();
            p.root.Visit(pp);

            StringAssert.Contains("A1 := ((2 + 3) * 4)", pp.Text);
        }

        [Test]
        public void PrintsNamedFormulaTest()
        {
            var p = Parse("tax(rate, amount) := amount * rate");
            Assert.IsTrue(p.Parse());

            var pp = new PrettyPrintVisitor();
            p.root.Visit(pp);

            StringAssert.Contains("tax := (rate, amount) -> (amount * rate)", pp.Text);
        }

        [Test]
        public void PrintsStringWithoutEscapingTest()
        {
            var p = Parse("A1 := \"hello\"");
            Assert.IsTrue(p.Parse());

            var pp = new PrettyPrintVisitor();
            p.root.Visit(pp);

            StringAssert.Contains("A1 := \"hello\"", pp.Text);
        }
    }
}

