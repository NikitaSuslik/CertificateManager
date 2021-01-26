using System;
using System.Collections.Generic;
using System.Text;
using CertificateManager.Models;

namespace CertificateManager.WindowsModels
{
    class CreateUserWindowModel : MyWindowModel
    {
        private Cert _CA = null;
        private Server _Server = null;

        public CertificateGroupBoxModel CertificateModel
        {
            get; set;
        } = new CertificateGroupBoxModel();
        public UserGroupBoxModel UserModel
        {
            get; set;
        } = new UserGroupBoxModel();

        public CreateUserWindowModel()
        {
            UserModel.ANameChanged += ServerNameChanged;
        }

        private void ServerNameChanged()
        {
            CertificateModel.Name = UserModel.Login;
            CertificateModel.CommonName = UserModel.Login;
        }

        private CommandRelise _OkButton;

        public CommandRelise OkButton
        {
            get
            {
                return _OkButton ?? (_OkButton = new CommandRelise(obj =>
                {
                    try
                    {
                        User user = UserModel.NewUser;
                        if(user == null)
                        {
                            WindowsManager.Shared.ShowMessage("Info", "Fill all fields in \"User\" group!", false);
                            return;
                        }
                        user.certificate = CertificateModel.NewCert;
                        if (user.certificate == null)
                        {
                            WindowsManager.Shared.ShowMessage("Info", "Fill all fields in \"Certificate\" group!", false);
                            return;
                        }
                        SQLManager.Shared.AddUser(user, _Server, _CA);
                    }
                    catch(Exception err)
                    {
                        WindowsManager.Shared.ShowMessage("User create error!", err.Message, true);
                        return;
                    }
                    WindowsManager.Shared.CloseCurrentWindow();
                }));
            }
        }



        protected override void _PropsChanged()
        {
            _CA = (Cert)props[0];
            _Server = (Server)props[1];
            CertificateModel.props = props;
            UserModel.props = props;
        }
    }
}
