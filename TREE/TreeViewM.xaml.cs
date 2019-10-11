using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TREE.DB;
using static TREE.InitTreePanel;

namespace TREE
{
    /// <summary>
    /// TreeViewM.xaml 的交互逻辑
    /// TreeViewManager
    /// </summary>
    public partial class TreeViewM : UserControl {
        private Tree _StoredTree;
        public Tree StoreTree
        {
            get { return _StoredTree; }
            set { _StoredTree = value;TreeNameText.Text = _StoredTree.Name; }
        }
        private ContextMenu Menu;
        public TextNodeDB textNodeDB;
        public TreeDB treeDB;

        public event CallEvent UpdateTreeToDB;

        private bool selected = false;
        public TreeViewMItem SelectedItem
        {
            get {
                if (tree == null) return null;
                //if (!selected) return null;
                return tree.SelectedItem as TreeViewMItem;
            }
        }

        public TreeViewM() {
            InitializeComponent();
            Menu = GetMenu();
            Reset();
        }

        public void Reset() {
            tree.Items.Clear();
            tree.UpdateLayout();
            _StoredTree = null;

            TreeNameText.Text = "请先创建/打开树";
            NodeNameText.Text = "右键创建节点";
        }

        public void SetNewTree(Tree newTree) {
            tree.Items.Clear();
            StoreTree = newTree;
            TreeNameText.Text = "<Tree Name>";
        }

        public void FocusNode() {
            if (tree.Items.Count > 0) {
                TreeViewItem root = tree.Items[0] as TreeViewItem;
                root.Focus();
            }
            else {
                tree.Focus();
            }
        }

        #region "添加/删除节点"
        private bool AddNode(TreeViewMItem root, string NodeName = "") {
            if (StoreTree == null) {
                MessageBox.Show("请先创建树");
                return false;
            }

            TreeViewMItem node = null;
            if (root != null) {
                node = new TreeViewMItem(NodeName, root.Path);
                root.Items.Add(node);
                root.IsExpanded = true;         // 展开 root 节点
            }
            else {
                node = new TreeViewMItem("Root", "");
                tree.Items.Add(node);
            }
            textNodeDB.AddNewNode(node);    /// 新节点写入数据库并获取 ID
            tree.UpdateLayout();
            node.Focus();
            return true;
        }

