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


                command.CommandText = "CREATE TABLE IF NOT EXISTS CAs (" +
                        "id INTEGER PRIMARY KEY NOT NULL UNIQUE ON CONFLICT ABORT, " +
                        "name STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
                        "cert STRING NOT NULL," +
                        "key STRING NOT NULL" +
                        ")";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE TABLE IF NOT EXISTS Servers (" +
                        "id INTEGER PRIMARY KEY NOT NULL UNIQUE ON CONFLICT ABORT, " +
                        "ca INTEGER NOT NULL REFERENCES CAs (id) ON DELETE CASCADE," +
                        "name STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
                        "ip STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
                        "port INTEGER NOT NULL," +
                        "mode INTEGER NOT NULL," +
                        "proto INTEGER NOT NULL," +
                        "cipher INTEGER NOT NULL," +
                        "auth INTEGER NOT NULL," +
                        "params STRING NOT NULL" +
                        ")";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE TABLE IF NOT EXISTS ServerCert (" +
                        "id INTEGER PRIMARY KEY NOT NULL UNIQUE ON CONFLICT ABORT, " +
                        "server INTEGER NOT NULL REFERENCES Servers (id) ON DELETE CASCADE, " +
                        "name STRING NOT NULL UNIQUE ON CONFLICT ABORT, " +
                        "cert TEXT NOT NULL, " +
                        "key TEXT NOT NULL" +
                        ")";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE TABLE IF NOT EXISTS Users (" +
                        "id INTEGER PRIMARY KEY NOT NULL UNIQUE ON CONFLICT ABORT, " +
                        "server INTEGER NOT NULL REFERENCES Servers (id) ON DELETE CASCADE, " +
                        "login STRING NOT NULL," +
                        "password STRING NOT NULL," +
                        "params STRING NOT NULL" +
                        ")";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE TABLE IF NOT EXISTS UserCert (" +
                        "id INTEGER PRIMARY KEY NOT NULL UNIQUE ON CONFLICT ABORT, " +
                        "user INTEGER NOT NULL REFERENCES Users (id) ON DELETE CASCADE, " +
                        "name STRING NOT NULL UNIQUE ON CONFLICT ABORT," +
                        "cert TEXT NOT NULL, " +
                        "key TEXT NOT NULL" +
                        ")";
                command.ExecuteNonQuery();
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
        }

        public void AddCACert(Cert ca)
        {
            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "INSERT INTO CAs (name, cert, key) VALUES(@name, @cert, @key)";
            command.Parameters.AddWithValue("@name", ca.Name);

            try
            {
                string[] ca64 = RSAHelper.CreateCACert(ca);
                command.Parameters.AddWithValue("@cert", ca64[0]);
                command.Parameters.AddWithValue("@key", ca64[1]);
                command.ExecuteNonQuery();
            }
            catch (Exception err)
            {
                throw new Exception($"CA create error: {err.Message}");
            }
        }

        public void DeleteCA(Cert ca)
        {
            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "DELETE FROM CAs WHERE id = @id";
            command.Parameters.AddWithValue("@id", ca.ID);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception err)
            {
                throw new Exception($"Delete CA error: {err.Message}");
            }
        }

        public void AddServer(Server server, Cert ca)
        {
            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "SELECT MAX(id) AS id FROM Servers";
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();

            server.ID = (reader["id"] is DBNull ? 0 : (long)reader["id"]) + 1;

            reader.Close();

            command.CommandText = "INSERT INTO Servers (id, ca, name, ip, port, mode, proto, cipher, auth, params) VALUES(@id, @ca, @name, @ip, @port, @mode, @proto, @cipher, @auth, @params)";
            command.Parameters.AddWithValue("@id", server.ID);
            command.Parameters.AddWithValue("@ca", ca.ID);
            command.Parameters.AddWithValue("@name", server.Name);
            command.Parameters.AddWithValue("@ip", server.IP);
            command.Parameters.AddWithValue("@port", (long)server.Port);
            command.Parameters.AddWithValue("@mode", (long)server.Mode);
            command.Parameters.AddWithValue("@proto", (long)server.Proto);
            command.Parameters.AddWithValue("@cipher", (long)server.Cipher);
            command.Parameters.AddWithValue("@auth", (long)server.Auth);
            command.Parameters.AddWithValue("@params", server.Params);

            try
            {
                command.ExecuteNonQuery();
                _AddServerCert(server, ca);
            }
            catch (Exception err)
            {
                throw new Exception($"Error add server: {err.Message}");
            }
        }

        private void _AddServerCert(Server server, Cert ca)
        {
            string[] cert = RSAHelper.CreateServerCert(server.certificate, ca);

            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "INSERT INTO ServerCert (server, name, cert, key) VALUES(@server, @name, @cert, @key)";
            command.Parameters.AddWithValue("@server", server.ID);
            command.Parameters.AddWithValue("@name", server.certificate.Name);
            command.Parameters.AddWithValue("@cert", cert[0]);
            command.Parameters.AddWithValue("@key", cert[1]);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SQLiteException err)
            {
                if (err.ResultCode == SQLiteErrorCode.Constraint)
                    throw new Exception($"Error add CA certificate: this Name ({server.certificate.Name}) already used!");
                else
                    throw new Exception($"Error add CA certificate: {err.Message}");
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

        public void AddUser(User user, Server server, Cert ca)
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
            command.Parameters.AddWithValue("@params", user.Params);

            try
            {
                command.ExecuteNonQuery();
                _AddUserCert(user, server, ca);
            }
            catch (Exception err)
            {
                throw new Exception($"Error add user: {err.Message}");
            }
        }
        private void _AddUserCert(User user, Server server, Cert ca)
        {
            string[] cert = RSAHelper.CreateUserCert(user.certificate, ca);

            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "INSERT INTO UserCert (user, name, cert, key) VALUES(@user, @name, @cert, @key)";
            command.Parameters.AddWithValue("@user", user.ID);
            command.Parameters.AddWithValue("@name", user.certificate.Name);
            command.Parameters.AddWithValue("@cert", cert[0]);
            command.Parameters.AddWithValue("@key", cert[1]);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SQLiteException err)
            {
                if (err.ResultCode == SQLiteErrorCode.Constraint)
                    throw new Exception($"Error add user certificate: this Name ({user.certificate.Name}) already used!");
                else
                    throw new Exception($"Error add user certificate: {err.Message}");
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

        public List<Cert> GetCAs()
        {
            List<Cert> res = new List<Cert>();

            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "SELECT id, name, cert, key FROM CAs";
            try
            {
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Cert c = new Cert();
                    c.ID = (long)reader["id"];
                    c.Name = (string)reader["name"];
                    RSAHelper.FillInfo(ref c, new string[] { (string)reader["cert"], (string)reader["key"] });
                    res.Add(c);
                }

                reader.Close();
            }
            catch (Exception err)
            {
                throw new Exception($"Read CAs error: {err.Message}");
            }

            return res;
        }

        public List<Server> GetServersForCa(Cert ca)
        {
            List<Server> servers = new List<Server>();

            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "SELECT Servers.id as Sid, Servers.name as Sname, ip, port, mode, proto, cipher, auth, params, ServerCert.id as Cid, ServerCert.name as Cname, cert, key FROM Servers INNER JOIN ServerCert ON Servers.id = ServerCert.server WHERE Servers.CA = @ca";
            command.Parameters.AddWithValue("@ca", ca.ID);

            try
            {
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Server s = new Server();
                    Cert c = new Cert();
                    s.ID = (long)reader["Sid"];
                    s.Name = (string)reader["Sname"];
                    s.IP = (string)reader["ip"];
                    s.Port = (long)reader["port"];
                    s.Mode = (Server.ModeName)(long)reader["mode"];
                    s.Proto = (Server.ProtocolName)(long)reader["proto"];
                    s.Cipher = (Server.CipherName)(long)reader["cipher"];
                    s.Auth = (Server.AuthName)(long)reader["auth"];
                    s.Params = (string)reader["params"];

                    c.ID = (long)reader["Cid"];
                    c.Name = (string)reader["Cname"];
                    RSAHelper.FillInfo(ref c, new string[] { (string)reader["cert"], (string)reader["key"]});
                    s.certificate = c;

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

            command.CommandText = "SELECT Users.id as Uid, login, password, params, UserCert.id as Cid, UserCert.name as Cname, cert, key FROM Users INNER JOIN UserCert ON UserCert.user = Uid WHERE Users.server = @server";
            command.Parameters.AddWithValue("@server", server.ID);

            try
            {
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    User u = new User();
                    Cert c = new Cert();
                    u.ID = (long)reader["Uid"];
                    u.Login = (string)reader["login"];
                    u.Password = (string)reader["password"];
                    u.Params = (string)reader["params"];

                    c.ID = (long)reader["Cid"];
                    c.Name = (string)reader["Cname"];
                    RSAHelper.FillInfo(ref c, new string[] { (string)reader["cert"], (string)reader["key"] });
                    u.certificate = c;

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

        public void EditUser(User user)
        {
            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "UPDATE Users SET login=@login, password=@password, params=@params WHERE id = @id";
            command.Parameters.AddWithValue("@login", user.Login);
            command.Parameters.AddWithValue("@password", user.Password);
            command.Parameters.AddWithValue("@params", user.Params);
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

        public void EditServer(Server server)
        {
            SQLiteCommand command = dataBase.CreateCommand();
            command.Connection = dataBase;

            command.CommandText = "UPDATE Servers SET name=@name, " +
                                                    "ip=@ip, " +
                                                    "port=@port, " +
                                                    "mode=@mode, " +
                                                    "proto=@proto, " +
                                                    "cipher=@cipher, " +
                                                    "auth=@auth, " +
                                                    "params=@params " +
                                                    "WHERE id = @id";
            command.Parameters.AddWithValue("@name", server.Name);
            command.Parameters.AddWithValue("@ip", server.IP);
            command.Parameters.AddWithValue("@port", server.Port);
            command.Parameters.AddWithValue("@mode", server.Mode);
            command.Parameters.AddWithValue("@proto", server.Proto);
            command.Parameters.AddWithValue("@cipher", server.Cipher);
            command.Parameters.AddWithValue("@auth", server.Auth);
            command.Parameters.AddWithValue("@params", server.Params);
            command.Parameters.AddWithValue("@id", server.ID);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
        }
    }
}
