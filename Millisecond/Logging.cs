using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Millisecond
{
    public class Logging
    {

        /// <summary>
        /// Creates a binary file from parameters
        /// </summary>
        /// <param name="fileLocation">File location</param>
        /// <param name="email">Something to write</param>
        private static void WriteBinary(string email)
        {
            try
            {
                string filename = DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                using (var binWriter = new BinaryWriter(File.Open(ConfigurationManager.AppSettings["Logging.Location"] + filename, FileMode.Append)))
                {
                    // Write string   
                    binWriter.Write(DateTime.Now.ToString("HH-mm-ss") + " " + email);
                    binWriter.Write(";");
                }
            }
            catch (IOException ioexp)
            {
                Console.WriteLine("Error: {0}", ioexp.Message);
            }
        }

        /// <summary>
        /// Writes email to log
        /// </summary>
        /// <param name="email"></param>
        public static void Log(string email)
        {
            var hash = HashSHA1(email);
            WriteBinary(hash);
        }

        /// <summary>
        /// Hashes string
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string HashSHA1(string value)
        {
            var sha1 = SHA1.Create();
            var inputBytes = Encoding.UTF8.GetBytes(value);
            var hash = sha1.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }

}
