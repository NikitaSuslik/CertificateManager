using System;
using System.Collections.Generic;
using System.Text;

using CertificateManager.Models;

namespace CertificateManager.WindowsModels
{
    class CertificateGroupBoxModel : MyWindowModel
    {
        private Cert _parentCert = null;
        private Cert _ParentCert
        {
            get
            {
                return _parentCert;
            }
            set
            {
                _parentCert = value;
                OnPropertyChanged("CanEdit");
            }
        }

        public bool CanEdit
        {
            get
            {
                return _parentCert == null;
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
                Cert c = new Cert();
                c.Name = Name;
                c.CommonName = CommonName;
                c.Country = Country;
                c.Organisation = Organisation;
                c.OrganisationUnit = OrganisationUnit;
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
            {
                return _Name;
            }
            set
            {
                _Name = value;
                OnPropertyChanged("Name");
            }
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

        public List<long> KeySize
        {
            get { return new List<long>() { 256, 512, 1024, 2048, 4096}; }
        }

        private int _SelectedKeySizeIndex = 0;
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
                _ParentCert = ((Server)props[0]).certificate;
                Country = _ParentCert.Country;
                Organisation = _ParentCert.Organisation;
                SelectedKeySizeIndex = KeySize.IndexOf(_ParentCert.KeySize);
            }
        }

    }
}
