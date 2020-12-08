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
        public string Parameters
        {
            get; set;
        }
    }
}
