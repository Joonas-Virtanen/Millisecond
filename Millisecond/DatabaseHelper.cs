using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Millisecond
{
    class DatabaseHelper
    {

        /// <summary>
        /// Creates a SQLlite Database
        /// </summary>
        public static void CreateDatabase()
        {
            using (var con = new SQLiteConnection(ConfigurationManager.AppSettings["Database.Location"]))
            {
                con.Open();

                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = "DROP TABLE IF EXISTS 'Attributes'";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DROP TABLE IF EXISTS 'EmailMessages'";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = @"CREATE TABLE Attributes(Id Integer PRIMARY KEY AUTOINCREMENT, EmailID INT, Attribute nvarchar(100), DateCreated TEXT)";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"CREATE TABLE EmailMessages(Id Integer PRIMARY KEY AUTOINCREMENT, Key INT, Email nvarchar(100), DateCreated TEXT)";
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
        }


        /// <summary>
        /// Inserts Attribute to DB
        /// </summary>
        /// <param name="EmailId"></param>
        /// <param name="Attribute"></param>
        public static void InsertAttribute(int EmailId, string Attribute)
        {
            using (var con = new SQLiteConnection(ConfigurationManager.AppSettings["Database.Location"]))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {

                    cmd.CommandText = "INSERT INTO Attributes(EmailID,Attribute, DateCreated) VALUES(@EmailID, @Attribute, @Date)";
                    cmd.Parameters.Add(new SQLiteParameter("@EmailID", EmailId));
                    cmd.Parameters.Add(new SQLiteParameter("@Attribute", Attribute));
                    cmd.Parameters.Add(new SQLiteParameter("@Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
        }

        /// <summary>
        /// Gets information for email
        /// </summary>
        /// <returns></returns>
        public static Email GetEmailInformation()
        {
            Email emailobj = new Email();
            List<string> attributes = new List<string>();
            int emailId = -1;

            using (var con = new SQLiteConnection(ConfigurationManager.AppSettings["Database.Location"]))
            {
                con.Open();

                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = "Select Id, Email from EmailMessages  E " +
                        "where ( select Count(*) from Attributes a where e.Id = a.EmailID ) = 10";

                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {

                        while (rdr.Read())
                        {
                            emailId = Convert.ToInt32(rdr["Id"]);
                            emailobj.EmailAddress = rdr["Email"].ToString();
                        }
                    }
                    cmd.CommandText = "Select Attribute from Attributes  A " +
                                 "where a.EmailID = @EmailID";
                    cmd.Parameters.Add(new SQLiteParameter("@EmailID", emailId));
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            attributes.Add((rdr["Attribute"]).ToString());
                        }
                    }
                }
            }
            emailobj.Attributes = string.Join(",", attributes.ToArray());
            return emailobj;
        }

        /// <summary>
        /// Inserts emailMessage to DB
        /// </summary>
        /// <param name="key"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public static int InsertEmailMessages(int key, string email)
        {
            using (var con = new SQLiteConnection(ConfigurationManager.AppSettings["Database.Location"]))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = "INSERT INTO EmailMessages(Key, Email,DateCreated) VALUES(@Key, @Email, @Date)";
                    cmd.Parameters.Add(new SQLiteParameter("@Key", key));
                    cmd.Parameters.Add(new SQLiteParameter("@Email", email));
                    cmd.Parameters.Add(new SQLiteParameter("@Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "select last_insert_rowid()";
                    long LastRowID64 = (long)cmd.ExecuteScalar();
                    return (int)LastRowID64;
                }
                con.Close();
            }
        }

        /// <summary>
        /// Read the SQLlite database tables
        /// </summary>
        public static void ReadDatabase()
        {
            Console.WriteLine("");
            Console.WriteLine("All database tables:");
            using (var con = new SQLiteConnection(ConfigurationManager.AppSettings["Database.Location"]))
            {
                con.Open();
                Console.WriteLine("EmailMessages:");
                string select_EmailMessages = "SELECT * FROM EmailMessages";

                using (var cmd = new SQLiteCommand(select_EmailMessages, con))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {

                        while (rdr.Read())
                        {
                            Console.WriteLine($"{rdr["Id"]} {rdr["Key"]} {rdr["Email"]} {rdr["DateCreated"]}");
                        }
                    }
                }
                Console.WriteLine("Attributes:");
                string select_Attributes = "SELECT * FROM Attributes";

                using (var cmd = new SQLiteCommand(select_Attributes, con))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            Console.WriteLine($"{rdr["Id"]} {rdr["EmailID"]} {rdr["Attribute"]} {rdr["DateCreated"]}");
                        }
                    }
                }
                con.Close();
            }
        }


    }
}
