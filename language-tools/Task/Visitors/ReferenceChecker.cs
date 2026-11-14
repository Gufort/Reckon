using System;
using System.Collections.Generic;
using System.Linq;
using ProgramTree;

namespace SimpleLang.Visitors
{
    public class ReferenceChecker : AutoVisitor
    {
        public void Analyze(ProgramNode program)
        {
            throw new NotImplementedException();
        }

        public bool HasUndefinedReferences()
        {
            throw new NotImplementedException();
        }

        public List<string> UndefinedReferences
        {
            get { throw new NotImplementedException(); }
        }
    }
}