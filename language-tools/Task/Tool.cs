using System;
using System.Collections.Generic;
using System.Text;
using ProgramTree;
using SimpleScanner;
using SimpleParser;
using SimpleLang.Visitors;

namespace Task
{
    public class ReckonCycleDetector
    {
        public string DetectCycles(Node root)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Tool
    {
        public abstract string Process(string source);
    }

    public class ReckonTool : Tool
    {
        public override string Process(string source)
        {
            throw new NotImplementedException();
        }
    }
}