using System;
using NUnit.Framework;
using SimpleParser;
using SimpleScanner;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[assembly: Property("Task", "AST_PARSER_reckon")]

namespace Tests
{
    [TestFixture, Property("Score", 8)]
    [Property("Job", "TestASTParser")]
    public class TestASTParser
    {
        public static JObject ParseToTree(string text)
        {
            Scanner scanner = new Scanner();
            scanner.SetSource(text, 0);

            Parser parser = new Parser(scanner);

            var b = parser.Parse();
            if (!b)
                Assert.Fail("Parsing failed");

            JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
            jsonSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonSettings.TypeNameHandling = TypeNameHandling.All;
            string output = JsonConvert.SerializeObject(parser.root, jsonSettings);
            return JObject.Parse(output);
        }

        [Test, Property("ScorePercentage", 20)]
        public void TestSimpleDefinitionAndNumber()
        {
            var tree = ParseToTree("A1 := 42\nA2 := A1 + 1");
            var typeStr = (string)tree["$type"];
            Assert.IsTrue(typeStr.IndexOf("ProgramNode") >= 0);
            var defs = tree["Defs"]["$values"] as JArray;
            Assert.AreEqual(2, defs.Count);
            var def1 = defs[0];
            Assert.IsTrue(((string)def1["Rhs"]["$type"]).IndexOf("NumberNode") >= 0);
            Assert.AreEqual("42", ((string)def1["Rhs"]["Value"]).Trim());
        }

        [Test, Property("ScorePercentage", 20)]
        public void TestStringBoolErrorLiterals()
        {
            var tree = ParseToTree("A1 := \"hello\"\nA2 := true\nA3 := #DIV/0!");
            var defs = tree["Defs"]["$values"] as JArray;
            Assert.AreEqual(3, defs.Count);

            Assert.IsTrue(((string)defs[0]["Rhs"]["$type"]).IndexOf("StringNode") >= 0);
            Assert.IsTrue(((string)defs[1]["Rhs"]["$type"]).IndexOf("BoolNode") >= 0);
            Assert.IsTrue(((string)defs[2]["Rhs"]["$type"]).IndexOf("ErrorNode") >= 0);
            Assert.AreEqual("#DIV/0!", ((string)defs[2]["Rhs"]["Code"]).Trim());
        }

        [Test, Property("ScorePercentage", 20)]
        public void TestCellRefsAndNames()
        {
            var tree = ParseToTree("total := 100\nA1 := total + $B$2\nA2 := @A\nA3 := @1");
            var defs = tree["Defs"]["$values"] as JArray;
            Assert.AreEqual(4, defs.Count);

            Assert.IsTrue(((string)defs[0]["Lhs"]["$type"]).IndexOf("NameNode") >= 0);
            Assert.AreEqual("total", ((string)defs[0]["Lhs"]["Name"]).Trim());

            Assert.IsTrue(((string)defs[1]["Rhs"]["$type"]).IndexOf("BinOpNode") >= 0);
            Assert.AreEqual("+", ((string)defs[1]["Rhs"]["Op"]).Trim());

            Assert.IsTrue(((string)defs[2]["Rhs"]["$type"]).IndexOf("ColRefNode") >= 0);
            Assert.IsTrue(((string)defs[3]["Rhs"]["$type"]).IndexOf("RowRefNode") >= 0);
        }

        [Test, Property("ScorePercentage", 15)]
        public void TestRangeAndSheetRef()
        {
            var tree = ParseToTree("result := SUM(A1:A10)\nval := Data!C5");
            var defs = tree["Defs"]["$values"] as JArray;
            Assert.AreEqual(2, defs.Count);

            var callNode = defs[0]["Rhs"];
            Assert.IsTrue(((string)callNode["$type"]).IndexOf("CallNode") >= 0);
            Assert.AreEqual("SUM", ((string)callNode["Name"]).Trim());

            var sheetRef = defs[1]["Rhs"];
            Assert.IsTrue(((string)sheetRef["$type"]).IndexOf("SheetRefNode") >= 0);
            Assert.AreEqual("Data", ((string)sheetRef["Sheet"]).Trim());
        }

        [Test, Property("ScorePercentage", 15)]
        public void TestHigherOrderAndLambda()
        {
            var tree = ParseToTree(@"A1 := MAP(A1:A5, x -> x * 2)
A2 := FILTER(A1:A5, x -> x > 0)
A3 := REDUCE(A1:A5, 0, (acc, x) -> acc + x)");
            var defs = tree["Defs"]["$values"] as JArray;
            Assert.AreEqual(3, defs.Count);

            Assert.AreEqual("MAP", ((string)defs[0]["Rhs"]["Name"]).Trim());
            Assert.AreEqual("FILTER", ((string)defs[1]["Rhs"]["Name"]).Trim());
            Assert.AreEqual("REDUCE", ((string)defs[2]["Rhs"]["Name"]).Trim());
        }

        [Test, Property("ScorePercentage", 10)]
        public void TestNamedFormula()
        {
            var tree = ParseToTree("tax(rate, amount) := amount * rate\nprice := 100\ntotal := price + tax(0.2, price)");
            var defs = tree["Defs"]["$values"] as JArray;
            Assert.AreEqual(3, defs.Count);

            var lhs = defs[0]["Lhs"];
            Assert.IsTrue(((string)lhs["$type"]).IndexOf("NameNode") >= 0);
            Assert.AreEqual("tax", ((string)lhs["Name"]).Trim());

            var rhs = defs[0]["Rhs"];
            Assert.IsTrue(((string)rhs["$type"]).IndexOf("LambdaNode") >= 0);
        }
    }
}