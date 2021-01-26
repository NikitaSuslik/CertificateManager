using System;
using System.Collections.Generic;
using System.Text;

namespace CertificateManager.Models
{
    class User
    {
        public Cert certificate
        {
            get; set;
        } = new Cert();
        public long ID
        {
            get; set;
        } = -1;
        public string Login
        {
            get; set;
        } = "";
        public string Password
        {
            get; set;
        }
        public string Params
        {
            get; set;
        }

        public string GetConfigForServer(Server serv, Cert CA, bool certInConfig = true)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("client");
            builder.AppendLine($"remote {serv.IP} {serv.Port}");
            builder.AppendLine($"dev {serv.SMode}");
            builder.AppendLine($"proto {serv.SProto}");
            builder.AppendLine("remote-cert-tls server");
            builder.AppendLine("auth SHA1");
            builder.AppendLine($"cipher {serv.SCipher}");
            builder.AppendLine("auth-user-pass");
            builder.AppendLine("resolv-retry infinite");
            builder.AppendLine("persist-key");
            builder.AppendLine("persist-tun");

            if(certInConfig)
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
                builder.AppendLine($"cert {Login}.crt");
                builder.AppendLine($"key {Login}.key");
            }

            builder.AppendLine(Params);

            return builder.ToString();
        }
    }
}
