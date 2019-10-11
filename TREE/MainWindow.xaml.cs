using KeyboardSimulation;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml;
using TREE.DB;

namespace TREE
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private TextNodeDB textNodeDB;
        private TreeDB treeDB;
        private XMLer xmler = new XMLer();
        private static string DatabaseName = "Tree";
        private string ConnectString;


        public MainWindow() {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // 数据库连接建立
            string s = "Server =.; Database = {0}; integrated security = SSPI";
            ConnectString = String.Format(s, DatabaseName);
            textNodeDB = new TextNodeDB(ConnectString);
            treeDB = new TreeDB(ConnectString);
            
            // 数据库初始化（创建数据库/表）
            InitDB initDB = new InitDB("Server=.;Database=master;integrated security=SSPI");
            initDB.DatabaseName = DatabaseName;
            try {
                initDB.Init();
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
            }
            
            treeView.UpdateTreeToDB += SaveTree;
            this.Loaded += OpenOrCreateTreeWindow;
        }

        private void OpenOrCreateTreeWindow(object sender = null, RoutedEventArgs e = null) {
            InitTreePanel window = new InitTreePanel();
            window.CreateCallEvent += CreateNewTree;
            window.OpenCallEvent += LoadTree;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Activate();
            window.Show();
        }


        #region 控件事件处理

        private void TreeView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            UpdateContent();
        }

        private void UpdateContent() {
            if (treeView.SelectedItem != null) {
                TextNode item = textNodeDB.QueryNode(treeView.SelectedItem.ID);
                if (item == null) {
                    MessageBox.Show("读取节点内容错误，无此节点");
                    return;
                }
                LoadText(ContentText, item.Content);
            }
        }

        private void TreeView_Loaded(object sender, RoutedEventArgs e) {
            // 添加对 DB 的引用
            treeView.textNodeDB = textNodeDB;
            treeView.treeDB = treeDB;
        }

        private void ContentText_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (treeView.SelectedItem == null) return;
            textNodeDB.UpdateNode(new TextNode(
                treeView.SelectedItem.ID,
                GetText(ContentText),
                treeView.SelectedItem.Path
                ));
        }

        // Help 窗
        private void HelpTip(object sender, RoutedEventArgs e) {
            string help = "双击节点 - 显示/编辑节点内容\n节点升级 - 节点成为其父节点的兄弟节点\n节点降级 - 节点成为第一个子节点的孩子节点，无子节点则成为前一兄弟的子节点\n";
            MessageBox.Show(help, "Help - 操作提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion


        #region 树的操作（创建/加载/保存/删除）

        // 检查当前指向的树是否为NULL，弹出提示框。
        private bool CheckTreeReady() {
            if (treeView.StoreTree == null) {
                MessageBox.Show("请先创建/打开树!");
                return false;
            }
            return true;
        }

        #region "创建树"
        private void CreateTree(object sender, RoutedEventArgs e) {
            // 先保存原树
            if (treeView.StoreTree != null)
                SaveTree();
            CreateNewTree();
        }

        private void CreateNewTree() {
            Tree tree = new Tree();
            tree.Name = "<Tree Name>";
            treeDB.AddNewTree(tree);

            treeView.tree.Items.Clear();
            treeView.StoreTree = tree;
            treeView.TreeNameText.Text = "<Tree Name>";
            treeView.SetNewTree(tree);
            SaveTree();
        }
        #endregion

        #region "加载树"
        private void LoadTree(object sender, RoutedEventArgs e) {
            LoadTree();
        }

        private void LoadTree() {
            Tree[] trees = treeDB.QueryAll();
            TreeListPanel treeListWindow = new TreeListPanel(trees);
            treeListWindow.Activate();
            treeListWindow.Show();
            treeListWindow.LoadTreeEvent += LoadTree;
        }

        private void LoadTree(int ID) {
            treeView.StoreTree = treeDB.QueryTree(ID);
            xmler.LoadTreeViewState(treeView.tree, treeView.StoreTree.XML);
            // 树全部展开
            treeView.ExpandAll();
            treeView.FocusNode();
            // Content 刷新
            ContentText.Document.Blocks.Clear();
        }
        #endregion

        #region "保存树"
        private void SaveTree(object sender, RoutedEventArgs e) {
            if (!CheckTreeReady()) return;
            SaveTree();
            MessageBox.Show("树保存成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void SaveTree() {
            string XML = xmler.SaveTreeViewState(treeView.tree);
            treeView.StoreTree.XML = XML;
            treeDB.UpdateTree(treeView.StoreTree);
        }
        #endregion

        #region "删除树"
        private void DeleteTree(object sender, RoutedEventArgs e) {
            DeleteTree();
        }
        private void DeleteTree() {
            if (!CheckTreeReady()) return;
            string s = "确定删除树 “{0}” ？";
            s = String.Format(s, treeView.StoreTree.Name);
            if (MessageBox.Show(s, "警告", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes) {
                return;
            }

            treeDB.DeleteTree(treeView.StoreTree.ID);
            treeView.Reset();
            OpenOrCreateTreeWindow();
        }
        #endregion

        #endregion


        #region 富文本事件处理

        private void BoldText_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            ShortcutSim(Key.B);
        }
        private void ItalicText_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            ShortcutSim(Key.I);
        }
        private void UnderlineText_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            ShortcutSim(Key.U);
        }

        private void ShortcutSim(Key key) {
            KeyboardM.Press(Key.LeftCtrl);
            KeyboardM.Press(key);
            KeyboardM.Release(key);
            KeyboardM.Release(Key.LeftCtrl);
        }
        
        #endregion
        
        #region "RichTextBox 的读取/写入/保存"
        private string GetText(RichTextBox richTextBox) {
            FlowDocument document = richTextBox.Document;
            string text = System.Windows.Markup.XamlWriter.Save(document);
            return text;
        }
        private void LoadText(RichTextBox richTextBox, string content) {
            richTextBox.Document.Blocks.Clear();
            try {
                richTextBox.Document = System.Windows.Markup.XamlReader.Parse(content) as FlowDocument;
            }
            catch (XmlException) {
                richTextBox.Document.Blocks.Add(new Paragraph(new Run(content)));
            }
        }
        private void SaveText(RichTextBox richTextBox) {

        }

        #endregion
        
        
    }
}
