using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

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
                    string PathExport = 
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
