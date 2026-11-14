using System;
using System.Collections.Generic;
using ProgramTree;

namespace SimpleLang.Visitors
{
    public class DependencyExtractor : AutoVisitor
    {
        public Dictionary<string, HashSet<string>> Dependencies { get; } = new Dictionary<string, HashSet<string>>();

        public HashSet<string> GetDependencies(string cell)
        {
            throw new NotImplementedException();
        }
    }
}