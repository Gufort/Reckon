using System;
using System.Collections.Generic;
using ProgramTree;

namespace SimpleLang.Visitors
{
    public class CycleDetector : AutoVisitor
    {
        public void Analyze(ProgramNode program)
        {
            throw new NotImplementedException();
        }

        public bool HasCycles()
        {
            throw new NotImplementedException();
        }

        public List<List<string>> Cycles
        {
            get { throw new NotImplementedException(); }
        }
    }
}