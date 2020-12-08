using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;

using CertificateManager.WindowsModels;

namespace CertificateManager.Models
{
    class SQLManager
    {
        private string FileBasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CertManager\\Cert.db";
        private string DirBasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CertManager";

        //private SQLiteCommand command;
        private SQLiteConnection dataBase;

        static private SQLManager shared = null;

        static public SQLManager Shared
        {
            get
            {
                return shared ?? (shared = new SQLManager());
            }
        }

        public bool IsConnect
        {
            get;
            private set;
        }

        private SQLManager()
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
                WindowsManager.Shared.ShowMessage("Error", "Can't connect to base: " + err.Message, true);
                Environment.Exit(1);
            }
        }

        private void _openBase()
        {
            dataBase = new SQLiteConnection("Data Source = " + FileBasePath + "; Version = 3; foreign keys = true;");
            dataBase.Open();
            //command = dataBase.CreateCommand();
            //command.Connection = dataBase;
        }

        private void _initBase()
        {
            try
            {
                if (!Directory.Exists(DirBasePath))
                    Directory.CreateDirectory(DirBasePath);

                SQLiteConnection.CreateFile(FileBasePath);

                _openBase();

                SQLiteCommand command = dataBase.CreateCommand();
                command.Connection = dataBase;

                command.CommandText = "CREATE TABLE IF NOT EXISTS CACert (" +
                        "id INTEGER PRIMARY KEY NOT NULL UNIQUE ON CONFLICT ABORT, " +
                        "server INTEGER NOT NULL REFERENCES Servers (id) ON DELETE CASCADE, " +
                        "name STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
                        "commonname STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
                        "country STRING NOT NULL," +
                        "organisation STRING NOT NULL," +
                        "organisationunit STRING NOT NULL," +
                        "keysize INTEGER NOT NULL," +
                        "cert TEXT NOT NULL, " +
                        "certkey TEXT NOT NULL, " +
                        "keysign STRING NOT NULL" +
                        ")";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE TABLE IF NOT EXISTS ChildCert (" +
                        "id INTEGER PRIMARY KEY NOT NULL UNIQUE ON CONFLICT ABORT, " +
                        "user INTEGER NOT NULL REFERENCES Users (id) ON DELETE CASCADE, " +
                        "cacert INTEGER NOT NULL REFERENCES CACert (id) ON DELETE CASCADE, " +
                        "name STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
                        "commonname STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
                        "country STRING NOT NULL," +
                        "organisation STRING NOT NULL," +
                        "organisationunit STRING NOT NULL," +
                        "keysize INTEGER NOT NULL," +
                        "cert TEXT NOT NULL, " +
                        "certkey TEXT NOT NULL," +
                        "keysign STRING NOT NULL" +
                        ")";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE TABLE IF NOT EXISTS Servers (" +
                        "id INTEGER PRIMARY KEY NOT NULL UNIQUE ON CONFLICT ABORT, " +
                        "name STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
                        "ip STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
                        "port INTEGER NOT NULL," +
                        "mode INTEGER NOT NULL," +
                        "proto INTEGER NOT NULL," +
                        "params STRING NOT NULL" +
                        ")";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE TABLE IF NOT EXISTS Users (" +
                        "id INTEGER PRIMARY KEY NOT NULL UNIQUE ON CONFLICT ABORT, " +
                        "server INTEGER NOT NULL REFERENCES Servers (id) ON DELETE CASCADE, " +
                        "login STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
                        "password STRING NOT NULL," +
                        "params STRING NOT NULL" +
                        ")";
                command.ExecuteNonQuery();
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
        }


        private void _AddUserCert(Cert newCert, string cert64, string key64, string keySignature, User user, Server server)
        {
            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "INSERT INTO ChildCert (user, cacert, name, commonname, country, organisation, organisationunit, keysize, cert, certkey, keysign) VALUES(@user, @cacert, @name, @commonname, @country, @organisation, @organisationunit, @keysize, @cert, @certkey, @keysign)";
            command.Parameters.AddWithValue("@user", user.ID);
            command.Parameters.AddWithValue("@cacert", server.certificate.ID);

            //        "id INTEGER PRIMARY KEY NOT NULL UNIQUE ON CONFLICT ABORT, " +
            //        "user INTEGER NOT NULL REFERENCES Users (id) ON DELETE CASCADE, " +
            //        "cacert INTEGER NOT NULL REFERENCES CACert (id) ON DELETE CASCADE, " +
            //        "name STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
            //        "commonname STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
            //        "country STRING NOT NULL," +
            //        "organisation STRING NOT NULL," +
            //        "organisationunit STRING NOT NULL," +
            //        "keysize INTEGER NOT NULL," +
            //        "cert TEXT NOT NULL, " +
            //        "certkey TEXT NOT NULL," +
            //        "keysign STRING NOT NULL" +

            command.Parameters.AddWithValue("@name", newCert.Name);
            command.Parameters.AddWithValue("@commonname", newCert.CommonName);
            command.Parameters.AddWithValue("@country", newCert.Country);
            command.Parameters.AddWithValue("@organisation", newCert.Organisation);
            command.Parameters.AddWithValue("@organisationunit", newCert.OrganisationUnit);
            command.Parameters.AddWithValue("@keysize", newCert.KeySize);
            command.Parameters.AddWithValue("@cert", cert64);
            command.Parameters.AddWithValue("@certkey", key64);
            command.Parameters.AddWithValue("@keysign", keySignature);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SQLiteException err)
            {
                if (err.ResultCode == SQLiteErrorCode.Constraint)
                    throw new Exception($"Error add user certificate: this Name ({newCert.Name}) or Common Name ({newCert.CommonName}) already used!");
                else
                    throw new Exception($"Error add user certificate: {err.Message}");
            }
        }

        private void _AddServerCert(Cert newCert, string cert64, string key64, string keySignature, Server server)
        {
            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "INSERT INTO CACert (server, name, commonname, country, organisation, organisationunit, keysize, cert, certkey, keysign) VALUES(@server, @name, @commonname, @country, @organisation, @organisationunit, @keysize, @cert, @certkey, @keysign)";
            command.Parameters.AddWithValue("@server", server.ID);

            //        "id INTEGER PRIMARY KEY NOT NULL UNIQUE ON CONFLICT ABORT, " +
            //        "server INTEGER NOT NULL REFERENCES Servers (id) ON DELETE CASCADE, " +
            //        "name STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
            //        "commonname STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
            //        "country STRING NOT NULL," +
            //        "organisation STRING NOT NULL," +
            //        "organisationunit STRING NOT NULL," +
            //        "keysize INTEGER NOT NULL," +
            //        "cert TEXT NOT NULL, " +
            //        "certkey TEXT NOT NULL, " +
            //        "keysign STRING NOT NULL" +

            command.Parameters.AddWithValue("@name", newCert.Name);
            command.Parameters.AddWithValue("@commonname", newCert.CommonName);
            command.Parameters.AddWithValue("@country", newCert.Country);
            command.Parameters.AddWithValue("@organisation", newCert.Organisation);
            command.Parameters.AddWithValue("@organisationunit", newCert.OrganisationUnit);
            command.Parameters.AddWithValue("@keysize", newCert.KeySize);
            command.Parameters.AddWithValue("@cert", cert64);
            command.Parameters.AddWithValue("@certkey", key64);
            command.Parameters.AddWithValue("@keysign", keySignature);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SQLiteException err)
            {
                if (err.ResultCode == SQLiteErrorCode.Constraint)
                    throw new Exception($"Error add CA certificate: this Name ({newCert.Name}) or Common Name ({newCert.CommonName}) already used!");
                else
                    throw new Exception($"Error add CA certificate: {err.Message}");
            }
        }

        public void AddServer(ref Server server)
        {
            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "SELECT MAX(id) AS id FROM Servers";
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();

            server.ID = (reader["id"] is DBNull ? 0 : (long)reader["id"]) + 1;

            reader.Close();

            command.CommandText = "INSERT INTO Servers (id, name, ip, port, mode, proto, params) VALUES(@id, @name, @ip, @port, @mode, @proto, @params)";
            command.Parameters.AddWithValue("@id", server.ID);
            command.Parameters.AddWithValue("@name", server.Name);
            command.Parameters.AddWithValue("@ip", server.IP);
            command.Parameters.AddWithValue("@port", (long)server.Port);
            command.Parameters.AddWithValue("@mode", (long)server.Mode);
            command.Parameters.AddWithValue("@proto", (long)server.Proto);
            command.Parameters.AddWithValue("@params", server.Params);

            try
            {
                string[] cert = RSAHelper.CreateCACert(server.certificate);

                command.ExecuteNonQuery();
                _AddServerCert(server.certificate, cert[0], cert[1], cert[2], server);
            }
            catch (Exception err)
            {
                throw new Exception($"Error add server: {err.Message}");
            }
        }

        public void DeleteServer(Server server)
        {
            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "DELETE FROM Servers WHERE id = @id";
            command.Parameters.AddWithValue("@id", server.ID);

            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception err)
            {
                throw new Exception(err.Message);
            }
        }

        public void AddUser(ref User user, Server server)
        {
            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "SELECT MAX(id) as id FROM Users";
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();

            user.ID = (reader["id"] is DBNull ? 1 : (long)reader["id"]) + 1;

            reader.Close();

            command.CommandText = "INSERT INTO Users (id, server, login, password, params) VALUES(@id, @server, @login, @password, @params)";
            command.Parameters.AddWithValue("@id", user.ID);
            command.Parameters.AddWithValue("@server", server.ID);
            command.Parameters.AddWithValue("@login", user.Login);
            command.Parameters.AddWithValue("@password", user.Password);
            command.Parameters.AddWithValue("@params", user.Parameters);

            try
            {
                string[] ca = GetCA(server.certificate);

                string[] cert = RSAHelper.CreateChildCert(user.certificate, ca);

                command.ExecuteNonQuery();
                _AddUserCert(user.certificate, cert[0], cert[1], cert[2], user, server);
            }
            catch (Exception err)
            {
                throw new Exception($"Error add user: {err.Message}");
            }
        }

        public void DeleteUser(User user)
        {
            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "DELETE FROM Users WHERE id = @id";
            command.Parameters.AddWithValue("@id", user.ID);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
        }

        public List<Server> GetServers()
        {
            List<Server> servers = new List<Server>();

            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "SELECT Servers.id as Sid, Servers.name as Sname, ip, port, mode, proto, params, CACert.id as Cid, CACert.name as Cname, commonname, country, organisation, organisationunit, keysize FROM Servers INNER JOIN CACert ON Servers.id = CACert.server";

            try
            {
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Server s = new Server();
                    s.ID = (long)reader["Sid"];
                    s.Name = (string)reader["Sname"];
                    s.IP = (string)reader["ip"];
                    s.Port = (long)reader["port"];
                    s.Mode = (Server.LayerMode)(long)reader["mode"];
                    s.Proto = (Server.Protocol)(long)reader["proto"];
                    s.Params = (string)reader["params"];
                    s.certificate.ID = (long)reader["Cid"];
                    s.certificate.Name = (string)reader["Cname"];
                    s.certificate.CommonName = (string)reader["commonname"];
                    s.certificate.Country = (string)reader["country"];
                    s.certificate.Organisation = (string)reader["organisation"];
                    s.certificate.OrganisationUnit = (string)reader["organisationunit"];
                    s.certificate.KeySize = (long)reader["keysize"];
                    servers.Add(s);
                }

                reader.Close();
            }
            catch (Exception err)
            {
                WindowsManager.Shared.ShowMessage("Error SQLite read servers!", err.Message, true);
            }

            return servers;
        }

        public List<User> GetUsersFor(Server server)
        {
            List<User> users = new List<User>();

            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "SELECT Users.id as Uid, login, password, params, ChildCert.id as Cid, ChildCert.name as Cname, commonname, country, organisation, organisationunit, keysize FROM Users INNER JOIN ChildCert ON ChildCert.user = Uid WHERE Users.server = @server";
            command.Parameters.AddWithValue("@server", server.ID);

            try
            {
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    User u = new User();
                    u.ID = (long)reader["Uid"];
                    u.Login = (string)reader["login"];
                    u.Password = (string)reader["password"];
                    u.Parameters = (string)reader["params"];
                    u.certificate.ID = (long)reader["Cid"];
                    u.certificate.Name = (string)reader["Cname"];
                    u.certificate.CommonName = (string)reader["commonname"];
                    u.certificate.Country = (string)reader["country"];
                    u.certificate.Organisation = (string)reader["organisation"];
                    u.certificate.OrganisationUnit = (string)reader["organisationunit"];
                    u.certificate.KeySize = (long)reader["keysize"];
                    users.Add(u);
                }

                reader.Close();
            }
            catch (Exception err)
            {
                WindowsManager.Shared.ShowMessage("Error SQLite read users!", err.Message, true);
            }

            return users;
        }

        public string[] GetCA(Cert cert)
        {
            string[] res = new string[3];

            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "SELECT cert, certkey, keysign FROM CACert WHERE id = @id";
            command.Parameters.AddWithValue("@id", cert.ID);
            SQLiteDataReader reader = command.ExecuteReader();

            reader.Read();

            res[0] = (string)reader["cert"];
            res[1] = (string)reader["certkey"];
            res[2] = (string)reader["keysign"];

            reader.Close();

            return res;
        }

        public string[] GetUserCert(Cert cert)
        {
            string[] res = new string[3];

            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "SELECT cert, certkey, keysign FROM ChildCert WHERE id = @id";
            command.Parameters.AddWithValue("@id", cert.ID);
            SQLiteDataReader reader = command.ExecuteReader();

            reader.Read();

            res[0] = (string)reader["cert"];
            res[1] = (string)reader["certkey"];
            res[2] = (string)reader["keysign"];

            reader.Close();

            return res;
        }

    }
}
