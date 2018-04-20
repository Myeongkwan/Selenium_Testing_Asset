using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Data.Common;

namespace Selenium_Testing_Asset
{
    public class DBManager
    {
        SqlCommand cmd;
        string DBInstance;
        string DBUser;
        string DBPass;

        private SqlConnection GetConnection(string DBName)
        {
            DBInstance = "10.30.136.70";
            DBUser = "qateam";
            DBPass = "bespin!2018";
            string connectionString = @"Data Source=" + DBInstance + ";Initial Catalog=" + DBName + ";User ID=" + DBUser + ";Password=" + DBPass + ";";
            //string connectionString = "Data Source=.\\;Initial Catalog=" + DBName + ";Trusted_Connection=Yes;";
            SqlConnection cn = new SqlConnection();
            cn.ConnectionString = connectionString;
            cn.Open();
            return cn;
        }

        public Boolean ExecSQL(string sql)
        {
            //XmlNode El = Context.TaskInfo.FirstChild;
            //string SqlFile = El.Attributes["Value"].Value;
            SqlConnection Cn = GetConnection("master");

            Regex regex = new Regex(@"\r{0,1}\nGO\r{0,1}\n");
            string[] commands = regex.Split(sql);

            for (int i = 0; i < commands.Length; i++)
            {
                try
                {
                    if (commands[i] != string.Empty)
                    {
                        using (SqlCommand command = new SqlCommand(commands[i], Cn))
                        {
                            command.CommandTimeout = 0;
                            command.ExecuteNonQuery();
                            command.Dispose();
                        }
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            Cn.Close();
            return true;
        }

        public DataSet ExecuteDataQuery(string QueryString, string ConnectionDB)
        {
            SqlConnection con = GetConnection(ConnectionDB);
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = QueryString;
            cmd.Connection = con;

            try
            {
                //con.Open();
                DataSet ds = new DataSet();
                DbDataAdapter dpt = new SqlDataAdapter((SqlCommand)cmd);
                dpt.Fill(ds);

                return ds;
            }
            finally
            {
                con.Close();
                con.Dispose();
                cmd.Dispose();
            }
        }
    }
}
