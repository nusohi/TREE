using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TREE.DB
{
    public class TreeDB
    {
        private string ConnectString;


        public TreeDB(string connectString) {
            ConnectString = connectString;
        }

        public int AddNewTree(Tree tree) {
            string SQL = @"insert into [dbo].[Tree_Table](XML) values(@XML);
                    select @ID=@@IDENTITY;";
            SqlParameter[] para = {
                new SqlParameter("@ID", SqlDbType.Int),
                new SqlParameter("@XML", SqlDbType.NVarChar)
            };
            para[0].Direction = ParameterDirection.Output;
            para[1].Value = "";

            using (SqlConnection con = new SqlConnection(ConnectString)) {
                SqlCommand cmd = new SqlCommand(SQL, con);
                cmd.Parameters.AddRange(para);
                con.Open();
                cmd.ExecuteNonQuery();
                tree.ID = (int)para[0].Value;
            }

            return tree.ID;
        }

        public Tree QueryTree(int id) {
            string SQL = @"select XML, Name from Tree_Table where ID=@ID;";

            Tree tree = null;
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
                    tree = new Tree(id, reader[0].ToString().Trim(), reader[1].ToString().Trim());
                }
            }
            return tree;
        }

        public Tree[] QueryAll() {
            string SQL = @"select ID, XML, Name from Tree_Table;";

            List<Tree> trees = new List<Tree>();

            using (SqlConnection con = new SqlConnection(ConnectString)) {
                SqlCommand cmd = new SqlCommand(SQL, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    trees.Add(new Tree(int.Parse(reader[0].ToString()), reader[1].ToString().Trim(), reader[2].ToString().Trim()));
                }
            }
            return trees.ToArray();
        }

        public void UpdateTree(Tree tree) {
            string SQL = @"update [dbo].[Tree_Table] set XML=@XML,Name=@Name where ID=@ID;";

            SqlParameter[] para = {
                new SqlParameter("@ID", SqlDbType.Int),
                new SqlParameter("@XML", SqlDbType.NVarChar),
                new SqlParameter("@Name", SqlDbType.NVarChar)
            };
            para[0].Value = tree.ID;
            para[1].Value = tree.XML == null ? "" : tree.XML;
             para[2].Value = tree.Name;

            using (SqlConnection con = new SqlConnection(ConnectString)) {
                SqlCommand cmd = new SqlCommand(SQL, con);
                cmd.Parameters.AddRange(para);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteTree(int id) {
            string SQL = @"delete from Tree_Table where ID=@ID;";

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
