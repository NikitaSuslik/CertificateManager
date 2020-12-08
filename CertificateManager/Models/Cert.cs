using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
