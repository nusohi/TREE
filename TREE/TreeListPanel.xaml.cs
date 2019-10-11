using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TREE.DB;

namespace TREE
{
    /// <summary>
    /// TreeListPanel.xaml 的交互逻辑
    /// </summary>
    public partial class TreeListPanel : Window
    {
        public delegate void LoadTree(int treeID);
        public event LoadTree LoadTreeEvent;

        public TreeListPanel(Tree[] trees) {
            InitializeComponent();
            foreach (Tree tree in trees) {
                TreeListViewItem item = new TreeListViewItem(tree.ID, tree.Name);
                TreeListView.Items.Add(item);
            }
        }

        private void TreeListView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            TreeListViewItem item = TreeListView.SelectedItem as TreeListViewItem;
            LoadTreeEvent(item.ID);
            this.Close();
        }
    }
}
