using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Ookii.Dialogs.Wpf;

using CertificateManager.Windows;
using CertificateManager.Models;

namespace CertificateManager.WindowsModels
{
    class MainWindowModel : MyWindowModel
    {
        private long _SelectedIndexExport = 0;
        public long SelectedIndexExport
        {
            get
            {
                return _SelectedIndexExport;
            }
            set
            {
                _SelectedIndexExport = value;
                OnPropertyChanged("SelectedIndexExport");
            }
        }

        private List<Server> _Servers = new List<Server>();
        public List<Server> Servers
        {
            get
            {
                return _Servers;
            }
            set
            {
                _Servers = value;
                OnPropertyChanged("Servers");
            }
        }

        private List<User> _Users = new List<User>();
        public List<User> Users
        {
            get { return _Users; }
            set { _Users = value; OnPropertyChanged("Users"); }
        }


        private Server _SelectedServer = new Server();
        public Server SelectedServer
        {
            get
            {
                return _SelectedServer;
            }
            set
            {
                _SelectedServer = value;
                OnPropertyChanged("SelectedServer");
                _UpdateUsers();
            }
        }

        private User _SelectedUser = new User();

        public User SelectedUser
        {
            get { return _SelectedUser; }
            set { _SelectedUser = value; OnPropertyChanged("SelectedUser"); }
        }


        private CommandRelise _CreateServer;
        public CommandRelise CreateServer
        {
            get
            {
                return _CreateServer ?? (_CreateServer = new CommandRelise(obj =>
                {
                    WindowsManager.Shared.ShowWindow(new CreateServerWindow());
                }));
            }
        }

        private CommandRelise _CreateUser;
        public CommandRelise CreateUser
        {
            get
            {
                return _CreateUser ?? (_CreateUser = new CommandRelise(obj =>
                {
                    WindowsManager.Shared.ShowWindow(new CreateUserWindow(), SelectedServer);
                },(can) => SelectedServer.ID != -1));
            }
        }

        private CommandRelise _DeleteServer;
        public CommandRelise DeleteServer
        {
            get
            {
                return _DeleteServer ?? (_DeleteServer = new CommandRelise(obj =>
                {
                    try
                    {
                        if (WindowsManager.Shared.ShowQuestion("Warning!", $"Are you sure want delete this server \"{SelectedServer.Name}\"?\nAll users and certificates this server will delete too!"))
                        {
                            SQLManager.Shared.DeleteServer(SelectedServer);
                            _WindowUpdate(this, new EventArgs());
                        }
                    }
                    catch (Exception err)
                    {
                        WindowsManager.Shared.ShowMessage("Delete server error!", err.Message, true);
                    }
                },(can) => SelectedServer.ID != -1));
            }
        }

        private CommandRelise _DeleteUser;

        public CommandRelise DeleteUser
        {
            get
            {
                return _DeleteUser ?? (_DeleteUser = new CommandRelise(obj =>
                {
                    try
                    {
                        if (WindowsManager.Shared.ShowQuestion("Warning!", $"Are you sure want delete this user \"{SelectedUser.Login}\"?\nAll certificate this user will delete too!"))
                        {
                            SQLManager.Shared.DeleteUser(SelectedUser);
                            _WindowUpdate(this, new EventArgs());
                        }
                    }
                    catch (Exception err)
                    {
                        WindowsManager.Shared.ShowMessage("Delete user error", err.Message, true);
                    }
                }, (can) => SelectedUser.ID != -1));
            }
        }

        private CommandRelise _ExportButton;
        public CommandRelise ExportButton
        {
            get
            {
                return _ExportButton ?? (_ExportButton = new CommandRelise(obj =>
                {
                    VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
                    folderBrowserDialog.UseDescriptionForTitle = true;
                    folderBrowserDialog.Description = "Select export folder";
                    folderBrowserDialog.RootFolder = Environment.SpecialFolder.DesktopDirectory;
                    folderBrowserDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\";

                    if (folderBrowserDialog.ShowDialog().GetValueOrDefault())
                    {
                        string ExportFolderPath = folderBrowserDialog.SelectedPath;
                        StringBuilder ConfigData = new StringBuilder();
                        StringBuilder CAData = new StringBuilder();
                        StringBuilder UserCertData = new StringBuilder();
                        StringBuilder UserKeyData = new StringBuilder();

                        try
                        {
                            switch (SelectedIndexExport)
                            {
                                case 0:
                                    string[] CA = SQLManager.Shared.GetCA(SelectedServer.certificate);
                                    string[] UserCert = SQLManager.Shared.GetUserCert(SelectedUser.certificate);

                                    CAData.AppendLine("-----BEGIN CERTIFICATE-----");
                                    CAData.AppendLine(CA[0]);
                                    CAData.AppendLine("-----END CERTIFICATE-----");

                                    UserCertData.AppendLine("-----BEGIN CERTIFICATE-----");
                                    UserCertData.AppendLine(UserCert[0]);
                                    UserCertData.AppendLine("-----END CERTIFICATE-----");

                                    UserKeyData.AppendLine("-----BEGIN PRIVATE KEY-----");
                                    UserKeyData.AppendLine(UserCert[1]);
                                    UserKeyData.AppendLine("-----END PRIVATE KEY-----");

                                    ConfigData.AppendLine("client");
                                    ConfigData.AppendLine($"dev {SelectedServer.SMode}");
                                    ConfigData.AppendLine($"proto {SelectedServer.SProto}");
                                    ConfigData.AppendLine($"remote {SelectedServer.IP} {SelectedServer.Port}");
                                    ConfigData.AppendLine("resolv-retry infinite");
                                    ConfigData.AppendLine("remote-cert-tls server");
                                    ConfigData.AppendLine("<ca>");
                                    ConfigData.AppendLine(CAData.ToString());
                                    ConfigData.AppendLine("</ca>");
                                    ConfigData.AppendLine("<cert>");
                                    ConfigData.AppendLine(UserCertData.ToString());
                                    ConfigData.AppendLine("</cert>");
                                    ConfigData.AppendLine("<key>");
                                    ConfigData.AppendLine(UserKeyData.ToString());
                                    ConfigData.AppendLine("</key>");

                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedUser.Login}Config.ovpn", ConfigData.ToString());
                                    break;
                                case 1:

                                    break;
                                case 2:

                                    break;
                                case 3:

                                    break;
                                default:
                                    WindowsManager.Shared.ShowMessage("Programm Error!", $"Unknown eport type {SelectedIndexExport}", true);
                                    break;
                            }
                        }
                        catch (Exception err)
                        {
                            WindowsManager.Shared.ShowMessage("Error export!", err.Message, true);
                        }
                    }
                },(can) => SelectedUser.ID != -1));
            }
        }


        public MainWindowModel()
        {
        }

        public override void _WindowUpdate(object sender, EventArgs e)
        {
            Servers = SQLManager.Shared.GetServers();
            SelectedServer = Servers.Count != 0 ? Servers[0] : new Server();
        }

        private void _UpdateUsers()
        {
            if (SelectedServer != null && SelectedServer.ID != -1)
                Users = SQLManager.Shared.GetUsersFor(SelectedServer);
            else
                Users = new List<User>();
            SelectedUser = Users.Count != 0 ? Users[0] : new User();
        }

    }
}
