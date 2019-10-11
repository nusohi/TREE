using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TREE.DB
{
    public class TextNodeDB
    {
        private string ConnectString;


        public TextNodeDB(string connectString) {
            ConnectString = connectString;
        }

        public int AddNewNode(TreeViewMItem node) {
            string SQL = @"insert into [dbo].[TextNode_Table](Path, Content) values(@Path, @Content);
                    select @ID=@@IDENTITY;";
            SqlParameter[] para = {
                new SqlParameter("@Path", SqlDbType.NVarChar),
                new SqlParameter("@ID", SqlDbType.Int),
                new SqlParameter("@Content", SqlDbType.NVarChar)
            };
            para[0].Value = node.Path;
            para[1].Direction = ParameterDirection.Output;
            para[2].Value = "<FlowDocument AllowDrop=\"True\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph></Paragraph></FlowDocument>";

            using (SqlConnection con = new SqlConnection(ConnectString)) {
                SqlCommand cmd = new SqlCommand(SQL, con);
                cmd.Parameters.AddRange(para);
                con.Open();
                cmd.ExecuteNonQuery();
                node.ID = (int)para[1].Value;
            }
            
            return node.ID;
        }

        public TextNode QueryNode(int id) {
            string SQL = @"select Content,Path from TextNode_Table where ID=@ID;";
            /*
             * select ID,Content,Path 
             * from TextNode_Table
             * where ID=2;
             * 
             */
            TextNode node = null;
            SqlParameter[] para = {
                new SqlParameter("@ID", SqlDbType.Int),
            };
            para[0].Value = id;

            using (SqlConnection con = new SqlConnection(ConnectString)) {
                SqlCommand cmd = new SqlCommand(SQL, con);
                cmd.Parameters.AddRange(para);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    node = new TextNode(id, reader[0].ToString(), reader[1].ToString());
                }
            }
            return node;
        }

        public void UpdateNode(TextNode node) {
            string SQL = @"update [dbo].[TextNode_Table] set Content=@Content where ID=@ID;";
            if (node.Content == null) /*SQL = @"update [dbo].[TextNode_Table] set Path=@Path where ID=@ID;";*/
                return;

            SqlParameter[] para = {
                //new SqlParameter("@Path", SqlDbType.NVarChar),
                new SqlParameter("@ID", SqlDbType.Int),
                new SqlParameter("@Content", SqlDbType.NVarChar)
            };
            //para[0].Value = node.Path;
            para[0].Value = node.ID;
            para[1].Value = node.Content == null ? "" : node.Content;

            using (SqlConnection con = new SqlConnection(ConnectString)) {
                SqlCommand cmd = new SqlCommand(SQL, con);
                cmd.Parameters.AddRange(para);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteNode(int id) {
            string SQL = @"delete from TextNode_Table where ID=@ID;";
            
            SqlParameter[] para = {
                new SqlParameter("@ID", SqlDbType.Int),
            };
            para[0].Value = id;

            using (SqlConnection con = new SqlConnection(ConnectString)) {
                SqlCommand cmd = new SqlCommand(SQL, con);
                cmd.Parameters.AddRange(para);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
