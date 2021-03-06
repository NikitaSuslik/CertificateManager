using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace CertificateManager.Models
{
    class Cert
    {
        public long ID
        {
            get; set;
        } = -1;
        public string Name
        {
            get; set;
        }
        public string CommonName
        {
            get; set;
        }
        public string Country
        {
            get; set;
        }
        public string State
        {
            get; set;
        }
        public string Local
        {
            get; set;
        }
        public string Organisation
        {
            get; set;
        }
        public string OrganisationUnit
        {
            get; set;
        }
        public long KeySize
        {
            get; set;
        }
        public DateTime DateStart
        {
            get; set;
        } = DateTime.Now;
        public DateTime DateStop
        {
            get; set;
        } = DateTime.Now;
        public X509Certificate2 certificate
        {
            get; set;
        }

        public string SDateStart
        {
            get
            {
                return DateStart.ToString("dd.MM.yyyy");
            }
        }
        public string SDateStop
        {
            get
            {
                return DateStop.ToString("dd.MM.yyyy");
            }
        }

        public string CertToFile()
        {
            string[] cert64 = RSAHelper.GetCert64(this);

            StringBuilder builder = new StringBuilder();
            StringBuilder cert = new StringBuilder(cert64[0]);
            for (int i = 70; i < cert.Length; i += 71)
            {
                cert.Insert(i, '\n');
            }

            builder.AppendLine("-----BEGIN CERTIFICATE-----");
            builder.AppendLine(cert.ToString());
            builder.AppendLine("-----END CERTIFICATE-----");

            return builder.ToString();
        }

        public string KeyToFile()
        {
            string[] cert64 = RSAHelper.GetCert64(this);

            StringBuilder builder = new StringBuilder();
            StringBuilder key = new StringBuilder(cert64[1]);
            for (int i = 70; i < key.Length; i += 71)
            {
                key.Insert(i, '\n');
            }

            builder.AppendLine($"-----BEGIN {cert64[2]} PRIVATE KEY-----");
            builder.AppendLine(key.ToString());
            builder.AppendLine($"-----END {cert64[2]} PRIVATE KEY-----");

            return builder.ToString();
        }

    }
}
