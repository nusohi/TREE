using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace TREE
{
    public class TreeListViewItem : ListViewItem
    {
        public int ID;

        public TreeListViewItem() { }

        public TreeListViewItem(int id, string content) {
            ID = id;
            Content = content;
        }
    }
}
