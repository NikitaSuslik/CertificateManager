using System;
using System.Collections.Generic;
using System.Text;
using CertificateManager.Models;

namespace CertificateManager.WindowsModels
{
    class ServerGroupBoxModel : MyWindowModel
    {
        public delegate void NameChanged();
        public NameChanged ANameChanged;

        public ServerGroupBoxModel()
        {

        }

        private string _Name = "";
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                OnPropertyChanged("Name");
                ANameChanged?.Invoke();
            }
        }
        private string _IP = "";
        public string IP
        {
            get
            {
                return _IP;
            }
            set
            {
                _IP = value;
                OnPropertyChanged("IP");
            }
        }
        private string _Port = "";
        public string Port
        {
            get
            {
                return _Port;
            }
            set
            {
                _Port = value;
                OnPropertyChanged("Port");
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

        public Server NewServer
        {
            get
            {
                if (!_AllField())
                    return null;
                long port;
                try
                {
                    port = long.Parse(Port);
                }
                catch (FormatException)
                {
                    throw new Exception("Wrong port format!");
                }
                Server s = new Server();
                s.Name = Name;
                s.IP = IP;
                s.Port = port;
                s.Params = Params;
                s.Proto = (Server.Protocol)Proto;
                s.Mode = (Server.LayerMode)Mode;
                return s;
            }
        }

        private bool _AllField()
        {
            return Name != "" && IP != "" && Port != "";
        }
    }
}
