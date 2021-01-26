using CertificateManager.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CertificateManager.WindowsModels
{
    class CreateCAWindowModel : MyWindowModel
    {
        public CertificateGroupBoxModel CertificateModel
        {
            get; set;
        } = new CertificateGroupBoxModel();

        private CommandRelise _OkButton;
        public CommandRelise OkButton
        {
            get
            {
                return _OkButton ?? (_OkButton = new CommandRelise(obj =>
                {
                    try
                    {
                        Cert cert = new Cert();
                        cert = CertificateModel.NewCert;
                        if (cert == null)
                        {
                            WindowsManager.Shared.ShowMessage("Info", "Fill all fields in \"Certificate\" group!", false);
                            return;
                        }
                        SQLManager.Shared.AddCACert(cert);
                    }
                    catch (Exception err)
                    {
                        WindowsManager.Shared.ShowMessage("Server create error!", err.Message, true);
                        return;
                    }
                    WindowsManager.Shared.CloseCurrentWindow();
                }));
            }
        }

    }
}