        // 带有警告提示框
        private bool RemoveNode(TreeViewMItem node) {
            if (StoreTree == null) {
                MessageBox.Show("请先创建树");
                return false;
            }

            if (node == null) return false;
            string s = "确定删除节点 “{0}” 及其所有子节点？";
            s = String.Format(s, node.Header);
            if (MessageBox.Show(s, "警告", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes) {
                return false;
            }
            _removeNode(node);
            return true;
        }

        private void _removeNode(TreeViewMItem node) {
            if (node == null) return;
            TreeViewMItem parent = node.Parent as TreeViewMItem;
            textNodeDB.DeleteNode(node.ID);
            for(int i = node.Items.Count - 1; i >= 0; i--) {
                _removeNode(node.Items[i] as TreeViewMItem);
            }
            if (parent == null) {
                tree.Items.Remove(node);
                tree.UpdateLayout();
            }
            else {
                parent.Items.Remove(node);
                parent.UpdateLayout();
            }
        }
        #endregion

        #region "节点升降级"
        // 节点升级
        private bool UpgradeNode(TreeViewMItem node) {
            if (node == null) return false;

            TreeViewMItem parent = node.Parent as TreeViewMItem;
            if (parent == null) return false;

            TreeViewMItem grandPa = parent.Parent as TreeViewMItem;
            parent.Items.Remove(node);

            if (grandPa != null)
                grandPa.Items.Add(node);
            else
                tree.Items.Add(node);
            node.Focus();
            return true;
        }

        // 节点降级
        private bool DegradeNode(TreeViewItem node) {
            if (node == null) return false;

            TreeViewItem parent = node.Parent as TreeViewItem;
            TreeViewItem firstSon = null;

            // 没有孩子，尝试降级到前一个兄弟的子节点
            if (node.Items.Count == 0) {
                // node 有 parent，即不是根节点
                TreeViewItem brother = null;
                if (parent != null) {
                    int index = parent.Items.IndexOf(node);
                    if (index - 1 >= 0) {   // 有前一个兄弟
                        parent.Items.RemoveAt(index);
                        brother = parent.Items[index - 1] as TreeViewItem;
                        brother.Items.Add(node);
                    }
                    else if (index + 1 < parent.Items.Count) {   // 有后一个兄弟
                        parent.Items.RemoveAt(index);
                        brother = parent.Items[index] as TreeViewItem;
                        brother.Items.Add(node);
                    }
                    else {  // 没有兄弟
                        return false;
                    }
                }
                // node 是根节点
                else {
                    int index = tree.Items.IndexOf(node);
                    if (index - 1 >= 0) {   // 有前一个兄弟
                        tree.Items.RemoveAt(index);
                        brother = tree.Items[index - 1] as TreeViewItem;
                        brother.Items.Add(node);
                    }
                    else if (index + 1 < tree.Items.Count) {   // 有后一个兄弟
                        tree.Items.RemoveAt(index);
                        brother = tree.Items[index] as TreeViewItem;
                        brother.Items.Add(node);
                    }
                    else {  // 没有兄弟
                        return false;
                    }
                }

                // 执行到这时 brother != null
                ExpandNode(brother as ItemsControl, true);
                node.Focus();
                return true;
            }
            // 有孩子，找到第一个孩子
            else {
                firstSon = node.Items[0] as TreeViewItem;
            }

            // 成为第一个孩子的子节点
            if (parent != null) {
                node.Items.RemoveAt(0);
                int index = parent.Items.IndexOf(node);
                parent.Items.Insert(index, firstSon);

                parent.Items.Remove(node);
                firstSon.Items.Add(node);
            }
            else {
                node.Items.RemoveAt(0);
                int index = tree.Items.IndexOf(node);
                tree.Items.Insert(index, firstSon);

                tree.Items.Remove(node);
                firstSon.Items.Add(node);
            }

            // 其他孩子也作为第一个孩子的子节点
            int count = node.Items.Count;
            for (int i = 0; i < count; i++) {
                TreeViewItem child = node.Items[0] as TreeViewItem;
                node.Items.RemoveAt(0);
                firstSon.Items.Add(child);
            }
            ExpandNode(firstSon as ItemsControl, true);
            node.Focus();
            return true;
        }

        

        #endregion

        #region "节点上移下移"

        private bool MoveUp(TreeViewItem node) {
            if (node == null) return false;
            TreeViewItem parent = node.Parent as TreeViewItem;
            if (parent != null) {
                int index = parent.Items.IndexOf(node);
                if (index == 0) return false;
                parent.Items.RemoveAt(index);
                parent.Items.Insert(index - 1, node);
            }
            else {
                int index = tree.Items.IndexOf(node);
                if (index == 0) return false;
                tree.Items.RemoveAt(index);
                tree.Items.Insert(index - 1, node);
            }
            node.Focus();
            return true;
        }

        private bool MoveDown(TreeViewItem node) {
            if (node == null) return false;
            TreeViewItem parent = node.Parent as TreeViewItem;
            if (parent != null) {
                int index = parent.Items.IndexOf(node);
                if (index == parent.Items.Count - 1) return false;
                parent.Items.RemoveAt(index);
                parent.Items.Insert(index + 1, node);
            }
            else {
                int index = tree.Items.IndexOf(node);
                if (index == tree.Items.Count - 1) return false;
                tree.Items.RemoveAt(index);
                tree.Items.Insert(index + 1, node);
            }
            node.Focus();
            return true;
        }

        #endregion


        #region "展开节点"
        public void ExpandAll(bool expand = true) {
            ExpandNode(tree, expand);
        }

        private void ExpandNode(ItemsControl control, bool expand) {
            if (control == null) return;
            TreeViewItem selfItem = control as TreeViewItem;
            if (selfItem != null)
                selfItem.IsExpanded = expand;

            foreach (object item in control.Items) {
                TreeViewItem treeItem = control.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                if (treeItem != null && treeItem.HasItems) {
                    treeItem.IsExpanded = expand;
                    if (treeItem.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated) {
                        treeItem.UpdateLayout();
                    }
                    ExpandNode(treeItem as ItemsControl, expand);
                }
            }
        }

        #endregion

        #region 辅助函数
        // 获取菜单
        private ContextMenu GetMenu() {
            ContextMenu menu = new ContextMenu();

            MenuItem deleteMenu = new MenuItem();
            deleteMenu.Header = "添加";
            deleteMenu.Click += menu_AddNode;
            menu.Items.Add(deleteMenu);

            MenuItem editMenu = new MenuItem();
            editMenu.Header = "删除";
            editMenu.Click += menu_RemoveNode;
            menu.Items.Add(editMenu);

            return menu;
        }

        // 查找鼠标位于哪个 T 控件上
        private static DependencyObject VisualUpwardSearch<T>(DependencyObject source) {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }
        #endregion


        #region 控件的各种事件处理
        
        #region "鼠标各种点击"
        // 右键点击
        private void Tree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            SetSelectStatus(e);
            this.ContextMenu = Menu;
        }
        // 双击
        private void Tree_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            SetSelectStatus(e);
        }
        // 点击 - 选择状态判断
        private void SetSelectStatus(MouseButtonEventArgs e) {
            var treeViewMItem = VisualUpwardSearch<TreeViewMItem>(e.OriginalSource as DependencyObject) as TreeViewMItem;
            if (treeViewMItem != null) {
                treeViewMItem.Focus();
                e.Handled = true;
                selected = true;
            }
            else {
                selected = false;
            }
        }
        #endregion


