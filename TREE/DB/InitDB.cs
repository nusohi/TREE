using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TREE.DB
{
    public class InitDB
    {
        private string ConnectString;
        public string DatabaseName = "Tree";


        public InitDB(string connectString) {
            ConnectString = connectString;
        }

        // 初始化（创建数据库、表）
        public void Init() {
            string directory = System.IO.Directory.GetCurrentDirectory() + "\\";
            string SQL = "if db_id('{1}') is null CREATE DATABASE {1} ON PRIMARY (NAME = {1}_Data, FILENAME = '{0}{1}Data.mdf', SIZE = 2MB, MAXSIZE = 10MB, FILEGROWTH = 10%) LOG ON (NAME = {1}_Log, FILENAME = '{0}{1}Log.ldf', SIZE = 1MB, MAXSIZE = 5MB, FILEGROWTH = 10%);";
            string SQL_Table_Tree = "USE [{0}]; if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[{1}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) CREATE TABLE [dbo].[Tree_Table]([ID] [bigint] IDENTITY(1,1) NOT NULL, [XML] [nvarchar](max) NULL, [Name] [nchar](100) NULL, PRIMARY KEY CLUSTERED ([ID] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];";
            string SQL_Table_TextNode = "USE [{0}]; if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[{1}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) CREATE TABLE [dbo].[TextNode_Table]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Content] [nvarchar](max) NULL, [Path] [nchar](100) NOT NULL, PRIMARY KEY CLUSTERED ([ID] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]; ";
            SQL = String.Format(SQL, directory, DatabaseName, "Tree_Table", "TextNode_Table");
            SQL_Table_Tree = String.Format(SQL_Table_Tree, DatabaseName, "Tree_Table");
            SQL_Table_TextNode = String.Format(SQL_Table_TextNode, DatabaseName, "TextNode_Table");

            using (SqlConnection con = new SqlConnection(ConnectString)) {
                SqlCommand cmd = new SqlCommand(SQL, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            using (SqlConnection con = new SqlConnection(ConnectString)) {
                SqlCommand cmd_TreeTable = new SqlCommand(SQL_Table_Tree, con);
                SqlCommand cmd_TextNodeTable = new SqlCommand(SQL_Table_TextNode, con);
                con.Open();
                cmd_TreeTable.ExecuteNonQuery();
                cmd_TextNodeTable.ExecuteNonQuery();
            }
        }

    }
}
