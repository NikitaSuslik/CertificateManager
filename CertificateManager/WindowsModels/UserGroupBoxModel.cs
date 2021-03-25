using System;
using System.Collections.Generic;
using System.Text;
using CertificateManager.Models;

namespace CertificateManager.WindowsModels
{
    class UserGroupBoxModel : MyWindowModel
    {
        public delegate void NameChanged();
        public NameChanged ANameChanged;

        public UserGroupBoxModel()
        {
            
        }

        private Server _server = null;
        public Server Server
        {
            get
            {
                return _server;
            }
            private set
            {
                _server = value;
            }
        }

        private string _Login = "";
        public string Login
        {
            get
            {
                return _Login;
            }
            set
            {
                _Login = value;
                OnPropertyChanged("Login");
                ANameChanged?.Invoke();
            }
        }
        private string _Password = "";
        public string Password
        {
            get
            {
                return _Password;
            }
            set
            {
                _Password = value;
                OnPropertyChanged("Password");
            }
        }
        private string _ServerIP = "";
        public string ServerIP
        {
            get
            {
                return _ServerIP;
            }
            set
            {
                _ServerIP = value;
                OnPropertyChanged("ServerIP");
            }
        }
        private string _ServerPort = "";
        public string ServerPort
        {
            get
            {
                return _ServerPort;
            }
            set
            {
                _ServerPort = value;
                OnPropertyChanged("ServerPort");
            }
        }
        private string _Params = "";
        public string Params
        {
            get
            {
                return _Params;
            }
            set
            {
                _Params = value;
                OnPropertyChanged("Params");
            }
        }
        private long _Proto = 0;
        public long Proto
        {
            get
            {
                return _Proto;
            }
            set
            {
                _Proto = value;
                OnPropertyChanged("Proto");
            }
        }
        private long _Mode = 0;
        public long Mode
        {
            get
            {
                return _Mode;
            }
            set
            {
                _Mode = value;
                OnPropertyChanged("Mode");
            }
        }

        private CommandRelise _PasswordGenerate;
        public CommandRelise PasswordGenerate
        {
            get
            {
                return _PasswordGenerate ?? (_PasswordGenerate = new CommandRelise(obj =>
                {
                    Password = _GeneratePassword();
                }));
            }
        }

        private string _GeneratePassword()
        {
            int length = 6;
            char[] cpass = new char[length];
            List<int> sym = new List<int> { 34, 39, 40, 41, 42, 43, 44, 45, 46, 47, 58, 59, 60, 61, 62, 63, 91, 92, 93, 94, 95, 96};
            Random r = new Random();
            for (int i = 0; i < length; i++)
            {
                int c = r.Next(33, 122);
                while (sym.Contains(c))
                {
                    c = r.Next(33, 122);
                }
                cpass[i] = (char)c;
            }

            return new string(cpass);
        }

        public User NewUser
        {
            get
            {
                User u = new User();
                u.Login = Login;
                u.Password = Password;
                u.Params = Params;
                return u;
            }
        }

        protected override void _PropsChanged()
        {
            if (props.Length != 0)
            {
                Server = (Server)props[1];
                ServerIP = Server.IP;
                ServerPort = Server.Port.ToString();
                Mode = (long)Server.Mode;
                Proto = (long)Server.Proto;
            }
        }

    }
}
