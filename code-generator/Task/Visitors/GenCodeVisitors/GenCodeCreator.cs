using System;

namespace SimpleLang.Visitors
{
    public class GenCodeCreator
    {
        public GenCodeVisitor GetVisitor()
        {
            throw new NotImplementedException();
        }

        public void Run(ProgramTree.ProgramNode program)
        {
            throw new NotImplementedException();
        }

        public object GetCellValue(string address)
        {
            throw new NotImplementedException();
        }
    }
}