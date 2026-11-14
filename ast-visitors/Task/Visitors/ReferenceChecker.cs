using System;
using System.Collections.Generic;
using ProgramTree;

namespace SimpleLang.Visitors
{
    public class ReferenceChecker : AutoVisitor
    {
        public List<string> UndefinedReferences { get; private set; }

        public void Analyze(ProgramNode program)
        {
            throw new NotImplementedException();
        }

        public bool HasUndefinedReferences()
        {
            throw new NotImplementedException();
        }
    }
}