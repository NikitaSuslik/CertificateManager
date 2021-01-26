using CertificateManager.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CertificateManager.WindowsModels
{
    class EditServerWindowModel : MyWindowModel
    {
        public ServerGroupBoxModel ServerModel
        {
            get; set;
        } = new ServerGroupBoxModel();

        private CommandRelise _CancelButton;
        public CommandRelise CancelButton
        {
            get
            {
                return _CancelButton ?? (_CancelButton = new CommandRelise(obj =>
                {
                    WindowsManager.Shared.CloseCurrentWindow();
                }));
            }
        }

        private CommandRelise _SaveButton;

        public CommandRelise SaveButton
        {
            get
            {
                return _SaveButton ?? (_SaveButton = new CommandRelise(obj =>
                {

                }));
            }
        }

        protected override void _PropsChanged()
        {
            Server server = props[0] as Server;
            ServerModel.Name = server.Name;
            ServerModel.IP = server.IP;
            ServerModel.Mode = (long)server.Mode;
            ServerModel.Params = server.Params;
            ServerModel.Port = server.Port.ToString();
            ServerModel.Proto = (long)server.Proto;
            ServerModel.CipherIndex = (long)server.Cipher;
        }

        public EditServerWindowModel()
        {

        }
    }
}
