using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace CertificateManager.Models
{
    class Server
    {
        public enum ProtocolName
        {
            TCP,
            UDP
        }
        public enum ModeName
        {
            TUN,
            TAP
        }
        public enum CipherName
        {
            AES128,
            AES192,
            AES256
        }

        public enum AuthName
        {
            SHA1
        }

        public string Name
        {
            get; set;
        }
        public long ID
        {
            get; set;
        } = -1;

        public Cert certificate
        {
            get; set;
        } = new Cert();
        public string IP
        {
            get; set;
        }
        public long Port
        {
            get; set;
        }
        public ProtocolName Proto
        {
            get; set;
        }
        public ModeName Mode
        {
            get; set;
        }
        public CipherName Cipher
        {
            get; set;
        }
        public AuthName Auth
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
        public string SCipher
        {
            get
            {
                if (ID == -1)
                    return "";
                else
                {
                    switch (Cipher)
                    {
                        case CipherName.AES128:
                            return "AES-128-CBC";
                        case CipherName.AES192:
                            return "AES-192-CBC";
                        case CipherName.AES256:
                            return "AES-256-CBC";
                        default:
                            return "Unknown";
                    }
                }
            }
        }

        public string GetConfig(Cert CA, bool certInConfig = true)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("mode server");
            builder.AppendLine($"dev {SMode}");
            builder.AppendLine($"proto {SProto}");
            builder.AppendLine($"port {Port}");
            builder.AppendLine("tls-server");
            builder.AppendLine("auth SHA1");
            builder.AppendLine($"cipher {SCipher}");
            builder.AppendLine("resolv-retry infinite");
            builder.AppendLine("persist-key");
            builder.AppendLine("persist-tun");

            if (certInConfig)
            {
                builder.AppendLine("<ca>");
                builder.AppendLine(CA.CertToFile());
                builder.AppendLine("</ca>");
                builder.AppendLine("<cert>");
                builder.AppendLine(certificate.CertToFile());
                builder.AppendLine("</cert>");
                builder.AppendLine("<key>");
                builder.AppendLine(certificate.KeyToFile());
                builder.AppendLine("</key>");
            }
            else
            {
                builder.AppendLine($"ca {CA.Name}_CA.crt");
                builder.AppendLine($"cert {Name}.crt");
                builder.AppendLine($"key {Name}.key");
            }

            builder.AppendLine(Params);

            return builder.ToString();
        }
    }
}
