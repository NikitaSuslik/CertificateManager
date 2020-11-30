using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;

namespace CertificateCreator.Models
{
    class CertManager
    {
        private string FileBasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CertManager\\Cert.db";
        private string DirBasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CertManager";

        private SQLiteCommand command;
        private SQLiteConnection dataBase;

        public bool IsConnect
        {
            get;
            private set;
        }

        public CertManager()
        {
            try
            {
                if (!File.Exists(FileBasePath))
                    _initBase();
                else
                    _openBase();
            }
            catch (Exception err)
            {
                throw new Exception("Can't connect to base: " + err.Message);
            }
        }

        private void _openBase()
        {
            dataBase = new SQLiteConnection("Data Source = " + FileBasePath + "; Version = 3; foreign keys = true;");
            dataBase.Open();
            command = dataBase.CreateCommand();
            command.Connection = dataBase;
        }

        private void _initBase()
        {
            try
            {
                if (!Directory.Exists(DirBasePath))
                    Directory.CreateDirectory(DirBasePath);

                SQLiteConnection.CreateFile(FileBasePath);

                _openBase();

                command.CommandText = "CREATE TABLE IF NOT EXISTS CACert (" +
                        "id INTEGER PRIMARY KEY NOT NULL UNIQUE ON CONFLICT ABORT, " +
                        "name STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
                        "cert TEXT NOT NULL, " +
                        "certkey TEXT NOT NULL, " +
                        "keysign STRING NOT NULL" +
                        ")";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE TABLE IF NOT EXISTS ChildCert (" +
                        "id INTEGER PRIMARY KEY NOT NULL UNIQUE ON CONFLICT ABORT, " +
                        "cacert INTEGER NOT NULL REFERENCES CACert (id) ON DELETE CASCADE, " +
                        "name STRING NOT NULL," +
                        "cert TEXT NOT NULL, " +
                        "certkey TEXT NOT NULL," +
                        "keysign STRING NOT NULL" +
                        ")";
                command.ExecuteNonQuery();
            }
            catch(Exception err)
            {
                throw new Exception(err.Message);
            }
        }

        public void AddCACert(string name, string cert, string key, string keySignature)
        {
            command.CommandText = "INSERT INTO CACert (name, cert, certkey, keysign) VALUES(@name, @cert, @certkey, @keysign)";
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@cert", cert);
            command.Parameters.AddWithValue("@certkey", key);
            command.Parameters.AddWithValue("@keysign", keySignature);

            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception err)
            {
                throw new Exception($"Error add CA certificate: {err.Message}");
            }
        }

        public void AddChildCert(long parentCert, string name, string cert, string key, string keySignature)
        {
            command.CommandText = "INSERT INTO ChildCert (name, cert, certkey, keysign, cacert) VALUES(@name, @cert, @certkey, @keysign, @cacert)";
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@cert", cert);
            command.Parameters.AddWithValue("@certkey", key);
            command.Parameters.AddWithValue("@keysign", keySignature);
            command.Parameters.AddWithValue("@cacert", parentCert);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception err)
            {
                throw new Exception($"Error add ChildCert certificate: {err.Message}");
            }
        }

        public List<string> GetCAs()
        {
            List<string> res = new List<string>();

            command.CommandText = "SELECT name FROM CACert";
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                res.Add((string)reader["name"]);
            }

            return res;
        }

    }
}
