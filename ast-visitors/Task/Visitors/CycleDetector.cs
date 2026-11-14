using System;
using System.Collections.Generic;
using ProgramTree;

namespace SimpleLang.Visitors
{
    public class CycleDetector : AutoVisitor
    {
        public List<List<string>> Cycles { get; private set; }

        public void Analyze(ProgramNode program)
        {
            throw new NotImplementedException();
        }

        public bool HasCycles()
        {
            throw new NotImplementedException();
        }
    }
}