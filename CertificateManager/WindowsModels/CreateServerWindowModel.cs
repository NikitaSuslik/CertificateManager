using System;
using System.Collections.Generic;
using System.Text;
using CertificateManager.Models;

namespace CertificateManager.WindowsModels
{
    class CreateServerWindowModel : MyWindowModel
    {

        private Cert _CA = new Cert();

        public CertificateGroupBoxModel CertificateModel
        {
            get; set;
        } = new CertificateGroupBoxModel();
        public ServerGroupBoxModel ServerModel
        {
            get; set;
        } = new ServerGroupBoxModel();

        public CreateServerWindowModel()
        {
            ServerModel.ANameChanged += ServerNameChanged;
        }

        private void ServerNameChanged()
        {
            CertificateModel.Name = ServerModel.Name;
            CertificateModel.CommonName = ServerModel.Name;
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
                        Server server = ServerModel.NewServer;
                        if(server == null)
                        {
                            WindowsManager.Shared.ShowMessage("Info", "Fill all fields in \"Server\" group!", false);
                            return;
                        }
                        server.certificate = CertificateModel.NewCert;
                        if (server.certificate == null)
                        {
                            WindowsManager.Shared.ShowMessage("Info", "Fill all fields in \"Certificate\" group!", false);
                            return;
                        }
                        SQLManager.Shared.AddServer(server, _CA);
                    }
                    catch(Exception err)
                    {
                        WindowsManager.Shared.ShowMessage("Server create error!", err.Message, true);
                        return;
                    }
                    WindowsManager.Shared.CloseCurrentWindow();
                }));
            }
        }
        protected override void _PropsChanged()
        {
            _CA = (Cert)props[0];
            CertificateModel.props = props;
        }
    }
}
