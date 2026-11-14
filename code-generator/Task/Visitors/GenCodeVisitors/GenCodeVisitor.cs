using System;
using System.Collections.Generic;
using ProgramTree;

namespace SimpleLang.Visitors
{
    public class GenCodeVisitor : Visitor
    {
        public override void VisitProgramNode(ProgramNode n)
        {
            throw new NotImplementedException();
        }

        public object GetResult(string name)
        {
            throw new NotImplementedException();
        }

        public void EndProgram()
        {
            throw new NotImplementedException();
        }

        public void RunProgram()
        {
            throw new NotImplementedException();
        }
    }
}