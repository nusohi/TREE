using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TREE.DB
{
    public class Tree
    {
        public int ID;
        public string Name;
        public string XML;

        public Tree() { }

        public Tree(int id, string xml,string name=null) {
            ID = id;
            Name = name;
            XML = xml;
        }
    }
}