        #region "菜单/按钮 添加/删除节点的事件处理"
        private void menu_AddNode(object sender, RoutedEventArgs e) {
            if (AddNode(tree.SelectedItem as TreeViewMItem, "新节点"))
                UpdateTreeToDB();   // 更新XML
        }

        private void menu_RemoveNode(object sender, RoutedEventArgs e) {
            if (RemoveNode(SelectedItem))
                UpdateTreeToDB();   // 更新XML
        }

        // 添加/删除 节点（添加根节点）
        private void AddBtn_Click(object sender, RoutedEventArgs e) {
            if (AddNode(tree.SelectedItem as TreeViewMItem, "新节点"))
                UpdateTreeToDB();   // 更新XML
        }
        private void DeleteBtn_Click(object sender, RoutedEventArgs e) {
            if (RemoveNode(SelectedItem))
                UpdateTreeToDB();   // 更新XML
        }
        private void AddRootBtn_Click(object sender, RoutedEventArgs e) {
            if (AddNode(null, "根节点"))
                UpdateTreeToDB();   // 更新XML
        }

        // 节点 升降级
        private void UpdateBtn_Click(object sender, RoutedEventArgs e) {
            if (UpgradeNode(SelectedItem))
                UpdateTreeToDB();   // 更新XML
        }
        private void DegradeBtn_Click(object sender, RoutedEventArgs e) {
            if (DegradeNode(SelectedItem))
                UpdateTreeToDB();   // 更新XML
        }

        // 节点 上移/下移
        private void MoveUpBtn_Click(object sender, RoutedEventArgs e) {
            if (MoveUp(SelectedItem))
                UpdateTreeToDB();   // 更新XML
        }
        private void MoveDownBtn_Click(object sender, RoutedEventArgs e) {
            if (MoveDown(SelectedItem))
                UpdateTreeToDB();   // 更新XML
        }
        #endregion


        #region "其他控件事件处理（树名/节点名更改，节点名显示）"

        private void NodeNameText_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (SelectedItem == null)
                return;
            SelectedItem.UpdateHeader(NodeNameText.Text);
            textNodeDB.UpdateNode(new TextNode(SelectedItem.ID, null, SelectedItem.Path));
        }

        private void TreeNameText_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (StoreTree == null) return;
            StoreTree.Name = TreeNameText.Text;
            treeDB.UpdateTree(StoreTree);
        }

        private void InnerTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            var item = tree.SelectedItem as TreeViewMItem;
            if (item != null)
                NodeNameText.Text = item.Header.ToString();
        }


        #endregion

        #endregion
        
    }
}
