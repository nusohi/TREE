using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TREE.DB
{
    public class TextNode
    {
        public int ID;
        public string Content;
        public string Path;     // 暂无用

        public TextNode(int id, string content, string path) {
            this.ID = id;
            this.Content = content;
            this.Path = path;
        }
    }
}
