using System;
using System.Collections.Generic;
using System.Text;

using CertificateManager.Models;

namespace CertificateManager.WindowsModels
{
    class CertificateGroupBoxModel : MyWindowModel
    {
        private Cert _CA = null;
        private Cert CA
        {
            get
            {
                return _CA;
            }
            set
            {
                _CA = value;
                OnPropertyChanged("IsActive");
            }
        }

        public bool IsActive
        {
            get
            {
                return CA == null;
            }
        }

        public Cert NewCert
        {
            get
            {
                if (!_AllField())
                {
                    return null;
                }
                long days = 0;

                try
                {
                    days = long.Parse(Days);
                }
                catch (FormatException)
                {
                    throw new Exception($"In \"Days\" can digits only!");
                }
                catch (Exception err)
                {
                    throw new Exception($"Error parse new certificate: {err.Message}");
                }

                Cert c = new Cert();
                c.Name = Name;
                c.CommonName = CommonName;
                c.Country = Country;
                c.State = State;
                c.Local = Local;
                c.Organisation = Organisation;
                c.OrganisationUnit = OrganisationUnit;
                c.DateStart = DateTime.Now;
                c.DateStop = DateTime.Now.AddDays(days);
                c.KeySize = KeySize[SelectedKeySizeIndex];
                return c;
            }
        }

        private bool _AllField()
        {
            return Name != "" && CommonName != "" && Country.Length == 2 && Organisation != "" && OrganisationUnit != "";
        }

        private string _Name = "";
        public string Name
        {
            get
            { return _Name; }
            set
            { _Name = value; OnPropertyChanged("Name"); CommonName = value; }
        }

        private string _CommonName = "";
        public string CommonName
        {
            get { return _CommonName; }
            set { _CommonName = value; OnPropertyChanged("CommonName"); }
        }

        private string _Country = "";
        public string Country
        {
            get { return _Country; }
            set { _Country = value; OnPropertyChanged("Country"); }
        }

        private string _State = "";
        public string State
        {
            get { return _State; }
            set { _State = value; OnPropertyChanged("State"); }
        }

        private string  _Local;

        public string  Local
        {
            get { return _Local; }
            set { _Local = value; OnPropertyChanged("Local"); }
        }


        private string _Organisation = "";
        public string Organisation
        {
            get { return _Organisation; }
            set { _Organisation = value; OnPropertyChanged("Organisation"); }
        }

        private string _OrganisationUnit = "";
        public string OrganisationUnit
        {
            get { return _OrganisationUnit; }
            set { _OrganisationUnit = value; OnPropertyChanged("OrganisationUnit"); }
        }

        private string _Days;
        public string Days
        {
            get { return _Days; }
            set { _Days = value; OnPropertyChanged("Days"); }
        }

        public List<long> KeySize
        {
            get { return new List<long>() { 256, 512, 1024, 2048, 4096}; }
        }

        private int _SelectedKeySizeIndex = 3;
        public int SelectedKeySizeIndex
        {
            get { return _SelectedKeySizeIndex; }
            set { _SelectedKeySizeIndex = value; OnPropertyChanged("SelectedKeySizeIndex"); }
        }

        public CertificateGroupBoxModel()
        {

        }

        protected override void _PropsChanged()
        {
            if (props.Length != 0)
            {
                CA = (Cert)props[0];
                Country = CA.Country;
                State = CA.State;
                Local = CA.Local;
                Organisation = CA.Organisation;
                OrganisationUnit = CA.OrganisationUnit;
                SelectedKeySizeIndex = KeySize.IndexOf(CA.KeySize);
            }
        }

    }
}
