using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;


namespace TREE
{
    public class TreeViewMItem : TreeViewItem
    {
        public int ID;
        public string Path; // 暂无用

        public TreeViewMItem() { }

        public TreeViewMItem(string header, string rootPath) {
            this.Header = header;
            this.IsExpanded = true;
            this.Path = rootPath + "/" + header;
        }

        public void UpdateHeader(string newHeader) {
            //Path = Path.TrimEnd(Header.ToString().ToCharArray());
            //Path = Path + newHeader;
            Header = newHeader;
        }

    }
}
