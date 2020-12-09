using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace CertificateManager.Models
{
    class Server
    {
        public enum Protocol
        {
            TCP,
            UDP
        }
        public enum LayerMode
        {
            TUN,
            TAP
        }

        public Cert certificate
        {
            get; set;
        } = new Cert();
        public long ID
        {
            get; set;
        } = -1;
        public string IP
        {
            get; set;
        }
        public long Port
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public Protocol Proto
        {
            get; set;
        }
        public LayerMode Mode
        {
            get; set;
        }
        public string Params
        {
            get; set;
        }

        public string SProto
        {
            get
            {
                return Proto == 0 ? "tcp" : "udp";
            }
        }

        public string SMode
        {
            get
            {
                return Mode == 0 ? "tun" : "tap";
            }
        }
    }
}
