using System.Xml;
using System.Text;
using System.Windows.Controls;
using TREE;
using System.IO;

namespace TREE.DB
{
    public class XMLer
    {
        public XMLer() { }

        #region const
        private const string XmlNodeTag = "TreeNode";
        private const string XmlNodeIndex = "ID";
        private const string XmlNodeTextAtt = "Header";
        private const string XmlNodeTagAtt = "tag";
        private const string XmlNodeExpandState = "expland";
        private const string XmlNodeIsSelect = "lastselect";
        #endregion

        #region 保存TREEVIEW状态
        public string SaveTreeViewState(TreeView treeView) {
            using (var sw = new StringWriter()) {
                using (var writer = XmlWriter.Create(sw)) {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("TreeViewM");
                    SaveXmlNodes(treeView.Items, writer);
                    writer.WriteEndElement();
                    writer.Close();
                }
                return sw.ToString();
            }
        }
        #endregion

        #region 读取TreeView状态
        public void LoadTreeViewState(TreeView treeView, string XML) {
            using (StringReader stringReader = new StringReader(XML)) {
                using(XmlReader reader = XmlReader.Create(stringReader)) {
                    treeView.Items.Clear();
                    TreeViewMItem parentNode = null;

                    while (reader.Read()) {
                        if (reader.NodeType == XmlNodeType.Element) {
                            if (reader.Name == XmlNodeTag) {
                                TreeViewMItem newNode = new TreeViewMItem();
                                bool isEmptyElement = reader.IsEmptyElement;

                                // loading node attributes
                                for (int i = 0; i < reader.AttributeCount; i++) {
                                    reader.MoveToAttribute(i);
                                    SetAttributeValue(newNode, reader.Name, reader.Value);
                                    //SetTreeViewState(treeView);
                                }

                                // add new node to Parent Node or TreeView
                                if (parentNode != null)
                                    parentNode.Items.Add(newNode);
                                else
                                    treeView.Items.Add(newNode);

                                // making current node 'ParentNode' if its not empty
                                if (!isEmptyElement) {
                                    parentNode = newNode;
                                }
                            }
                        }
                        // moving up to in TreeView if end tag is encountered
                        else if (reader.NodeType == XmlNodeType.EndElement) {
                            if (reader.Name == XmlNodeTag) {
                                parentNode = parentNode.Parent as TreeViewMItem;
                            }
                        }
                        else if (reader.NodeType == XmlNodeType.XmlDeclaration) { //Ignore Xml Declaration                    
                        }
                        else if (reader.NodeType == XmlNodeType.None) {
                            return;
                        }
                        else if (reader.NodeType == XmlNodeType.Text) {
                            parentNode.Items.Add(reader.Value);
                        }
                    }
                }
            }
        }
        #endregion
        

        #region 保存XML节点
        private void SaveXmlNodes(ItemCollection nodesCollection, XmlWriter textWriter) {
            for (int i = 0; i < nodesCollection.Count; i++) {
                TreeViewMItem node = nodesCollection[i] as TreeViewMItem;
                textWriter.WriteStartElement(XmlNodeTag);       // <TreeNode>

                textWriter.WriteAttributeString(XmlNodeIndex, node.ID.ToString());          // ID
                textWriter.WriteAttributeString(XmlNodeTextAtt, node.Header.ToString());    // Header

                if (node.IsExpanded == true)
                    textWriter.WriteAttributeString(XmlNodeExpandState, node.IsExpanded.ToString());////展开状态

                if (node.IsSelected)
                    textWriter.WriteAttributeString(XmlNodeIsSelect, node.IsSelected.ToString());//是否选中

                if (node.Tag != null)
                    textWriter.WriteAttributeString(XmlNodeTagAtt, node.Tag.ToString());


                // add other node properties to serialize here

                if (node.Items.Count > 0) {
                    SaveXmlNodes(node.Items, textWriter);
                }
                textWriter.WriteEndElement();
            }
        }
        #endregion

        #region 设置XML属性
        private void SetAttributeValue(TreeViewMItem node, string propertyName, string value) {
            if (propertyName == XmlNodeTextAtt) //Header
            {
                node.Header = value;
            }
            else if (propertyName == XmlNodeExpandState) {
                //ExpandNode = node;
            }
            else if (propertyName == XmlNodeIsSelect) {
                //LastSelectNode = node;
            }
            else if (propertyName == XmlNodeTagAtt) {
                node.Tag = value;
            }
            else if (propertyName == XmlNodeIndex) {
                // 用来标识 这样看XML文件时结构清晰
                node.ID = int.Parse(value);
            }
        }
        #endregion
        
    }
}