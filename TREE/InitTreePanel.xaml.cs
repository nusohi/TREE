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

namespace TREE
{
    /// <summary>
    /// InitTreePanel.xaml 的交互逻辑
    /// </summary>
    public partial class InitTreePanel : Window
    {
        public delegate void CallEvent();
        public event CallEvent CreateCallEvent;
        public event CallEvent OpenCallEvent;

        public InitTreePanel()
        {
            InitializeComponent();
        }

        private void CreateBtn_Click(object sender, RoutedEventArgs e) {
            CreateCallEvent();
            this.Close();
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e) {
            OpenCallEvent();
            this.Close();
        }
    }
}
