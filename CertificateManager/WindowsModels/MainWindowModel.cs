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
        private long _UserExportIndex = 0;
        public long UserExportIndex
        {
            get
            {
                return _UserExportIndex;
            }
            set
            {
                _UserExportIndex = value;
                OnPropertyChanged("UserExportIndex");
            }
        }

        private long _ServerExportIndex = 0;
        public long ServerExportIndex
        {
            get
            {
                return _ServerExportIndex;
            }
            set
            {
                _ServerExportIndex = value;
                OnPropertyChanged("ServerExportIndex");
            }
        }


        private List<Cert> _CAs = new List<Cert>();
        public List<Cert> CAs
        {
            get
            {
                return _CAs;
            }
            set
            {
                _CAs = value;
                OnPropertyChanged("CAs");
            }
        }

        private Cert _SelectedCA = new Cert();
        public Cert SelectedCA
        {
            get
            {
                return _SelectedCA;
            }
            set
            {
                _SelectedCA = value;
                OnPropertyChanged("SelectedCA");
                _UpdateServers();
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

        private CommandRelise _CreateCA;
        public CommandRelise CreateCA
        {
            get
            {
                return _CreateCA ?? (_CreateCA = new CommandRelise(obj =>
                {
                    WindowsManager.Shared.ShowWindow(new CreateCAWindow());
                }));
            }
        }

        private CommandRelise _CreateServer;
        public CommandRelise CreateServer
        {
            get
            {
                return _CreateServer ?? (_CreateServer = new CommandRelise(obj =>
                {
                    WindowsManager.Shared.ShowWindow(new CreateServerWindow(), SelectedCA);
                }, (can) => SelectedCA.ID != -1));
            }
        }

        private CommandRelise _CreateUser;
        public CommandRelise CreateUser
        {
            get
            {
                return _CreateUser ?? (_CreateUser = new CommandRelise(obj =>
                {
                    WindowsManager.Shared.ShowWindow(new CreateUserWindow(), SelectedCA, SelectedServer);
                }, (can) => SelectedServer.ID != -1));
            }
        }

        private CommandRelise _DeleteCA;
        public CommandRelise DeleteCA
        {
            get
            {
                return _DeleteCA ?? (_DeleteCA = new CommandRelise(obj =>
                {
                    if (WindowsManager.Shared.ShowQuestion("Warning!", $"Are you sure want delete this CA \"{SelectedCA.Name}\"?\nAllServers and users will delete too!"))
                    {
                        try
                        {
                            SQLManager.Shared.DeleteCA(SelectedCA);
                            WindowUpdate();
                        }
                        catch (Exception err)
                        {
                            WindowsManager.Shared.ShowMessage("Delete CA error!", err.Message, true);
                        }
                    }
                }, (can) => SelectedCA.ID != -1));
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
                            WindowUpdate();
                        }
                    }
                    catch (Exception err)
                    {
                        WindowsManager.Shared.ShowMessage("Delete server error!", err.Message, true);
                    }
                }, (can) => SelectedServer.ID != -1));
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
                            WindowUpdate();
                        }
                    }
                    catch (Exception err)
                    {
                        WindowsManager.Shared.ShowMessage("Delete user error", err.Message, true);
                    }
                }, (can) => SelectedUser.ID != -1));
            }
        }

        private CommandRelise _EditServer;
        public CommandRelise EditServer
        {
            get
            {
                return _EditServer ?? (_EditServer = new CommandRelise(obj =>
                {
                    WindowsManager.Shared.ShowWindow(new EditServerWindow(), SelectedServer);
                }, (can) => SelectedServer.ID != -1));
            }
        }

        private CommandRelise _EditUser;
        public CommandRelise EditUser
        {
            get
            {
                return _EditUser ?? (_EditUser = new CommandRelise(obj =>
                {
                    WindowsManager.Shared.ShowWindow(new EditUserWindow(), SelectedUser, SelectedServer);
                }, (can) => SelectedUser.ID != -1));
            }
        }

        private CommandRelise _CAExport;
        public CommandRelise CAExport
        {
            get
            {
                return _CAExport ?? (_CAExport = new CommandRelise(obj =>
                {
                    VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
                    folderBrowserDialog.UseDescriptionForTitle = true;
                    folderBrowserDialog.Description = "Select export folder";
                    folderBrowserDialog.RootFolder = Environment.SpecialFolder.DesktopDirectory;
                    folderBrowserDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\";
                    if (folderBrowserDialog.ShowDialog().GetValueOrDefault())
                    {
                        string ExportFolderPath = folderBrowserDialog.SelectedPath;

                        try
                        {
                            File.WriteAllText(ExportFolderPath + $"\\{SelectedCA.Name}_CA.crt", SelectedCA.CertToFile());
                            File.WriteAllText(ExportFolderPath + $"\\{SelectedCA.Name}_CA.key", SelectedCA.KeyToFile());
                        }
                        catch (Exception err)
                        {
                            WindowsManager.Shared.ShowMessage("Error export!", err.Message, true);
                        }
                    }
                },(can) => SelectedCA.ID != -1));
            }
        }

        private CommandRelise _ServerExport;
        public CommandRelise ServerExport
        {
            get
            {
                return _ServerExport ?? (_ServerExport = new CommandRelise(obj =>
                {
                    VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
                    folderBrowserDialog.UseDescriptionForTitle = true;
                    folderBrowserDialog.Description = "Select export folder";
                    folderBrowserDialog.RootFolder = Environment.SpecialFolder.DesktopDirectory;
                    folderBrowserDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\";
                    if (folderBrowserDialog.ShowDialog().GetValueOrDefault())
                    {
                        string ExportFolderPath = folderBrowserDialog.SelectedPath;

                        try
                        {
                            switch (ServerExportIndex)
                            {
                                case 0:
                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedCA.Name}_CA.crt", SelectedCA.CertToFile());
                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedServer.Name}.crt", SelectedServer.certificate.CertToFile());
                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedServer.Name}.key", SelectedServer.certificate.KeyToFile());
                                    break;
                                case 1:
                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedServer.Name}.conf", SelectedServer.GetConfig(SelectedCA));
                                    break;
                                case 2:
                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedServer.Name}.conf", SelectedServer.GetConfig(SelectedCA, false));

                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedCA.Name}_CA.crt", SelectedCA.CertToFile());
                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedServer.Name}.crt", SelectedServer.certificate.CertToFile());
                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedServer.Name}.key", SelectedServer.certificate.KeyToFile());
                                    break;
                                default:
                                    WindowsManager.Shared.ShowMessage("Programm Error!", $"Unknown eport type {UserExportIndex}", true);
                                    break;
                            }
                        }
                        catch (Exception err)
                        {
                            WindowsManager.Shared.ShowMessage("Error export!", err.Message, true);
                        }
                    }

                }, (can) => SelectedServer.ID != -1));
            }
        }

        private CommandRelise _UserExport;
        public CommandRelise UserExport
        {
            get
            {
                return _UserExport ?? (_UserExport = new CommandRelise(obj =>
                {
                    VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
                    folderBrowserDialog.UseDescriptionForTitle = true;
                    folderBrowserDialog.Description = "Select export folder";
                    folderBrowserDialog.RootFolder = Environment.SpecialFolder.DesktopDirectory;
                    folderBrowserDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\";

                    if (folderBrowserDialog.ShowDialog().GetValueOrDefault())
                    {
                        string ExportFolderPath = folderBrowserDialog.SelectedPath;
                        

                        try
                        {
                            switch (UserExportIndex)
                            {
                                case 0:

                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedUser.Login}.ovpn", SelectedUser.GetConfigForServer(SelectedServer, SelectedCA));
                                    break;
                                case 1:

                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedUser.Login}.ovpn", SelectedUser.GetConfigForServer(SelectedServer, SelectedCA, false));

                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedCA.Name}_CA.crt", SelectedCA.CertToFile());
                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedUser.Login}.crt", SelectedUser.certificate.CertToFile());
                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedUser.Login}.key", SelectedUser.certificate.KeyToFile());
                                    break;
                                case 2:
                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedCA.Name}_CA.crt", SelectedCA.CertToFile());
                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedUser.Login}.crt", SelectedUser.certificate.CertToFile());
                                    File.WriteAllText(ExportFolderPath + $"\\{SelectedUser.Login}.key", SelectedUser.certificate.KeyToFile());
                                    break;
                                default:
                                    WindowsManager.Shared.ShowMessage("Programm Error!", $"Unknown eport type {UserExportIndex}", true);
                                    break;
                            }
                        }
                        catch (Exception err)
                        {
                            WindowsManager.Shared.ShowMessage("Error export!", err.Message, true);
                        }
                    }
                }, (can) => SelectedUser.ID != -1));
            }
        }

        public  void WindowUpdate()
        {
            CAs = SQLManager.Shared.GetCAs();
            SelectedCA = CAs.Count != 0 ? CAs[0] : new Cert();
        }

        private void _UpdateServers()
        {
            if (SelectedCA != null && SelectedCA.ID != -1)
                Servers = SQLManager.Shared.GetServersForCa(SelectedCA);
            else
                Servers = new List<Server>();
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
